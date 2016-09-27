using System.Collections.Generic;
using System.Text.RegularExpressions;
using MonoGame.Squid.Skinning;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Xml;

namespace MonoGame.Squid.Util
{
    /// <summary>
    /// A TextElement. This class is used by the internal Text layout engine.
    /// </summary>
    internal class TextElement
    {
        public string Control = string.Empty;

        /// <summary>
        /// The color
        /// </summary>
        public int? Color;
        /// <summary>
        /// The text
        /// </summary>
        public string Text = string.Empty;
        /// <summary>
        /// The font
        /// </summary>
        public string Font;
        /// <summary>
        /// The href
        /// </summary>
        public string Href;
        /// <summary>
        /// The linebreak
        /// </summary>
        public bool Linebreak;

        /// <summary>
        /// The size
        /// </summary>
        public Point Size;
        /// <summary>
        /// The position
        /// </summary>
        public Point Position;
        /// <summary>
        /// The rectangle
        /// </summary>
        public Rectangle Rectangle;

        /// <summary>
        /// Gets a value indicating whether this instance is link.
        /// </summary>
        /// <value><c>true</c> if this instance is link; otherwise, <c>false</c>.</value>
        public bool IsLink { get { return !string.IsNullOrEmpty(Href); } }

        public bool IsControl { get { return !string.IsNullOrEmpty(Control); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextElement"/> class.
        /// </summary>
        public TextElement() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextElement"/> class.
        /// </summary>
        /// <param name="copy">The copy.</param>
        public TextElement(TextElement copy)
        {
            Font = copy.Font;
            Color = copy.Color;
            Href = copy.Href;
        }
    }

    /// <summary>
    /// A TextLine. This class is used by the internal Text layout engine.
    /// </summary>
    internal class TextLine
    {
        /// <summary>
        /// The elements
        /// </summary>
        public List<TextElement> Elements = new List<TextElement>();
        /// <summary>
        /// The width
        /// </summary>
        public int Width;
        /// <summary>
        /// The char length
        /// </summary>
        public int CharLength;
    }

    internal static class BbCode
    {
        private static readonly Regex _tagB;
        private static readonly Regex _tagUrl;
        private static readonly Regex _tagImg;
        private static readonly Regex _tagColor;
        private static readonly Regex _tagFont;
        private static readonly Regex _tagCtrl1;

        private static Xml.XmlReader _reader;

        static BbCode()
        {
            _tagB = new Regex(@"\[b\](.+?)\[/b\]");
            _tagUrl = new Regex(@"\[url\=([^\]]+)\]([^\]]+)\[/url\]");
            _tagImg = new Regex(@"\[img\]([^\]]+)\[/img\]");
            _tagColor = new Regex(@"\[color\=([^\]]+)\]([^\]]+)\[/color\]");
            _tagFont = new Regex(@"\[font\=([^\]]+)\]([^\]]+)\[/font\]");
            _tagCtrl1 = new Regex(@"\[ctrl\=([^\]]+)\/]");
        }

        private static string UnescapeXml(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            var returnString = s;
            returnString = returnString.Replace("&apos;", "'");
            returnString = returnString.Replace("&quot;", "\"");
            returnString = returnString.Replace("&gt;", ">");
            returnString = returnString.Replace("&lt;", "<");
            returnString = returnString.Replace("&amp;", "&");

            return returnString;
        }

        private static string EscapeXml(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            var returnString = s;
            returnString = returnString.Replace("&", "&amp;");
            returnString = returnString.Replace("'", "&apos;");
            returnString = returnString.Replace("\"", "&quot;");
            returnString = returnString.Replace(">", "&gt;");
            returnString = returnString.Replace("<", "&lt;");

            return returnString;
        }

        /// <summary>
        /// A method to convert basic BBCode to HTML
        /// </summary>
        /// <param name="str">A string formatted in BBCode</param>
        /// <returns>The HTML representation of the BBCode string</returns>
        private static string ConvertBbCodeToHtml(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            // format the bold tags: [b][/b]
            // becomes: <strong></strong>
            str = _tagB.Replace(str, "<b>$1</b>");

            // format the url tags: [url=www.website.com]my site[/url]
            // becomes: <a href="www.website.com">my site</a>
            str = _tagUrl.Replace(str, "<a href=\"$1\">$2</a>");

            // format the img tags: [img]www.website.com/img/image.jpeg[/img]
            // becomes: <img src="www.website.com/img/image.jpeg" />
            str = _tagImg.Replace(str, "<img src=\"$1\" />");

            while (_tagCtrl1.Matches(str).Count > 0)
                str = _tagCtrl1.Replace(str, "<control key=\"$1\"/>");

            //format the colour tags: [color=red][/color]
            // becomes: <font color="red"></font>
            while (_tagColor.Matches(str).Count > 0)
                str = _tagColor.Replace(str, "<font color=\"$1\">$2</font>");

            //format the font tags: [font=fontName][/font]
            // becomes: <font name="fontName"></font>
            str = _tagFont.Replace(str, "<font name=\"$1\">$2</font>");

            // lastly, replace any new line characters with <br />
            str = str.Replace("\r\n", "<br/>");
            str = str.Replace("\n", "<br/>");

            return str;
        }

        public static List<TextElement> Parse(string text, Style style, bool enabled)
        {
            if (string.IsNullOrEmpty(text)) return new List<TextElement>();

            if (enabled)
                text = ConvertBbCodeToHtml(text);
            else
            {
                text = EscapeXml(text);

                text = text.Replace("\r\n", "<br/>");
                text = text.Replace("\n", "<br/>");

                text = text.Replace("&quot;r&quot;n", "<br/>");
                text = text.Replace("&quot;n", "<br/>");
            }

            var element = new TextElement { Font = style.Font, Color = style.TextColor };
            var lastElement = new TextElement { Font = style.Font, Color = style.TextColor };
            var result = new List<TextElement>();

            if (_reader == null)
                _reader = new Xml.XmlReader(text);
            else
                _reader.New(text);

            while (_reader.Read())
            {
                switch (_reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (_reader.Name.ToLower())
                        {
                            case "control":
                                if (_reader.HasAttributes)
                                {
                                    while (_reader.MoveToNextAttribute())
                                    {
                                        if (_reader.Name.Equals("key"))
                                        {
                                            element.Control = _reader.Value;
                                        }
                                        else if (_reader.Name.Equals("data"))
                                        {
                                            // element.Data = reader.Value;
                                        }
                                    }
                                }

                                result.Add(element);
                                element = new TextElement(element);

                                break;
                            case "font":
                                if (_reader.HasAttributes)
                                {
                                    lastElement.Color = element.Color;
                                    lastElement.Font = element.Font;

                                    while (_reader.MoveToNextAttribute())
                                    {
                                        if (_reader.Name.Equals("name"))
                                        {
                                            element.Font = _reader.Value;
                                        }
                                        else if (_reader.Name.Equals("color"))
                                        {

                                            var color = element.Color.Value;

                                            if (int.TryParse(_reader.Value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out color))
                                                element.Color = color;
                                            else if (int.TryParse(_reader.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out color))
                                                element.Color = color;
                                        }
                                    }
                                }

                                break;
                            case "a":
                                if (_reader.HasAttributes)
                                {
                                    while (_reader.MoveToNextAttribute())
                                    {
                                        if (_reader.Name.Equals("href"))
                                            element.Href = _reader.Value;
                                    }
                                }
                                break;
                            case "br":
                                element.Linebreak = true;
                                result.Add(element);

                                element = new TextElement(element);
                                break;

                            default: break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        switch (_reader.Name.ToLower())
                        {
                            case "control":
                                element.Font = lastElement.Font;
                                element.Color = lastElement.Color;
                                break;
                            case "font":
                                element.Font = lastElement.Font;
                                element.Color = lastElement.Color;
                                break;
                            case "a":
                                element.Href = null;
                                break;
                        }

                        break;
                    case XmlNodeType.Text:
                        if (_reader.Value.Length > 0)
                        {
                            element.Text = UnescapeXml(_reader.Value);
                            result.Add(element);

                            element = new TextElement(element);
                        }
                        break;

                    default:
                        break;
                }
            }

            return result;
        }
    }
}
