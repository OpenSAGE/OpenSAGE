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

        public unsafe Cursor(FileSystemEntry entry)
        {
            _aniFile = AniFile.FromFileSystemEntry(entry);

            var width = (int) _aniFile.IconWidth;
            var height = (int) _aniFile.IconHeight;

            _surfaces = new Sdl2Interop.SDL_Surface[_aniFile.Images.Length];
            _cursors = new Sdl2Interop.SDL_Cursor[_aniFile.Images.Length];

            for (var i = 0; i < _aniFile.Images.Length; i++)
            {
                var pixels = _aniFile.Images[i].PixelsBgra;

                fixed (byte* pixelsPtr = pixels)
                {
                    _surfaces[i] = Sdl2Interop.SDL_CreateRGBSurfaceWithFormatFrom(
                        pixelsPtr,
                        width,
                        height,
                        32,
                        width * 4,
                        Sdl2Interop.SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888);
                }

                AddDisposeAction(() => Sdl2Interop.SDL_FreeSurface(_surfaces[i]));

                _cursors[i] = Sdl2Interop.SDL_CreateColorCursor(
                    _surfaces[i],
                    (int) _aniFile.HotspotX,
                    (int) _aniFile.HotspotY);

                AddDisposeAction(() => Sdl2Interop.SDL_FreeCursor(_cursors[i]));
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
