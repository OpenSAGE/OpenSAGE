using System;
using OpenSage.Data.Ani;
using OpenSage.Gui;
using Veldrid.Sdl2;

namespace OpenSage.Input.Cursors
{
    internal abstract class CursorBase : DisposableBase
    {
        protected CursorAnimationFrame[] _animationFrames;
        protected int _currentFrame;
        protected TimeSpan _nextFrameTime;
        abstract protected bool ShowSystemCursor { get; }

        public void Update(in TimeInterval time)
        {
            Sdl2Native.SDL_ShowCursor(ShowSystemCursor ? Sdl2Native.SDL_ENABLE
                                                       : Sdl2Native.SDL_DISABLE);

            if (_animationFrames.Length == 0)
            {
                return;
            }

            while (time.TotalTime > _nextFrameTime)
            {
                DisplayNextFrame(time);
            }
        }

        public abstract void Apply(in TimeInterval time);
        public abstract void Render(DrawingContext2D drawingContext);
        protected abstract void DisplayNextFrame(in TimeInterval time);
    }
}
