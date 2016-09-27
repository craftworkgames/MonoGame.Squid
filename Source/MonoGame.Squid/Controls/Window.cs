using System;
using System.ComponentModel;
using MonoGame.Squid.Interfaces;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// A Window
    /// </summary>
    [Toolbox]
    public class Window : Control, IControlContainer
    {
        private readonly Resizer _sizer;
        private Point _clickedPos;
        private bool _isDragging;

        [Hidden]
        public ControlCollection Controls { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Window"/> is modal.
        /// </summary>
        /// <value><c>true</c> if modal; otherwise, <c>false</c>.</value>
        [Hidden]
        public bool Modal { get; set; }

        /// <summary>
        /// Gets or sets the snap distance.
        /// </summary>
        /// <value>The snap distance.</value>
        [Category("Behavior")]
        public int SnapDistance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow drag out].
        /// </summary>
        /// <value><c>true</c> if [allow drag out]; otherwise, <c>false</c>.</value>
        [Category("Behavior")]
        public bool AllowDragOut { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Window"/> is resizable.
        /// </summary>
        /// <value><c>true</c> if resizable; otherwise, <c>false</c>.</value>
        [Category("Behavior")]
        public bool Resizable
        {
            get { return _sizer.ParentControl == this; }
            set
            {
                if (value)
                {
                    if (_sizer.ParentControl != this)
                        Elements.Add(_sizer);
                }
                else
                {
                    Elements.Remove(_sizer);
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the grip.
        /// </summary>
        /// <value>The size of the grip.</value>
        [Category("Behavior")]
        public Margin GripSize
        {
            get { return _sizer.GripSize; }
            set { _sizer.GripSize = value; }
        }

        /// <summary>
        /// Called when [update].
        /// </summary>
        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (_isDragging)
                Drag();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        public Window()
        {
            Style = "window";
            Scissor = true;
            _sizer = new Resizer();
            _sizer.Dock = DockStyle.Fill;
            SnapDistance = 12;

            MinSize = new Point(200, 100);
            MaxSize = new Point(800, 600);
        }

        protected override void DrawBefore()
        {
            if (Modal)
            {
                SetScissor(0, 0, Desktop.Size.X, Desktop.Size.Y);
                Gui.Renderer.DrawBox(0, 0, Desktop.Size.X, Desktop.Size.Y, ColorInt.FromArgb(GetOpacity(1), Desktop.ModalColor));
                ResetScissor();
            }

            base.DrawBefore();
        }

        /// <summary>
        /// Starts the drag.
        /// </summary>
        public void StartDrag()
        {
            _clickedPos = Gui.MousePosition - Position;
            _isDragging = true;
        }

        /// <summary>
        /// Stops the drag.
        /// </summary>
        public void StopDrag()
        {
            _isDragging = false;
        }

        private void Drag()
        {
            var p = Gui.MousePosition - _clickedPos;

            if (!Modal)
            {
                foreach (var win in Container.Controls)
                {
                    if (!(win is Window)) continue;
                    if (win == this) continue;

                    var top = win.Position.Y;
                    var bottom = win.Position.Y + win.Size.Y;
                    var left = win.Position.X;
                    var right = win.Position.X + win.Size.X;

                    if (Math.Abs(p.X - right) <= SnapDistance)
                    {
                        if (!(p.Y + Size.Y < top) && !(p.Y > bottom))
                            p.X = right;
                    }

                    if (Math.Abs(p.X + Size.X - left) <= SnapDistance)
                    {
                        if (!(p.Y + Size.Y < top) && !(p.Y > bottom))
                            p.X = left - Size.X;
                    }

                    if (Math.Abs(p.Y - bottom) <= SnapDistance)
                    {
                        if (!(p.X + Size.X < left) && !(p.X > right))
                            p.Y = bottom;
                    }

                    if (Math.Abs(p.Y + Size.Y - top) <= SnapDistance)
                    {
                        if (!(p.X + Size.X < left) && !(p.X > right))
                            p.Y = top - Size.Y;
                    }
                }
            }

            if (!AllowDragOut)
            {
                if (p.X < 0) p.X = 0;
                if (p.Y < 0) p.Y = 0;
                if (p.X + Size.X > Parent.Size.X) p.X = Parent.Size.X - Size.X;
                if (p.Y + Size.Y > Parent.Size.Y) p.Y = Parent.Size.Y - Size.Y;

                if (p.X < SnapDistance) p.X = 0;
                if (p.Y < SnapDistance) p.Y = 0;
                if (p.X + Size.X > Parent.Size.X - SnapDistance) p.X = Parent.Size.X - Size.X;
                if (p.Y + Size.Y > Parent.Size.Y - SnapDistance) p.Y = Parent.Size.Y - Size.Y;
            }

            Position = p;
        }

        /// <summary>
        /// Shows this window on the specified Desktop.
        /// </summary>
        /// <param name="target">The target.</param>
        public virtual void Show(Desktop target)
        {
            if (Parent == target) return;

            target.Controls.Add(this);

            SetDepth();

            if (Modal)
                target.RegisterModal(this);

            Visible = true;
        }

        /// <summary>
        /// Closes this window
        /// </summary>
        public virtual void Close()
        {
            if (Desktop == null) return;

            if (Modal)
                Desktop.UnregisterModal(this);

            Desktop.Controls.Remove(this);

            Visible = false;
        }
    }
}
