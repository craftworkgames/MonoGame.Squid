using System;
using System.ComponentModel;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// A Slider control. Also known as TrackBar.
    /// </summary>
    [Toolbox]
    public class Slider : Control
    {
        private float _value;
        private float _easeScroll;
        private Point _offset;

        /// <summary>
        /// Raised when [value changed].
        /// </summary>
        public event VoidEvent ValueChanged;

        /// <summary>
        /// Gets or sets the steps.
        /// </summary>
        /// <value>The steps.</value>
        public float Steps { get; set; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>The scale.</value>
        public float Scale { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto scale].
        /// </summary>
        /// <value><c>true</c> if [auto scale]; otherwise, <c>false</c>.</value>
        public bool AutoScale { get; set; }

        public int MinHandleSize { get; set; }

        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// <value>The minimum.</value>
        public float Minimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>The maximum.</value>
        public float Maximum { get; set; }

        /// <summary>
        /// Gets the button.
        /// </summary>
        /// <value>The button.</value>
        public Button Button { get; private set; }
        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        /// <value>The orientation.</value>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Slider"/> is ease.
        /// </summary>
        /// <value><c>true</c> if ease; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        public bool Ease { get; set; }

        /// <summary>
        /// Gets the eased value.
        /// </summary>
        /// <value>The eased value.</value>
        public float EasedValue { get; private set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == value) return;
                _value = value;
                _value = Math.Min(_value, Maximum);
                _value = Math.Max(_value, Minimum);

                if (ValueChanged != null)
                    ValueChanged(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Slider"/> class.
        /// </summary>
        public Slider()
        {
            Minimum = 0;
            Maximum = 100;
            Ease = true;

            Style = "slider";
            Scale = 1;
            Size = new Point(20, 100);
            Orientation = Orientation.Vertical;

            Button = new Button();
            Button.Size = new Point(20, 20);
            Button.MouseDown += Button_MouseDown;
            Button.Style = "sliderButton";
            Elements.Add(Button);

            MouseDown += Slider_MouseDown;
        }

        public void SetValue(float value)
        {
            Value = EasedValue = value;
            _easeScroll = value;
        }

        void Slider_MouseDown(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            var position = Gui.MousePosition - Location - Button.Size / 2;

            if (Orientation == Orientation.Vertical)
                Value = Minimum + (Maximum - Minimum) * position.Y / (Size.Y - Button.Size.Y);
            else
                Value = Minimum + (Maximum - Minimum) * position.X / (Size.X - Button.Size.X);

            Snap();
        }

        private void Snap()
        {
            if (Steps > 1)
            {
                var snap = 1f / Steps;
                Value = (float)Math.Ceiling(_value / snap) * snap;
            }
        }

        void Button_MouseDown(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            _offset = Gui.MousePosition - sender.Location;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            var root = Desktop;
            if (root == null) return;

            if (AutoScale)
            {
                Scale = Math.Min(1, Math.Max(0, Scale));

                if (Orientation == Orientation.Vertical)
                {
                    var size = Size.Y;// (int)(Size.y - Button.Margin.Top - Button.Margin.Bottom);
                    var actualSize = (int)((float)size * Scale);

                    if (MinHandleSize > 0 && actualSize < MinHandleSize)
                        Scale = (float)MinHandleSize / (float)size;
                    
                    Button.Size = new Point(Button.Size.X, (int)(size * Scale));
                }
                else
                {
                    var size = Size.X;// (int)(Size.y - Button.Margin.Top - Button.Margin.Bottom);
                    var actualSize = (int)((float)size * Scale);

                    if (MinHandleSize > 0 && actualSize < MinHandleSize)
                        Scale = MinHandleSize / size;

                    Button.Size = new Point((int)(size * Scale), Button.Size.Y);
                }
            }

            Button.Dock = Orientation == Orientation.Vertical ? DockStyle.FillX : DockStyle.FillY;

            if (root.PressedControl == Button)
            {
                if (!(Scale >= 1 && AutoScale))
                {
                    var position = Gui.MousePosition - Location;

                    if (Orientation == Orientation.Vertical)
                    {
                        position.X = Button.Position.X;
                        position.Y -= _offset.Y;
                        position.Y = Math.Max(0, Math.Min(Size.Y - Button.Size.Y, position.Y));

                        Button.Position = position;

                        Value = Minimum + (Maximum - Minimum) * position.Y / (Size.Y - Button.Size.Y);
                    }
                    else
                    {
                        position.Y = Button.Position.Y;
                        position.X -= _offset.X;
                        position.X = Math.Max(0, Math.Min(Size.X - Button.Size.X, position.X));

                        Button.Position = position;

                        Value = Minimum + (Maximum - Minimum) * position.X / (Size.X - Button.Size.X);
                    }

                    Snap();

                    _easeScroll = _value;
                }
            }
            else
            {
                Snap();

                var m = _value;

                if (Ease)
                {
                    _easeScroll += ((_value - _easeScroll) / 8f) * Math.Min(8, Gui.TimeElapsed * 0.1f);
                    m = (_easeScroll - Minimum) / (Maximum - Minimum);
                }
                else
                {
                    m = (_value - Minimum) / (Maximum - Minimum);
                }

                var end = Point.Zero;

                if (Orientation == Orientation.Vertical)
                {
                    var size = Size.Y;// (int)(Size.y - Button.Margin.Top - Button.Margin.Bottom);
                    var y = (int)(m * (size - Button.Size.Y));
                    end = new Point(Button.Position.X, y);

                    Button.Position = end;
                }
                else
                {
                    var x = (int)(m * (Size.X - Button.Size.X));
                    end = new Point(x, Button.Position.Y);

                    Button.Position = end;
                }
            }

            if (Ease)
                EasedValue += ((_value - EasedValue) / 8f) * Math.Min(8, Gui.TimeElapsed * 0.1f);
            else
                EasedValue = _value;
        }
    }
}
