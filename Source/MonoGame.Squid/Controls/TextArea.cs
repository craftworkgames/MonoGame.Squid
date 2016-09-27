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
    /// A multiline text input. Text does not scroll. (use a Panel).
    /// </summary>
    [Toolbox]
    public class TextArea : Control, IText
    {
        private float _blinkTime;
        private int _doBlink;
        private bool _hasFocus;
        private string _savedText;
        private readonly List<TextLine> _lines = new List<TextLine>();
        private bool _isDirty;
        private Point _textSize;
        private string _activeHref;
        private Point _lastSize;
        private string _text = string.Empty;
        // private int Ln;
        // private int Col;
        private int _caret = 0;

        private bool IsSelection { get { return _selectStart != _selectEnd; } }
        private int _selectStart = 0;
        private int _selectEnd = 0;

        /// <summary>
        /// Raised when [text changed].
        /// </summary>
        public event VoidEvent TextChanged;

        /// <summary>
        /// Raised when [text commit].
        /// </summary>
        public event EventHandler TextCommit;

        /// <summary>
        /// Raised when [text cancel].
        /// </summary>
        public event EventHandler TextCancel;

        /// <summary>
        /// Gets or sets a value indicating whether [read only].
        /// </summary>
        /// <value><c>true</c> if [read only]; otherwise, <c>false</c>.</value>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [text wrap].
        /// </summary>
        public bool TextWrap { get; set; }

        /// <summary>
        /// Gets or sets the leading.
        /// </summary>
        /// <value>The leading.</value>
        [DefaultValue(0)]
        public int Leading { get; set; }

        /// <summary>
        /// Gets or sets the color of the link.
        /// </summary>
        /// <value>The color of the link.</value>
        [IntColor, DefaultValue(-1)]
        public int LinkColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        /// <value>The color of the text.</value>
        [IntColor, DefaultValue(-1)]
        public int TextColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the blink.
        /// </summary>
        /// <value>The color of the blink.</value>
        [IntColor, DefaultValue(-1)]
        public int BlinkColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use text color].
        /// </summary>
        /// <value><c>true</c> if [use text color]; otherwise, <c>false</c>.</value>
        [Category("Skin"), DefaultValue(false)]
        public bool UseTextColor { get; set; }

        /// <summary>
        /// Gets or sets the text align.
        /// </summary>
        /// <value>The text align.</value>
        public Alignment TextAlign { get; set; }

        /// <summary>
        /// Gets or sets the blink interval.
        /// </summary>
        /// <value>The blink interval.</value>
        [DefaultValue(500f)]
        public float BlinkInterval { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        [Multiline]
        public string Text
        {
            get { return _text.Replace("\n","\r\n"); }
            set
            {
                if (_text == value) return;

                _text = value.Replace("\r\n", "\n");
                //_text = value;

                _caret = _text.Length;
                _isDirty = true;

                _selectStart = 0;
                _selectEnd = 0;

                if (TextChanged != null)
                    TextChanged(this);
            }
        }

        private void SetText(string text)
        {
            //_text = text.Replace("\r\n", "\n");
            _text = text;

            _isDirty = true;
            if (TextChanged != null)
                TextChanged(this);
        }

        /// <summary>
        /// Gets the selection start.
        /// </summary>
        /// <value>The selection start.</value>
        [Xml.XmlIgnore]
        public int SelectionStart
        {
            get { return Math.Min(_selectStart, _selectEnd); }
        }

        /// <summary>
        /// Gets the selection end.
        /// </summary>
        /// <value>The selection end.</value>
        [Xml.XmlIgnore]
        public int SelectionEnd
        {
            get { return Math.Max(_selectStart, _selectEnd); }
        }

        /// <summary>
        /// Gets the cursor.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetCursor()
        {
            return _caret;
        }

        /// <summary>
        /// Sets the cursor.
        /// </summary>
        /// <param name="index">The index.</param>
        public void SetCursor(int index)
        {
            _caret = Math.Min(_text.Length, Math.Max(0, index));
            _selectStart = _selectEnd = _caret;
        }

        /// <summary>
        /// Selects the specified start.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public void Select(int start, int end)
        {
            _selectStart = Math.Min(_text.Length, Math.Max(0, start));
            _selectEnd = Math.Min(_text.Length, Math.Max(0, end));
            _caret = _selectEnd;
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <value>The selection.</value>
        [Xml.XmlIgnore]
        public string Selection
        {
            get
            {
                if (IsSelection)
                {
                    if (_text == null || _text.Length == 0) return string.Empty;

                    var start = SelectionStart;
                    var end = SelectionEnd;

                    return _text.Substring(start, end - start);
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the color of the selection.
        /// </summary>
        /// <value>The color of the selection.</value>
        [IntColor, DefaultValue(-1)]
        public int SelectionColor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextArea"/> class.
        /// </summary>
        public TextArea()
        {
            LinkColor = -1;
            TextColor = -1;
            BlinkColor = -1;
            BlinkInterval = 500;
            SelectionColor = ColorInt.Argb(.5f, 1, 1, 1);

            Style = "textbox";
            Cursor = CursorNames.Select;
            Size = new Point(100, 60);
            TextAlign = Alignment.TopLeft;
            AllowFocus = true;

            MouseDown += TextBox_MouseDown;
            MousePress += TextBox_MousePress;
            MouseDoubleClick += TextBox_MouseDoubleClick;
            LostFocus += TextBox_LostFocus;
            GotFocus += TextBox_GotFocus;
        }

        protected override void OnUpdate()
        {
            _blinkTime += Gui.TimeElapsed;

            if (_blinkTime > BlinkInterval)
            {
                _doBlink = 1 - _doBlink;
                _blinkTime = 0;
            }

            base.OnUpdate();
        }

        void TextBox_GotFocus(Control sender)
        {
            _savedText = _text;
            _hasFocus = true;
        }

        void TextBox_LostFocus(Control sender)
        {
            _hasFocus = false;

            if (TextCommit != null)
                TextCommit(this, null);
        }

        void TextBox_MouseDoubleClick(Control sender, MouseEventArgs args)
        {
        }

        void TextBox_MousePress(Control sender, MouseEventArgs args)
        {
            var m = Gui.MousePosition;
            var total = 0;
            Rectangle rect;

            foreach (var line in _lines)
            {
                for (var i = 0; i < line.Elements.Count; i++)
                {
                    var firstElement = i == 0;
                    var lastElement = i == line.Elements.Count - 1;

                    rect = line.Elements[i].Rectangle;

                    if (firstElement)
                        rect.Left = Location.X;

                    if (lastElement)
                        rect.Right = Location.X + Size.X;

                    if (lastElement && line.Elements[i].Linebreak)
                    {
                        rect.Top -= line.Elements[i].Size.Y;
                        rect.Bottom -= line.Elements[i].Size.Y;
                    }

                    if (rect.Contains(m))
                    {
                        var p = new Point(line.Elements[i].Rectangle.Left, line.Elements[i].Rectangle.Top);
                        var mb = m - p;
                        var font = Gui.Renderer.GetFont(line.Elements[i].Font);
                        var off = 0;
                        var c = 0;

                        while (c < line.Elements[i].Text.Length)
                        {
                            off = Gui.Renderer.GetTextSize(line.Elements[i].Text.Substring(0, c), font).X;

                            if (off > mb.X)
                                break;

                            c++;
                        }

                        _selectEnd = total + c;
                        return;
                    }

                    if (line.Elements[i].Linebreak)
                        total++;
                    else
                        total += line.Elements[i].Text.Length;
                }
            }
        }

        void TextBox_MouseDown(Control sender, MouseEventArgs args)
        {
            var m = Gui.MousePosition;
            var total = 0;
            Rectangle rect;

            foreach (var line in _lines)
            {
                for (var i = 0; i < line.Elements.Count; i++)
                {
                    var firstElement = i == 0;
                    var lastElement = i == line.Elements.Count - 1;

                    rect = line.Elements[i].Rectangle;

                    if (firstElement)
                        rect.Left = Location.X;

                    if (lastElement)
                        rect.Right = Location.X + Size.X;

                    if (lastElement && line.Elements[i].Linebreak)
                    {
                        rect.Top -= line.Elements[i].Size.Y;
                        rect.Bottom -= line.Elements[i].Size.Y;
                    }

                    if (rect.Contains(m))
                    {
                        var p = new Point(line.Elements[i].Rectangle.Left, line.Elements[i].Rectangle.Top);
                        var mb = m - p;
                        var font = Gui.Renderer.GetFont(line.Elements[i].Font);
                        var off = 0;
                        var c = 0;

                        while (c < line.Elements[i].Text.Length)
                        {
                            off = Gui.Renderer.GetTextSize(line.Elements[i].Text.Substring(0, c), font).X;

                            if (off > mb.X)
                                break;

                            c++;
                        }

                        _selectStart = _selectEnd = _caret = total + c;
                        return;
                    }

                    if (line.Elements[i].Linebreak)
                        total++;
                    else
                        total += line.Elements[i].Text.Length;
                }
            }
        }

        private int FindIndexLeft(int start, string text)
        {
            if (start <= 0) return start;

            while (true)
            {
                start--;

                if (char.IsWhiteSpace(text, start) || char.IsPunctuation(text, start) || start <= 0)
                    return start;
            }
        }

        private int FindIndexRight(int start, string text)
        {
            if (start >= text.Length - 1) return start;

            while (true)
            {
                start++;

                if (char.IsWhiteSpace(text, start) || char.IsPunctuation(text, start) || start >= text.Length - 1)
                    return start + 1;
            }
        }

        private void HandleLeftArrow()
        {
            //if (Caret > 0)
            //    Caret--;

            if (Gui.CtrlPressed)
            {
                var masked = _text;

                if (Gui.ShiftPressed)
                {
                    if (_selectStart == _caret)
                    {
                        _caret = _selectStart = FindIndexLeft(_selectStart, masked);
                    }
                    else if (_selectEnd == _caret)
                    {
                        var index = FindIndexLeft(_selectEnd, masked);

                        if (index < _selectStart)
                        {
                            _selectEnd = _selectStart;
                            _selectStart = index;
                            _caret = index;
                        }
                        else
                            _caret = _selectEnd = index;
                    }
                }
                else
                {
                    _selectStart = _selectEnd = _caret = FindIndexLeft(_caret, masked);
                }
            }
            else
            {
                if (_caret > 0)
                {
                    if (Gui.ShiftPressed)
                    {
                        if (_selectStart == _caret)
                            _selectStart--;
                        else if (_selectEnd == _caret)
                            _selectEnd--;
                        _caret--;
                    }
                    else
                    {
                        if (IsSelection)
                        {
                            _selectStart = _selectEnd = _caret;
                        }
                        else
                        {
                            _caret--;
                            _selectStart = _selectEnd = _caret;
                        }
                    }
                }
                else if (!Gui.ShiftPressed)
                {
                    _selectStart = _selectEnd = _caret;
                }
            }
        }

        private void HandleRightArrow()
        {
            //if (Caret < _text.Length)
            //    Caret++;

            if (Gui.CtrlPressed)
            {
                if (Gui.ShiftPressed)
                {
                    if (_selectEnd == _caret)
                    {
                        var index = FindIndexRight(_selectEnd, _text);

                        _caret = _selectEnd = index;
                    }
                    else if (_selectStart == _caret)
                    {
                        var index = FindIndexRight(_selectStart, _text);
                        if (index > _selectEnd)
                        {
                            _selectStart = _selectEnd;
                            _selectEnd = index;
                            _caret = index;
                        }
                        else
                            _caret = _selectStart = index;
                    }
                }
                else
                {
                    _selectStart = _selectEnd = _caret = FindIndexRight(_caret, _text);
                }
            }
            else
            {
                if (_caret < _text.Length)
                {
                    if (Gui.ShiftPressed)
                    {
                        if (_selectEnd == _caret)
                            _selectEnd++;
                        else if (_selectStart == _caret)
                            _selectStart++;

                        _caret++;
                    }
                    else
                    {
                        if (IsSelection)
                        {
                            _selectStart = _selectEnd = _caret;
                        }
                        else
                        {
                            _caret++;
                            _selectStart = _selectEnd = _caret;
                        }
                    }
                }
                else if (!Gui.ShiftPressed)
                {
                    _selectStart = _selectEnd = _caret;
                }
            }
        }

        private void HandleUpArrow()
        {
            var textlength = 0;
            var lastline = 0;

            if (Gui.ShiftPressed)
            {
                if (!IsSelection)
                    _selectStart = _caret;
            }
            else
            {
                _selectStart = _selectEnd = 0;
            }

            for (var i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];

                if (textlength + line.CharLength > _caret)
                {
                    var left = _caret - textlength;

                    if (i > 0 && left > -1)
                    {
                        var lastlen = _lines[i - 1].CharLength;
                        _caret = textlength + Math.Min(lastlen - 1, left) - lastlen;

                        if (Gui.ShiftPressed)
                            _selectEnd = _caret;

                        break;
                    }
                }
                else if (textlength + line.CharLength == _caret)
                {
                    var left = 0;

                    if (i > 0 && left > -1)
                    {
                        var lastlen = _lines[i - 1].CharLength;

                        if (i == _lines.Count - 1)
                            _caret = lastline + Math.Min(lastlen - 1, line.CharLength);
                        else
                            _caret = textlength + Math.Min(lastlen - 1, left);

                        if (Gui.ShiftPressed)
                            _selectEnd = _caret;

                        break;
                    }
                }

                lastline = textlength;
                textlength += line.CharLength;
            }
        }

        private void HandleDownArrow()
        {
            var total = 0;

            if (Gui.ShiftPressed)
            {
                if (!IsSelection)
                    _selectStart = _caret;
            }
            else
            {
                _selectStart = _selectEnd = 0;
            }

            for (var i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];

                if (total + line.CharLength > _caret)
                {
                    var left = _caret - total;

                    if (_lines.Count > i + 1)
                    {
                        var nextlen = _lines[i + 1].CharLength;

                        if (total + line.CharLength == _caret + 1 && i + 1 != _lines.Count - 1)
                        {
                            if (left == 0 && line.CharLength == 1)
                                _caret = total + line.CharLength + Math.Min(nextlen, left);
                            else
                                _caret = total + line.CharLength - 1 + Math.Min(nextlen, left);
                        }
                        else
                            _caret = total + line.CharLength + Math.Min(nextlen, left);

                        if (Gui.ShiftPressed)
                            _selectEnd = _caret;

                        break;
                    }
                }

                total += line.CharLength;
            }
        }

        protected override void OnKeyDown(KeyEventArgs args)
        {
            var root = Desktop;
            if (root == null) return;

            _blinkTime = 0; _doBlink = 1;

            if (args.Key == Keys.Home && !ReadOnly) // home
            {
                if (Gui.ShiftPressed)
                {
                    if (_selectStart == _caret)
                    {
                        _caret = _selectStart = 0;
                    }
                    else if (_selectEnd == _caret)
                    {
                        if (0 < _selectStart)
                        {
                            _selectEnd = _selectStart;
                            _selectStart = 0;
                            _caret = 0;
                        }
                        else
                            _caret = _selectEnd = 0;
                    }
                }
                else
                {
                    _caret = 0;
                    _selectStart = _selectEnd = _caret;
                }
            }
            else if (args.Key == Keys.End && !ReadOnly) // end
            {
                if (Gui.ShiftPressed)
                {
                    if (_selectEnd == _caret)
                    {
                        _caret = _selectEnd = _text.Length;
                    }
                    else if (_selectStart == _caret)
                    {
                        if (_text.Length > _selectEnd)
                        {
                            _selectStart = _selectEnd;
                            _selectEnd = _text.Length;
                            _caret = _text.Length;
                        }
                        else
                            _caret = _selectStart = _text.Length;
                    }
                }
                else
                {
                    _caret = _text.Length;
                    _selectStart = _selectEnd = _caret;
                }
            }
            else if (args.Key == Keys.Rightarrow && !ReadOnly) // right arrow
            {
                HandleRightArrow();
            }
            else if (args.Key == Keys.Leftarrow && !ReadOnly) // left arrow
            {
                HandleLeftArrow();
            }
            else if (args.Key == Keys.Uparrow && !ReadOnly) // up arrow
            {
                HandleUpArrow();
            }
            else if (args.Key == Keys.Downarrow && !ReadOnly) // down arrow
            {
                HandleDownArrow();
            }
            else if (args.Key == Keys.Backspace && !ReadOnly) // backspace
            {
                //if (Caret > 0)
                //{
                //    HandleLeftArrow();

                //    if (_text.Length > Caret)
                //        SetText(_text.Remove(Caret, 1));
                //}

                if (IsSelection)
                {
                    var start = SelectionStart;
                    var end = SelectionEnd;

                    SetText(_text.Remove(start, end - start));
                    _caret = start;
                }
                else
                {
                    if (_caret > 0)
                    {
                        SetText(_text.Remove(_caret - 1, 1));
                        if (_caret > 0) _caret--;
                    }
                }

                _selectStart = _selectEnd = _caret;
            }
            else if (args.Key == Keys.Delete && !ReadOnly) // delete
            {
                //if (_text.Length > Caret)
                //    SetText(_text.Remove(Caret, 1));

                if (IsSelection)
                {
                    var start = SelectionStart;
                    var end = SelectionEnd;

                    SetText(_text.Remove(start, end - start));
                    _caret = start;
                }
                else
                {
                    if (_text.Length > _caret)
                        SetText(_text.Remove(_caret, 1));
                }

                _selectStart = _selectEnd = _caret;
            }
            else if (args.Key == Keys.Return || args.Key == Keys.Numpadenter) // return/enter
            {
                SetText(_text.Insert(_caret, "\n"));
                _caret++;
            }
            else if (args.Key == Keys.Escape)
            {
                LostFocus -= TextBox_LostFocus;

                SetText(_savedText);
                _caret = 0;

                root.FocusedControl = null;

                LostFocus += TextBox_LostFocus;

                if (TextCancel != null)
                    TextCancel(this, null);
            }
            else
            {
                if (Gui.CtrlPressed && !Gui.AltPressed)
                {
                    if (args.Key == Keys.A) // select all
                    {
                        _selectStart = 0;
                        _selectEnd = _text.Length;
                        _caret = _text.Length;
                    }
                    else if (args.Key == Keys.C) // copy
                    {
                        if (IsSelection)
                            Gui.SetClipboard(Selection.Replace("\n", "\r\n"));

                    }
                    else if (args.Key == Keys.X) // copy
                    {
                        if (IsSelection)
                        {
                            Gui.SetClipboard(Selection);

                            var start = SelectionStart;
                            var end = SelectionEnd;

                            SetText(_text.Remove(start, end - start));
                            //Caret = start;
                            _selectStart = _selectEnd = _caret = start;
                        }
                    }
                    else if (args.Key == Keys.V && !ReadOnly) // pasteb
                    {
                        var paste = Gui.GetClipboard().Replace("\r\n","\n");
                        if (!string.IsNullOrEmpty(paste))
                        {
                            if (IsSelection)
                            {
                                var start = SelectionStart;
                                var end = SelectionEnd;

                                SetText(_text.Remove(start, end - start));
                                _caret = start;
                            }

                            SetText(_text.Insert(_caret, paste.ToString()));

                            if (_caret < _text.Length)
                                _caret += paste.Length;

                            _selectStart = _selectEnd = _caret;
                        }
                    }
                }
                else
                {
                    if (args.Key != Keys.Tab)
                    {
                        if (args.Char.HasValue)
                        {
                            var valid = true;
                            var c = args.Char.Value;

                            if (valid)
                            {
                                if (IsSelection)
                                {
                                    var start = SelectionStart;
                                    var end = SelectionEnd;

                                    SetText(_text.Remove(start, end - start));
                                    _caret = start;
                                }

                                SetText(_text.Insert(_caret, c.ToString()));
                                if (_caret < _text.Length)
                                    _caret++;

                                _selectStart = _selectEnd = _caret;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateText(Style style)
        {
            _lines.Clear();

            if (string.IsNullOrEmpty(_text)) return;

            var def = new TextElement();
            def.Font = style.Font;

            var elements = BbCode.Parse(_text, style, false);
            var textElements = new List<TextElement>();

            var pos = new Point();
            var tsize = new Point();

            var lineHeight = 0;
            var thisLine = new List<TextElement>();

            _textSize = Point.Zero;

            if (TextWrap)
            {
                #region TextWrap = true
                foreach (var element in elements)
                {
                    var font = Gui.Renderer.GetFont(element.Font);

                    if (element.Linebreak)
                    {
                        pos.X = 0;
                        pos.Y += lineHeight + Leading;

                        element.Position = pos;
                        element.Size = Gui.Renderer.GetTextSize(" ", font);

                        foreach (var el in thisLine)
                        {
                            if (!el.Linebreak)
                                el.Position.Y += lineHeight - el.Size.Y;
                        }

                        thisLine.Clear();
                        lineHeight = element.Size.Y;
                        textElements.Add(element);
                    }
                    else
                    {
                        #region wrap

                        var words = element.Text.Split(' ');

                        var sub = new List<TextElement>();

                        var e = new TextElement(element);
                        e.Text = string.Empty;
                        e.Position = pos;
                        sub.Add(e);

                        var i = 0;

                        foreach (var word in words)
                        {
                            var temp = word;
                            if (i > 0) temp = " " + word;

                            tsize = Gui.Renderer.GetTextSize(temp, font);
                            lineHeight = Math.Max(lineHeight, tsize.Y);
                            var limit = Size.X - (style.TextPadding.Left + style.TextPadding.Right);

                            i++;

                            // the word fits, add to current element
                            if (pos.X + tsize.X < limit)
                            {
                                e.Text += temp;
                                e.Size = Gui.Renderer.GetTextSize(e.Text, font);
                                pos.X += tsize.X;
                            }
                            else
                            {
                                // the whole word is larger than the text area
                                if (tsize.X > limit)
                                {

                                }
                                // the whole word fits into text area
                                else
                                {
                                    pos.X = 0;
                                    pos.Y += lineHeight + Leading;
                                    thisLine.Add(e);

                                    foreach (var el in thisLine)
                                    {
                                        if (!el.Linebreak)
                                            el.Position.Y += lineHeight - el.Size.Y;
                                    }

                                    thisLine.Clear();
                                    lineHeight = 0;

                                    var linebreak = new TextElement(e);
                                    linebreak.Text = string.Empty;
                                    linebreak.Linebreak = true;
                                    linebreak.Position = pos;
                                    sub.Add(linebreak);

                                    e = new TextElement(element);
                                    e.Text = word;
                                    e.Position = pos;
                                    e.Size = Gui.Renderer.GetTextSize(e.Text, font);
                                    sub.Add(e);

                                    lineHeight = Math.Max(lineHeight, e.Size.Y);

                                    pos.X += tsize.X;
                                }
                            }
                        }

                        thisLine.AddRange(sub);
                        textElements.AddRange(sub);
                        #endregion
                    }
                }

                foreach (var el in thisLine)
                {
                    if (!el.Linebreak)
                        el.Position.Y += lineHeight - el.Size.Y;
                }
                #endregion
            }
            else
            {
                foreach (var element in elements)
                {
                    var font = Gui.Renderer.GetFont(element.Font);

                    if (element.Linebreak)
                    {
                        pos.X = 0;
                        pos.Y += lineHeight + Leading;

                        element.Position = pos;
                        element.Size = Gui.Renderer.GetTextSize(" ", font);

                        foreach (var el in thisLine)
                        {
                            if (!el.Linebreak)
                                el.Position.Y += lineHeight - el.Size.Y;
                        }

                        thisLine.Clear();
                        lineHeight = element.Size.Y;

                        textElements.Add(element);
                    }
                    else
                    {
                        //if (!string.IsNullOrEmpty(element.Text))
                        //{
                        tsize = Gui.Renderer.GetTextSize(string.IsNullOrEmpty(element.Text) ? " " : element.Text, font);
                        lineHeight = Math.Max(lineHeight, tsize.Y);

                        element.Position = pos;
                        element.Size = tsize;

                        textElements.Add(element);

                        pos.X += tsize.X;

                        thisLine.Add(element);
                        //}
                    }
                }

                foreach (var el in thisLine)
                {
                    if (!el.Linebreak)
                        el.Position.Y += lineHeight - el.Size.Y;
                }

            }

            var line = new TextLine();

            foreach (var element in textElements)
            {
                if (element.Linebreak)
                {
                    line.CharLength += 1;
                    line.Elements.Add(element);
                    _lines.Add(line);
                    line = new TextLine();
                }
                else
                {
                    line.Width += element.Size.X;
                    line.CharLength += element.Text.Length;
                    line.Elements.Add(element);
                }

                _textSize.X = Math.Max(_textSize.X, line.Width);
                _textSize.Y = Math.Max(_textSize.Y, element.Position.Y + element.Size.Y);
            }

            _lines.Add(line);

            _textSize += new Point(style.TextPadding.Left + style.TextPadding.Right, style.TextPadding.Top + style.TextPadding.Bottom);

            _lastSize = Size;
            _isDirty = false;

            //GetLineAndCol();
        }


        //protected override void Bind(object instance)
        //{
        //    if (string.IsNullOrEmpty(Aspect)) return;
        //    if (instance == null) return;

        //    Type type = instance.GetType();
        //    object value = null;

        //    System.Reflection.PropertyInfo property = type.GetProperty(Aspect);
        //    if (property != null)
        //    {
        //        value = property.GetValue(instance, null);
        //        if (value != null) Text = value.ToString();
        //    }
        //    else
        //    {
        //        System.Reflection.FieldInfo field = type.GetField(Aspect);
        //        if (field != null)
        //        {
        //            value = field.GetValue(instance);
        //            if (value != null) Text = value.ToString();
        //        }
        //    }
        //}

        protected override void OnStateChanged()
        {
            var style = Desktop.GetStyle(Style).Styles[State];
            UpdateText(style);
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
                            _activeHref = element.Href;
                            break;
                        }
                    }
                }
            }
        }

        protected override void DrawText(Style style, float opacity)
        {
            if (_isDirty) UpdateText(style);

            int font;
            var total = 0;
            var numLine = 0;
            Point p1, p2, size;
            var align = TextAlign != Alignment.Inherit ? TextAlign : style.TextAlign;
            var drawCaret = _hasFocus && _doBlink > 0;

            if (_lines.Count == 0)
            {
                if (drawCaret)
                {
                    p1 = Location;
                    font = Gui.Renderer.GetFont(style.Font);

                    if (align == Alignment.TopLeft || align == Alignment.TopCenter || align == Alignment.TopRight)
                        p1.Y += style.TextPadding.Top;

                    if (align == Alignment.BottomLeft || align == Alignment.BottomCenter || align == Alignment.BottomRight)
                        p1.Y += Size.Y - _textSize.Y;

                    if (align == Alignment.MiddleLeft || align == Alignment.MiddleCenter || align == Alignment.MiddleRight)
                        p1.Y += (Size.Y - (_textSize.Y - (style.TextPadding.Top + style.TextPadding.Bottom))) / 2;

                    if (align == Alignment.TopLeft || align == Alignment.MiddleLeft || align == Alignment.BottomLeft)
                        p1.X += style.TextPadding.Left;

                    if (align == Alignment.TopRight || align == Alignment.MiddleRight || align == Alignment.BottomRight)
                        p1.X += Size.X - style.TextPadding.Right;

                    if (align == Alignment.TopCenter || align == Alignment.MiddleCenter || align == Alignment.BottomCenter)
                        p1.X += Size.X / 2;

                    var subsize = Gui.Renderer.GetTextSize(" ", font);
                    Gui.Renderer.DrawBox(p1.X, p1.Y, 2, subsize.Y, ColorInt.FromArgb(opacity, BlinkColor));

                }

                return;
            }

            foreach (var line in _lines)
            {
                var perline = 0;

                foreach (var element in line.Elements)
                {
                    //if (element.Linebreak)
                    //    continue;

                    font = Gui.Renderer.GetFont(element.Font);

                    if (element.Linebreak)
                        total++;
                    else
                        total += element.Text.Length;

                    size = element.Size;
                    p1 = Location;

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

                    if (element.IsLink && element.Href == _activeHref)
                        Gui.Renderer.DrawBox(p2.X, p2.Y, size.X - 1, size.Y, ColorInt.FromArgb(opacity, LinkColor));

                    if (drawCaret && total >= _caret)
                    {
                        drawCaret = false;

                        if (string.IsNullOrEmpty(element.Text))
                        {
                            var subsize = Gui.Renderer.GetTextSize(" ", font);
                            Gui.Renderer.DrawBox(p2.X, p2.Y, 2, subsize.Y, ColorInt.FromArgb(opacity, BlinkColor));
                        }
                        else
                        {
                            var substr = element.Text.Substring(0, element.Text.Length - (total - _caret));

                            if (string.IsNullOrEmpty(substr))
                            {
                                var subsize = Gui.Renderer.GetTextSize(" ", font);
                                Gui.Renderer.DrawBox(p2.X, p2.Y, 2, subsize.Y, ColorInt.FromArgb(opacity, BlinkColor));
                            }
                            else
                            {
                                var subsize = Gui.Renderer.GetTextSize(substr, font);
                                Gui.Renderer.DrawBox(p2.X + subsize.X, p2.Y, 2, subsize.Y, ColorInt.FromArgb(opacity, BlinkColor));
                            }
                        }
                    }

                    if (UseTextColor)
                        Gui.Renderer.DrawText(element.Text, p2.X, p2.Y, font, ColorInt.FromArgb(opacity, TextColor));
                    else
                        Gui.Renderer.DrawText(element.Text, p2.X, p2.Y, font, ColorInt.FromArgb(opacity, element.Color.HasValue ? (int)element.Color : style.TextColor));

                    //    Gui.Renderer.DrawBox(element.Rectangle.Left, element.Rectangle.Top, element.Rectangle.Width, element.Rectangle.Height, -1);

                    if (IsSelection && total >= SelectionStart && perline < SelectionEnd && !element.Linebreak)
                    {
                        var start = SelectionStart;
                        var end = SelectionEnd;
                        var color = ColorInt.FromArgb(0.5f, -1);

                        //int origin = perline - element.Text.Length;
                        //start = Math.Max(0, origin - start);

                        //if (string.IsNullOrEmpty(element.Text))
                        //{
                        //    Point subsize = Gui.Renderer.GetTextSize(" ", font);
                        //    Gui.Renderer.DrawBox(p2.x, p2.y, subsize.x, subsize.y, ColorInt.FromArgb(opacity, BlinkColor));
                        //}
                        //else
                        //{
                        var begin = element.Text.Length - (total - start);
                        if (begin < 0) begin = 0;

                        var len = element.Text.Length - begin - (total - end);
                        if (len < 0) len = 0;
                        if (len > element.Text.Length) len = element.Text.Length;
                        if (begin + len > element.Text.Length) len = element.Text.Length - begin;

                        var strOffset = element.Text.Substring(0, begin);
                        var strSelected = element.Text.Substring(begin, len);

                        var offset = Gui.Renderer.GetTextSize(strOffset, font);
                        var selection = Gui.Renderer.GetTextSize(strSelected, font);
                        Gui.Renderer.DrawBox(p2.X + offset.X, p2.Y, selection.X, selection.Y, ColorInt.FromArgb(opacity, SelectionColor));
                        //}
                    }

                    if (!string.IsNullOrEmpty(element.Text))
                        perline += element.Text.Length;
                    else
                        perline++;

                }

                numLine++;
            }
        }
    }
}
