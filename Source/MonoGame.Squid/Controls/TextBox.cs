using System;
using System.ComponentModel;
using MonoGame.Squid.Interfaces;
using MonoGame.Squid.Skinning;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// A single-line text input. Text scrolls horizontally.
    /// </summary>
    [Toolbox]
    public class TextBox : Control, IText
    {
        private float _blinkTime;
        private int _doBlink;
        private string _text = string.Empty;
        private bool IsSelection { get { return _selectStart != _selectEnd; } }
        private int _selectStart = 0;
        private int _selectEnd = 0;
        private int _offset;
        private int _caret;
        private bool _hasFocus;
        private string _savedText;

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
        /// Gets or sets a value indicating whether this instance is password.
        /// </summary>
        /// <value><c>true</c> if this instance is password; otherwise, <c>false</c>.</value>
        public bool IsPassword { get; set; }

        /// <summary>
        /// Gets or sets the password char.
        /// </summary>
        /// <value>The password char.</value>
        public char PasswordChar { get; set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        public TextBoxMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the blink interval.
        /// </summary>
        /// <value>The blink interval.</value>
        [DefaultValue(500f)]
        public float BlinkInterval { get; set; }

        /// <summary>
        /// Gets or sets the color of the selection.
        /// </summary>
        /// <value>The color of the selection.</value>
        [IntColor, DefaultValue(-1)]
        public int SelectionColor { get; set; }

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
        [DefaultValue(false)]
        public bool UseTextColor { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;

                if (!string.IsNullOrEmpty(value))
                    _text = value.Replace("\r\n", "");
                else
                    _text = value;

                if (_text == null)
                    _text = string.Empty;

                _selectStart = 0;
                _selectEnd = 0;

                if (TextChanged != null)
                    TextChanged(this);
            }
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
            _caret = Math.Min(Text.Length, Math.Max(0, index));
            _selectStart = _selectEnd = _caret;
        }

        /// <summary>
        /// Selects the specified start.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public void Select(int start, int end)
        {
            _selectStart = Math.Min(Text.Length, Math.Max(0, start));
            _selectEnd = Math.Min(Text.Length, Math.Max(0, end));
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
                    var masked = Text;
                    if (IsPassword)
                        masked = new string(PasswordChar, masked.Length);

                    if (masked == null || masked.Length == 0) return string.Empty;

                    var start = Math.Min(_selectStart, _selectEnd);
                    var end = Math.Max(_selectStart, _selectEnd);

                    return masked.Substring(start, end - start);
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBox"/> class.
        /// </summary>
        public TextBox()
        {
            BlinkColor = -1;
            SelectionColor = -1;
            BlinkInterval = 500;
            PasswordChar = '*';
            Style = "textbox";
            _text = string.Empty;
            AllowFocus = true;
            Cursor = CursorNames.Select;

            MouseDown += TextBox_MouseDown;
            MousePress += TextBox_MousePress;
            MouseDoubleClick += TextBox_MouseDoubleClick;
            LostFocus += TextBox_LostFocus;
            GotFocus += TextBox_GotFocus;
        }

        void TextBox_GotFocus(Control sender)
        {
            _selectStart = _selectEnd = 0;
        }

        void TextBox_LostFocus(Control sender)
        {
            if (TextCommit != null)
                TextCommit(this, null);
        }

        void TextBox_MouseDoubleClick(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            var masked = Text;
            if (IsPassword)
                masked = new string(PasswordChar, masked.Length);

            if (string.IsNullOrEmpty(masked)) return;

            var left = FindIndexLeft(_caret, masked);
            var right = FindIndexRight(_caret, masked);

            if (char.IsWhiteSpace(masked, left) || char.IsPunctuation(masked, left))
                left++;

            if (char.IsWhiteSpace(masked, right - 1) || char.IsPunctuation(masked, right - 1))
                right--;

            _selectStart = left;
            _selectEnd = right;

            _caret = _selectEnd;
        }

        void TextBox_MousePress(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;
            if (Gui.CtrlPressed) return;

            var style = Desktop.GetStyle(Style).Styles[State];

            var masked = Text;
            if (IsPassword)
                masked = new string(PasswordChar, masked.Length);

            if (string.IsNullOrEmpty(masked)) return;

            var font = Gui.Renderer.GetFont(style.Font);
            if (font < 0) return;

            var p = Gui.MousePosition - Location;
            var s1 = Gui.Renderer.GetTextSize(masked, font);
            var carex = p.X + _offset + s1.X;
            var x = 0;

            var text = string.Empty;
            var caret = _caret;

            for (var i = 1; i <= masked.Length; i++)
            {
                text = masked.Substring(0, i);
                x = _offset + Gui.Renderer.GetTextSize(text, font).X;
                if (x > p.X)
                {
                    _selectEnd = i - 1;
                    break;
                }
            }

            if (x < p.X)
                _selectEnd = masked.Length;

            var start = Math.Min(_selectStart, _selectEnd);
            var end = Math.Max(_selectStart, _selectEnd);

            if (_selectEnd < _selectStart)
                _caret = start;
            else
                _caret = end;
        }

        void TextBox_MouseDown(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            if (!_hasFocus)
            {
                _savedText = Text;
                _hasFocus = true;
            }

            var style = Desktop.GetStyle(Style).Styles[State];

            var masked = Text;
            if (IsPassword)
                masked = new string(PasswordChar, masked.Length);

            if (string.IsNullOrEmpty(masked)) return;

            var font = Gui.Renderer.GetFont(style.Font);
            if (font < 0) return;

            var p = Gui.MousePosition - Location;
            var s1 = Gui.Renderer.GetTextSize(masked, font);
            var carex = p.X + _offset + s1.X;
            var x = 0;

            var text = string.Empty;

            for (var i = 1; i <= masked.Length; i++)
            {
                text = masked.Substring(0, i);
                x = _offset + Gui.Renderer.GetTextSize(text, font).X;
                if (x > p.X)
                {
                    _caret = i - 1;
                    break;
                }
            }

            if (x < p.X)
                _caret = masked.Length;

            if (Gui.CtrlPressed)
            {
                var left = FindIndexLeft(_caret, masked);
                var right = FindIndexRight(_caret, masked);

                if (char.IsWhiteSpace(masked, left) || char.IsPunctuation(masked, left))
                    left++;

                if (char.IsWhiteSpace(masked, right - 1) || char.IsPunctuation(masked, right - 1))
                    right--;

                _selectStart = left;
                _selectEnd = right;

                _caret = _selectEnd;
            }
            else if (Gui.ShiftPressed)
            {
                _selectEnd = _caret;
            }
            else
            {
                _selectStart = _caret;
                _selectEnd = _caret;
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

        private void HandleRightArrow()
        {
            if (Gui.CtrlPressed)
            {
                var masked = Text;
                if (IsPassword)
                    masked = new string(PasswordChar, masked.Length);

                if (Gui.ShiftPressed)
                {
                    if (_selectEnd == _caret)
                    {
                        var index = FindIndexRight(_selectEnd, masked);

                        _caret = _selectEnd = index;
                    }
                    else if (_selectStart == _caret)
                    {
                        var index = FindIndexRight(_selectStart, masked);
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
                    _selectStart = _selectEnd = _caret = FindIndexRight(_caret, masked);
                }
            }
            else
            {
                if (_caret < Text.Length)
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

        private void HandleLeftArrow()
        {
            if (Gui.CtrlPressed)
            {
                var masked = Text;
                if (IsPassword)
                    masked = new string(PasswordChar, masked.Length);

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

        protected override void OnKeyDown(KeyEventArgs args)
        {
            // UnityEngine.Debug.Log(args.Key);

            var root = Desktop;
            if (root == null) return;

            _blinkTime = 0; _doBlink = 1;

            if (ReadOnly) return;

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
                        _caret = _selectEnd = Text.Length;
                    }
                    else if (_selectStart == _caret)
                    {
                        if (Text.Length > _selectEnd)
                        {
                            _selectStart = _selectEnd;
                            _selectEnd = Text.Length;
                            _caret = Text.Length;
                        }
                        else
                            _caret = _selectStart = Text.Length;
                    }
                }
                else
                {
                    _caret = Text.Length;
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
            else if (args.Key == Keys.Backspace && !ReadOnly) // backspace
            {
                if (IsSelection)
                {
                    var start = Math.Min(_selectStart, _selectEnd);
                    var end = Math.Max(_selectStart, _selectEnd);

                    Text = Text.Remove(start, end - start);
                    _caret = start;
                    _offset = 0;
                }
                else
                {
                    if (_caret > 0)
                    {
                        Text = Text.Remove(_caret - 1, 1);
                        if (_caret > 0) _caret--;
                    }
                }

                _selectStart = _selectEnd = _caret;
            }
            else if (args.Key == Keys.Delete && !ReadOnly) // delete
            {
                if (IsSelection)
                {
                    var start = Math.Min(_selectStart, _selectEnd);
                    var end = Math.Max(_selectStart, _selectEnd);

                    Text = Text.Remove(start, end - start);
                    _caret = start;
                    _offset = 0;
                }
                else
                {
                    if (Text.Length > _caret)
                        Text = Text.Remove(_caret, 1);
                }

                _selectStart = _selectEnd = _caret;
            }
            else if (args.Key == Keys.Return || args.Key == Keys.Numpadenter) // return/enter
            {
                LostFocus -= TextBox_LostFocus;

                root.FocusedControl = null;
                _caret = 0;

                _selectStart = _selectEnd = _caret;

                LostFocus += TextBox_LostFocus;

                if (TextCommit != null)
                    TextCommit(this, null);
            }
            else if (args.Key == Keys.Escape)
            {
                LostFocus -= TextBox_LostFocus;

                Text = _savedText;
                root.FocusedControl = null;
                _caret = 0;
                _hasFocus = false;
                _selectStart = _selectEnd = _caret;

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
                        _selectEnd = Text.Length;
                        _caret = Text.Length;
                    }
                    else if (args.Key == Keys.C) // copy
                    {
                        if (IsSelection)
                            Gui.SetClipboard(Selection);

                    }
                    else if (args.Key == Keys.X) // copy
                    {
                        if (IsSelection)
                        {
                            Gui.SetClipboard(Selection);

                            var start = Math.Min(_selectStart, _selectEnd);
                            var end = Math.Max(_selectStart, _selectEnd);

                            Text = Text.Remove(start, end - start);
                            _caret = start;
                            _offset = 0;
                        }
                    }
                    else if (args.Key == Keys.V && !ReadOnly) // paste
                    {
                        var paste = Gui.GetClipboard();
                        if (!string.IsNullOrEmpty(paste))
                        {
                            if (IsSelection)
                            {
                                var start = Math.Min(_selectStart, _selectEnd);
                                var end = Math.Max(_selectStart, _selectEnd);

                                Text = Text.Remove(start, end - start);
                                _caret = start;
                            }

                            Text = Text.Insert(_caret, paste.ToString());
                            if (_caret < Text.Length)
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

                            if (Mode == TextBoxMode.Numeric)
                                valid = char.IsNumber(c) || char.IsDigit(c) || (c.ToString() == ".") || (c.ToString() == ",");

                            if (valid)
                            {
                                if (IsSelection)
                                {
                                    var start = Math.Min(_selectStart, _selectEnd);
                                    var end = Math.Max(_selectStart, _selectEnd);

                                    Text = Text.Remove(start, end - start);
                                    _caret = start;
                                }

                                Text = Text.Insert(_caret, c.ToString());
                                if (_caret < Text.Length)
                                    _caret++;

                                _selectStart = _selectEnd = _caret;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            _blinkTime += Gui.TimeElapsed;

            if (_blinkTime > BlinkInterval)
            {
                _doBlink = 1 - _doBlink;
                _blinkTime = 0;
            }
        }

        protected override void DrawText(Style style, float opacity)
        {
            if (_text == null) _text = string.Empty;

            var masked = _text;
            if (IsPassword)
                masked = new string(PasswordChar, masked.Length);

            var font = Gui.Renderer.GetFont(style.Font);
            if (font < 0) return;

            var p = AlignText(masked, Alignment.MiddleLeft, style.TextPadding, font);

            var clip = new Rectangle(Location, Size);
            clip.Left += style.TextPadding.Left;
            clip.Right -= style.TextPadding.Right - 1;
            clip = Clip(clip);

            if (clip.Width < 1 || clip.Height < 1) return;

            SetScissor(clip.Left, clip.Top, clip.Width, clip.Height);

            if (_caret > masked.Length) _caret = masked.Length;

            if (Desktop.FocusedControl == this)
            {
                var rect = new Rectangle(Location, Size);

                var s1 = Gui.Renderer.GetTextSize(masked, font);
                var s2 = Gui.Renderer.GetTextSize(masked.Substring(0, _caret), font);

                if (string.IsNullOrEmpty(masked))
                {
                    s2.Y = Gui.Renderer.GetTextSize(" ", font).Y;
                    p = AlignText(" ", Alignment.MiddleLeft, style.TextPadding, font);
                }
                else if (s2.Y == 0)
                {
                    s2.Y = Gui.Renderer.GetTextSize(" ", font).Y;
                }

                var carex = p.X + _offset + s2.X;

                var lim1 = rect.Left + style.TextPadding.Left;
                var lim2 = rect.Right - style.TextPadding.Right;

                if (carex < lim1)
                    _offset += lim1 - carex;

                if (carex > lim2)
                    _offset += lim2 - carex;

                if (_offset < 0)
                {
                    if (p.X + _offset + s1.X < lim2)
                        _offset += lim2 - (p.X + _offset + s1.X);
                }

                p.X += _offset;

                Gui.Renderer.DrawText(masked, p.X, p.Y, font, ColorInt.FromArgb(opacity, UseTextColor ? TextColor : style.TextColor));

                if (!ReadOnly && _doBlink > 0)
                    Gui.Renderer.DrawBox(p.X + s2.X, p.Y, 1, s2.Y, ColorInt.FromArgb(opacity, BlinkColor));

                if (IsSelection)
                {
                    var start = Math.Min(_selectStart, _selectEnd);
                    var end = Math.Max(_selectStart, _selectEnd);
                    var color = ColorInt.FromArgb(0.5f, SelectionColor);
                    var text = masked.Substring(0, start);
                    var text2 = masked.Substring(start, end - start);

                    var size1 = Gui.Renderer.GetTextSize(text, font);
                    var size2 = Gui.Renderer.GetTextSize(text2, font);

                    Gui.Renderer.DrawBox(p.X + size1.X, p.Y, size2.X, size2.Y, ColorInt.FromArgb(opacity, color));
                }
            }
            else
            {
                _hasFocus = false;
                _offset = 0;
                Gui.Renderer.DrawText(masked, p.X, p.Y, font, ColorInt.FromArgb(opacity, style.TextColor));
            }

            ResetScissor();
        }
    }
}
