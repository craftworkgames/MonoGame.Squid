using Demo.SampleControls;
using Microsoft.Xna.Framework;
using MonoGame.Squid;
using MonoGame.Squid.Controls;
using Point = MonoGame.Squid.Structs.Point;
using Rectangle = MonoGame.Squid.Structs.Rectangle;

namespace Demo
{
    public class SampleScene : DrawableGameComponent
    {
        private Desktop _desktop;

        public SampleScene(Game game)
            : base(game)
        {
            UpdateOrder = 10;
        }

        protected override void LoadContent()
        {
            //Desktop = new GameGui { Name = "desk" };
            _desktop = new SampleDesktop {Name = "desk"};
            _desktop.ShowCursor = true;

            // -- Uncomment to load and apply the style texture
            //ReadAtlas("SampleMap");

            base.LoadContent();
        }

        public override void Draw(GameTime time)
        {
            _desktop.Size = new Point(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
            _desktop.Update();
            _desktop.Draw();

            var tex = Gui.Renderer.GetTexture("xna_logo.png");
            var size = Gui.Renderer.GetTextureSize(tex);
            var rect = new Rectangle(0, 0, size.X, size.Y);

            Gui.Renderer.StartBatch();
            Gui.Renderer.DrawTexture(tex, Game.GraphicsDevice.Viewport.Width - 130,
                Game.GraphicsDevice.Viewport.Height - 130, 128, 128, rect, -1);
            Gui.Renderer.EndBatch(true);
        }

        //    atlas.LoadFile(AppDomain.CurrentDomain.BaseDirectory + "Content\\" + mapName + ".xml");
        //    Atlas atlas = new Atlas();
        //{

        //private void ReadAtlas(string mapName)

        //    foreach (ControlStyle style in Gui.GetSkin().Styles.Values)
        //    {
        //        foreach (Style state in style.Styles.Values)
        //        {
        //            if (string.IsNullOrEmpty(state.Texture))
        //                continue;

        //            if (atlas.Contains(state.Texture))
        //            {
        //                state.TextureRect = atlas.GetRect(state.Texture);
        //                state.Texture = mapName + ".png";
        //            }
        //        }
        //    }
        //}
    }
}