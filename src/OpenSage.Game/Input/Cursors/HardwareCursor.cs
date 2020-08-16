using OpenSage.Data;
using OpenSage.Data.Ani;
using OpenSage.Gui;
using OpenSage.Utilities;

namespace OpenSage.Input.Cursors
{
    internal sealed class HardwareCursor : CursorBase
    {
        private readonly Sdl2Interop.SDL_Surface[] _surfaces;
        private readonly Sdl2Interop.SDL_Cursor[] _cursors;
        protected override bool ShowSystemCursor => true;

        public unsafe HardwareCursor(FileSystemEntry entry, GameWindow window)
        {
            var cursorFile = CursorFile.FromFileSystemEntry(entry);

            _animationFrames = cursorFile.AnimationFrames;

            _surfaces = new Sdl2Interop.SDL_Surface[cursorFile.Images.Length];
            _cursors = new Sdl2Interop.SDL_Cursor[cursorFile.Images.Length];

            var windowScale = window.WindowScale;

            for (var i = 0; i < cursorFile.Images.Length; i++)
            {
                var image = cursorFile.Images[i];

                var width = (int) image.Width;
                var height = (int) image.Height;

                fixed (byte* pixelsPtr = image.PixelsBgra)
                {
                    var surface = Sdl2Interop.SDL_CreateRGBSurfaceWithFormatFrom(
                        pixelsPtr,
                        width,
                        height,
                        32,
                        width * 4,
                        Sdl2Interop.SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888);

                    if (windowScale != 1.0f)
                    {
                        var scaledWidth = (int) (windowScale * width);
                        var scaledHeight = (int) (windowScale * height);

                        var scaledSurface = Sdl2Interop.SDL_CreateRGBSurfaceWithFormat(
                            0,
                            scaledWidth,
                            scaledHeight,
                            32,
                            Sdl2Interop.SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888);

                        Sdl2Interop.SDL_BlitScaled(
                            surface,
                            new Sdl2Interop.SDL_Rect(0, 0, width, height),
                            scaledSurface,
                            new Sdl2Interop.SDL_Rect(0, 0, scaledWidth, scaledHeight));

                        Sdl2Interop.SDL_FreeSurface(surface);

                        surface = scaledSurface;
                    }

                    AddDisposeAction(() => Sdl2Interop.SDL_FreeSurface(surface));

                    _surfaces[i] = surface;
                }

                var cursor = Sdl2Interop.SDL_CreateColorCursor(
                    _surfaces[i],
                    (int) (image.HotspotX * windowScale),
                    (int) (image.HotspotY * windowScale));

                AddDisposeAction(() => Sdl2Interop.SDL_FreeCursor(cursor));

                _cursors[i] = cursor;
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
                Sdl2Interop.SDL_SetCursor(_cursors[0]);
            }
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
            Sdl2Interop.SDL_SetCursor(_cursors[frameIndex]);

            _nextFrameTime += nextFrame.Duration;
        }

        public override void Render(DrawingContext2D drawingContext)
        {
            // Cursor is rendered by the OS
        }
    }
}
