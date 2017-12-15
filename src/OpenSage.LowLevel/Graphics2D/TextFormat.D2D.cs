using DW = SharpDX.DirectWrite;

namespace OpenSage.LowLevel.Graphics2D
{
    partial class TextFormat
    {
        internal DW.TextFormat DeviceTextFormat { get; private set; }

        private void PlatformConstruct(GraphicsDevice2D graphicsDevice, string fontFamily, float fontSize, FontWeight fontWeight)
        {
            DeviceTextFormat = AddDisposable(new DW.TextFormat(
                graphicsDevice.DirectWriteFactory,
                fontFamily,
                ToDirectWriteFontWeight(fontWeight),
                DW.FontStyle.Normal,
                DW.FontStretch.Normal,
                fontSize));

            // TODO: Make this configurable.
            DeviceTextFormat.TextAlignment = DW.TextAlignment.Center;
            DeviceTextFormat.ParagraphAlignment = DW.ParagraphAlignment.Center;
        }

        private static DW.FontWeight ToDirectWriteFontWeight(FontWeight value)
        {
            switch (value)
            {
                case FontWeight.Normal:
                    return DW.FontWeight.Normal;

                case FontWeight.Bold:
                    return DW.FontWeight.Bold;

                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}
