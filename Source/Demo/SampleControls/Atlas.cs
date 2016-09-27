using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MonoGame.Squid.Structs;

namespace Demo.SampleControls
{
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            var wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                var key = (TKey) keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                var value = (TValue) valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            foreach (var key in Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key, ns);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                var value = this[key];
                valueSerializer.Serialize(writer, value, ns);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        public override int GetHashCode()
        {
            var hash = 0;
            var counter = 1;

            foreach (object item in Values)
                try
                {
                    hash ^= item.GetHashCode() + counter++;
                }
                catch (Exception)
                {
                    return base.GetHashCode();
                }

            if (hash == 0)
                return base.GetHashCode();

            return hash;
        }
    }

    public class Atlas
    {
        private SerializableDictionary<string, Rectangle> _data;
        private readonly Rectangle _default = new Rectangle(0, 0, 1, 1);

        public Atlas()
        {
            _data = new SerializableDictionary<string, Rectangle>();
        }

        public void Add(string key, Rectangle coords)
        {
            if (_data.ContainsKey(key))
                _data[key] = coords;
            else
                _data.Add(key, coords);
        }

        public bool Contains(string key)
        {
            return _data.ContainsKey(key);
        }

        public Rectangle GetRect(string key)
        {
            if (_data.ContainsKey(key))
                return _data[key];

            return _default;
        }

        public void LoadBytes(byte[] data)
        {
            var type = typeof(SerializableDictionary<string, Rectangle>);
            var serializer = new XmlSerializer(type);
            var reader = new StringReader(Encoding.UTF8.GetString(data));
            _data = serializer.Deserialize(reader) as SerializableDictionary<string, Rectangle>;
        }

        public void LoadFile(string path)
        {
            var type = typeof(SerializableDictionary<string, Rectangle>);
            var serializer = new XmlSerializer(type);
            var reader = new StringReader(File.ReadAllText(path));
            _data = serializer.Deserialize(reader) as SerializableDictionary<string, Rectangle>;
        }

        public void LoadXml(string xml)
        {
            var type = typeof(SerializableDictionary<string, Rectangle>);
            var serializer = new XmlSerializer(type);
            var reader = new StringReader(xml);
            _data = serializer.Deserialize(reader) as SerializableDictionary<string, Rectangle>;
        }

        public void Save(string path)
        {
            var type = typeof(SerializableDictionary<string, Rectangle>);
            var serializer = new XmlSerializer(type);

            var stringwriter = new StringWriter();

            var xmlwriter = new XmlTextWriter(stringwriter);
            xmlwriter.Formatting = Formatting.Indented;
            xmlwriter.WriteRaw("");

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(xmlwriter, _data, ns);

            var result = stringwriter.ToString();
            File.WriteAllText(path, result);

            stringwriter.Close();
            serializer = null;
            xmlwriter = null;
        }
    }
}