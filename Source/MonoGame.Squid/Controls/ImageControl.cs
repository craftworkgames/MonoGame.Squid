using System.ComponentModel;
using MonoGame.Squid.Skinning;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// A control that show a texture
    /// </summary>
    [Toolbox]
    public class ImageControl : Control
    {
        private string _texture;

        /// <summary>
        /// Gets or sets the texture.
        /// </summary>
        /// <value>The texture.</value>
        [Texture, Category("Image")]
        public string Texture
        {
            get { return _texture; }
            set { _texture = value; TextureRect = new Rectangle(0, 0, 0, 0); }
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        [IntColor, Category("Image")]
        public int Color { get; set; }

        /// <summary>
        /// Gets or sets the texture rect.
        /// </summary>
        /// <value>The texture rect.</value>
        [Category("Image")]
        public Rectangle TextureRect { get; set; }

        /// <summary>
        /// Gets or sets the texture tiling
        /// </summary>
        [Category("Image")]
        public TextureMode Tiling { get; set; }

        /// <summary>
        /// Gets or sets the slice9 grid
        /// </summary>
        [Category("Image")]
        public Margin Grid { get; set; }

        public bool ExcludeFromAtlas = false;

        [Category("Image")]
        public Margin Inset { get; set; }

        public bool ColorByTint { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageControl"/> class.
        /// </summary>
        public ImageControl()
        {
            Color = -1;
            Inset = new Margin();
        }

        protected override void DrawStyle(Style style, float opacity)
        {
            base.DrawStyle(style, opacity);

            var texture = Gui.Renderer.GetTexture(Texture);
            if (texture < 0) return;

            if(ColorByTint)
                Color = style.Tint;

            var color = Color;

            if(Tint != -1)
                color = ColorInt.Blend(Tint, color);

            color = ColorInt.FromArgb(opacity, color);

            if (TextureRect.IsEmpty())
            {
                var texsize = Gui.Renderer.GetTextureSize(texture);
                TextureRect = new Rectangle(Point.Zero, texsize);
            }

            //bool atlas = SpriteBatch.AutoAtlas;

            //if (ExcludeFromAtlas)
            //    SpriteBatch.AutoAtlas = false;

            if (Tiling == TextureMode.Grid || Tiling == TextureMode.GridRepeat)
            {
                SliceTexture(texture, Tiling, TextureRect, Grid, opacity, color);
            }
            else if (Tiling == TextureMode.Stretch)
            {
                Gui.Renderer.DrawTexture(texture, Location.X + Inset.Left, Location.Y + Inset.Top, Size.X - (Inset.Left + Inset.Right), Size.Y - (Inset.Top + Inset.Bottom), TextureRect, color);
            }
            else if (Tiling == TextureMode.Center)
            {
                var center = Location + Size / 2;
                var rectsize = new Point(TextureRect.Width, TextureRect.Height);
                var pos = center - rectsize / 2;

                Gui.Renderer.DrawTexture(texture, pos.X, pos.Y, rectsize.X, rectsize.Y, TextureRect, color);
            }
            else
            {
                RepeatTexture(texture, Location, TextureRect, Tiling, opacity, color);
            }

            //if (ExcludeFromAtlas)
            //    SpriteBatch.AutoAtlas = atlas;
        }
    }
}
