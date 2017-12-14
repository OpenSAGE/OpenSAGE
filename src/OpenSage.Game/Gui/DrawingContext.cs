using OpenSage.LowLevel.Graphics3D;
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

        public void Begin(CommandEncoder commandEncoder, in Rectangle viewport, in SamplerStateDescription samplerState)
        {
            _spriteBatch.Begin(commandEncoder, viewport, BlendStateDescription.Opaque, samplerState);
        }

        public void DrawText(string text)
        {
            // TODO
        }

        public void DrawImage(Texture texture, in Rectangle destinationRect, in Rectangle? sourceRect = null, in ColorRgbaF? tintColor = null)
        {
            _spriteBatch.Draw(
                texture, 
                sourceRect ?? new Rectangle(0, 0, texture.Width, texture.Height), 
                destinationRect,
                tintColor);
        }

        public void End()
        {
            _spriteBatch.End();
        }
    }
}
