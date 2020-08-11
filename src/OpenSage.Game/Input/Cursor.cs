using System;
using OpenSage.Data;
using OpenSage.Data.Ani;
using OpenSage.Utilities;

namespace OpenSage.Input
{
    internal sealed class Cursor : DisposableBase
    {
        private readonly AniFile _aniFile;

        private readonly Sdl2Interop.SDL_Surface[] _surfaces;
        private readonly Sdl2Interop.SDL_Cursor[] _cursors;

        private int _currentSequenceIndex;
        private TimeSpan _nextFrameTime;

        public unsafe Cursor(FileSystemEntry entry, GameWindow window)
        {
            _aniFile = AniFile.FromFileSystemEntry(entry);

            var width = (int) _aniFile.IconWidth;
            var height = (int) _aniFile.IconHeight;

            _surfaces = new Sdl2Interop.SDL_Surface[_aniFile.Images.Length];
            _cursors = new Sdl2Interop.SDL_Cursor[_aniFile.Images.Length];

            var windowScale = window.WindowScale;

            for (var i = 0; i < _aniFile.Images.Length; i++)
            {
                var pixels = _aniFile.Images[i].PixelsBgra;

                fixed (byte* pixelsPtr = pixels)
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
                    (int) _aniFile.HotspotX,
                    (int) _aniFile.HotspotY);

                AddDisposeAction(() => Sdl2Interop.SDL_FreeCursor(cursor));

                _cursors[i] = cursor;
            }
        }

        public void Apply(in TimeInterval time)
        {
            if (_aniFile.Sequence != null)
            {
                _currentSequenceIndex = -1;
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
            _currentSequenceIndex++;

            if (_currentSequenceIndex >= _aniFile.Sequence.FrameIndices.Length)
            {
                _currentSequenceIndex = 0;
            }

            var frameIndex = _aniFile.Sequence.FrameIndices[_currentSequenceIndex];
            Sdl2Interop.SDL_SetCursor(_cursors[frameIndex]);

            _nextFrameTime += TimeSpan.FromSeconds(1 / 60.0) * _aniFile.Rates.Durations[_currentSequenceIndex];
        }

        public void Update(in TimeInterval time)
        {
            if (_aniFile.Sequence == null)
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
