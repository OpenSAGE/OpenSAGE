namespace OpenSage.LowLevel.Graphics2D
{
    public sealed partial class TextFormat : DisposableBase
    {
        public TextFormat(GraphicsDevice2D graphicsDevice, string fontFamily, float fontSize, FontWeight fontWeight, TextAlignment alignment)
        {
            PlatformConstruct(graphicsDevice, fontFamily, fontSize, fontWeight, alignment);
        }
    }

    public enum TextAlignment
    {
        Leading,
        Center
    }
}
