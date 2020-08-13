using System;

namespace OpenSage.Data.Ani
{
    public sealed class CursorAnimationFrame
    {
        public readonly uint FrameIndex;
        public readonly TimeSpan Duration;

        internal CursorAnimationFrame(uint frameIndex, TimeSpan duration)
        {
            FrameIndex = frameIndex;
            Duration = duration;
        }
    }
}
