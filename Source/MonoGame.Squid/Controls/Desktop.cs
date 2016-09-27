using System.Collections.Generic;
using MonoGame.Squid.Interfaces;
using MonoGame.Squid.Skinning;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// Delegate CursorChangedEvent
    /// </summary>
    /// <param name="cursor">The cursor.</param>
    public delegate void CursorChangedEvent(Cursor cursor);

    /// <summary>
    ///The root Control
    /// </summary>
    public class Desktop : Control, IControlContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Desktop"/> class.
        /// </summary>
        public Desktop()
        {
            Skin = new Skin();
            CursorSet = new CursorCollection();
            TooltipControl = new SimpleTooltip();
            DragDropSnap = 0;
        }

        /// <summary>
        /// Raised when [cursor changed].
        /// </summary>
        public event CursorChangedEvent CursorChanged;

        /// <summary>
        /// Enum PickMode
        /// </summary>
        public enum PickMode
        {
            /// <summary>
            /// The control
            /// </summary>
            Control,

            /// <summary>
            /// The container
            /// </summary>
            Container
        }

        /// <summary>
        /// Gets or sets the controls.
        /// </summary>
        /// <value>The controls.</value>
        public ControlCollection Controls { get; set; }

        /// <summary>
        /// Gets or sets the current cursor.
        /// </summary>
        /// <value>The current cursor.</value>
        [Xml.XmlIgnore]
        public string CurrentCursor
        {
            get
            {
                return _cursor;
            }
            set
            {
                if (value == _cursor) return;
                _cursor = value;

                if (CursorChanged != null)
                {
                    var cursor = GetCursor();
                    CursorChanged(cursor);
                }

                // Cursor cursor = GetCursor();

                // if (cursor == null)
                //    UnityEngine.Cursor.SetCursor(null, UnityEngine.Vector2.zero, UnityEngine.CursorMode.Auto);
                // else
                // {
                //    UnityEngine.Texture2D tex = ((UnityRenderer)Gui.Renderer).FindTexture(cursor.Texture);
                //    UnityEngine.Cursor.SetCursor(tex, new UnityEngine.Vector2(cursor.HotSpot.x, cursor.HotSpot.y), UnityEngine.CursorMode.Auto);
                // }

                // if (CursorChanged != null)
                //    CursorChanged(cursor);
            }
        }

        [Xml.XmlIgnore]
        public CursorCollection CursorSet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [design mode].
        /// </summary>
        /// <value><c>true</c> if [design mode]; otherwise, <c>false</c>.</value>
        [Xml.XmlIgnore]
        public bool DesignMode { get; set; }

        /// <summary>
        /// Gets or sets the drag drop snap.
        /// </summary>
        /// <value>The drag drop snap.</value>
        public int DragDropSnap { get; set; }

        /// <summary>
        /// Gets the drop target control.
        /// </summary>
        /// <value>The drop target control.</value>
        public Control DropTarget { get { return _dropTarget; } }

        /// <summary>
        /// Gets the focused control.
        /// </summary>
        /// <value>The focused control.</value>
        [Xml.XmlIgnore]
        public Control FocusedControl
        {
            get
            {
                return _focused;
            }
            internal set
            {
                if (_focused == value) return;
                if (_focused != null) _focused.OnLostFocus();
                _focused = value;
                if (_focused != null) _focused.OnGotFocus();
            }
        }

        /// <summary>
        /// Gets the hot control.
        /// </summary>
        /// <value>The hot control.</value>
        public Control HotControl { get; internal set; }

        [IntColor]
        public int ModalColor { get; set; }

        /// <summary>
        /// Gets the pressed control.
        /// </summary>
        /// <value>The pressed control.</value>
        public Control PressedControl { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show cursor].
        /// </summary>
        /// <value><c>true</c> if [show cursor]; otherwise, <c>false</c>.</value>
        public bool ShowCursor { get; set; }

        [Xml.XmlIgnore]
        public Skin Skin { get; set; }

        /// <summary>
        /// Gets or sets the tooltip control.
        /// </summary>
        /// <value>The tooltip control.</value>
        [Xml.XmlIgnore]
        public Tooltip TooltipControl { get; set; }

        /// <summary>
        /// Closes the dropdowns.
        /// </summary>
        public void CloseDropdowns()
        {
            foreach (var control in _dropdowns)
                control.Parent = null;

            _dropdowns.Clear();
        }

        /// <summary>
        /// Draws this instance.
        /// </summary>
        public new void Draw()
        {
            Gui.Renderer.Scissor(ClipRect.Left, ClipRect.Top, ClipRect.Width, ClipRect.Height);
            Gui.Renderer.StartBatch();

            base.Draw();

            if (ShowCursor)
                DrawCursor(Gui.MousePosition.X, Gui.MousePosition.Y);

            Gui.Renderer.EndBatch(true);
        }

        /// <summary>
        /// Draws the cursor.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public void DrawCursor(int x, int y)
        {
            var cursor = GetCursor();

            if (cursor != null)
                cursor.Draw(x, y);
        }

        /// <summary>
        /// Gets the cursor.
        /// </summary>
        /// <returns>Cursor.</returns>
        public Cursor GetCursor()
        {
            if (_cursor == null)
                _cursor = string.Empty;

            if (CursorSet.ContainsKey(_cursor))
                return CursorSet[_cursor];
            else if (CursorSet.ContainsKey("default"))
                return CursorSet["default"];

            return null;
        }

        /// <summary>
        /// returns the style with the given name
        /// </summary>
        /// <param name="name">the name to search for</param>
        /// <returns>matching ControlStyle</returns>
        public ControlStyle GetStyle(string name)
        {
            if (string.IsNullOrEmpty(name))
                return _defaultStyle;

            //if (AdditionalStyles.ContainsKey(name))
            //    return AdditionalStyles[name];

            if (Skin == null)
                return _defaultStyle;

            if (Skin.ContainsKey(name))
                return Skin[name];

            return _defaultStyle;
        }

        /// <summary>
        /// Gets the window at.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>Window.</returns>
        public Window GetWindowAt(int x, int y)
        {
            for (var i = Controls.Count - 1; i >= 0; i--)
            {
                var w = Controls[i] as Window;
                if (w == null) continue;

                if (w.Enabled && w.Visible && w.Hit(Gui.MousePosition.X, Gui.MousePosition.Y))
                    return w;
            }

            return null;
        }

        /// <summary>
        /// Picks the control.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="mode">The mode.</param>
        /// <returns>Control.</returns>
        public Control PickControl(int x, int y, PickMode mode)
        {
            if (mode == PickMode.Control) return PickFirst(x, y);
            if (mode == PickMode.Container) return PickDeep(x, y);
            return null;
        }

        /// <summary>
        /// Sets the hot.
        /// </summary>
        /// <param name="control">The control.</param>
        public void SetHot(Control control) { HotControl = control; }

        /// <summary>
        /// Sets the tooltip.
        /// </summary>
        /// <param name="context">The context.</param>
        public void SetTooltip(Control context)
        {
            if (DesignMode) return;
            if (TooltipControl == null) return;

            if (context != _currentContext)
            {
                _currentContext = context;
                Elements.Add(TooltipControl);
                TooltipControl.SetContext(context);
            }

            if (TooltipControl.Parent != null)
            {
                if (!TooltipControl.Visible)
                    Elements.Remove(TooltipControl);
                else// if (TooltipControl.AutoLayout)
                    TooltipControl.LayoutTooltip();
            }
            else if (TooltipControl.Visible)
            {
                Elements.Add(TooltipControl);
            }
        }

        /// <summary>
        /// Shows the dropdown.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="stack">if set to <c>true</c> [stack].</param>
        public void ShowDropdown(Control control, bool stack)
        {
            if (DesignMode) return;
            if (control == null) return;

            if (!stack)
                CloseDropdowns();

            if (_dropdowns.Contains(control)) return;

            if (_dropdowns.Count > 0)
            {
                var index = 0;
                var found = false;

                for (var i = 0; i < _dropdowns.Count; i++)
                {
                    if (_dropdowns[i].Owner == control.Owner)
                    {
                        index = i;
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    for (var i = _dropdowns.Count - 1; i >= index; i--)
                    {
                        _dropdowns[i].Parent = null;
                        _dropdowns.RemoveAt(i);
                    }
                }
            }

            control.Parent = this;
            _dropdowns.Add(control);
        }

        /// <summary>
        /// Tabs the next.
        /// </summary>
        public void TabNext()
        {
            Tab(1);
        }

        /// <summary>
        /// Tabs the previous.
        /// </summary>
        public void TabPrevious()
        {
            Tab(-1);
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public new void Update()
        {
            if (!DesignMode)
            {
                ProcessDragDrop();

                if (Gui.GetButton(0) == ButtonState.Up)
                    EndDragDrop();
            }

            var pressed = -1;
            var down = -1;

            for (var i = 0; i < Gui.Buttons.Length; i++)
            {
                if (Gui.GetButton(i) == ButtonState.Press)
                    pressed = i;

                if (Gui.GetButton(i) == ButtonState.Down)
                    down = i;
            }

            if (pressed == -1)
            {
                if (TooltipControl != null) TooltipControl.Visible = false;
                _hot = GetControlAt(Gui.MousePosition.X, Gui.MousePosition.Y);
                if (TooltipControl != null) TooltipControl.Visible = true;

                if (!DesignMode && _hot != null && _modalQueue.Count > 0)
                {
                    Control check = _modalQueue[_modalQueue.Count - 1];
                    var found = check == _hot || _hot.IsChildOf(check);

                    if (!found && _dropdowns.Count > 0)
                    {
                        for (var i = _dropdowns.Count - 1; i >= 0; i--)
                        {
                            if (_dropdowns[i].Contains(_hot))
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found && _hot.Owner != null)
                        found = _hot.Owner.IsChildOf(check);

                    if (!found)
                        _hot = this;
                }

                if (_hot != HotControl)
                {
                    if (HotControl != null)
                        HotControl.OnMouseLeave();

                    if (_hot != null)
                    {
                        CurrentCursor = _hot.Cursor;
                        _hot.OnMouseEnter();
                    }

                    HotControl = _hot;
                }
            }
            else if (pressed > 1)
            {
                _hot = null;
            }

            for (var i = 0; i < Gui.Buttons.Length; i++)
            {
                if (Gui.GetButton(i) == ButtonState.Up)
                {
                    if (MouseDownControl != null)
                    {
                        MouseDownControl.OnMouseUp(i);
                        break;
                    }
                }
            }

            if (!DesignMode && down > -1)
            {
                if (_modalQueue.Count == 0)
                {
                    var w = GetWindowAt(Gui.MousePosition.X, Gui.MousePosition.Y);
                    if (w != null && w != _window && w.Dock == DockStyle.None)
                    {
                        w.BringToFront();
                        w.Focus();
                        _window = w;
                    }
                }

                if (_hot != null)
                {
                    if (_hot.AllowFocus)
                        FocusedControl = _hot;
                    else if (!_hot.PreventFocusChange)
                        FocusedControl = null;
                }
                else
                    FocusedControl = null;

                //if(OnClick != null)
                //    OnClick(hot);

                if (_dropdowns.Count > 0)
                {
                    if (_hot == null)
                        CloseDropdowns();
                    else
                    {
                        for (var i = _dropdowns.Count - 1; i >= 0; i--)
                        {
                            if (_hot != _dropdowns[i])
                            {
                                if (!_dropdowns[i].Contains(_hot))
                                {
                                    _dropdowns[i].Parent = null;
                                    _dropdowns.RemoveAt(i);
                                }
                                else break;
                            }
                            else break;
                        }
                    }
                }
            }

            if (!DesignMode)
            {
                if (_hot != null)
                    _hot.DoEvents();

                DoKeyEvents();

                if (FocusedControl != null)
                    FocusedControl.DoKeyEvents();

                if (_isDragging)
                {
                    SetTooltip(_dropTarget);
                }
                else
                {
                    SetTooltip((down > -1 || pressed > -1) ? null : _hot);
                }
            }

            PerformUpdate();
            PerformLayout();
            PerformLateUpdate();

            foreach (var data in Gui.KeyBuffer)
            {
                if (data.Pressed && data.Key == Keys.Tab)
                {
                    if (Gui.ShiftPressed)
                        TabPrevious();
                    else
                        TabNext();
                }
            }
        }

        internal Control MouseDownControl;

        internal bool CheckModalLock(Control control)
        {
            if (_modalQueue.Count > 0)
            {
                Control check = _modalQueue[_modalQueue.Count - 1];
                var found = check == control || control.IsChildOf(check);

                return !found;
            }

            return false;
        }

        internal void DoDragDrop(Control sender, Control data)
        {
            if (_isDragging) return;
            if (data == null) return;

            _isDragging = true;

            _dragDropArgs = new DragDropEventArgs();
            _dragDropArgs.DraggedControl = data;
            _dragDropArgs.Source = sender;

            _dragDropOffset = data.Location - Gui.MousePosition;

            _dragData = data;
            _dragDropSender = sender;
            Controls.Add(_dragData);
        }

        internal void RegisterModal(Window control)
        {
            _modalQueue.Add(control);
        }

        internal void Tab(int dir)
        {
            var index = 0;

            if (FocusedControl != null)
                index = FocusedControl.TabIndex;

            index += dir;

            if (index <= 0)
                index = FindHighestTabIndex(0);

            if (index > 0)
            {
                var result = FindTabIndex(index);

                if (result == null)
                    result = FindTabIndex(1);

                if (result != null)
                    result.Focus();
            }
        }

        internal void UnregisterModal(Window control)
        {
            _modalQueue.Remove(control);
        }

        /// <summary>
        /// Draws the style.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="opacity">The opacity.</param>
        protected override void DrawStyle(Style style, float opacity)
        {
        }

        /// <summary>
        /// The _cursor
        /// </summary>
        private string _cursor;

        private Control _focused;
        private Control _currentContext;
        private readonly ControlStyle _defaultStyle = new ControlStyle();
        private Control _dragData;
        private DragDropEventArgs _dragDropArgs;
        private Point _dragDropOffset;
        private Control _dragDropSender;
        private readonly List<Control> _dropdowns = new List<Control>();
        private Control _dropTarget;
        private Control _hot;
        private bool _isDragging;
        private bool _isDropInvalid;
        private readonly List<Window> _modalQueue = new List<Window>();
        private Window _window;

        private void EndDragDrop()
        {
            if (!_isDragging) return;
            _isDragging = false;

            if (_isDropInvalid)
            {
                _isDropInvalid = false;

                if (_dropTarget != null)
                {
                    _dragDropArgs.Cancel = false;
                    _dropTarget.OnDragLeave(_dragDropArgs);
                }

                return;
            }

            _dragDropArgs.Cancel = false;

            if (_dropTarget != null)
                _dropTarget.OnDragDrop(_dragDropArgs);
            else
                OnDragDrop(_dragDropArgs);
        }

        private void ProcessDragDrop()
        {
            if (_dragData == null) return;

            if (_isDragging)
            {
                _dragData.Position = _dragDropOffset + Gui.MousePosition;
                _dragData.Position = Snap(_dragData.Position);

                _dragData.Visible = false;
                var drop = GetDropTarget(_dragDropSender);
                _dragData.Visible = true;

                if (drop != _dropTarget)
                {
                    if (_dropTarget != null)
                    {
                        _dragDropArgs.Cancel = false;
                        _dropTarget.OnDragLeave(_dragDropArgs);
                    }

                    _dropTarget = drop;

                    if (_dropTarget != null)
                    {
                        _dragDropArgs.Cancel = false;
                        _dropTarget.OnDragEnter(_dragDropArgs);
                        if (_dragDropArgs.Cancel) _isDropInvalid = true;
                    }
                }

                if (_dropTarget != null)
                {
                    _dragDropArgs.Cancel = false;
                    _dropTarget.OnDragResponse(_dragDropArgs);
                    if (_dragDropArgs.Cancel) _isDropInvalid = true;
                }
            }
            else
            {
                Controls.Remove(_dragData);
                _dragData = null;
                _dropTarget = null;
            }
        }

        private Point Snap(Point p)
        {
            if (DragDropSnap > 0)
            {
                var x = (int)System.Math.Floor((float)p.X / DragDropSnap) * DragDropSnap;
                var y = (int)System.Math.Floor((float)p.Y / DragDropSnap) * DragDropSnap;

                p = new Point(x, y);
            }

            return p;
        }
    }
}