using MonoGame.Squid.Structs;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// The Tooltip base class. Inherit this to create custom Tooltip controls.
    /// </summary>
    public abstract class Tooltip : Frame
    {
        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        /// <value>The offset.</value>
        public Point Offset { get; set; }

        /// <summary>
        /// Gets or sets the alignment.
        /// </summary>
        /// <value>The alignment.</value>
        public Alignment Alignment { get; set; }

        ///// <summary>
        ///// Gets or sets a value indicating whether [auto layout].
        ///// </summary>
        ///// <value><c>true</c> if [auto layout]; otherwise, <c>false</c>.</value>
        //public bool AutoLayout { get; set; }

        public Tooltip()
        {
            //AutoLayout = true;
            Alignment = Alignment.BottomRight;
            Offset = new Point(-8, -8);
        }

        /// <summary>
        /// Sets the context.
        /// </summary>
        /// <param name="context">The context.</param>
        public abstract void SetContext(Control context);

        public virtual void LayoutTooltip()
        {
            var p = Gui.MousePosition;

            switch (Alignment)
            {
                case Alignment.TopLeft:
                    p = Gui.MousePosition;
                    break;
                case Alignment.TopRight:
                    p = Gui.MousePosition - new Point(Size.X, 0);
                    break;
                case Alignment.TopCenter:
                    p = Gui.MousePosition - new Point(Size.X / 2, 0);
                    break;
                case Alignment.MiddleLeft:
                    p = Gui.MousePosition - new Point(0, Size.Y / 2);
                    break;
                case Alignment.MiddleRight:
                    p = Gui.MousePosition - new Point(Size.X, Size.Y / 2);
                    break;
                case Alignment.MiddleCenter:
                    p = Gui.MousePosition - new Point(Size.X / 2, Size.Y / 2);
                    break;
                case Alignment.BottomRight:
                    p = Gui.MousePosition - new Point(Size.X, Size.Y);
                    break;
                case Alignment.BottomLeft:
                    p = Gui.MousePosition - new Point(0, Size.Y);
                    break;
                case Alignment.BottomCenter:
                    p = Gui.MousePosition - new Point(Size.X / 2, Size.Y);
                    break;
            }

            p += Offset;

            if (p.X < 0) p.X = 0;
            if (p.Y < 0) p.Y = 0;

            var p2 = p + Size;

            if (p2.X > Desktop.Size.X)
                p.X = Desktop.Size.X - Size.X;

            if (p2.Y > Desktop.Size.Y)
                p.Y = Desktop.Size.Y - Size.Y;

            Position = p;
            PerformUpdate();
        }
    }
}
