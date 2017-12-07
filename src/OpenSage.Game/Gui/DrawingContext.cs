using LL.Graphics3D;
using OpenSage.Graphics;
using OpenSage.Mathematics;

namespace OpenSage.Gui
{
    public sealed class DrawingContext
    {
        private readonly SpriteBatch _spriteBatch;

        internal DrawingContext(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
        }

        public void Begin(CommandEncoder commandEncoder, in Rectangle viewport)
        {
            _spriteBatch.Begin(commandEncoder, in viewport);
        }

        public void DrawText(string text)
        {
            // TODO
        }

        public void DrawImage(Texture texture, in Rectangle destinationRect, in Rectangle? sourceRect = null)
        {
            _spriteBatch.Draw(
                texture, 
                sourceRect ?? new Rectangle(0, 0, texture.Width, texture.Height), 
                destinationRect);
        }

        public void End()
        {
            _spriteBatch.End();
        }
    }
}
