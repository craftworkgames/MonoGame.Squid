using System;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// A container that auto-layouts its children.
    /// Children must not be docked for this to work.
    /// </summary>
    [Toolbox]
    public class FlowLayoutFrame : Frame
    {
        private Point _lastSize;

        /// <summary>
        /// Gets or sets the flow direction.
        /// </summary>
        /// <value>The flow direction.</value>
        public FlowDirection FlowDirection { get; set; }

        /// <summary>
        /// Gets or sets the H spacing.
        /// </summary>
        /// <value>The H spacing.</value>
        public int HSpacing { get; set; }

        /// <summary>
        /// Gets or sets the V spacing.
        /// </summary>
        /// <value>The V spacing.</value>
        public int VSpacing { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowLayoutFrame"/> class.
        /// </summary>
        public FlowLayoutFrame()
        {
            Size = new Point(100, 100);

            FlowDirection = FlowDirection.LeftToRight;
            HSpacing = 4;
            VSpacing = 4;

            Controls.ItemAdded += Controls_ItemAdded;
            Controls.ItemRemoved += Controls_ItemRemoved;
            Controls.ItemsSorted += Controls_ItemsSorted;
        }

        void Controls_ItemsSorted(object sender, EventArgs e)
        {
            Refresh();
        }

        void Controls_ItemRemoved(object sender, ListEventArgs<Control> e)
        {
            Refresh();
        }

        void Controls_ItemAdded(object sender, ListEventArgs<Control> e)
        {
            Refresh();
        }

        private void LayoutLeftToRight()
        {
            var x = HSpacing;
            var y = VSpacing;
            var max = 0;
            var c = 0;

            foreach (var control in Controls)
            {
                if (!control.Visible) continue;

                if (x + control.Size.X + HSpacing <= Size.X)
                {
                    control.Position = new Point(x, y);

                    x = x + control.Size.X + HSpacing;
                }
                else
                {
                    x = HSpacing;

                    if (c > 0)
                    {
                        y += max + VSpacing;
                        max = 0;
                        c = 0;
                    }

                    control.Position = new Point(x, y);

                    x = x + control.Size.X + HSpacing;
                }

                max = Math.Max(max, control.Size.Y);

                c++;
            }
        }

        private void LayoutRightToLeft()
        {
            var x = Size.X;
            var y = HSpacing;
            var max = 0;
            var c = 0;

            foreach (var control in Controls)
            {
                if (!control.Visible) continue;

                if (x - control.Size.X - HSpacing >= HSpacing)
                {
                    control.Position = new Point(x - control.Size.X - HSpacing, y);

                    x = x - control.Size.X - HSpacing;
                }
                else
                {
                    x = Size.X;

                    if (c > 0)
                    {
                        y += max + VSpacing;
                        max = 0;
                        c = 0;
                    }

                    control.Position = new Point(x, y);

                    x = x - control.Size.X - HSpacing;
                }

                max = Math.Max(max, control.Size.Y);

                c++;
            }
        }

        private void LayoutTopToBottom()
        {
            var x = HSpacing;
            var y = HSpacing;
            var max = 0;
            var c = 0;

            foreach (var control in Controls)
            {
                if (!control.Visible) continue;

                if (y + control.Size.Y + VSpacing <= Size.Y)
                {
                    control.Position = new Point(x, y);

                    y = y + control.Size.Y + VSpacing;
                }
                else
                {
                    y = VSpacing;

                    if (c > 0)
                    {
                        x += max + HSpacing;
                        max = 0;
                        c = 0;
                    }

                    control.Position = new Point(x, y);

                    y = y + control.Size.Y + VSpacing;
                }

                max = Math.Max(max, control.Size.X);

                c++;
            }
        }

        private void LayoutBottomToTop()
        {
            var x = HSpacing;
            var y = Size.Y;
            var max = 0;
            var c = 0;

            foreach (var control in Controls)
            {
                if (!control.Visible) continue;

                if (y - control.Size.Y - VSpacing >= VSpacing)
                {
                    control.Position = new Point(x, y - control.Size.Y - VSpacing);

                    y = y - control.Size.Y - VSpacing;
                }
                else
                {
                    y = Size.Y;

                    if (c > 0)
                    {
                        x += max + HSpacing;
                        max = 0;
                        c = 0;
                    }

                    control.Position = new Point(x, y);

                    y = y - control.Size.Y - VSpacing;
                }

                max = Math.Max(max, control.Size.X);

                c++;
            }
        }

        protected override void OnUpdate()
        {
            if (!_lastSize.Equals(Size))
            {
                Refresh();
                _lastSize = Size;
            }
        }

        /// <summary>
        /// Forces the flow layout.
        /// </summary>
        public void ForceFlowLayout()
        {
            Refresh();
        }

        protected void Refresh()
        {
            switch (FlowDirection)
            {
                case FlowDirection.None:
                    return;
                case FlowDirection.LeftToRight:
                    LayoutLeftToRight();
                    break;
                case FlowDirection.TopToBottom:
                    LayoutTopToBottom();
                    break;
                case FlowDirection.RightToLeft:
                    LayoutRightToLeft();
                    break;
                case FlowDirection.BottomToTop:
                    LayoutBottomToTop();
                    break;
            }
        }
    }
}
