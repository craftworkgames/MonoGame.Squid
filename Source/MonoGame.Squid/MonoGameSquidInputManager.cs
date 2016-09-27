using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Squid.Structs;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MonoGame.Squid
{
    public class MonoGameSquidInputManager : GameComponent
    {
        private class InputKey
        {
            // ReSharper disable once NotAccessedField.Local
            public Keys Key = Keys.None;
            public int ScanCode;
            public double Repeat = _repeatDelay;
        }

        private static readonly Dictionary<Keys, int> _specialKeys = new Dictionary<Keys, int>();
        private readonly Dictionary<Keys, InputKey> _inputKeys = new Dictionary<Keys, InputKey>();

        private KeyboardState _lastKeyboardState;
        private int _lastScroll;

        private const int _repeatDelay = 500;
        private const int _repeatRate = 25;

        public MonoGameSquidInputManager(Game game)
            : base(game)
        {
            _specialKeys.Add(Keys.Home, 0xC7);
            _specialKeys.Add(Keys.Up, 0xC8);
            _specialKeys.Add(Keys.Left, 0xCB);
            _specialKeys.Add(Keys.Right, 0xCD);
            _specialKeys.Add(Keys.End, 0xCF);
            _specialKeys.Add(Keys.Down, 0xD0);
            _specialKeys.Add(Keys.Insert, 0xD2);
            _specialKeys.Add(Keys.Delete, 0xD3);
            _specialKeys.Add(Keys.MediaPreviousTrack, 0x90);

            foreach (Keys k in System.Enum.GetValues(typeof(Keys)))
            {
                var key = new InputKey();
                key.Key = k;
                key.ScanCode = VirtualKeyToScancode(k);
                _inputKeys.Add(k, key);
            }
        }

        private int VirtualKeyToScancode(Keys key)
        {
            var sc = MonoGameSquidRenderer.VirtualKeyToScancode((int)key);

            if (_specialKeys.ContainsKey(key))
                sc = _specialKeys[key];

            return sc;
        }


        public override void Update(GameTime gameTime)
        {
            // Mouse
            var mouseState = Mouse.GetState();

            if(mouseState.LeftButton == ButtonState.Pressed)
                System.Diagnostics.Debug.WriteLine("Mouse Clicked."); 

            var wheel = mouseState.ScrollWheelValue > _lastScroll ? -1 : (mouseState.ScrollWheelValue < _lastScroll ? 1 : 0);
            _lastScroll = mouseState.ScrollWheelValue;

            Gui.SetMouse(mouseState.X, mouseState.Y, wheel);
            Gui.SetButtons(mouseState.LeftButton == ButtonState.Pressed, mouseState.RightButton == ButtonState.Pressed);

            // Keyboard
            var keyboardState = Keyboard.GetState();
            var squidKeys = new List<KeyData>();

            double ms = Gui.TimeElapsed;

            var now = keyboardState.GetPressedKeys();
            var last = _lastKeyboardState.GetPressedKeys();

            foreach (var key in now)
            {
                var wasDown = _lastKeyboardState.IsKeyDown(key);
              
                _inputKeys[key].Repeat -= ms;

                if (_inputKeys[key].Repeat < 0 || !wasDown)
                {
                    squidKeys.Add(new KeyData()
                    {
                        Scancode = _inputKeys[key].ScanCode,
                        Pressed = true
                    });
                    _inputKeys[key].Repeat = !wasDown ? _repeatDelay : _repeatRate;
                }
            }

            foreach (var key in last)
            {
                var isDown = keyboardState.IsKeyDown(key);

                if (!isDown)
                {
                    squidKeys.Add(new KeyData()
                    {
                        Scancode = _inputKeys[key].ScanCode,
                        Released = true
                    });
                    _inputKeys[key].Repeat = _repeatDelay;
                }
            }

            _lastKeyboardState = keyboardState;

            Gui.SetKeyboard(squidKeys.ToArray());
            Gui.TimeElapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            base.Update(gameTime);
        }
    }
}
