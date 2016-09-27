using MonoGame.Squid.Structs;

namespace MonoGame.Squid.Util
{
    /// <summary>
    /// Helper class to manage a sprite sheet animation.
    /// </summary>
    public sealed class Flipbook
    {
        public int Rows = 1;
        public int Columns = 1;
        public float Speed = 60;

        private float _timer;
        private int _col = 0;
        private int _row = 0;

        private Rectangle _rect = new Rectangle();

        public void Draw(int texture, int x, int y, int width, int height, int color)
        {
            if (texture < 0) return;

            _timer += Gui.TimeElapsed;
            
            if (_timer >= Speed)
            {
                _timer = 0;
                Advance();
            }

            var size = Gui.Renderer.GetTextureSize(texture);
            
            var w = (int)((float)size.X / (float)Columns);
            var h = (int)((float)size.Y / (float)Rows);

            _rect.Left = w * _col;
            _rect.Right = (w * (_col + 1));

            _rect.Top = h * _row;
            _rect.Bottom = (h * (_row + 1));

            Gui.Renderer.DrawTexture(texture, x, y, width, height, _rect, color);
        }

        private void Advance()
        {
            if (_col < Columns)
            {
                _col++;

                if (_col >= Columns)
                {
                    _col = 0;

                    if (_row < Rows)
                    {
                        _row++;

                        if (_row >= Rows)
                            _row = 0;
                    }
                }
            }
        }
    }
}
