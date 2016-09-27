using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MonoGame.Squid.Xml
{
    public enum XmlNodeType
    {
        Element,
        Text,
        EndElement
    }

    public class XmlAttribute
    {
        public string Name;
        public string Value;
    }

    public class XmlReader
    {
        private string _xmlString = "";
        private int _idx = 0;
        private XmlNodeType _nodeType;

        public XmlNodeType NodeType { get { return _nodeType; } }
        public bool HasAttributes { get; private set; }

        private readonly List<XmlAttribute> _attributes = new List<XmlAttribute>();
        private int _attributeIndex = -1;
        private bool _isEmptyElement;

        public XmlReader(string xml)
        {
            _xmlString = xml;
            _nodeType = XmlNodeType.Text;
        }

        public void New(string xml)
        {
            _xmlString = xml;
            _nodeType = XmlNodeType.Text;

            _idx = 0;
            Name = "";
            Value = "";
            _isEmptyElement = false;
            _attributeIndex = -1;
            _attributes.Clear();
            HasAttributes = false;
        }
    
        public string Name = "";
        public string Value = "";

        // properly looks for the next index of _c, without stopping at line endings, allowing tags to be break lines   
        int IndexOf(char c, int _i)
        {
            var i = _i;
            while (i < _xmlString.Length)
            {
                if (_xmlString[i] == c)
                    return i;

                ++i;
            }

            return -1;
        }

        public bool Eof
        {
            get { return _idx < 0; }
        }

        public bool IsEmptyElement
        {
            get { return _isEmptyElement; }
        }

        public string GetAttribute(string name)
        {
            foreach (var att in _attributes)
            {
                if (att.Name.Equals(name))
                    return att.Value;
            }

            return string.Empty;
        }

        public bool MoveToNextAttribute()
        {
            _attributeIndex++;

            if (_attributeIndex < _attributes.Count)
            {
                Name = _attributes[_attributeIndex].Name;
                Value = _attributes[_attributeIndex].Value;
            }

            return _attributeIndex < _attributes.Count;
        }

        public bool Read()
        {
            var newindex = _idx;

            if (_idx > -1)
               newindex = _xmlString.IndexOf("<", _idx);

            Name = string.Empty;
            Value = string.Empty;
            HasAttributes = false;
            _attributes.Clear();
            _attributeIndex = -1;
            _isEmptyElement = false;

            if (newindex != _idx)
            {
                if (newindex == -1)
                {
                    if (_idx > 0) _idx++;

                    Value = _xmlString.Substring(_idx, _xmlString.Length - _idx);
                    _nodeType = XmlNodeType.Text;
                    _idx = newindex;
                    return true;
                }
                else
                {
                    if (_idx > 0) _idx++;

                    Value = _xmlString.Substring(_idx, newindex - _idx);
                    _nodeType = XmlNodeType.Text;
                    _idx = newindex;
                    return true;
                }
            }

            if (_idx == -1)
                return false;
            
            ++_idx;

            // skip attributes, don't include them in the name!
            var endOfTag = IndexOf('>', _idx);
            var endOfName = IndexOf(' ', _idx);
            if ((endOfName == -1) || (endOfTag < endOfName))
            {
                endOfName = endOfTag;
            }

            if (endOfTag == -1)
            {
                return false;
            }

            Name = _xmlString.Substring(_idx, endOfName - _idx);

            _idx = endOfTag;

            // check if a closing tag
            if (Name.StartsWith("/"))
            {
                _isEmptyElement = false;
                _nodeType = XmlNodeType.EndElement;
                Name = Name.Remove(0, 1); // remove the slash
            }
            else if(Name.EndsWith("/"))
            {
                _isEmptyElement = true;
                _nodeType = XmlNodeType.Element;
                Name = Name.Replace("/", ""); // remove the slash  
            }
            else
            {
                var temp = _xmlString.Substring(endOfName, endOfTag - endOfName);

                var r = new Regex("([a-z0-9]+)=(\"(.*?)\")");

                foreach (Match m in r.Matches(temp))
                {
                    var name = m.Value.Substring(0, m.Value.IndexOf("="));
                    var i0 = m.Value.IndexOf("\"") + 1;
                    var i1 = m.Value.LastIndexOf("\"");
                    var val = m.Value.Substring(i0, i1 - i0);

                    _attributes.Add(new XmlAttribute { Name = name, Value = val });
                }

                r = new Regex("([a-z0-9]+)=('(.*?)')");

                foreach (Match m in r.Matches(temp))
                {
                    var name = m.Value.Substring(0, m.Value.IndexOf("="));
                    var i0 = m.Value.IndexOf("'") + 1;
                    var i1 = m.Value.LastIndexOf("'");
                    var val = m.Value.Substring(i0, i1 - i0);

                    _attributes.Add(new XmlAttribute { Name = name, Value = val });
                }

                HasAttributes = _attributes.Count > 0;

                _nodeType = XmlNodeType.Element;
            }

            return _idx < _xmlString.Length;
        }
    }
}