using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Squid;
using Squid;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Demo
{
    public class GameDesktop : Desktop
    {
        public GameDesktop()
        {
            var button = new Button()
            {
                Text = "Hello",
                Position = new Squid.Point(100, 100),
                Size = new Squid.Point(100, 50)
            };

            Controls.Add(button);
        }
    }

    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameDesktop _desktop;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = 1050,
                PreferredBackBufferWidth = 1600,
                SynchronizeWithVerticalRetrace = false
            };
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Components.Add(new MonoGameSquidInputManager(this));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Gui.Renderer = new MonoGameSquidRenderer(GraphicsDevice, Content);

            _desktop = new GameDesktop();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            var gamePadState = GamePad.GetState(PlayerIndex.One);
            var keyboardState = Keyboard.GetState();

            if (gamePadState.Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            _desktop.Size = new Squid.Point(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
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
