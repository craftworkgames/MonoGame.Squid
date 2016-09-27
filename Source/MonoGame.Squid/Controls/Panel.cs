using System;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// A container with scrollbars
    /// </summary>
    [Toolbox]
    public class Panel : Control
    {
        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>The content.</value>
        public Frame Content { get; private set; }

        /// <summary>
        /// Gets the clip frame.
        /// </summary>
        /// <value>The clip frame.</value>
        public Frame ClipFrame { get; private set; }
        
        /// <summary>
        /// Gets the H scroll.
        /// </summary>
        /// <value>The H scroll.</value>
        public ScrollBar HScroll { get; private set; }
        
        /// <summary>
        /// Gets the V scroll.
        /// </summary>
        /// <value>The V scroll.</value>
        public ScrollBar VScroll { get; private set; }

        public bool UseWheelForHScroll;

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class.
        /// </summary>
        public Panel()
        {
            Size = new Point(100, 100);

            VScroll = new ScrollBar();
            VScroll.Dock = DockStyle.Right;
            VScroll.Size = new Point(25, 25);
            Elements.Add(VScroll);

            HScroll = new ScrollBar();
            HScroll.Dock = DockStyle.Bottom;
            HScroll.Size = new Point(25, 25);
            HScroll.Orientation = Orientation.Horizontal;
            Elements.Add(HScroll);

            ClipFrame = new Frame();
            ClipFrame.Dock = DockStyle.Fill;
            ClipFrame.Scissor = true;
            Elements.Add(ClipFrame);

            Content = new Frame();
            Content.AutoSize = AutoSize.Vertical;
            ClipFrame.Controls.Add(Content);

            MouseWheel += Panel_MouseWheel;
        }

        void Panel_MouseWheel(Control sender, MouseEventArgs args)
        {
            // scroll

            if (UseWheelForHScroll)
                HScroll.Scroll(Gui.MouseScroll);
            else
                VScroll.Scroll(Gui.MouseScroll);

            // consume the mouse event
            args.Cancel = true;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            var position = Point.Zero;

            if (Content.Size.X < ClipFrame.Size.X || Content.AutoSize == AutoSize.Vertical)
                Content.Size = new Point(ClipFrame.Size.X, Content.Size.Y);

            if (Content.Size.Y < ClipFrame.Size.Y || Content.AutoSize == AutoSize.Horizontal)
                Content.Size = new Point(Content.Size.X, ClipFrame.Size.Y);

            if (!VScroll.ShowAlways && Content.Size.Y <= ClipFrame.Size.Y)
            {
                VScroll.Visible = false;
            }
            else
            {
                VScroll.Visible = true;
                VScroll.Scale = Math.Min(1, (float)ClipFrame.Size.Y / (float)Content.Size.Y);
                position.Y = (int)((ClipFrame.Size.Y - Content.Size.Y) * VScroll.EasedValue);

                //hack
                if (VScroll.ShowAlways)
                    VScroll.Slider.Visible = VScroll.Scale < 1;
                else
                    VScroll.Slider.Visible = true;
            }

            if (!HScroll.ShowAlways && Content.Size.X <= ClipFrame.Size.X)
            {
                HScroll.Visible = false;
            }
            else
            {
                HScroll.Visible = true;
                HScroll.Scale = Math.Min(1, (float)ClipFrame.Size.X / (float)Content.Size.X);
                position.X = (int)((ClipFrame.Size.X - Content.Size.X) * HScroll.EasedValue);
            }

            Content.Position = position;
        }
    }
}
