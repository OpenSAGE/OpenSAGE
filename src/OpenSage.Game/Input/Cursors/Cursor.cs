using System;
using System.Runtime.InteropServices;
using OpenSage.Data.Ani;
using OpenSage.IO;
using OpenSage.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenSage.Input.Cursors
{
    internal sealed class Cursor : DisposableBase
    {
        private readonly CursorAnimationFrame[] _animationFrames;

        private readonly Sdl2Interop.SDL_Surface[] _surfaces;
        private readonly Sdl2Interop.SDL_Cursor[] _cursors;

        private int _currentFrame;
        private TimeSpan _nextFrameTime;

        public unsafe Cursor(FileSystemEntry entry, float windowScale)
        {
            var cursorFile = CursorFile.FromFileSystemEntry(entry);

            _animationFrames = cursorFile.AnimationFrames;

            _surfaces = new Sdl2Interop.SDL_Surface[cursorFile.Images.Length];
            _cursors = new Sdl2Interop.SDL_Cursor[cursorFile.Images.Length];

            for (var i = 0; i < cursorFile.Images.Length; i++)
            {
                var image = cursorFile.Images[i];

                var width = (int) image.Width;
                var height = (int) image.Height;

                Sdl2Interop.SDL_Surface surface;
                if (windowScale == 1.0f)
                {
                    fixed (byte* pixelsPtr = image.PixelsBgra)
                    {
                        surface = Sdl2Interop.SDL_CreateRGBSurfaceWithFormatFrom(
                                pixelsPtr,
                                width,
                                height,
                                32,
                                width * 4,
                                Sdl2Interop.SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888);
                    }
                }
                else
                {
                    var scaledWidth = (int) (windowScale * width);
                    var scaledHeight = (int) (windowScale * height);

                    var scaledImage = Image.LoadPixelData<Bgra32>(image.PixelsBgra, width, height);
                    scaledImage.Mutate(x => x.Resize(scaledWidth, scaledHeight));

                    if (!scaledImage.TryGetSinglePixelSpan(out Span<Bgra32> pixelSpan))
                    {
                        throw new InvalidOperationException("Unable to get image pixelspan.");
                    }
                    fixed (void* pin = &MemoryMarshal.GetReference(pixelSpan))
                    {
                        surface = Sdl2Interop.SDL_CreateRGBSurfaceWithFormatFrom(
                           (byte*) pin,
                           scaledWidth,
                           scaledHeight,
                           32,
                           scaledWidth * 4,
                           Sdl2Interop.SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888);
                    }
                }

                AddDisposeAction(() => Sdl2Interop.SDL_FreeSurface(surface));

                _surfaces[i] = surface;

                var cursor = Sdl2Interop.SDL_CreateColorCursor(
                    _surfaces[i],
                    (int) (image.HotspotX * windowScale),
                    (int) (image.HotspotY * windowScale));

                AddDisposeAction(() => Sdl2Interop.SDL_FreeCursor(cursor));

                _cursors[i] = cursor;
            }
        }

        public void Apply(in TimeInterval time)
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

        private void DisplayNextFrame(in TimeInterval time)
        {
            _currentFrame++;

            if (_currentFrame >= _animationFrames.Length)
            {
                _currentFrame = 0;
            }

            var nextFrame = _animationFrames[_currentFrame];
            _nextFrameTime += nextFrame.Duration;

            // Only update the cursor if this is the last frame this tick
            if(_nextFrameTime > time.TotalTime)
            {
                var frameIndex = nextFrame.FrameIndex;
                Sdl2Interop.SDL_SetCursor(_cursors[frameIndex]);
            }
        }

        public void Update(in TimeInterval time)
        {
            if (_animationFrames.Length == 0)
            {
                return;
            }

            while (time.TotalTime > _nextFrameTime)
            {
                DisplayNextFrame(time);
            }
        }
    }
}
