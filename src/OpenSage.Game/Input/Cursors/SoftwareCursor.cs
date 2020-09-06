using OpenSage.Data;
using OpenSage.Data.Ani;
using OpenSage.Gui;
using OpenSage.Mathematics;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Input.Cursors
{
    internal sealed class SoftwareCursor : CursorBase
    {
        private readonly Texture[] _textures;
        private Texture _currentTexture;

        protected override bool ShowSystemCursor => false;

        public SoftwareCursor(FileSystemEntry entry, GameWindow window, GraphicsDevice gd)
        {
            var cursorFile = CursorFile.FromFileSystemEntry(entry);

            _animationFrames = cursorFile.AnimationFrames;

            _textures = new Texture[cursorFile.Images.Length];

            var windowScale = window.WindowScale;

            for (var i = 0; i < cursorFile.Images.Length; i++)
            {
                var image = cursorFile.Images[i];

                var width = image.Width;
                var height = image.Height;

                //TODO: pack those into a texture array
                _textures[i] = gd.CreateStaticTexture2D(
                    width, height, 1,
                    new TextureMipMapData(image.PixelsBgra, width * 4, width * height * 4, width, height),
                    PixelFormat.B8_G8_R8_A8_UNorm);
            }
        }

        public override void Apply(in TimeInterval time)
        {
            if (_animationFrames.Length > 0)
            {
                _currentFrame = -1;
                _nextFrameTime = time.TotalTime;
                DisplayNextFrame(time);
            }
            else
            {
                _currentTexture = _textures[0];
            }
        }

        public override unsafe void Render(DrawingContext2D drawingContext)
        {
            int curX, curY;
            Sdl2Interop.SDL_GetMouseState(&curX, &curY);
            var cursorPos = new Point2D(curX, curY);
            var cursorSize = new Size((int) _currentTexture.Width, (int) _currentTexture.Height);
            var destRect = new Rectangle(cursorPos, cursorSize);
            drawingContext.DrawImage(_currentTexture, null, destRect);
        }

        protected override void DisplayNextFrame(in TimeInterval time)
        {
            _currentFrame++;

            if (_currentFrame >= _animationFrames.Length)
            {
                _currentFrame = 0;
            }

            var nextFrame = _animationFrames[_currentFrame];

            var frameIndex = nextFrame.FrameIndex;
            _currentTexture = _textures[frameIndex];

            _nextFrameTime += nextFrame.Duration;
        }
    }
}
