using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using MonoGame.Squid.Interfaces;
using MonoGame.Squid.Skinning;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;
using MonoGame.Squid.Xml;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// Delegate VoidEvent
    /// </summary>
    /// <param name="sender">The sender.</param>
    public delegate void VoidEvent(Control sender);
    /// <summary>
    /// Delegate KeyEvent
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
    public delegate void KeyEvent(Control sender, KeyEventArgs args);
    /// <summary>
    /// Delegate MouseEvent
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
    public delegate void MouseEvent(Control sender, MouseEventArgs args);
    /// <summary>
    /// Delegate DragDropEvent
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="DragDropEventArgs"/> instance containing the event data.</param>
    public delegate void DragDropEvent(Control sender, DragDropEventArgs e);
    /// <summary>
    /// Delegate EventWithArgs
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="SquidEventArgs"/> instance containing the event data.</param>
    public delegate void EventWithArgs(Control sender, SquidEventArgs args);

    /// <summary>
    /// The base class of all Controls
    /// </summary>
    public partial class Control
    {
        /// <summary>
        /// Raised when [update].
        /// </summary>
        public event VoidEvent Update;

        /// <exclude />
        public event VoidEvent Layout;

        /// <summary>
        /// Raised when [late update].
        /// </summary>
        public event VoidEvent LateUpdate;

        /// <summary>
        /// Raised when [size changed].
        /// </summary>
        public event VoidEvent SizeChanged;

        /// <summary>
        /// Raised when [position changed].
        /// </summary>
        public event VoidEvent PositionChanged;

        /// <summary>
        /// Raised when [mouse enter].
        /// </summary>
        public event VoidEvent MouseEnter;

        /// <summary>
        /// Raised when [mouse leave].
        /// </summary>
        public event VoidEvent MouseLeave;

        /// <summary>
        /// Raised when [got focus].
        /// </summary>
        public event VoidEvent GotFocus;

        /// <summary>
        /// Raised when [lost focus].
        /// </summary>
        public event VoidEvent LostFocus;

        /// <summary>
        /// Raised when [key down].
        /// </summary>
        public event KeyEvent KeyDown;

        /// <summary>
        /// Raised when [key up].
        /// </summary>
        public event KeyEvent KeyUp;

        /// <summary>
        /// Raised when [mouse drag].
        /// </summary>
        public event MouseEvent MouseDrag;

        /// <summary>
        /// Raised when [mouse up].
        /// </summary>
        public event MouseEvent MouseUp;

        /// <summary>
        /// Raised when [mouse down].
        /// </summary>
        public event MouseEvent MouseDown;

        /// <summary>
        /// Raised when [mouse press].
        /// </summary>
        public event MouseEvent MousePress;

        /// <summary>
        /// Raised when [mouse click].
        /// </summary>
        public event MouseEvent MouseClick;

        /// <summary>
        /// Raised when [mouse double click].
        /// </summary>
        public event MouseEvent MouseDoubleClick;

        /// <summary>
        /// Raised when [mouse wheel].
        /// </summary>
        public event MouseEvent MouseWheel;

        /// <summary>
        /// Raised when [drag drop].
        /// </summary>
        public event DragDropEvent DragDrop;

        /// <summary>
        /// Raised during Drag&Drop, when this control becomes the active DropTarget.
        /// AllowDrop must be True for this event to fire.
        /// </summary>
        public event DragDropEvent DragEnter;

        /// <summary>
        /// Raised during Drag&Drop, when the active DropTarget changes from this control to a different one.
        /// AllowDrop must be True for this event to fire.
        /// </summary>
        public event DragDropEvent DragLeave;

        /// <summary>
        /// Raised during Drag&Drop, while this control is the active DropTargt.
        /// AllowDrop must be True for this event to fire.
        /// </summary>
        public event DragDropEvent DragResponse;

        /// <summary>
        /// Raised when control is dropped.
        /// </summary>
        public event DragDropEvent Drop;
    }

    public partial class Control
    {
        internal bool _isElement;
        internal Rectangle Bounds;
        internal Rectangle DockAreaC;
        internal Rectangle DockAreaE;
        internal bool IsRemoved;
        internal Control Owner;

        internal Control ParentControl
        {
            get { return _parent; }
            set
            {
                if (_parent == value) return;
                _parent = value;
                SetBounds();
            }
        }

        private bool _textureFade;
        private bool _fontFade;

        private float _fadeIn;
        private float _fadeOut = 1;
        private bool _isMouseDrag;
        private bool _stateWasSet;
        private bool _visible = true;
        private bool _enabled = true;
        private bool _parentEnabled = true;
        private DateTime _timeClicked;
        private bool _isDoubleClick;
        private Margin _padding;
        private Margin _margin;
        private Point _size;
        private Point _position;
        private Control _parent;
        private ControlState _state;
        private ControlState _oldState;
        private AnchorStyles _anchor = AnchorStyles.Top | AnchorStyles.Left;
        private Point _floatingPosition;
        private Point _floatingSize;
        private DockStyle _dock;
        private static readonly Dictionary<int, KeyValuePair<Control, Rectangle>> _scissorStack = new Dictionary<int, KeyValuePair<Control, Rectangle>>();

        /// <summary>
        /// The area
        /// </summary>
        protected Rectangle ClipRect;

        /// <summary>
        /// The elements
        /// </summary>
        protected ElementCollection Elements;

        /// <summary>
        /// Returns all child elements
        /// </summary>
        /// <returns></returns>
        public ElementCollection GetElements()
        {
            return Elements;
        }

        private bool _useTranslation = false;

        /// <summary>
        /// Gets or sets a value indicating whether [use translation].
        /// </summary>
        /// <value><c>true</c> if [use translation]; otherwise, <c>false</c>.</value>
        public bool UseTranslation
        {
            get { return _useTranslation; }
            set
            {
                if (value == _useTranslation) return;
                var from = _useTranslation;
                var to = value;
                TranslationChanged(from, to);
                _useTranslation = value;
            }
        }

        internal void UpdateTranslation()
        {
            if (UseTranslation)
            {
                TranslationChanged(true, false);
                TranslationChanged(false, true);
            }

            for (var i = 0; i < Elements.Count; i++)
                Elements[i].UpdateTranslation();

            if (this is IControlContainer)
            {
                var controls = ((IControlContainer)this).Controls;

                for (var i = 0; i < controls.Count; i++)
                    controls[i].UpdateTranslation();
            }
        }

        protected virtual void TranslationChanged(bool from, bool to)
        {
            if (string.IsNullOrEmpty(_originalTooltip)) return;

            if (from == false && to == true)
            {
                _tooltip = TranslateText(_originalTooltip);
            }
            else if (from == true && to == false)
                _tooltip = _originalTooltip;
        }

        protected string TranslateText(string text)
        {
            var result = string.Empty;
            var szPattern = @"(\{tk:(.*?)\})+";
            var tokens = new List<string>();

            foreach (Match match in Regex.Matches(text, szPattern))
                tokens.Add(match.Value);

            string key;
            string translated;

            result = text;

            foreach (var token in tokens)
            {
                key = token.Substring(4, token.Length - 5);
                translated = Gui.Translate(key);
                result = result.Replace(token, translated);
            }

            if (tokens.Count == 0)
                result = Gui.Translate(text);

            return result;
        }

        /// <summary>
        /// Returns true if the control is a child element
        /// </summary>
        public bool IsElement { get { return _isElement; } }

        /// <summary>
        /// Returns the depth of the control
        /// </summary>
        public int Depth { get; internal set; }

        /// <summary>
        /// Gets/Sets the color used to Tint the used Style
        /// </summary>
        [IntColor]
        [Category("Design")]
        [DefaultValue(-1)]
        public int Tint { get; set; }

        /// <summary>
        /// Name of the control
        /// </summary>
        [DefaultValue("")]
        [Category("Base")]
        public string Name { get; set; }

        /// <summary>
        /// Opacity of the control
        /// This is multiplied with any style opacity and hierarchical opacity
        /// </summary>
        [ValueRange(0, 1)]
        [DefaultValue(1.0f)]
        [Category("Design")]
        public float Opacity { get; set; }

        /// <summary>
        /// Gets/Sets the autosize behavior
        /// </summary>
        [DefaultValue(AutoSize.None)]
        [Category("Layout")]
        public AutoSize AutoSize { get; set; }

        /// <summary>
        /// Gets/Sets whether or not the control processes DragDrop events
        /// </summary>
        [DefaultValue(false)]
        [Category("Behavior")]
        public bool AllowDrop { get; set; }

        /// <summary>
        /// Gets/Sets whether or not the control is able to acquire focus
        /// </summary>
        [DefaultValue(false)]
        [Category("Behavior")]
        public bool AllowFocus { get; set; }

        /// <summary>
        /// Gets/Sets whether or not the control prevents the focus to change
        /// </summary>
        [DefaultValue(false)]
        [Category("Behavior")]
        public bool PreventFocusChange { get; set; }

        /// <summary>
        /// Gets/Sets whether or not hardware scissor test is used
        /// </summary>
        [DefaultValue(false)]
        [Category("Design")]
        public bool Scissor { get; set; }

        /// <summary>
        /// Gets/Sets the docking behavior
        /// </summary>
        [DefaultValue(DockStyle.None)]
        [Category("Layout")]
        public DockStyle Dock
        {
            get { return _dock; }
            set
            {
                if (value == DockStyle.None)
                {
                    _size = _floatingSize;
                    _position = _floatingPosition;
                }
                else if (_dock == DockStyle.None)
                {
                    _floatingSize = _size;
                    _floatingPosition = _position;
                }

                _dock = value;
            }
        }

        /// <summary>
        /// Name of the ControlStyle 
        /// </summary>
        [Style]
        [DefaultValue("")]
        [Category("Design")]
        public string Style { get; set; }

        /// <summary>
        /// Gets/Sets whether or not the control is interactive and processes any events
        /// </summary>
        [Category("Behavior")]
        public bool NoEvents { get; set; }

        /// <summary>
        /// user defined data object
        /// </summary>
        [XmlIgnore, Hidden]
        public object Tag { get; set; }

        /// <summary>
        /// user defined data object
        /// </summary>
        [XmlIgnore, Hidden]
        public object UserData { get; set; }

        /// <summary>
        /// internal data object
        /// </summary>
        [XmlIgnore, Hidden]
        internal object InternalTag { get; set; }

        /// <summary>
        /// tab index
        /// </summary>
        [DefaultValue(0)]
        [Category("Behavior")]
        public int TabIndex { get; set; }

        /// <summary>
        /// Gets/Sets whether or not the control is visible
        /// </summary>
        [DefaultValue(true)]
        [Category("Base")]
        public bool Visible { get { return _visible; } set { _visible = value; } }

        /// <summary>
        /// Returns the parent of the control as IControlContainer
        /// </summary>
        public IControlContainer Container { get { return _parent as IControlContainer; } }

        /// <summary>
        /// Name of the cursor to use 
        /// </summary>
        [Cursor]
        [DefaultValue("")]
        [Category("Behavior")]
        public string Cursor { get; set; }

        private string _tooltip;
        private string _originalTooltip;

        /// <summary>
        /// Tooltip text 
        /// </summary>
        [DefaultValue("")]
        [Multiline]
        [Category("Base")]
        public string Tooltip
        {
            get { return _tooltip; }
            set
            {
                _originalTooltip = value;

                if (UseTranslation)
                    _tooltip = TranslateText(_originalTooltip);
                else
                    _tooltip = value;
            }
        }

        [DefaultValue(Alignment.TopCenter)]
        public Alignment TooltipAlign { get; set; }

        /// <summary>
        /// Returns the z-index
        /// </summary>
        public int ZIndex
        {
            get
            {
                if (Container != null)
                    return Container.Controls.Count - (Container.Controls.IndexOf(this) + 1);
                else return 0;
            }
        }

        /// <summary>
        /// Gets/Sets whether or not the control, and all its children, is enabled
        /// </summary>
        [DefaultValue(true)]
        [Category("Base")]
        public bool Enabled
        {
            get
            {
                if (!_parentEnabled)
                    return false;

                return _enabled;
            }
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;

                if (this is IControlContainer)
                {
                    foreach (var control in ((IControlContainer)this).Controls)
                        control.SetEnabled(value);
                }

                foreach (var control in Elements)
                    control.SetEnabled(value);
            }
        }

        /// <summary>
        /// Gets/Sets the parent
        /// </summary>
        [XmlIgnore, Hidden]
        public Control Parent
        {
            get { return _parent; }
            set
            {
                if (value == this) return;
                if (value == _parent) return;

                if (value != null)
                {
                    if (value.IsChildOf(this))
                        return;
                }

                if (Container != null)
                    Container.Controls.Remove(this);

                if (value is IControlContainer)
                {
                    ((IControlContainer)value).Controls.Add(this);
                }
            }
        }

        /// <summary>
        /// Gets/Sets the minimum size.
        /// This is only used during Control.Resize
        /// </summary>
        [DefaultValue(typeof(Point), "0; 0")]
        [Category("Layout")]
        public Point MinSize { get; set; }

        /// <summary>
        /// Gets/Sets the maximum size.
        /// This is only used during Control.Resize
        /// </summary>
        [DefaultValue(typeof(Point), "0; 0")]
        [Category("Layout")]
        public Point MaxSize { get; set; }

        /// <summary>
        /// Gets/Sets the size
        /// </summary>
        [DefaultValue(typeof(Point), "0; 0")]
        [Category("Layout")]
        public Point Size
        {
            get { return _size; }
            set
            {
                if (_size.X == value.X && _size.Y == value.Y) return;
                _size = value;

                if (SizeChanged != null) SizeChanged(this);

                SetBounds();
            }
        }

        /// <summary>
        /// Gets/Sets the position (relative to parent)
        /// </summary>
        [DefaultValue(typeof(Point), "0; 0")]
        [Category("Layout")]
        public Point Position
        {
            get { return _position; }
            set
            {
                if (_position.X == value.X && _position.Y == value.Y) return;
                _position = value;

                if (PositionChanged != null) PositionChanged(this);

                SetBounds();
            }
        }

        /// <summary>
        /// Gets/Sets the anchoring behavior
        /// </summary>
        [DefaultValue(AnchorStyles.Top | AnchorStyles.Left)]
        [Category("Layout")]
        public AnchorStyles Anchor
        {
            get { return _anchor; }
            set
            {
                if (_anchor == value) return;
                _anchor = value;
                SetBounds();
            }
        }

        /// <summary>
        /// Defines the space around a control that keeps other controls at a specified distance from the control's borders.
        /// </summary>
        [DefaultValue(typeof(Margin), "0; 0; 0; 0")]
        [Category("Layout")]
        public Margin Margin
        {
            get { return _margin; }
            set { _margin = value; }
        }

        /// <summary>
        /// Defines the space inside of a control that keeps child controls at a specified distance from the control's borders.
        /// </summary>
        [DefaultValue(typeof(Margin), "0; 0; 0; 0")]
        [Category("Layout")]
        public Margin Padding
        {
            get { return _padding; }
            set { _padding = value; }
        }

        /// <summary>
        /// Gets/Sets the state
        /// </summary>
        [XmlIgnore, Hidden]
        public ControlState State
        {
            get { return _state; }
            set
            {
                _stateWasSet = true;

                if (_state == value) return;

                _oldState = _state;
                _state = value;

                var last =  Desktop.GetStyle(Style).Styles[_oldState];
                var next = Desktop.GetStyle(Style).Styles[_state];

                _textureFade = last.IsTextureDifferent(next);
                _fontFade = last.IsFontDifferent(next);

                OnStateChanged();

                var fade = _fadeIn;
                _fadeIn = _fadeOut;
                _fadeOut = fade;
            }
        }

        /// <summary>
        /// Gets/Sets the local fading speed
        /// </summary>
        [DefaultValue(0.0f)]
        [Category("Design")]
        public float FadeSpeed { get; set; }

        /// <summary>
        /// Returns the screen position
        /// </summary>
        public Point Location
        {
            get
            {
                if (_parent != null)
                    return _parent.Location + _position;

                return _position;
            }
        }

        /// <summary>
        /// Returns the root control
        /// </summary>
        public Desktop Desktop
        {
            get
            {
                if (_parent != null)
                    return _parent.Desktop;
                else
                    return this as Desktop;
            }
        }

        //public ControlAnimation Animation;
        public GuiActionList Actions;

        ///// <summary>
        ///// Name of a property or field to bind to this control
        ///// </summary>
        //public string Aspect { get; set; }

        /// <summary>
        /// Gets the unique auto id.
        /// </summary>
        /// <value>The auto id.</value>
        [XmlIgnore]
        public int AutoId { get; private set; }

        private static int _autoIncrement;

        /// <summary>
        /// Control ctor
        /// </summary>
        public Control()
        {
            Name = "";
            Tooltip = "";
            Cursor = "";

            TooltipAlign = Alignment.TopCenter;
            Size = new Point(100, 30);
            AutoId = _autoIncrement++;

            Tint = -1;
            Opacity = 1;
            Anchor = AnchorStyles.Top | AnchorStyles.Left;
            Elements = new ElementCollection(this);
            Actions = new GuiActionList(this);
            //Animation = new ControlAnimation(this);

            if (this is IControlContainer)
            {
                ((IControlContainer)this).Controls = new ControlCollection(this);
            }
        }

        //protected virtual void Bind(object value)
        //{
        //    if (this is IControlContainer)
        //    {
        //        foreach (Control child in ((IControlContainer)this).Controls)
        //            child.Bind(value);
        //    }
        //}

        /// <summary>
        /// Makes the control the focused control
        /// </summary>
        public void Focus()
        {
            if (!AllowFocus) return;
            if (Desktop == null) return;

            Desktop.FocusedControl = this;
        }

        /// <summary>
        /// Return true if the point collides with the control
        /// </summary>
        /// <param name="x">x screen coord</param>
        /// <param name="y">y screen coord</param>
        /// <returns></returns>
        public bool Hit(int x, int y)
        {
            return x >= ClipRect.Left && x <= ClipRect.Right && y >= ClipRect.Top && y <= ClipRect.Bottom;
        }

        /// <summary>
        /// Returns the first control found below the given screen point
        /// Returns elements and controls
        /// </summary>
        /// <param name="x">x screen coord</param>
        /// <param name="y">y screen coord</param>
        /// <returns></returns>
        public Control GetControlAt(int x, int y)
        {
            return GetControlAt(x, y, true);
        }

        /// <summary>
        /// Returns the first control matching the given name.
        /// This method searches recursively
        /// </summary>
        /// <param name="name">Name of the control to find</param>
        /// <returns>Matching control</returns>
        public Control GetControl(string name)
        {
            if (Name == name) return this;

            Control result = null;

            if (this is IControlContainer)
            {
                foreach (var child in (this as IControlContainer).Controls)
                {
                    result = child.GetControl(name);
                    if (result != null)
                        return result;
                }
            }

            foreach (var child in Elements)
            {
                result = child.GetControl(name);
                if (result != null)
                    return result;
            }

            return result;
        }

        /// <summary>
        /// Returns direct children of the given type
        /// </summary>
        /// <returns>Matching controls</returns>
        public List<T> GetControls<T>() where T : Control
        {
            var result = new List<T>();

            if (this is IControlContainer)
            {
                foreach (var child in (this as IControlContainer).Controls)
                {
                    if (child is T)
                        result.Add(child as T);
                }
            }

            foreach (var child in Elements)
            {
                if (child is T)
                    result.Add(child as T);
            }

            return result;
        }

        /// <summary>
        /// Returns the first control found below the given screen point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Control GetControlAt(int x, int y, bool elements)
        {
            if (!Enabled) return null;
            if (!Visible) return null;
            if (!Hit(x, y)) return null;

            var found = NoEvents ? null : this;
            if (!elements && _isElement)
                found = null;

            if (this is IControlContainer)
            {
                var container = this as IControlContainer;
                for (var i = container.Controls.Count - 1; i >= 0; i--)
                {
                    var child = container.Controls[i].GetControlAt(x, y, elements);

                    if (child != null && child.Enabled && child.Visible && !child.NoEvents)
                    {
                        found = child;
                        break;
                    }
                }
            }

            for (var i = Elements.Count - 1; i >= 0; i--)
            {
                var child = Elements[i].GetControlAt(x, y, elements);

                if (child != null && child.Enabled && child.Visible && !child.NoEvents)
                {
                    found = child;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Performs a drag drop operation
        /// </summary>
        /// <param name="data">The control to be displayed as dragged</param>
        public void DoDragDrop(Control data)
        {
            if (Desktop == null) return;

            Desktop.DoDragDrop(this, data);
        }

        /// <summary>
        /// Returns true if the given control is a child  of the control
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool IsChildOf(Control control)
        {
            if (control.Elements.Contains(this)) return true;

            foreach (var child in control.Elements)
            {
                if (IsChildOf(child))
                    return true;
            }

            var container = control as IControlContainer;
            if (container == null) return false;
            if (container.Controls.Contains(this)) return true;

            foreach (var child in container.Controls)
            {
                if (IsChildOf(child))
                    return true;
            }

            return false;
        }

        public bool InRect(Point start, Point end)
        {
            var r1 = new Rectangle();
            r1.Left = Math.Min(start.X, end.X);
            r1.Top = Math.Min(start.Y, end.Y);
            r1.Right = Math.Max(start.X, end.X);
            r1.Bottom = Math.Max(start.Y, end.Y);

            //  Rectangle r = new Rectangle(Location, Size);

            return !(ClipRect.Left > r1.Right
                     || ClipRect.Right < r1.Left
                     || ClipRect.Top > r1.Bottom
                     || ClipRect.Bottom < r1.Top
                    );
        }

        /// <summary>
        /// Override this in custom controls to handle special cases
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public virtual bool Contains(Control control)
        {
            if (control == null)
                return false;

            return control.IsChildOf(this);
        }

        /// <summary>
        /// Fires the MouseClick event
        /// </summary>
        public virtual void Click(int button)
        {
            if (Desktop.CheckModalLock(this))
                return;

            OnMouseDown(button);
            OnMouseUp(button);
            OnMouseRelease(button);
        }

        /// <summary>
        /// Sends the control to the front of its container
        /// </summary>
        public void BringToFront()
        {
            if (Container == null) return;
            var index = Container.Controls.IndexOf(this);
            if (index == Container.Controls.Count - 1) return;

            var parent = _parent;
            Parent = null;
            Parent = parent;
        }

        /// <summary>
        /// Sends the control to the back of its container
        /// </summary>
        public void BringToBack()
        {
            if (Container == null) return;
            var index = Container.Controls.IndexOf(this);
            if (index == 0) return;

            var container = Container;
            Parent = null;
            container.Controls.Insert(0, this);
        }

        /// <summary>
        /// Resizes to.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="anchor">The anchor.</param>
        /// <returns>Point.</returns>
        public Point ResizeTo(Point size, AnchorStyles anchor)
        {
            var oldSize = _size;
            var p = size - oldSize;

            switch (anchor)
            {
                case AnchorStyles.Bottom:
                    ResizeBottom(p.Y);
                    break;
                case AnchorStyles.Right:
                    ResizeRight(p.X);
                    break;
                case AnchorStyles.Top:
                    ResizeTop(-p.Y);
                    break;
                case AnchorStyles.Left:
                    ResizeLeft(-p.X);
                    break;
                case AnchorStyles.Bottom | AnchorStyles.Right:
                    ResizeRight(p.X);
                    ResizeBottom(p.Y);
                    break;
                case AnchorStyles.Bottom | AnchorStyles.Left:
                    ResizeLeft(-p.X);
                    ResizeBottom(p.Y);
                    break;
                case AnchorStyles.Top | AnchorStyles.Right:
                    ResizeRight(p.X);
                    ResizeTop(-p.Y);
                    break;
                case AnchorStyles.Top | AnchorStyles.Left:
                    ResizeLeft(-p.X);
                    ResizeTop(-p.Y);
                    break;
                case AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom:
                    ResizeLeft(-p.X / 2);
                    ResizeTop(-p.Y / 2);
                    ResizeRight(p.X / 2);
                    ResizeBottom(p.Y / 2);
                    break;
            }

            SetBounds();

            if (!Actions.IsUpdating)
            {
                if (Dock != DockStyle.None)
                {
                    if (_parent != null)
                        _parent.PerformLayout();
                }
                else
                    PerformLayout();
            }

            return _size - oldSize;
        }

        /// <summary>
        /// Processes the events.
        /// </summary>
        public void ProcessEvents() { DoEvents(); }

        #region old

        ///// <summary>
        ///// Performs the layout.
        ///// </summary>
        //public void PerformLayout()
        //{
        //    if (!Visible && !Desktop.DesignMode) return;

        //    OnUpdate();
        //    PerformLayoutAndClip();

        //    Point auto = Point.Zero;

        //    for (int i = 0; i < Elements.Count; i++)
        //    {
        //        Control child = Elements[i];

        //        child.PerformUpdate();

        //        if (AutoSize != AutoSize.None && !child.IsRemoved && child.Visible)
        //        {
        //            auto.x = Math.Max(auto.x, child.Position.x + child.Size.x);
        //            auto.y = Math.Max(auto.y, child.Position.y + child.Size.y);

        //            if (i == Elements.Count - 1)
        //            {
        //                if (child.Dock != DockStyle.None)
        //                {
        //                    auto.x += child.Margin.Right;
        //                    auto.y += child.Margin.Bottom;
        //                }
        //            }
        //        }
        //    }

        //    if (this is IControlContainer)
        //    {
        //        auto = Point.Zero;
        //        ControlCollection controls = ((IControlContainer)this).Controls;

        //        controls.SafeRemoval = true;

        //        for (int i = 0; i < controls.Count; i++)
        //        {
        //            Control child = controls[i];
        //            child.PerformUpdate();

        //            if (AutoSize != AutoSize.None && !child.IsRemoved && child.Visible)
        //            {
        //                auto.x = Math.Max(auto.x, child.Position.x + child.Size.x);
        //                auto.y = Math.Max(auto.y, child.Position.y + child.Size.y);

        //                if (i == controls.Count - 1)
        //                {
        //                    if (child.Dock != DockStyle.None)
        //                    {
        //                        auto.x += child.Margin.Right;
        //                        auto.y += child.Margin.Bottom;
        //                    }
        //                }
        //            }
        //        }

        //        controls.SafeRemoval = false;

        //        for (int i = controls.Count - 1; i >= 0; i--)
        //        {
        //            Control c = controls[i];

        //            if (c.IsRemoved)
        //            {
        //                controls.RemoveAt(i);
        //                c.Parent = null;
        //            }
        //        }
        //    }

        //    if (AutoSize != AutoSize.None)
        //    {
        //        if (MinSize.x > 0 && auto.x < MinSize.x) auto.x = MinSize.x;
        //        if (MinSize.y > 0 && auto.y < MinSize.y) auto.y = MinSize.y;

        //        if (AutoSize == AutoSize.Vertical)
        //            _size = new Point(_size.x, auto.y);
        //        else if (AutoSize == AutoSize.Horizontal)
        //            _size = new Point(auto.x, _size.y);
        //        else
        //            _size = new Point(auto.x, auto.y);

        //        OnAutoSize();
        //    }

        //    DetermineState();
        //}

        //internal void PerformUpdate()
        //{
        //    SetDepth();

        //    if (!Visible) return;

        //    if (FadeSpeed > 0 || Gui.GlobalFadeSpeed > 0)
        //    {
        //        float speed = FadeSpeed > 0 ? FadeSpeed : Gui.GlobalFadeSpeed;
        //        float delta = Gui.TimeElapsed / speed;

        //        FadeOut -= delta; FadeIn += delta;

        //        FadeIn = FadeIn < 0 ? 0 : (FadeIn > 1 ? 1 : FadeIn);
        //        FadeOut = FadeOut < 0 ? 0 : (FadeOut > 1 ? 1 : FadeOut);
        //    }

        //    Animation.Update();

        //    PerformLayout();
        //}

        #endregion

        #region new

        private int _localLanguage;

        internal void PerformUpdate()
        {
            SetDepth();

            if (!_visible) return;

            if (FadeSpeed > 0 || Gui.GlobalFadeSpeed > 0)
            {
                var speed = FadeSpeed > 0 ? FadeSpeed : Gui.GlobalFadeSpeed;
                var delta = Gui.TimeElapsed / speed;

                _fadeOut -= delta; _fadeIn += delta;

                _fadeIn = _fadeIn < 0 ? 0 : (_fadeIn > 1 ? 1 : _fadeIn);
                _fadeOut = _fadeOut < 0 ? 0 : (_fadeOut > 1 ? 1 : _fadeOut);
            }


            var elementCount = Elements.Count;
            var controlCount = 0;

            var iContainer = this as IControlContainer;
            if (iContainer != null)
            {
                controlCount = iContainer.Controls.Count;
                iContainer.Controls.IsLocked = true;
            }

            Elements.IsLocked = true;

            Actions.Update(Gui.TimeElapsed);

            if (!IsRemoved)
            {
                if (_localLanguage != Gui.Language)
                {
                    UpdateTranslation();
                    _localLanguage = Gui.Language;
                }

                OnUpdate();

                for (var i = 0; i < elementCount; i++)
                    Elements[i].PerformUpdate();

                for (var i = 0; i < controlCount; i++)
                    iContainer.Controls[i].PerformUpdate();
            }

            Elements.IsLocked = false;
            Elements.Cleanup();

            if (iContainer != null)
            {
                iContainer.Controls.IsLocked = false;
                iContainer.Controls.Cleanup();
            }

            if (!IsRemoved)
                DetermineState();
        }

        /// <summary>
        /// Performs the layout.
        /// </summary>
        public void PerformLayout()
        {
            if (!Visible && !Desktop.DesignMode) return;

            PerformLayoutAndClip();

            var auto = Point.Zero;
            Control child = null;
            var count = Elements.Count;

            for (var i = 0; i < count; i++)
            {
                child = Elements[i];
                child.PerformLayout();

                if (AutoSize != AutoSize.None && !child.IsRemoved && child.Visible)
                {
                    auto.X = Math.Max(auto.X, child.Position.X + child.Size.X + child.Margin.Right);
                    auto.Y = Math.Max(auto.Y, child.Position.Y + child.Size.Y + child.Margin.Bottom);

                    //if (i == count - 1)
                    //{
                    //    if (child.Dock != DockStyle.None)
                    //    {
                    //        auto.x += child.Margin.Right;
                    //        auto.y += child.Margin.Bottom;
                    //    }
                    //}
                }
            }

            if (this is IControlContainer)
            {
                auto = Point.Zero;
                var controls = ((IControlContainer)this).Controls;

                child = null;
                count = controls.Count;

                for (var i = 0; i < count; i++)
                {
                    child = controls[i];
                    child.PerformLayout();

                    if (AutoSize != AutoSize.None && !child.IsRemoved && child.Visible)
                    {
                        auto.X = Math.Max(auto.X, child.Position.X + child.Size.X + child.Margin.Right);
                        auto.Y = Math.Max(auto.Y, child.Position.Y + child.Size.Y + child.Margin.Bottom);

                        //if (i == count - 1)
                        //{
                        //    if (child.Dock != DockStyle.None)
                        //    {
                        //        auto.x += child.Margin.Right;
                        //        auto.y += child.Margin.Bottom;
                        //    }
                        //}
                    }
                }
            }

            if (AutoSize != AutoSize.None)
            {
                if (MinSize.X > 0 && auto.X < MinSize.X) auto.X = MinSize.X;
                if (MinSize.Y > 0 && auto.Y < MinSize.Y) auto.Y = MinSize.Y;

                if (AutoSize == AutoSize.Vertical)
                {
                    _size.Y = auto.Y;
                    //_size = new Point(_size.x, auto.y);
                }
                else if (AutoSize == AutoSize.Horizontal)
                {
                    _size.X = auto.X;
                    //_size = new Point(auto.x, _size.y);
                }
                else
                {
                    _size.X = auto.X;
                    _size.Y = auto.Y;
                    //_size = new Point(auto.x, auto.y);
                }

                OnAutoSize();
            }

            OnLayout();

            //OnLateUpdate();
        }

        #endregion

        // ---------- protected ----------------

        /// <summary>
        /// Gets the opacity.
        /// </summary>
        /// <returns>System.Single.</returns>
        public float GetOpacity()
        {
            var style = Desktop.GetStyle(Style);
            if (style == null) return 1;
            return GetOpacity(style.Styles[_state].Opacity);
        }

        /// <summary>
        /// Gets the opacity.
        /// </summary>
        /// <param name="opacity">The opacity.</param>
        /// <returns>System.Single.</returns>
        protected float GetOpacity(float opacity)
        {
            if (_parent != null)
                return _parent.GetOpacity() * opacity * Opacity;

            return opacity * Opacity;
        }

        /// <summary>
        /// Aligns the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="align">The align.</param>
        /// <param name="padding">The padding.</param>
        /// <param name="font">The font.</param>
        /// <returns>Point.</returns>
        protected Point AlignText(string text, Alignment align, Margin padding, int font)
        {
            var tsize = Gui.Renderer.GetTextSize(text, font);

            switch (align)
            {
                case Alignment.BottomCenter:
                    return Location + new Point((_size.X - tsize.X) / 2, _size.Y - tsize.Y - padding.Bottom);
                case Alignment.BottomLeft:
                    return Location + new Point(padding.Left, _size.Y - tsize.Y - padding.Bottom);
                case Alignment.BottomRight:
                    return Location + new Point(_size.X - tsize.X - padding.Right, _size.Y - tsize.Y - padding.Bottom);
                case Alignment.MiddleCenter:
                    return Location + new Point((_size.X - tsize.X) / 2, (_size.Y - tsize.Y) / 2);
                case Alignment.MiddleLeft:
                    return Location + new Point(padding.Left, (int)Math.Floor((float)(_size.Y - tsize.Y) / 2));
                case Alignment.MiddleRight:
                    return Location + new Point(_size.X - tsize.X - padding.Right, (_size.Y - tsize.Y) / 2);
                case Alignment.TopCenter:
                    return Location + new Point((_size.X - tsize.X) / 2, padding.Top);
                case Alignment.TopLeft:
                    return Location + new Point(padding.Left, padding.Top);
                case Alignment.TopRight:
                    return Location + new Point(_size.X - tsize.X - padding.Right, padding.Top);
                default:
                    return Location;
            }
        }

        /// <summary>
        /// Aligns the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="align">The align.</param>
        /// <param name="padding">The padding.</param>
        /// <param name="font">The font.</param>
        /// <param name="tsize">The tsize.</param>
        /// <returns>Point.</returns>
        protected Point AlignText(string text, Alignment align, Margin padding, int font, out Point tsize)
        {
            tsize = Gui.Renderer.GetTextSize(text, font);

            switch (align)
            {
                case Alignment.BottomCenter:
                    return Location + new Point((_size.X - tsize.X) / 2, _size.Y - tsize.Y - padding.Bottom);
                case Alignment.BottomLeft:
                    return Location + new Point(padding.Left, _size.Y - tsize.Y - padding.Bottom);
                case Alignment.BottomRight:
                    return Location + new Point(_size.X - tsize.X - padding.Right, _size.Y - tsize.Y - padding.Bottom);
                case Alignment.MiddleCenter:
                    return Location + new Point((_size.X - tsize.X) / 2, (_size.Y - tsize.Y) / 2);
                case Alignment.MiddleLeft:
                    return Location + new Point(padding.Left, (_size.Y - tsize.Y) / 2);
                case Alignment.MiddleRight:
                    return Location + new Point(_size.X - tsize.X - padding.Right, (_size.Y - tsize.Y) / 2);
                case Alignment.TopCenter:
                    return Location + new Point((_size.X - tsize.X) / 2, padding.Top);
                case Alignment.TopLeft:
                    return Location + new Point(padding.Left, padding.Top);
                case Alignment.TopRight:
                    return Location + new Point(_size.X - tsize.X - padding.Right, padding.Top);
                default:
                    return Location;
            }
        }

        /// <summary>
        /// Called every frame.
        /// Override this to do per-frame operations
        /// </summary>
        protected virtual void OnUpdate()
        {
            if (Update != null)
                Update(this);
        }

        /// <summary>
        /// Called every frame.
        /// Override this to do per-frame operations
        /// </summary>
        protected virtual void OnLayout()
        {
            if (Layout != null)
                Layout(this);
        }

        /// <summary>
        /// Called late every frame.
        /// Override this in special circumstances to do per-frame operations
        /// </summary>
        protected virtual void OnLateUpdate()
        {
            if (LateUpdate != null)
                LateUpdate(this);
        }

        /// <summary>
        /// Override this method to handle any keyevents passed to the control
        /// </summary>
        protected virtual void OnKeyDown(KeyEventArgs args) { }

        /// <summary>
        /// Override this method to handle any keyevents passed to the control
        /// </summary>
        protected virtual void OnKeyUp(KeyEventArgs args) { }

        /// <summary>
        /// Override this to implement your own AutoSize behavior
        /// </summary>
        protected virtual void OnAutoSize() { }

        /// <summary>
        /// Override this to draw text.
        /// </summary>
        /// <param name="style">The style to be used</param>
        /// <param name="opacity">The opacity to be used</param>
        protected virtual void DrawText(Style style, float opacity) { }

        /// <summary>
        /// Override this to do additional drawing before the control has been drawn
        /// </summary>
        protected virtual void DrawBefore() { }

        /// <summary>
        /// Override this to do additional drawing after the control has been drawn
        /// </summary>
        protected virtual void DrawCustom() { }

        /// <summary>
        /// Override this to handle state changes.
        /// This is useful to sync states between controls.
        /// </summary>
        protected virtual void OnStateChanged() { }

        /// <summary>
        /// Override this to do additional drawing
        /// Call the base method to draw the control as usual
        /// </summary>
        /// <param name="style"></param>
        /// <param name="opacity"></param>
        protected virtual void DrawStyle(Style style, float opacity)
        {
            if (opacity == 0) return;

            var loc = Location;

            var blend = style.BackColor;
            if (Tint != -1)
            {
                blend = ColorInt.Blend(Tint, style.BackColor);
            }

            if (blend != 0)
                Gui.Renderer.DrawBox(loc.X, loc.Y, _size.X, _size.Y, ColorInt.FromArgb(opacity, blend));

            if (!string.IsNullOrEmpty(style.Texture))
            {
                var texture = Gui.Renderer.GetTexture(style.Texture);

                if (texture > -1)
                {
                    blend = style.Tint;

                    if (Tint != -1)
                    {
                        //blend = ColorInt.Overlay(Tint, style.Tint);
                        //blend = ColorInt.Multiply(Tint, style.Tint);
                        //blend = ColorInt.Screen(style.Tint, Tint);
                        blend = ColorInt.Blend(Tint, style.Tint);
                    }

                    if (style.TextureRect.IsEmpty())
                    {
                        var texsize = Gui.Renderer.GetTextureSize(texture);
                        style.TextureRect = new Rectangle(Point.Zero, texsize);
                    }

                    var color = ColorInt.FromArgb(opacity, blend);

                    if (style.Tiling == TextureMode.Grid || style.Tiling == TextureMode.GridRepeat)
                    {
                        SliceTexture(texture, style.Tiling, style.TextureRect, style.Grid, opacity, color);
                    }
                    else if (style.Tiling == TextureMode.Stretch)
                    {
                        Gui.Renderer.DrawTexture(texture, loc.X, loc.Y, _size.X, _size.Y, style.TextureRect, color);
                    }
                    else if (style.Tiling == TextureMode.Center)
                    {
                        var center = loc + _size / 2;
                        var rectsize = new Point(style.TextureRect.Width, style.TextureRect.Height);
                        var pos = center - rectsize / 2;

                        Gui.Renderer.DrawTexture(texture, pos.X, pos.Y, rectsize.X, rectsize.Y, style.TextureRect, color);
                    }
                    else
                    {
                        RepeatTexture(texture, Location, style.TextureRect, style.Tiling, opacity, color);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the scissor rectangle
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected void SetScissor(int x, int y, int width, int height)
        {
            var r = new Rectangle(x, y, width, height);

            if (_scissorStack.ContainsKey(Depth))
                _scissorStack[Depth] = new KeyValuePair<Control, Rectangle>(this, r);
            else
                _scissorStack.Add(Depth, new KeyValuePair<Control, Rectangle>(this, r));

            Gui.Renderer.EndBatch(false);
            Gui.Renderer.Scissor(x, y, r.Width, r.Height);
            Gui.Renderer.StartBatch();
        }

        /// <summary>
        /// Resets the scissor test to whatever is was before the last SetScissor call
        /// </summary>
        protected void ResetScissor()
        {
            var r = Desktop.ClipRect;

            for (var i = Depth - 1; i >= 0; i--)
            {
                if (_scissorStack.ContainsKey(i))
                {
                    var pair = _scissorStack[i];
                    if (this.IsChildOf(pair.Key))
                    {
                        r = pair.Value;
                        break;
                    }
                }
            }

            Gui.Renderer.EndBatch(false);
            Gui.Renderer.Scissor(r.Left, r.Top, r.Width, r.Height);
            Gui.Renderer.StartBatch();
        }

        /// <summary>
        /// Clips the specified rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <returns>Rectangle.</returns>
        protected Rectangle Clip(Rectangle rect)
        {
            if (_parent != null)
                return _parent.ClipRect.Clip(rect);

            return rect;
        }



        // ---------- private ----------------

        private void ResizeLeft(int value)
        {
            var p = _position;
            var s = _size;

            s.X -= value;

            if (MinSize.X >= 0 && MaxSize.X > 0)
                s.X = Math.Min(s.X, MaxSize.X);

            s.X = Math.Max(s.X, MinSize.X);

            p.X += (_size.X - s.X);

            if (p.X < 0)
            {
                s.X += p.X;
                p.X = 0;
            }

            _position = p;
            _size = s;
        }

        private void ResizeRight(int value)
        {
            var s = _size;

            s.X += value;

            if (MinSize.X >= 0 && MaxSize.X > 0)
                s.X = Math.Min(s.X, MaxSize.X);

            s.X = Math.Max(s.X, MinSize.X);

            if (_parent != null && _parent.AutoSize != AutoSize.Horizontal && _parent.AutoSize != AutoSize.HorizontalVertical)
            {
                if (_position.X + s.X > _parent.Size.X)
                    s.X = _parent.Size.X - _position.X;
            }

            _size = s;
        }

        private void ResizeBottom(int value)
        {
            var s = _size;

            s.Y += value;

            if (MinSize.Y >= 0 && MaxSize.Y > 0)
                s.Y = Math.Min(s.Y, MaxSize.Y);

            s.Y = Math.Max(s.Y, MinSize.Y);

            if (_parent != null && _parent.AutoSize != AutoSize.Vertical && _parent.AutoSize != AutoSize.HorizontalVertical)
            {
                if (_position.Y + s.Y > _parent.Size.Y)
                    s.Y = _parent.Size.Y - _position.Y;
            }

            _size = s;
        }

        private void ResizeTop(int value)
        {
            var p = _position;
            var s = _size;

            s.Y -= value;

            if (MinSize.Y >= 0 && MaxSize.Y > 0)
                s.Y = Math.Min(s.Y, MaxSize.Y);

            s.Y = Math.Max(s.Y, MinSize.Y);

            p.Y += (_size.Y - s.Y);

            if (p.Y < 0)
            {
                s.Y += p.Y;
                p.Y = 0;
            }

            _position = p;
            _size = s;
        }

        private void SetBounds()
        {
            if (_parent == null) return;

            Bounds.Left = _position.X;
            Bounds.Top = _position.Y;
            Bounds.Right = _parent.Size.X - (_position.X + _size.X);
            Bounds.Bottom = _parent.Size.Y - (_position.Y + _size.Y);
        }

        protected void PerformLayoutAndClip()
        {
            var s = _size;

            if (Dock != DockStyle.None)
                LayoutDock();
            else
                LayoutAnchor();

            SetDockRegions();

            ClipRect = new Rectangle(Location, _size);

            if (_parent != null)
                ClipRect = _parent.ClipRect.Clip(ClipRect);

            if (_size.X != s.X || _size.Y != s.Y)
            {
                if (SizeChanged != null)
                    SizeChanged(this);
            }
        }

        private void SetDockRegions()
        {
            DockAreaC.Top = _padding.Top;
            DockAreaC.Left = _padding.Left;
            DockAreaC.Right = _size.X - _padding.Right;
            DockAreaC.Bottom = _size.Y - _padding.Bottom;

            DockAreaE.Top = 0;
            DockAreaE.Left = 0;
            DockAreaE.Right = _size.X;
            DockAreaE.Bottom = _size.Y;
        }

        private void LayoutDock()
        {
            var rect = _isElement ? _parent.DockAreaE : _parent.DockAreaC;

            if (Dock == DockStyle.Bottom)
            {
                var bottom = rect.Bottom - _margin.Bottom;
                var left = rect.Left + _margin.Left;
                var right = rect.Right - _margin.Right;

                _position.X = left;
                _position.Y = bottom - _size.Y;// -_margin.Top;

                _size.X = right - left;
                rect.Bottom = _position.Y - _margin.Top;
            }
            else if (Dock == DockStyle.Fill)
            {
                var bottom = rect.Bottom - _margin.Bottom;
                var left = rect.Left + _margin.Left;
                var right = rect.Right - _margin.Right;
                var top = rect.Top + _margin.Top;

                _position.X = left;
                _position.Y = top;

                _size.X = right - left;
                _size.Y = bottom - top;
            }
            else if (Dock == DockStyle.Left)
            {
                var left = rect.Left + _margin.Left;
                var top = rect.Top + _margin.Top;
                var bottom = rect.Bottom - _margin.Bottom;

                _position.X = left;
                _position.Y = top;

                _size.Y = bottom - top;
                rect.Left = left + _size.X + _margin.Right;
            }
            else if (Dock == DockStyle.Right)
            {
                var right = rect.Right - _margin.Right;
                var top = rect.Top + _margin.Top;
                var bottom = rect.Bottom - _margin.Bottom;

                _position.X = right - _size.X;
                _position.Y = top;

                _size.Y = bottom - top;
                rect.Right = _position.X - _margin.Left;
            }
            else if (Dock == DockStyle.Top)
            {
                var top = rect.Top + _margin.Top;
                var left = rect.Left + _margin.Left;
                var right = rect.Right - _margin.Right;

                _position.X = left;
                _position.Y = top;

                _size.X = right - left;
                rect.Top = top + _size.Y + _margin.Bottom;
            }
            else if (Dock == DockStyle.CenterY)
            {
                var left = rect.Left + _margin.Left;
                var right = rect.Right - _margin.Right;

                _position.X = left;

                _size.X = right - left;
                _position.Y = (_parent.Size.Y - _size.Y) / 2;
            }
            else if (Dock == DockStyle.CenterX)
            {
                var top = rect.Top + _margin.Top;
                var bottom = rect.Bottom - _margin.Bottom;

                _position.Y = top;

                _size.Y = bottom - top;
                _position.X = (_parent.Size.X - _size.X) / 2;
            }
            else if (Dock == DockStyle.Center)
            {
                _position = (_parent.Size - _size) / 2;
            }
            else if (Dock == DockStyle.FillY)
            {
                var bottom = rect.Bottom - _margin.Bottom;
                var top = rect.Top + _margin.Top;

                _position.Y = top;
                _size.Y = bottom - top;
            }
            else if (Dock == DockStyle.FillX)
            {
                var left = rect.Left + _margin.Left;
                var right = rect.Right - _margin.Right;

                _position.X = left;
                _size.X = right - left;
            }

            if (_isElement)
                _parent.DockAreaE = rect;
            else
                _parent.DockAreaC = rect;

            SetBounds();
        }

        private void LayoutAnchor()
        {
            switch (_anchor)
            {
                case AnchorStyles.Top:
                    _position.X = _parent.Size.X - Bounds.Right - _size.X;
                    break;
                case AnchorStyles.Top | AnchorStyles.Left:
                    // position unchanged
                    break;
                case AnchorStyles.Top | AnchorStyles.Right:
                    _position.X = _parent.Size.X - Bounds.Right - _size.X;
                    break;
                case AnchorStyles.Top | AnchorStyles.Bottom:
                    _size.Y = (_parent.Size.Y - Bounds.Bottom) - _position.Y;
                    break;
                case AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left:
                    _size.Y = (_parent.Size.Y - Bounds.Bottom) - _position.Y;
                    break;
                case AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right:
                    _size.Y = (_parent.Size.Y - Bounds.Bottom) - _position.Y;
                    _position.X = _parent.Size.X - Bounds.Right - _size.X;
                    break;
                case AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right:
                    _size.X = (_parent.Size.X - Bounds.Right) - _position.X;
                    break;
                case AnchorStyles.Left | AnchorStyles.Right:
                    _size.X = (_parent.Size.X - Bounds.Right) - _position.X;
                    break;
                case AnchorStyles.Bottom | AnchorStyles.Left:
                    _position.Y = _parent.Size.Y - Bounds.Bottom - _size.Y;
                    break;
                case AnchorStyles.Bottom | AnchorStyles.Right:
                    _position.X = _parent.Size.X - Bounds.Right - _size.X;
                    _position.Y = _parent.Size.Y - Bounds.Bottom - _size.Y;
                    break;
                case AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right:
                    _size.X = (_parent.Size.X - Bounds.Right) - _position.X;
                    _position.Y = _parent.Size.Y - Bounds.Bottom - _size.Y;
                    break;
                case AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right:
                    _size.X = (_parent.Size.X - Bounds.Right) - _position.X;
                    _size.Y = (_parent.Size.Y - Bounds.Bottom) - _position.Y;
                    break;
            }
        }

        private void DetermineState()
        {
            if (_stateWasSet)
            {
                _stateWasSet = false;
                return;
            }

            if (NoEvents) return;
            if (Desktop == null) return;

            // if (root.DesignMode) return;

            if (this is ICheckable && (this as ICheckable).Checked)
            {
                if (!Enabled)
                    State = ControlState.CheckedDisabled;
                else if (Desktop.FocusedControl == this)
                    State = ControlState.CheckedFocused;
                else if (Desktop.PressedControl == this)
                    State = ControlState.CheckedPressed;
                else if (Desktop.HotControl == this)
                    State = ControlState.CheckedHot;
                else
                    State = ControlState.Checked;
            }
            else if (this is ISelectable && (this as ISelectable).Selected)
            {
                if (!Enabled)
                    State = ControlState.SelectedDisabled;
                else if (Desktop.FocusedControl == this)
                    State = ControlState.SelectedFocused;
                else if (Desktop.PressedControl == this)
                    State = ControlState.SelectedPressed;
                else if (Desktop.HotControl == this)
                    State = ControlState.SelectedHot;
                else
                    State = ControlState.Selected;
            }
            else if (!Enabled)
                State = ControlState.Disabled;
            else if (Desktop.FocusedControl == this)
                State = ControlState.Focused;
            else if (Desktop.PressedControl == this)
                State = ControlState.Pressed;
            else if (Desktop.HotControl == this)
                State = ControlState.Hot;
            else
                State = ControlState.Default;

            _stateWasSet = false;
        }

        private void DrawChildren()
        {
            if (this is IControlContainer)
            {
                var ccont = this as IControlContainer;
                //ccont.Controls.Cleanup();

                for (var i = 0; i < ccont.Controls.Count; i++)
                    ccont.Controls[i].Draw();
            }
        }

        private void DrawElements()
        {
            //Elements.Cleanup();

            for (var i = 0; i < Elements.Count; i++)
                Elements[i].Draw();
        }

        protected void RepeatTexture(int texture, Point loc, Rectangle rect, TextureMode mode, float opacity, int color)
        {
            var texsize = Gui.Renderer.GetTextureSize(texture);

            var width = rect.Width != 0 ? rect.Width : texsize.X;
            var height = rect.Height != 0 ? rect.Height : texsize.Y;

            var countx = (int)Math.Ceiling((float)_size.X / width);
            var county = (int)Math.Ceiling((float)_size.Y / height);

            if (mode == TextureMode.RepeatX) county = 1;
            else if (mode == TextureMode.RepeatY) countx = 1;

            for (var j = 0; j < county; j++)
            {
                for (var i = 0; i < countx; i++)
                {
                    var newrect = rect;

                    var deltax = (width + width * i) - _size.X;
                    var deltay = (height + height * j) - _size.Y;

                    var clippedx = width;
                    var clippedy = height;

                    if (deltax > 0)
                    {
                        newrect.Right = rect.Right - deltax;
                        clippedx = width - deltax;
                    }

                    if (deltay > 0)
                    {
                        newrect.Bottom = rect.Bottom - deltay;
                        clippedy = height - deltay;
                    }

                    Gui.Renderer.DrawTexture(texture, loc.X + width * i, loc.Y + height * j, clippedx, clippedy, newrect, color);
                }
            }
        }

        protected void SliceTexture(int texture, TextureMode mode, Rectangle rect, Margin grid, float opacity, int color)
        {
            var repeat = mode == TextureMode.GridRepeat;

            var location = Location;
            var outside = new Rectangle(location, _size);
            var inside = new Rectangle(location + new Point(grid.Left, grid.Top), _size - new Point(grid.Left + grid.Right, grid.Top + grid.Bottom));

            var slice = new Rectangle();

            var x1 = rect.Left + grid.Left;
            var y1 = rect.Top + grid.Top;
            var x2 = rect.Right - grid.Right;
            var y2 = rect.Bottom - grid.Bottom;

            if (grid.Top > 0 && grid.Left > 0)
            {
                //// draw top left
                slice.Left = rect.Left;
                slice.Top = rect.Top;
                slice.Right = x1;
                slice.Bottom = y1;

                Gui.Renderer.DrawTexture(texture, outside.Left, outside.Top, grid.Left, grid.Top, slice, color);
            }

            if (grid.Top > 0 && grid.Right > 0)
            {
                //// draw top right
                slice.Left = x2;
                slice.Top = rect.Top;
                slice.Right = rect.Right;
                slice.Bottom = y1;

                Gui.Renderer.DrawTexture(texture, inside.Right, outside.Top, grid.Right, grid.Top, slice, color);
            }

            if (grid.Bottom > 0 && grid.Left > 0)
            {
                //// draw bottom left
                slice.Left = rect.Left;
                slice.Top = y2;
                slice.Right = x1;
                slice.Bottom = rect.Bottom;

                Gui.Renderer.DrawTexture(texture, outside.Left, inside.Bottom, grid.Left, grid.Bottom, slice, color);
            }

            if (grid.Bottom > 0 && grid.Right > 0)
            {
                //// draw bottom right
                slice.Left = x2;
                slice.Top = y2;
                slice.Right = rect.Right;
                slice.Bottom = rect.Bottom;

                Gui.Renderer.DrawTexture(texture, inside.Right, inside.Bottom, grid.Right, grid.Bottom, slice, color);
            }

            if (grid.Left > 0)
            {
                // draw left
                slice.Left = rect.Left;
                slice.Top = y1;
                slice.Right = x1;
                slice.Bottom = y2;

                if (!repeat)
                {
                    Gui.Renderer.DrawTexture(texture, outside.Left, inside.Top, grid.Left, inside.Height, slice, color);
                }
                else
                {
                    var sliceSize = rect.Height - (grid.Top + grid.Bottom);
                    var count = (int)Math.Ceiling((float)inside.Height / sliceSize);

                    if (inside.Height < sliceSize)
                    {
                        slice.Bottom = y2 - (sliceSize - inside.Height);
                        sliceSize = inside.Height;
                    }

                    var h = 0;
                    for (var i = 0; i < count; i++)
                    {
                        h += sliceSize;

                        if (h > inside.Height)
                        {
                            var delta = h - inside.Height;

                            slice.Bottom = y2 - delta;
                            var clipped = sliceSize - delta;

                            Gui.Renderer.DrawTexture(texture, outside.Left, inside.Top + sliceSize * i, grid.Left, clipped, slice, color);
                        }
                        else
                        {
                            Gui.Renderer.DrawTexture(texture, outside.Left, inside.Top + sliceSize * i, grid.Left, sliceSize, slice, color);
                        }
                    }
                }
            }

            if (grid.Top > 0)
            {
                // draw top
                slice.Left = x1;
                slice.Top = rect.Top;
                slice.Right = x2;
                slice.Bottom = y1;

                if (!repeat)
                {
                    Gui.Renderer.DrawTexture(texture, inside.Left, outside.Top, inside.Width, grid.Top, slice, color);
                }
                else
                {
                    var sliceSize = rect.Width - (grid.Left + grid.Right);
                    var count = (int)Math.Ceiling((float)inside.Width / sliceSize);

                    if (inside.Width < sliceSize)
                    {
                        slice.Right = x2 - (sliceSize - inside.Width);
                        sliceSize = inside.Width;
                    }

                    var w = 0;
                    for (var i = 0; i < count; i++)
                    {
                        w += sliceSize;

                        if (w > inside.Width)
                        {
                            var delta = w - inside.Width;

                            slice.Right = x2 - delta;
                            var clipped = sliceSize - delta;

                            Gui.Renderer.DrawTexture(texture, inside.Left + sliceSize * i, outside.Top, clipped, grid.Top, slice, color);
                        }
                        else
                        {
                            Gui.Renderer.DrawTexture(texture, inside.Left + sliceSize * i, outside.Top, sliceSize, grid.Top, slice, color);
                        }
                    }
                }
            }

            if (grid.Right > 0)
            {
                // draw right
                slice.Left = x2;
                slice.Top = y1;
                slice.Right = rect.Right;
                slice.Bottom = y2;

                if (!repeat)
                {
                    Gui.Renderer.DrawTexture(texture, inside.Right, inside.Top, grid.Right, inside.Height, slice, color);
                }
                else
                {
                    var sliceSize = rect.Height - (grid.Top + grid.Bottom);
                    var count = (int)Math.Ceiling((float)inside.Height / sliceSize);

                    if (inside.Height < sliceSize)
                    {
                        slice.Bottom = y2 - (sliceSize - inside.Height);
                        sliceSize = inside.Height;
                    }

                    var h = 0;
                    for (var i = 0; i < count; i++)
                    {
                        h += sliceSize;

                        if (h > inside.Height)
                        {
                            var delta = h - inside.Height;

                            slice.Bottom = y2 - delta;
                            var clipped = sliceSize - delta;

                            Gui.Renderer.DrawTexture(texture, inside.Right, inside.Top + sliceSize * i, grid.Right, clipped, slice, color);
                        }
                        else
                        {
                            Gui.Renderer.DrawTexture(texture, inside.Right, inside.Top + sliceSize * i, grid.Right, sliceSize, slice, color);
                        }
                    }
                }
            }

            if (grid.Bottom > 0)
            {
                // draw bottom
                slice.Left = x1;
                slice.Top = y2;
                slice.Right = x2;
                slice.Bottom = rect.Bottom;

                if (!repeat)
                {
                    Gui.Renderer.DrawTexture(texture, inside.Left, inside.Bottom, inside.Width, grid.Bottom, slice, color);
                }
                else
                {
                    var sliceSize = rect.Width - (grid.Left + grid.Right);
                    var count = (int)Math.Ceiling((float)inside.Width / sliceSize);

                    if (inside.Width < sliceSize)
                    {
                        slice.Right = x2 - (sliceSize - inside.Width);
                        sliceSize = inside.Width;
                    }

                    var w = 0;
                    for (var i = 0; i < count; i++)
                    {
                        w += sliceSize;

                        if (w > inside.Width)
                        {
                            var delta = w - inside.Width;

                            slice.Right = x2 - delta;
                            var clipped = sliceSize - delta;

                            Gui.Renderer.DrawTexture(texture, inside.Left + sliceSize * i, inside.Bottom, clipped, grid.Bottom, slice, color);
                        }
                        else
                        {
                            Gui.Renderer.DrawTexture(texture, inside.Left + sliceSize * i, inside.Bottom, sliceSize, grid.Bottom, slice, color);
                        }
                    }
                }
            }

            // draw center
            slice.Left = x1;
            slice.Top = y1;
            slice.Right = x2;
            slice.Bottom = y2;

            Gui.Renderer.DrawTexture(texture, inside.Left, inside.Top, inside.Width, inside.Height, slice, color);
        }

        private void SetEnabled(bool value)
        {
            if (_parentEnabled == value)
                return;

            _parentEnabled = value;

            if (this is IControlContainer)
            {
                foreach (var control in ((IControlContainer)this).Controls)
                    control.SetEnabled(value);
            }

            foreach (var control in Elements)
                control.SetEnabled(value);
        }


        // ---------- internal ----------------

        internal void PerformLateUpdate()
        {
            //if (IsRemoved) return;

            OnLateUpdate();

            for (var i = 0; i < Elements.Count; i++)
                Elements[i].PerformLateUpdate();

            if (this is IControlContainer)
            {
                var ccont = this as IControlContainer;
                for (var i = 0; i < ccont.Controls.Count; i++)
                    ccont.Controls[i].PerformLateUpdate();
            }
        }

        internal void SetDepth()
        {
            if (_parent != null)
                Depth = _parent.Depth + 1;
        }

        internal void Draw()
        {
            if (_visible)
            {
                if (_size.X <= 0 || _size.Y <= 0)
                    return;

                if (ClipRect.Width <= 0 || ClipRect.Height <= 0)
                    return;

                DrawBefore();

                if (Scissor || Gui.AlwaysScissor)
                    SetScissor(Math.Max(0, ClipRect.Left), Math.Max(0, ClipRect.Top), ClipRect.Width, ClipRect.Height);

                if (FadeSpeed > 0 || Gui.GlobalFadeSpeed > 0)
                {
                    var next = Desktop.GetStyle(Style).Styles[_state];
                    var opacity = GetOpacity(next.Opacity);

                    if (_oldState != _state && _fadeIn < 1 && (_textureFade || _fontFade))
                    {
                        var last = Desktop.GetStyle(Style).Styles[_oldState];

                        var a1 = GetOpacity(last.Opacity) * _fadeOut;
                        var a2 = GetOpacity(next.Opacity) * _fadeIn;

                        if (_textureFade)
                        {
                            if (_fadeOut > 0) DrawStyle(last, a1);
                            DrawStyle(next, a2);
                        }
                        else
                            DrawStyle(next, opacity);

                        if (_fontFade)
                        {
                            if (_fadeOut > 0) DrawText(last, a1);
                            DrawText(next, a2);
                        }
                        else
                            DrawText(next, opacity);
                    }
                    else
                    {
                        DrawStyle(next, opacity);
                        DrawText(next, opacity);
                    }
                }
                else
                {
                    var style = Desktop.GetStyle(Style).Styles[_state];
                    var opacity = GetOpacity(style.Opacity);

                    DrawStyle(style, opacity);
                    DrawText(style, opacity);
                }

                DrawChildren();
                DrawElements();

                if (Scissor || Gui.AlwaysScissor)
                    ResetScissor();

                DrawCustom();
            }

            if (Depth == 1 && _scissorStack.Count > 0)
                _scissorStack.Clear();
        }

        internal void DoKeyEvents()
        {
            if (NoEvents) return;

            if (Gui.NumKeyEvents > 0)
            {
                foreach (var data in Gui.KeyBuffer)
                {
                    //   if (data.Scancode == 0) continue;

                    // UnityEngine.Debug.Log(data.Scancode);

                    var args = new KeyEventArgs(data);

                    if (data.Pressed)
                    {
                        if (KeyDown != null)
                        {
                            KeyDown(this, args);

                            if (!args.Cancel)
                                OnKeyDown(args);
                        }
                        else
                            OnKeyDown(args);
                    }

                    if (data.Released)
                    {
                        if (KeyUp != null)
                        {
                            KeyUp(this, args);

                            if (!args.Cancel)
                                OnKeyUp(args);
                        }
                        else
                            OnKeyUp(args);
                    }
                }
            }
        }

        internal void DoEvents()
        {
            if (NoEvents) return;
            if (Desktop == null) return;

            if (Gui.MouseScroll != 0)
                OnMouseWheel();

            for (var i = 0; i < Gui.Buttons.Length; i++)
            {
                if (Gui.GetButton(i) == ButtonState.Down)
                {
                    _isMouseDrag = false;
                    OnMouseDown(i);
                    return;
                }
                else if (Gui.GetButton(i) == ButtonState.Press)
                {
                    OnMousePress(i);

                    if (!Gui.MouseMovement.IsEmpty && !_isMouseDrag)
                    {
                        _isMouseDrag = true;
                        OnMouseDrag(i);
                    }

                    return;
                }
                else if (Gui.GetButton(i) == ButtonState.Up)
                {
                    OnMouseRelease(i);
                    return;
                }
                else if (i == 0)
                {
                    //if (Desktop != null)
                    //{
                    //    Desktop.PressedControl = null;
                    //    Desktop.MouseDownControl = null;
                    //}
                }
            }

            if (Desktop.MouseDownControl != null)
            {
                Desktop.PressedControl = null;
                Desktop.MouseDownControl = null;
            }
        }

        internal Control PickDeep(int x, int y)
        {
            if (!Visible) return null;
            if (!Hit(x, y)) return null;

            Control found = null;

            if (this is IControlContainer)
            {
                found = this;

                var container = this as IControlContainer;
                for (var i = container.Controls.Count - 1; i >= 0; i--)
                {
                    var child = container.Controls[i].PickDeep(x, y);

                    if (child != null && child.Visible)
                    {
                        found = child;
                        break;
                    }
                }
            }

            for (var i = Elements.Count - 1; i >= 0; i--)
            {
                var child = Elements[i].PickDeep(x, y);

                if (child != null && child.Visible)
                {
                    found = child;
                    break;
                }
            }

            return found;
        }

        internal Control PickFirst(int x, int y)
        {
            if (!Visible) return null;
            if (!Hit(x, y)) return null;

            var found = _isElement ? null : this;

            if (this is IControlContainer)
            {
                var container = this as IControlContainer;
                for (var i = container.Controls.Count - 1; i >= 0; i--)
                {
                    var child = container.Controls[i].PickFirst(x, y);

                    if (child != null && child.Visible && !child._isElement)
                    {
                        found = child;
                        break;
                    }
                }
            }

            for (var i = Elements.Count - 1; i >= 0; i--)
            {
                var child = Elements[i].PickFirst(x, y);

                if (child != null && child.Visible && !(child is IControlContainer))
                {
                    found = child;
                    break;
                }
            }

            return found;
        }

        internal Control GetDropTarget(Control sender)
        {
            if (!Visible) return null;

            var x = Gui.MousePosition.X;
            var y = Gui.MousePosition.Y;

            if (!Hit(x, y)) return null;

            var found = Enabled && Visible && AllowDrop ? this : null;

            if (this is IControlContainer)
            {
                var container = this as IControlContainer;
                for (var i = container.Controls.Count - 1; i >= 0; i--)
                {
                    var child = container.Controls[i].GetDropTarget(sender);

                    if (child != null && sender != child && child.Enabled && child.Visible && !child.NoEvents && child.AllowDrop)
                    {
                        found = child;
                        break;
                    }
                }
            }

            for (var i = Elements.Count - 1; i >= 0; i--)
            {
                var child = Elements[i].GetDropTarget(sender);

                if (child != null && sender != child && child.Enabled && child.Visible && !child.NoEvents && child.AllowDrop)
                {
                    found = child;
                    break;
                }
            }

            return found;
        }

        internal Control FindTabIndex(int index)
        {
            Control control = null;
            IList<Control> controls = Elements;

            if (!Visible || Desktop.CheckModalLock(this))
                return null;

            if (!NoEvents && Enabled && TabIndex == index)
                control = this;

            for (var i = 0; i < controls.Count; i++)
            {
                var child = controls[i].FindTabIndex(index);

                if (child != null && !child.NoEvents && child.Visible && child.Enabled && child.TabIndex == index)
                {
                    control = child;
                    break;
                }
            }

            if (this is IControlContainer)
            {
                controls = ((IControlContainer)this).Controls;

                for (var i = 0; i < controls.Count; i++)
                {
                    var child = controls[i].FindTabIndex(index);

                    if (child != null && !child.NoEvents && child.Visible && child.Enabled && child.TabIndex == index)
                    {
                        control = child;
                        break;
                    }
                }
            }

            return control;
        }

        internal int FindHighestTabIndex(int max)
        {
            var index = max;
            IList<Control> all = Elements;

            if (!Visible)
                return index;

            if (!NoEvents && Enabled && TabIndex == index)
                index = TabIndex;

            for (var i = 0; i < all.Count; i++)
            {
                var result = all[i].FindHighestTabIndex(index);

                if (result > index)
                    index = result;
            }

            if (this is IControlContainer)
            {
                all = ((IControlContainer)this).Controls;

                for (var i = 0; i < all.Count; i++)
                {
                    var result = all[i].FindHighestTabIndex(index);

                    if (result > index)
                        index = result;
                }
            }

            return index;
        }

        /// <summary>
        /// Raises the <see cref="E:DragEnter" /> event.
        /// </summary>
        /// <param name="e">The <see cref="DragDropEventArgs"/> instance containing the event data.</param>
        internal void OnDragEnter(DragDropEventArgs e)
        {
            if (e.Cancel) return;

            if (DragEnter != null)
                DragEnter(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:DragLeave" /> event.
        /// </summary>
        /// <param name="e">The <see cref="DragDropEventArgs"/> instance containing the event data.</param>
        internal void OnDragLeave(DragDropEventArgs e)
        {
            if (e.Cancel) return;

            if (DragLeave != null)
                DragLeave(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:DragResponse" /> event.
        /// </summary>
        /// <param name="e">The <see cref="DragDropEventArgs"/> instance containing the event data.</param>
        internal void OnDragResponse(DragDropEventArgs e)
        {
            if (e.Cancel) return;

            if (DragResponse != null)
                DragResponse(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:DragDrop" /> event.
        /// </summary>
        /// <param name="e">The <see cref="DragDropEventArgs"/> instance containing the event data.</param>
        internal void OnDragDrop(DragDropEventArgs e)
        {
            if (DragDrop != null)
                DragDrop(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:Dropped" /> event.
        /// </summary>
        /// <param name="e">The <see cref="DragDropEventArgs"/> instance containing the event data.</param>
        internal void OnDrop(DragDropEventArgs e)
        {
            if (Drop != null)
                Drop(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:MouseDoubleClick" /> event.
        /// </summary>
        /// <param name="button">The button.</param>
        internal void OnMouseDoubleClick(int button)
        {
            if (MouseDoubleClick != null)
                MouseDoubleClick(this, new MouseEventArgs { Button = button });
        }

        /// <summary>
        /// Raised the MouseDrag event
        /// </summary>
        /// <param name="button">The button.</param>
        internal void OnMouseDrag(int button)
        {
            if (MouseDrag != null)
                MouseDrag(this, new MouseEventArgs { Button = button });
        }

        /// <summary>
        /// Raised the MousePress event
        /// </summary>
        /// <param name="button">The button.</param>
        internal void OnMousePress(int button)
        {
            if (Desktop == null) return;

            //if(button == 0)
            Desktop.PressedControl = this;
            Desktop.MouseDownControl = this;

            if (MousePress != null)
                MousePress(this, new MouseEventArgs { Button = button });
        }

        /// <summary>
        /// Raised the MouseClick event
        /// </summary>
        /// <param name="button">The button.</param>
        internal void OnMouseClick(int button)
        {
            if (MouseClick != null)
                MouseClick(this, new MouseEventArgs { Button = button });
        }

        /// <summary>
        /// Raised the MouseRelease event
        /// </summary>
        /// <param name="button"></param>
        internal void OnMouseRelease(int button)
        {
            if (Desktop == null) return;

            if (Desktop.MouseDownControl != this) return;

            OnMouseClick(button);

            if (_isDoubleClick)
            {
                _isDoubleClick = false;
                OnMouseDoubleClick(button);
            }
        }

        /// <summary>
        /// Raised the MouseDown event
        /// </summary>
        /// <param name="button"></param>
        internal void OnMouseDown(int button)
        {
            if (Desktop == null) return;

            //if (button == 0)
            Desktop.PressedControl = this;
            Desktop.MouseDownControl = this;

            var now = DateTime.Now;
            var delta = now.Subtract(_timeClicked);
            _timeClicked = now;
            _isDoubleClick = delta.TotalMilliseconds < Gui.DoubleClickSpeed;

            if (MouseDown != null)
                MouseDown(this, new MouseEventArgs { Button = button });
        }

        /// <summary>
        /// Raised the MouseEnter event
        /// </summary>
        internal void OnMouseEnter()
        {
            if (MouseEnter != null)
                MouseEnter(this);
        }

        /// <summary>
        /// Raised the MouseLeave event
        /// </summary>
        internal void OnMouseLeave()
        {
            if (MouseLeave != null)
                MouseLeave(this);
        }

        /// <summary>
        /// Raised the MouseUp event
        /// </summary>
        /// <param name="button"></param>
        internal void OnMouseUp(int button)
        {
            if (MouseUp != null)
                MouseUp(this, new MouseEventArgs { Button = button });
        }

        internal void OnMouseWheel()
        {
            if (MouseWheel != null)
            {
                var args = new MouseEventArgs();
                MouseWheel(this, args);
                if (args.Cancel) return;
            }

            if (_parent != null)
                _parent.OnMouseWheel();
        }

        /// <summary>
        /// Raised the GotFocus event
        /// </summary>
        internal void OnGotFocus()
        {
            if (GotFocus != null)
                GotFocus(this);
        }

        /// <summary>
        /// Raised the LostFocus event
        /// </summary>
        internal void OnLostFocus()
        {
            if (LostFocus != null)
                LostFocus(this);
        }


        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var control = obj as Control;
            if (control == null) return false;
            return control.AutoId == AutoId;
        }
    }
}
