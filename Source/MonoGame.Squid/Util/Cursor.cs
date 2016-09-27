using System.Collections.Generic;
using MonoGame.Squid.Structs;

namespace MonoGame.Squid.Util
{
    /// <summary>
    /// A collection of Cursors
    /// </summary>
    public class CursorCollection : Dictionary<string, Cursor> { }

    /// <summary>
    /// Represents the image that is displayed at the mouse position.
    /// </summary>
    public class Cursor
    {
        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        [IntColor]
        public int Color { get; set; }

        /// <summary>
        /// Gets or sets the texture.
        /// </summary>
        /// <value>The texture.</value>
        [Texture]
        public string Texture { get; set; }
        /// <summary>
        /// Gets or sets the hot spot.
        /// </summary>
        /// <value>The hot spot.</value>
        public Point HotSpot { get; set; }
        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public Point Size { get; set; }
        /// <summary>
        /// Gets or sets the texture rect.
        /// </summary>
        /// <value>The texture rect.</value>
        public Rectangle TextureRect { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cursor"/> class.
        /// </summary>
        public Cursor()
        {
            Color = -1;
        }

        /// <summary>
        /// Draws the cursor at the specified position.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public virtual void Draw(int x, int y)
        {
            var texture = Gui.Renderer.GetTexture(Texture);
            if (texture < 0) return;

            if (TextureRect.IsEmpty())
            {
                var texsize = Gui.Renderer.GetTextureSize(texture);
                TextureRect = new Rectangle(Point.Zero, texsize);
            }

            var p = new Point(x, y) - HotSpot;
            Gui.Renderer.DrawTexture(texture, p.X, p.Y, Size.X, Size.Y, TextureRect, Color);
        }
    }

    /// <summary>
    /// A flibook based implementation of an animated Cursor.
    /// </summary>
    public class FlipbookCursor : Cursor
    {
        /// <summary>
        /// Gets or sets the rows.
        /// </summary>
        /// <value>The rows.</value>
        public int Rows { get; set; }
        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public int Columns { get; set; }
        /// <summary>
        /// Gets or sets the speed.
        /// </summary>
        /// <value>The speed.</value>
        public float Speed { get; set; }

        /// <summary>
        /// The flip
        /// </summary>
        private readonly Flipbook _flip = new Flipbook();

        /// <summary>
        /// Initializes a new instance of the <see cref="FlipbookCursor"/> class.
        /// </summary>
        public FlipbookCursor()
        {
            Color = -1;
            Rows = 1;
            Columns = 1;
            Speed = 60;
        }

        /// <summary>
        /// Draws the cursor at the specified position.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public override void Draw(int x, int y)
        {
            var index = Gui.Renderer.GetTexture(Texture);
            if (index < 0) return;

            var p = new Point(x, y) - HotSpot;

            _flip.Speed = Speed;
            _flip.Rows = Rows;
            _flip.Columns = Columns;
            _flip.Draw(index, p.X, p.Y, Size.X, Size.Y, Color);       
        }
    }
}
