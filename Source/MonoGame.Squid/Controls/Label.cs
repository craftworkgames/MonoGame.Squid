using System;
using System.Collections.Generic;
using System.ComponentModel;
using MonoGame.Squid.Interfaces;
using MonoGame.Squid.Skinning;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// A Label without selection.
    /// Supports multi- and singleline, textwrap and some bbcode tags.
    /// </summary>
    [Toolbox]
    public class Label : Control, ISelectable
    {
        private readonly List<TextLine> _lines = new List<TextLine>();
        private bool _isDirty;
        private Point _textSize;
        private string _activeHref;
        private Point _lastSize;
        private string _text = string.Empty;

        /// <summary>
        /// Delegate LinkClickedEventHandler
        /// </summary>
        /// <param name="href">The href.</param>
        public delegate void LinkClickedEventHandler(string href);

        public delegate Control RequestControlHandler(string data);

        /// <summary>
        /// Raised when [link clicked].
        /// </summary>
        public event LinkClickedEventHandler LinkClicked;

        public event RequestControlHandler ControlRequest;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Label"/> is selected.
        /// </summary>
        /// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
        [DefaultValue(false)]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the leading.
        /// </summary>
        /// <value>The leading.</value>
        [DefaultValue(0)]
        public int Leading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [text wrap].
        /// </summary>
        /// <value><c>true</c> if [text wrap]; otherwise, <c>false</c>.</value>
        [DefaultValue(false)]
        public bool TextWrap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [BB code enabled].
        /// </summary>
        /// <value><c>true</c> if [BB code enabled]; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        public bool BbCodeEnabled { get; set; }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        /// <value>The color of the text.</value>
        [IntColor, DefaultValue(-1)]
        public int TextColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the link.
        /// </summary>
        /// <value>The color of the link.</value>
        [IntColor, DefaultValue(-1)]
        public int LinkColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use text color].
        /// </summary>
        /// <value><c>true</c> if [use text color]; otherwise, <c>false</c>.</value>
        [DefaultValue(false)]
        public bool UseTextColor { get; set; }

        /// <summary>
        /// Gets or sets the text align.
        /// </summary>
        /// <value>The text align.</value>
        public Alignment TextAlign { get; set; }

        /// <summary>
        /// Gets or sets the text padding.
        /// </summary>
        /// <value>The text padding.</value>
        public Margin TextPadding { get; set; }

        /// <summary>
        /// Get or sets the ellipsis
        /// </summary>
        public bool AutoEllipsis { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        [Multiline]
        public string Text
        {
            get { return _text; }
            set
            {
                if (value == _originalText) return;

                _originalText = value;
                _text = value;

                if(UseTranslation)
                    _text = TranslateText(_originalText);
                else
                    _text = value;

                _isDirty = true;
            }
        }

        private string _originalText;

        protected override void TranslationChanged(bool from, bool to)
        {
            base.TranslationChanged(from, to);

            if (string.IsNullOrEmpty(_originalText)) return;

            if (from == false && to == true)
                _text = TranslateText(_originalText);
            else if (from == true && to == false)
                _text = _originalText;

            _isDirty = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        public Label()
        {
            AutoEllipsis = true;
            TextAlign = Alignment.Inherit;
            Style = "label";
            LinkColor = ColorInt.Argb(.25f, 1f, 1f, 1f);
            TextColor = -1;
            BbCodeEnabled = false;
            MouseClick += Label_MouseClick;
        }

        void Label_MouseClick(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            if (LinkClicked != null && _activeHref != null)
                LinkClicked(_activeHref);
        }

        private readonly Dictionary<string, bool> _activeLibrary = new Dictionary<string, bool>();
        private readonly Dictionary<string, Control> _library = new Dictionary<string, Control>();

        private void UpdateText(Style style)
        {
            _activeLibrary.Clear();
            _lines.Clear();

            var def = new TextElement();
            def.Font = style.Font;

            var elements = BbCode.Parse(_text, style, BbCodeEnabled);
            var textElements = new List<TextElement>();

            var pos = new Point();
            var tsize = new Point();
            var advx = 0;

            var lineHeight = 0;
            var thisLine = new List<TextElement>();

            _textSize = Point.Zero;

            if (TextWrap)
            {
                #region TextWrap = true
                var firstInLine = true;

                foreach (var element in elements)
                {
                    var font = Gui.Renderer.GetFont(element.Font);

                    if (element.Linebreak)
                    {
                        if (firstInLine)
                            lineHeight = Gui.Renderer.GetTextSize(" ", font).Y;

                        pos.X = 0;
                        pos.Y += lineHeight + Leading;

                        foreach (var el in thisLine)
                            el.Position.Y += lineHeight - el.Size.Y;

                        thisLine.Clear();
                        lineHeight = 0;

                        textElements.Add(element);
                        firstInLine = true;
                    }
                    else if (element.IsControl)
                    {
                        Control ctrl = null;

                        if (_library.ContainsKey(element.Control))
                        {
                            ctrl = _library[element.Control];
                        }
                        else if (ControlRequest != null)
                        {
                            ctrl = ControlRequest(element.Control);
                            _library.Add(element.Control, ctrl);
                        }

                        if (ctrl != null)
                        {
                            _activeLibrary.Add(element.Control, true);

                            ctrl.Position = pos;
                            ctrl.UserData = element.Control;

                            if (ctrl.Parent == null)
                                Elements.Add(ctrl);

                            element.Size = ctrl.Size;
                            tsize = ctrl.Size;

                            if (pos.X + tsize.X < Size.X - (style.TextPadding.Left + style.TextPadding.Right))
                            {
                                element.Position = pos;
                                pos.X += ctrl.Size.X;
                                lineHeight = Math.Max(lineHeight, tsize.Y);
                            }
                            else
                            {
                                pos.X = 0;
                                pos.Y += lineHeight + Leading;

                                foreach (var el in thisLine)
                                    el.Position.Y += lineHeight - el.Size.Y;

                                thisLine.Clear();

                                textElements.Add(new TextElement { Linebreak = true });

                                firstInLine = true;

                                element.Position = pos;
                                pos.X += ctrl.Size.X;
                                lineHeight = tsize.Y;
                            }

                            textElements.Add(element);
                            //thisLine.Add(element);
                        }
                    }
                    else
                    {
                        #region wrap

                        var words = System.Text.RegularExpressions.Regex.Split(element.Text, @"(?=(?<=[^\s])\s+)");

                        var sub = new List<TextElement>();

                        var e = new TextElement(element);
                        e.Text = string.Empty;
                        e.Position = pos;

                        var c = 0;
                        var isBreak = false;

                        foreach (var word in words)
                        {
                            if (word.Length == 0) continue;

                            var temp = word;

                            // if this is the first word in a new line
                            // remove leading whitespaces
                            if (firstInLine) temp = word.TrimStart();

                            tsize = Gui.Renderer.GetTextSize(e.Text + temp, font);
                            lineHeight = Math.Max(lineHeight, tsize.Y);

                            if (pos.X + tsize.X < Size.X - (style.TextPadding.Left + style.TextPadding.Right))
                            {
                                // the word fits, add to current element
                                e.Text += temp;
                                e.Size = tsize;
                                firstInLine = false;
                            }
                            else
                            {
                                #region new

                                if (firstInLine)
                                {
                                    // the word fits, add to current element
                                    e.Text += temp;
                                    e.Size = tsize;
                                    firstInLine = false;
                                }
                                else
                                {
                                    // word does not fit, add the current element
                                    thisLine.Add(e);
                                    sub.Add(e);
                                    textElements.AddRange(sub);

                                    foreach (var el in thisLine)
                                        el.Position.Y += lineHeight - el.Size.Y;

                                    sub.Clear();
                                    thisLine.Clear();

                                    // reset line pos
                                    pos.X = 0;
                                    pos.Y += lineHeight + Leading;

                                    lineHeight = 0;

                                    // add a break
                                    var linebreak = new TextElement(e);
                                    linebreak.Linebreak = true;
                                    sub.Add(linebreak);

                                    // create new starting element
                                    e = new TextElement(element);
                                    e.Text = temp.TrimStart();
                                    e.Position = pos;
                                    e.Size = Gui.Renderer.GetTextSize(e.Text, font);
                                    lineHeight = Math.Max(lineHeight, e.Size.Y);
                                    firstInLine = false;
                                }

                                #endregion

                                #region old

                                //if (c > 0)
                                //{
                                //    isBreak = true;

                                //    // the word does not fit
                                //    pos.x = 0;
                                //    pos.y += lineHeight + Leading;
                                //}

                                //// is more than one word in this line exceeding the break limit?
                                //if (c > 0)
                                //{
                                //    // if so, we need to first add the current element,
                                //    thisLine.Add(e);
                                //    sub.Add(e);

                                //    foreach (TextElement el in thisLine)
                                //    {
                                //        el.Position.y += lineHeight - el.Size.y;
                                //        // el.Size.y += lineHeight - el.Size.y;
                                //    }

                                //    thisLine.Clear();
                                //    lineHeight = 0;

                                //    TextElement linebreak = new TextElement(e);
                                //    linebreak.Linebreak = true;
                                //    sub.Add(linebreak);

                                //    e = new TextElement(element);
                                //    e.Text = temp.TrimStart();
                                //    e.Position = pos;
                                //    e.Size = Gui.Renderer.GetTextSize(e.Text, font);

                                //    lineHeight = Math.Max(lineHeight, e.Size.y);

                                //    if (c <= words.Length)
                                //        isBreak = false;
                                //}
                                //else
                                //{
                                //    // if not, we just add the current word
                                //    e.Position = pos;
                                //    e.Text = temp;
                                //    e.Size = Gui.Renderer.GetTextSize(e.Text, font);
                                //    //sub.Add(e);
                                //}

                                #endregion

                                //pos.x += tsize.x;
                            }

                            c++;
                        }

                        if (!isBreak)
                        {
                            //e.Size = Gui.Renderer.GetTextSize(e.Text, font);
                            pos.X = pos.X + e.Size.X;
                            lineHeight = Math.Max(lineHeight, e.Size.Y);
                            sub.Add(e);
                        }

                        thisLine.AddRange(sub);
                        textElements.AddRange(sub);

                        #endregion
                    }
                }

                foreach (var el in thisLine)
                    el.Position.Y += lineHeight - el.Size.Y;

                #endregion
            }
            else
            {
                #region TextWrap = false

                var firstInLine = true;
                var singleLine = true;

                foreach (var element in elements)
                {
                    var font = Gui.Renderer.GetFont(element.Font);

                    if (element.Linebreak)
                    {
                        if (firstInLine)
                            lineHeight = Gui.Renderer.GetTextSize(" ", font).Y;

                        pos.X = 0;
                        pos.Y += lineHeight + Leading;

                        foreach (var el in thisLine)
                        {
                            el.Position.Y += lineHeight - el.Size.Y;
                            // el.Size.y += lineHeight - el.Size.y;
                        }

                        thisLine.Clear();
                        lineHeight = 0;

                        element.Position = pos;
                        textElements.Add(element);
                        firstInLine = true;
                        singleLine = false;
                    }
                    else if (element.IsControl)
                    {
                        Control ctrl = null;

                        if (_library.ContainsKey(element.Control))
                        {
                            ctrl = _library[element.Control];
                        }
                        else if (ControlRequest != null)
                        {
                            ctrl = ControlRequest(element.Control);
                            _library.Add(element.Control, ctrl);
                        }

                        if (ctrl != null)
                        {
                            ctrl.Position = pos;

                            if (ctrl.Parent == null)
                                Elements.Add(ctrl);

                            _activeLibrary.Add(element.Control, true);

                            element.Position = pos;
                            element.Size = ctrl.Size;

                            pos.X += ctrl.Size.X;

                            tsize = element.Size;
                            lineHeight = Math.Max(lineHeight, tsize.Y);
                            textElements.Add(element);
                            //thisLine.Add(element);
                        }
                    }
                    else
                    {
                        if (firstInLine)
                        {
                            element.Text = element.Text.TrimStart();
                            firstInLine = false;
                        }

                        tsize = Gui.Renderer.GetTextSize(string.IsNullOrEmpty(element.Text) ? " " : element.Text, font);
                        lineHeight = Math.Max(lineHeight, tsize.Y);

                        element.Position = pos;
                        element.Size = tsize;

                        textElements.Add(element);

                        pos.X += tsize.X;

                        thisLine.Add(element);
                    }
                }

                foreach (var el in thisLine)
                    el.Position.Y += lineHeight - el.Size.Y;

                #endregion

                #region AutoEllipsis (...)

                if (singleLine && AutoEllipsis && (AutoSize == AutoSize.None || AutoSize == AutoSize.Vertical))
                {
                    var removeAt = -1;
                    var width = 0;
                    var limit = Size.X - style.TextPadding.Left - style.TextPadding.Right;

                    var align = TextAlign != Alignment.Inherit ? TextAlign : style.TextAlign;

                    if (align == Alignment.TopLeft || align == Alignment.MiddleLeft || align == Alignment.BottomLeft)
                    {
                        for (var i = 0; i < textElements.Count; i++)
                        {
                            var font = Gui.Renderer.GetFont(textElements[i].Font);
                            var ellipsis = Gui.Renderer.GetTextSize("...", font).X;

                            if (width + textElements[i].Size.X + ellipsis <= limit)
                            {
                                width += textElements[i].Size.X;
                                continue;
                            }
                            else
                            {
                                var text = string.Empty;
                                var final = string.Empty;
                                removeAt = i + 1;

                                foreach (var c in textElements[i].Text)
                                {
                                    final = text + c + "...";

                                    var w = Gui.Renderer.GetTextSize(final, font).X;

                                    if (width + w >= limit)
                                    {
                                        textElements[i].Text = text + "...";
                                        break;
                                    }
                                    else
                                    {
                                        text += c;
                                    }
                                }

                                break;
                            }
                        }

                        if (removeAt > -1)
                            textElements.RemoveRange(removeAt, textElements.Count - removeAt);

                    }
                    else if (align == Alignment.TopRight || align == Alignment.MiddleRight || align == Alignment.BottomRight)
                    {
                        for (var i = textElements.Count - 1; i >= 0; i--)
                        {
                            var font = Gui.Renderer.GetFont(textElements[i].Font);
                            var ellipsis = Gui.Renderer.GetTextSize("...", font).X;
                            var fullsize = textElements[i].Size.X;
                            var oldpos = textElements[i].Position;

                            if (width + textElements[i].Size.X + ellipsis <= limit)
                            {
                                width += textElements[i].Size.X;
                            }
                            else
                            {
                                var inc = string.Empty;
                                var final = string.Empty;
                                var text = textElements[i].Text;
                                removeAt = i;

                                for (var j = text.Length; j >= 0; j--)
                                {
                                    var c = new char();

                                    if (j < text.Length)
                                    {
                                        c = text[j];
                                        final = "..." + c + inc;
                                        inc = c + inc;
                                    }
                                    else
                                    {
                                        final = "..." + inc;
                                    }

                                    if (j == 0 && removeAt == 0)
                                        final = inc;

                                    var w = Gui.Renderer.GetTextSize(final, font).X;

                                    var position = oldpos;
                                    position.X = oldpos.X + (fullsize - w);
                                    textElements[i].Text = final;
                                    textElements[i].Position = position;

                                    if (width + w > limit && j > 0)
                                        break;
                                }

                                break;
                            }
                        }

                        if (removeAt > 0)
                        {
                            var ww = 0;
                            for (var i = 0; i < removeAt; i++)
                                ww += textElements[i].Size.X;

                            textElements.RemoveRange(0, removeAt);

                            for (var i = 0; i < textElements.Count; i++)
                            {
                                var position = textElements[i].Position;
                                position.X -= ww;
                                textElements[i].Position = position;
                            }
                        }
                    }
                }

                #endregion
            }

            var line = new TextLine();

            foreach (var element in textElements)
            {
                line.Width += element.Size.X;
                line.Elements.Add(element);

                _textSize.X = Math.Max(_textSize.X, line.Width);
                _textSize.Y = Math.Max(_textSize.Y, element.Position.Y + element.Size.Y);

                if (element.Linebreak)
                {
                    _lines.Add(line);
                    line = new TextLine();
                }
            }

            _textSize += new Point(style.TextPadding.Left + style.TextPadding.Right, style.TextPadding.Top + style.TextPadding.Bottom);

            _lines.Add(line);

            _lastSize = Size;
            _isDirty = false;

            FinalizeTextLayout(style);

            foreach (var pair in _library)
            {
                if (!_activeLibrary.ContainsKey(pair.Key))
                    Elements.Remove(pair.Value);
            }
        }

        protected override void OnStateChanged()
        {
            _isDirty = true;
        }

        protected override void OnAutoSize()
        {
            if (_isDirty)
            {
                var style = Desktop.GetStyle(Style).Styles[State];
                UpdateText(style);
            }

            if (AutoSize == AutoSize.Vertical)
                Size = new Point(Size.X, _textSize.Y);
            else if (AutoSize == AutoSize.Horizontal)
                Size = new Point(_textSize.X, Size.Y);
            else if (AutoSize == AutoSize.HorizontalVertical)
                Size = _textSize;
        }

        protected override void OnLateUpdate()
        {
            if (!_isDirty)
                _isDirty = _lastSize.X != Size.X || _lastSize.Y != Size.Y;

            if (_isDirty)
            {
                var style = Desktop.GetStyle(Style).Styles[State];
                UpdateText(style);
            }

            if (Desktop.HotControl == this)
            {
                var m = Gui.MousePosition;
                _activeHref = null;

                foreach (var line in _lines)
                {
                    foreach (var element in line.Elements)
                    {
                        if (!element.IsLink) continue;

                        if (element.Rectangle.Contains(m))
                        {
                            Desktop.CurrentCursor = CursorNames.Link;
                            _activeHref = element.Href;
                            break;
                        }
                    }
                }
            }
        }

        private void FinalizeTextLayout(Style style)
        {
            if (_lines.Count == 0) return;

            int font;
            Point p1, p2, size;

            var align = TextAlign != Alignment.Inherit ? TextAlign : style.TextAlign;

            foreach (var line in _lines)
            {
                foreach (var element in line.Elements)
                {
                    if (element.Linebreak) continue;

                    font = Gui.Renderer.GetFont(element.Font);

                    size = element.Size;
                    p1 = Point.Zero;

                    if (align == Alignment.TopLeft || align == Alignment.TopCenter || align == Alignment.TopRight)
                        p1.Y += style.TextPadding.Top;

                    if (align == Alignment.BottomLeft || align == Alignment.BottomCenter || align == Alignment.BottomRight)
                        p1.Y += Size.Y - _textSize.Y;

                    if (align == Alignment.MiddleLeft || align == Alignment.MiddleCenter || align == Alignment.MiddleRight)
                        p1.Y += (Size.Y - (_textSize.Y - (style.TextPadding.Top + style.TextPadding.Bottom))) / 2;

                    if (align == Alignment.TopLeft || align == Alignment.MiddleLeft || align == Alignment.BottomLeft)
                        p1.X += style.TextPadding.Left;

                    if (align == Alignment.TopRight || align == Alignment.MiddleRight || align == Alignment.BottomRight)
                        p1.X += Size.X - line.Width - style.TextPadding.Right;

                    if (align == Alignment.TopCenter || align == Alignment.MiddleCenter || align == Alignment.BottomCenter)
                        p1.X += (Size.X - line.Width) / 2;

                    p2 = element.Position + p1;

                    element.Rectangle = new Rectangle(p2, size);
                    element.Position = p2;

                    if (element.IsControl)
                    {
                        element.Position = p2;

                        if (_library.ContainsKey(element.Control))
                            _library[element.Control].Position = p2;
                    }
                }
            }

        }

        protected override void DrawText(Style style, float opacity)
        {
            if (_isDirty)
            {
                UpdateText(style);
            }

            if (_lines.Count == 0)
                return;

            int font;
            Point p1, p2, size;

            foreach (var line in _lines)
            {
                foreach (var element in line.Elements)
                {
                    if (element.Linebreak) continue;

                    font = Gui.Renderer.GetFont(element.Font);
                    size = element.Size;
                    p2 = element.Position + Location;

                    element.Rectangle = new Rectangle(p2, size);

                    if (!element.Rectangle.Intersects(ClipRect))
                        continue;

                    if (element.IsControl)
                        continue;

                    if (element.IsLink)
                        Gui.Renderer.DrawBox(p2.X, p2.Y + size.Y, size.X - 1, 1, ColorInt.FromArgb(opacity, ColorInt.FromArgb(opacity, element.Color.HasValue ? (int)element.Color : style.TextColor)));

                    //if (element.IsLink && element.Href == ActiveHref)
                    //    Gui.Renderer.DrawBox(p2.x, p2.y, size.x - 1, size.y, ColorInt.FromArgb(opacity, LinkColor));

                    if (UseTextColor)
                        Gui.Renderer.DrawText(element.Text, p2.X, p2.Y, font, ColorInt.FromArgb(opacity, TextColor));
                    else
                        Gui.Renderer.DrawText(element.Text, p2.X, p2.Y, font, ColorInt.FromArgb(opacity, element.Color.HasValue ? (int)element.Color : style.TextColor));
                }
            }
        }
    }
}
