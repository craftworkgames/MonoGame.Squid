using Demo.SampleControls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Squid;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Point = MonoGame.Squid.Structs.Point;

namespace Demo
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SampleDesktop _desktop;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = 800,
                PreferredBackBufferWidth = 1440,
                SynchronizeWithVerticalRetrace = false
            };
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Gui.Renderer = new MonoGameSquidRenderer(GraphicsDevice, Content);

            Components.Add(new MonoGameSquidInputManager(this));
            Components.Add(new SampleScene(this));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _desktop = new SampleDesktop();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            var gamePadState = GamePad.GetState(PlayerIndex.One);
            var keyboardState = Keyboard.GetState();

            if ((gamePadState.Buttons.Back == ButtonState.Pressed) || keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            _desktop.Size = new Point(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            _desktop.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _desktop.Draw();

            base.Draw(gameTime);
        }
    }
}