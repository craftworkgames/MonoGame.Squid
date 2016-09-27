using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Squid.Interfaces;
using MonoGame.Squid.Util;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MonoGame.Squid
{
    public class MonoGameSquidRenderer : ISquidRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ContentManager _contentManager;

        [DllImport("user32.dll")]
        private static extern int GetKeyboardLayout(int dwLayout);
        [DllImport("user32.dll")]
        private static extern int GetKeyboardState(ref byte pbKeyState);
        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyEx")]
        private static extern int MapVirtualKeyExA(int uCode, int uMapType, int dwhkl);
        [DllImport("user32.dll")]
        private static extern int ToAsciiEx(int uVirtKey, int uScanCode, ref byte lpKeyState, ref short lpChar, int uFlags, int dwhkl);

        private static int _keyboardLayout;
        private readonly byte[] _keyStates;

        private readonly Dictionary<int, SpriteFont> _fonts = new Dictionary<int, SpriteFont>();
        private readonly Dictionary<string, int> _fontLookup = new Dictionary<string, int>();

        private readonly Dictionary<int, Texture2D> _textures = new Dictionary<int, Texture2D>();
        private readonly Dictionary<string, int> _textureLookup = new Dictionary<string, int>();

        private readonly Dictionary<string, Font> _fontTypes = new Dictionary<string, Font>();

        private readonly SpriteBatch _batch;

        private int _fontIndex;
        private int _textureIndex;
        private readonly Texture2D _blankTexture;


        private readonly RasterizerState _rasterizer;
        private readonly SamplerState _sampler;

        public MonoGameSquidRenderer(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            _graphicsDevice = graphicsDevice;
            _contentManager = contentManager;

            _batch = new SpriteBatch(graphicsDevice);

            _blankTexture = new Texture2D(graphicsDevice, 1, 1);
            _blankTexture.SetData(new[] { new Color(255, 255, 255, 255) });

            _fontTypes.Add(Font.Default, new Font { Name = "Arial10", Family = "Arial", Size = 8, Bold = true, International = true });

            _keyboardLayout = GetKeyboardLayout(0);
            _keyStates = new byte[0x100];

            _rasterizer = new RasterizerState {ScissorTestEnable = true};

            _sampler = new SamplerState {Filter = TextureFilter.Anisotropic};
        }

        public static int VirtualKeyToScancode(int key)
        {
            return MapVirtualKeyExA(key, 0, _keyboardLayout);
        }

        public bool TranslateKey(int code, ref char character)
        {
            short lpChar = 0;
            if (GetKeyboardState(ref _keyStates[0]) == 0)
                return false;

            var result = ToAsciiEx(MapVirtualKeyExA(code, 1, _keyboardLayout), code, ref _keyStates[0], ref lpChar, 0, _keyboardLayout);
            if (result == 1)
            {
                character = (char)((ushort)lpChar);
                return true;
            }

            return false;
        }

        private Color ColorFromtInt32(int color)
        {
            var bytes = BitConverter.GetBytes(color);

            return new Color(bytes[2], bytes[1], bytes[0], bytes[3]);
        }

        public int GetTexture(string name)
        {
            if (_textureLookup.ContainsKey(name))
                return _textureLookup[name];

            var texture = _contentManager.Load<Texture2D>(Path.ChangeExtension(name, null));
            _textureIndex++;

            _textureLookup.Add(name, _textureIndex);
            _textures.Add(_textureIndex, texture);

            return _textureIndex;
        }

        public int GetFont(string name)
        {
            if (_fontLookup.ContainsKey(name))
                return _fontLookup[name];

            if (!_fontTypes.ContainsKey(name))
                return -1;

            var type = _fontTypes[name];

            var font = _contentManager.Load<SpriteFont>(type.Name);
            _fontIndex++;

            _fontLookup.Add(name, _fontIndex);
            _fonts.Add(_fontIndex, font);

            return _fontIndex;
        }

        public global::MonoGame.Squid.Structs.Point GetTextSize(string text, int font)
        {
            if (string.IsNullOrEmpty(text))
                return new global::MonoGame.Squid.Structs.Point();

            var f = _fonts[font];
            var size = f.MeasureString(text);
            return new global::MonoGame.Squid.Structs.Point((int)size.X, (int)size.Y);
        }

        public global::MonoGame.Squid.Structs.Point GetTextureSize(int texture)
        {
            var tex = _textures[texture];
            return new global::MonoGame.Squid.Structs.Point(tex.Width, tex.Height);
        }

        public void DrawBox(int x, int y, int w, int h, int color)
        {
            var destination = new Rectangle(x, y, w, h);
            _batch.Draw(_blankTexture, destination, destination, ColorFromtInt32(color));
        }

        public void DrawText(string text, int x, int y, int font, int color)
        {
            if (!_fonts.ContainsKey(font)) 
                return;

            var f = _fonts[font];
            _batch.DrawString(f, text, new Vector2(x, y), ColorFromtInt32(color));
        }

        public void DrawTexture(int texture, int x, int y, int w, int h, global::MonoGame.Squid.Structs.Rectangle rect, int color)
        {
            if (!_textures.ContainsKey(texture))
                return;

            var tex = _textures[texture];

            var destination = new Rectangle(x, y, w, h);
            var source = new Rectangle();

            source.X = rect.Left;
            source.Y = rect.Top;
            source.Width = rect.Width;
            source.Height = rect.Height;

            _batch.Draw(tex, destination, source, ColorFromtInt32(color));
        }

        public void Scissor(int x, int y, int w, int h)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            _graphicsDevice.ScissorRectangle = new Rectangle(x, y, w, h);
        }

        public void StartBatch()
        {
            _batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, _sampler, null, _rasterizer); 
        }

        public void EndBatch(bool final)
        {
            _batch.End();
        }

        public void Dispose() { }
    }
}
