using System;

namespace OpenSage.Graphics.Rendering
{
    public sealed class Rendering2DEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; }

        public Rendering2DEventArgs(SpriteBatch spriteBatch)
        {
            SpriteBatch = spriteBatch;
        }
    }
}
