using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using MonoGame.Squid.Controls;
using MonoGame.Squid.Interfaces;
using MonoGame.Squid.Util;

namespace MonoGame.Squid.Xml
{
    public class XmlIgnoreAttribute : Attribute { }

    internal class Node
    {
        public Node Parent;
        public bool WriteType;
        public string Name;
        public string Type;
        public string Value;
        public string Key;
        public int RefId;
        public int Reference;

        public List<Node> Nodes = new List<Node>();
    }

    public class XmlSerializer : IDisposable
    {
        private Node _root;

        private readonly Dictionary<int, object> _readCache = new Dictionary<int, object>();
        private readonly Dictionary<object, Node> _writeCache = new Dictionary<object, Node>();

        private int _increment;

        public string Serialize(object data)
        {
            CreateLogicalTree(data);

            var output = new StringBuilder();
            var writer = new XmlWriter(output, true);

            Write(_root, writer);

            writer.Flush();
            writer.Close();
            writer = null;

            return output.ToString();
        }

        public T Deserialize<T>(string xml)
        {
            var reader = new XmlReader(xml);

            _root = ReadXml(reader);

            reader = null;

            _readCache.Clear();

            return (T)Deserialize(_root, typeof(T));
        }

        private void CreateLogicalTree(object data)
        {
            try
            {
                _writeCache.Clear();
                _root = CreateLogicalNode(data, false, false);
                _writeCache.Clear();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private Node CreateLogicalNode(object data, bool writeType, bool useCache)
        {
            var type = data.GetType();

            var result = new Node { Name = type.Name, Type = type.FullName, WriteType = writeType };

            if (_writeCache.ContainsKey(data))
            {
                if (useCache)
                    return _writeCache[data];

                if (_writeCache[data].RefId == 0)
                {
                    _increment++;
                    _writeCache[data].RefId = _increment;
                }

                result.WriteType = false;
                result.Reference = _writeCache[data].RefId;
                return result;
            }
            else
            {
                _writeCache.Add(data, result);
            }

            var properties = Reflector.GetProperties(type);// type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (properties.Length < 1) result.Value = data.ToString();

            foreach (var info in properties)
            {
                if (info.IsDefined(typeof(XmlIgnoreAttribute), true)) continue;
                if (!info.CanRead) continue;
                if (!info.CanWrite) continue;
                if (info.GetSetMethod() == null) continue;

                var value = info.GetValue(data, null);
                if (value == null) continue;

                // check vs. default value
                var attributes = info.GetCustomAttributes(typeof(DefaultValueAttribute), true);
                if (attributes.Length > 0)
                {
                    var def = attributes[0] as DefaultValueAttribute;
                    if (def.Value != null)
                    {
                        if (def.Value.Equals(value))
                            continue;
                    }
                }

                var valueType = value.GetType();

                if (info.PropertyType.IsPrimitive || info.PropertyType == typeof(string) || info.PropertyType.IsEnum)
                {
                    var flags = Reflector.GetAttribute<FlagsAttribute>(info.PropertyType);

                    if (flags != null)
                    {
                        var sub = new Node { Name = info.Name, Value = ((int)value).ToString() };
                        result.Nodes.Add(sub);
                    }
                    else
                    {
                        var sub = new Node { Name = info.Name, Value = value.ToString() };
                        result.Nodes.Add(sub);
                    }
                }
                else if (valueType.IsValueType)
                {
                    var converter = System.ComponentModel.TypeDescriptor.GetConverter(valueType);
                    if (converter != null)
                    {
                        try
                        {
                            var str = converter.ConvertToString(value);
                            var sub = new Node { Name = info.Name, Value = str };
                            result.Nodes.Add(sub);
                        }
                        catch (Exception exc)
                        {
                            throw exc;
                        }
                    }
                    else
                    {
                        var sub = CreateLogicalNode(value, !info.PropertyType.FullName.Equals(valueType.FullName), false);
                        sub.Name = info.Name;
                        result.Nodes.Add(sub);
                    }
                }
                else if (value is IList)
                {
                    var sub = new Node { Name = info.Name, Type = valueType.FullName };
                    result.Nodes.Add(sub);

                    string name = null;
                    string fullname = null;

                    if (valueType.IsGenericType || valueType.BaseType.IsGenericType)
                    {
                        var t = valueType.IsGenericType ? valueType : valueType.BaseType;

                        var gens = t.GetGenericArguments();
                        if (gens.Length > 0)
                        {
                            name = gens[0].Name;
                            fullname = gens[0].FullName;
                        }
                    }
                    else
                        fullname = ((IList)value)[0].GetType().FullName;

                    foreach (var item in ((IList)value))
                    {
                        if (item != null)
                        {
                            var stype = item.GetType();
                            var child = CreateLogicalNode(item, !fullname.Equals(item.GetType().FullName), false);

                            if (!string.IsNullOrEmpty(name))
                                child.Name = name;

                            sub.Nodes.Add(child);
                        }
                        else
                        {
                            var child = new Node { Name = name };
                            sub.Nodes.Add(child);
                        }
                    }
                }
                else if (value is IDictionary)
                {
                    var sub = new Node { Name = info.Name, Type = valueType.FullName };
                    result.Nodes.Add(sub);

                    Type itemType = null;
                  
                    if (valueType.IsGenericType)
                    {
                        var gens = valueType.GetGenericArguments();
                        if (gens.Length > 0)
                            itemType = gens[1];
                    }
                    else if (valueType.BaseType.IsGenericType)
                    {
                        var gens = valueType.BaseType.GetGenericArguments();
                        if (gens.Length > 0)
                            itemType = gens[1];
                    }

                    var dict = (IDictionary)value;
                   
                    foreach (var key in dict.Keys)
                    {
                        var element = CreateLogicalNode(dict[key], true, false);
                        //Node element = CreateLogicalNode(dict[key], !dict[key].GetType().FullName.Equals(itemType.FullName), false);
                        element.Key = key.ToString();
                        sub.Nodes.Add(element);
                    }
                }
                //else if (value is Entity)// && info.GetAttribute<NoRefAttribute>() == null)
                //{
                //    // entity as a property - we want a reference here, not the actual entity
                //    Node sub = new Node { Name = info.Name, WriteType = false };

                //    if (WriteCache.ContainsKey(value))
                //        sub.Reference = WriteCache[value].RefID;
                //    else
                //    {
                //        Node cached = CreateNode(value, false, false);

                //        if (WriteCache[value].RefID == 0)
                //        {
                //            Increment++;
                //            WriteCache[value].RefID = Increment;
                //        }

                //        sub.Reference = cached.RefID;
                //    }

                //    result.Nodes.Add(sub);
                //}
                else
                {
                    var sub = CreateLogicalNode(value, !info.PropertyType.FullName.Equals(valueType.FullName), false);
                    sub.Name = info.Name;
                    result.Nodes.Add(sub);
                }
            }

            if (data is Control)
            {
                //    Entity entity = data as Entity;

                //    if (!entity.IsPrototype)
                //    {
                //        Node sub = new Node { Name = "Components", Type = typeof(Component).FullName };
                //        result.Nodes.Add(sub);

                //        foreach (Component c in entity.Components)
                //        {
                //            Node child = CreateNode(c, true, false, resources);
                //            child.Name = "Component";
                //            sub.Nodes.Add(child);
                //        }

                var elements = ((Control)data).GetElements();

                if (elements.Count > 0)
                {
                    var sub = new Node { Name = "Elements", Type = typeof(Control).FullName };
                    result.Nodes.Add(sub);

                    foreach (var e in elements)
                        sub.Nodes.Add(CreateLogicalNode(e, false, true));
                }
                //    }
                //    else
                //    {
                //        Node sub = new Node { Name = "Components", Type = typeof(Component).FullName };
                //        result.Nodes.Add(sub);
                //        Node child = CreateNode(entity.Transform, true, false, resources);
                //        child.Name = "Component";
                //        sub.Nodes.Add(child);
                //    }
            }

            return result;
        }

        private void Write(Node node, XmlWriter writer)
        {
            writer.WriteStartElement(node.Name);

            if (node.WriteType) writer.WriteAttributeString("xtype", node.Type);
            if (!string.IsNullOrEmpty(node.Key)) writer.WriteAttributeString("xkey", node.Key);
            if (node.RefId != 0) writer.WriteAttributeString("xrefid", node.RefId.ToString());
            if (node.Reference != 0) writer.WriteAttributeString("xref", node.Reference.ToString());

            if (node.Value != null)
                writer.WriteValue(node.Value);
            else
            {
                foreach (var sub in node.Nodes)
                    Write(sub, writer);
            }

            writer.WriteEndElement();
        }

        private Node ReadXml(XmlReader reader)
        {
            try
            {
                Node current = null;
                _root = null;

                while (!reader.Eof)
                {
                    if (!reader.Read()) return _root;

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:

                            var node = new Node { Name = reader.Name };
                            node.Parent = current;

                            if (_root == null)
                                _root = node;

                            if (current != null)
                                current.Nodes.Add(node);

                            if (reader.HasAttributes)
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name.Equals("xtype"))
                                        node.Type = reader.Value;
                                    else if (reader.Name.Equals("xkey"))
                                        node.Key = reader.Value;
                                    else if (reader.Name.Equals("xrefid"))
                                        node.RefId = Convert.ToInt32(reader.Value);
                                    else if (reader.Name.Equals("xref"))
                                        node.Reference = Convert.ToInt32(reader.Value);
                                    else
                                    {
                                        var sub1 = new Node { Name = reader.Name, Value = reader.Value };
                                        node.Nodes.Add(sub1);
                                    }
                                }
                            }

                            current = node;

                            if (reader.IsEmptyElement)
                                current = current.Parent;

                            break;
                        case XmlNodeType.EndElement:
                            if (!reader.IsEmptyElement)
                                current = current.Parent;
                            break;
                        case XmlNodeType.Text:

                            if (!string.IsNullOrEmpty(reader.Value))
                            {
                                var value = reader.Value.Trim();

                                if (!string.IsNullOrEmpty(value))
                                    current.Value = reader.Value;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {
            }
            return _root;
        }

        private Node FindNode(Node parent, int reference)
        {
            var found = parent.Nodes.Find(x => x.RefId == reference);
            if (found != null) return found;

            foreach (var sub in parent.Nodes)
            {
                found = FindNode(sub, reference);
                if (found != null)
                    break;
            }
            return found;
        }

        private object DeserializeTo(object target, Node node, Type propertyType)
        {
            if (node.Reference != 0)
            {
                if (_readCache.ContainsKey(node.Reference))
                    return _readCache[node.Reference];
                else
                {
                    var refnode = FindNode(_root, node.Reference);
                    if (refnode != null)
                        return Deserialize(refnode, propertyType);
                }
            }

            var type = target.GetType();
            var result = target;
            object value = null;

            if (!(result is IList || result is IDictionary))
            {
                var properties = Reflector.GetProperties(type);

                foreach (var info in properties)
                {
                    if (info.IsDefined(typeof(XmlIgnoreAttribute), true)) continue;
                    if (!info.CanWrite) continue;
                    if (info.GetSetMethod() == null) continue;

                    var child = node.Nodes.Find(x => x.Name.Equals(info.Name));

                    if (child == null) continue;

                    value = null;

                    if (!string.IsNullOrEmpty(child.Value))
                    {
                        var converter = System.ComponentModel.TypeDescriptor.GetConverter(info.PropertyType);
                        if (converter != null)
                            value = converter.ConvertFromString(child.Value);
                    }
                    else
                    {
                        try
                        {
                            value = Deserialize(child, info.PropertyType);
                        }
                        catch
                        {

                        }
                    }

                    if (value != null)
                        info.SetValue(result, value, null);
                }
            }

            #region special typeof(Control) case

            if (result is Control)
            {
                Node child;

                if (result is IControlContainer)
                {
                    var container = result as IControlContainer;
                    child = node.Nodes.Find(x => x.Name.Equals("Controls"));

                    if (child != null)
                    {
                        for (var i = 0; i < child.Nodes.Count; i++)
                        {
                            value = null;

                            if (container.Controls.Count > i)
                            {
                                DeserializeTo(container.Controls[i], child.Nodes[i], typeof(Control));
                            }
                            else
                            {
                                value = Deserialize(child.Nodes[i], typeof(Control));

                                if (value != null)
                                    container.Controls.Add(value as Control);
                            }
                        }
                    }
                }

                var elements = ((Control)result).GetElements();
                child = node.Nodes.Find(x => x.Name.Equals("Elements"));

                if (child != null)
                {
                    for (var i = 0; i < child.Nodes.Count; i++)
                    {
                        DeserializeTo(elements[i], child.Nodes[i], typeof(Control));
                    }
                }
            }

            #endregion

            if (result is IList)
            {
                Type itemType = null;
                if (type.IsGenericType)
                {
                    var gens = type.GetGenericArguments();
                    if (gens.Length > 0)
                        itemType = gens[0];
                }

                foreach (var child in node.Nodes)
                {
                    ((IList)result).Add(Deserialize(child, itemType));
                }
            }

            if (result is IDictionary)
            {
                Type keyType = null;
                Type itemType = null;

                if (type.IsGenericType)
                {
                    var gens = type.GetGenericArguments();
                    if (gens.Length > 1)
                    {
                        keyType = gens[0];
                        itemType = gens[1];
                    }
                }

                var converter = System.ComponentModel.TypeDescriptor.GetConverter(keyType);
                if (converter != null)
                {
                    foreach (var child in node.Nodes)
                    {
                        var key = converter.ConvertFromString(child.Key);
                        ((IDictionary)result).Add(key, Deserialize(child, itemType));
                    }
                }
            }

            if (node.RefId != 0)
            {
                if (!_readCache.ContainsKey(node.RefId))
                    _readCache.Add(node.RefId, result);
            }

            return result;
        }

        private object Deserialize(Node node, Type propertyType)
        {
            if (node.Reference != 0)
            {
                if (_readCache.ContainsKey(node.Reference))
                    return _readCache[node.Reference];
                else
                {
                    var refnode = FindNode(_root, node.Reference);
                    if (refnode != null)
                        return Deserialize(refnode, propertyType);
                }
            }

            Type type = null;

            if (!string.IsNullOrEmpty(node.Type))
            {
                type = Reflector.GetType(node.Type);
            }
            else
                type = propertyType;

            if (type == null)
                return null;

            object result = null;
            object value = null;

            if(type.GetConstructor(new Type[0]{}) != null)
                result = Activator.CreateInstance(type);

            if (!(result is IList || result is IDictionary))
            {
                var properties = Reflector.GetProperties(type);

                foreach (var info in properties)
                {
                    value = null;

                    if (info.IsDefined(typeof(XmlIgnoreAttribute), true)) continue;
                    if (!info.CanWrite) continue;
                    if (!info.CanRead) continue;

                    if (info.GetSetMethod() == null) continue;

                    var child = node.Nodes.Find(x => x.Name.Equals(info.Name));
                    if (child == null) continue;

                    if (!string.IsNullOrEmpty(child.Value))
                    {
                        var converter = System.ComponentModel.TypeDescriptor.GetConverter(info.PropertyType);
                        if (converter != null)
                        {
                            if (converter.CanConvertFrom(typeof(string)))
                                value = converter.ConvertFromString(child.Value);
                        }
                    }
                    else
                    {
                        value = Deserialize(child, info.PropertyType);
                    }

                    if (value != null)
                        info.SetValue(result, value, null);
                }
            }

            #region special typeof(Control) case

            if (result is Control)
            {
                Node child;

                if (result is IControlContainer)
                {
                    var container = result as IControlContainer;
                    child = node.Nodes.Find(x => x.Name.Equals("Controls"));

                    if (child != null)
                    {
                        for (var i = 0; i < child.Nodes.Count; i++)
                        {
                            if (container.Controls.Count > i)
                            {
                                DeserializeTo(container.Controls[i], child.Nodes[i], typeof(Control));
                            }
                            else
                            {
                                value = Deserialize(child.Nodes[i], typeof(Control));

                                if (value != null)
                                    container.Controls.Add(value as Control);
                            }
                        }
                    }
                }

                var elements = ((Control)result).GetElements();
                child = node.Nodes.Find(x => x.Name.Equals("Elements"));

                if (child != null)
                {
                    for (var i = 0; i < child.Nodes.Count; i++)
                    {
                        DeserializeTo(elements[i], child.Nodes[i], typeof(Control));
                    }
                }
            }

            #endregion

            if (result is IList)
            {
                Type itemType = null;
                if (type.IsGenericType)
                {
                    var gens = type.GetGenericArguments();
                    if (gens.Length > 0)
                        itemType = gens[0];
                }

                foreach (var child in node.Nodes)
                {
                    ((IList)result).Add(Deserialize(child, itemType));
                }
            }

            if (result is IDictionary)
            {
                Type keyType = null;
                Type itemType = null;

                if (type.IsGenericType)
                {
                    var gens = type.GetGenericArguments();
                    if (gens.Length > 1)
                    {
                        keyType = gens[0];
                        itemType = gens[1];
                    }
                }
                else if (type.BaseType.IsGenericType)
                {
                    var gens = type.BaseType.GetGenericArguments();
                    if (gens.Length > 1)
                    {
                        keyType = gens[0];
                        itemType = gens[1];
                    }
                }

                var converter = System.ComponentModel.TypeDescriptor.GetConverter(keyType);
                if (converter != null)
                {
                    foreach (var child in node.Nodes)
                    {
                        var key = converter.ConvertFromString(child.Key);
                        ((IDictionary)result).Add(key, Deserialize(child, itemType));
                    }
                }
            }

            if (node.RefId != 0)
            {
                if (!_readCache.ContainsKey(node.RefId))
                    _readCache.Add(node.RefId, result);
            }

            return result;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposed)
        {
        }

        #endregion
    }

}

