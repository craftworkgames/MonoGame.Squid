using System.Collections.Generic;
using System.Text;

namespace MonoGame.Squid.Xml
{
    public class XmlWriter
    {
        class Element
        {
            public bool IsRoot;
            public string Name;
            public readonly Dictionary<string, string> Attributes = new Dictionary<string, string>();
            public string Value;

            public readonly List<Element> Elements = new List<Element>();
            public Element Parent;
        }

        readonly StringBuilder _builder;
        List<Element> _elements = new List<Element>();
        Element _current = new Element { IsRoot = true };

        readonly bool _indention;

        public XmlWriter(StringBuilder output, bool indent)
        {
            _indention = indent;
            _builder = output;
        }

        public void Flush()
        {
            foreach(var element in _current.Elements)
                Write(element, 0);           
        }

        private void Write(Element element, int level)
        {
            if (level > 0 && _indention)
                _builder.Append(new string(' ', level * 4));

            _builder.Append("<" + element.Name);
            
            if (element.Attributes.Count > 0)
            {
                _builder.Append(" ");

                var i = 0;
                foreach (var pair in element.Attributes)
                {
                    i++;
                    if(i == element.Attributes.Count)
                        _builder.Append(pair.Key + "='" + pair.Value + "'");
                    else
                        _builder.Append(pair.Key + "='" + pair.Value + "' ");
                }
            }

            var isEmpty = string.IsNullOrEmpty(element.Value) && element.Elements.Count == 0;

            if (!isEmpty)
            {
                _builder.Append(">");

                if (!string.IsNullOrEmpty(element.Value))
                {
                    _builder.Append(element.Value);
                }
                else
                {
                    if(_indention)
                        _builder.Append("\r\n");
                    
                    foreach (var child in element.Elements)
                        Write(child, level + 1);
                }

                if (string.IsNullOrEmpty(element.Value) && level > 0 && _indention)
                    _builder.Append(new string(' ', level * 4)); 
                
                _builder.Append("</" + element.Name + ">\r\n");
            }
            else
                _builder.Append("/>\r\n");
        }

        public void Close() { }

        public void WriteStartElement(string name)
        {
            var e = new Element();
            e.Name = name;
            e.Parent = _current;
         
            _current.Elements.Add(e);
            _current = e;
        }

        public void WriteAttributeString(string name, string value)
        {
            _current.Attributes.Add(name, value);
        }

        public void WriteEndElement()
        {
            _current = _current.Parent;
        }

        public void WriteValue(string value)
        {
            _current.Value = value;
        }
    }
}
