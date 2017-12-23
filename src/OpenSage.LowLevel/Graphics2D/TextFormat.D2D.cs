using DW = SharpDX.DirectWrite;

namespace OpenSage.LowLevel.Graphics2D
{
    partial class TextFormat
    {
        internal DW.TextFormat DeviceTextFormat { get; private set; }

        private void PlatformConstruct(GraphicsDevice2D graphicsDevice, string fontFamily, float fontSize, FontWeight fontWeight, TextAlignment alignment)
        {
            DeviceTextFormat = AddDisposable(new DW.TextFormat(
                graphicsDevice.DirectWriteFactory,
                fontFamily,
                ToDirectWriteFontWeight(fontWeight),
                DW.FontStyle.Normal,
                DW.FontStretch.Normal,
                fontSize));

            DeviceTextFormat.TextAlignment = ToDirectWriteTextAlignment(alignment);

            // TODO: Make this configurable.
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

        private static DW.TextAlignment ToDirectWriteTextAlignment(TextAlignment value)
        {
            switch (value)
            {
                case TextAlignment.Center:
                    return DW.TextAlignment.Center;

                case TextAlignment.Leading:
                    return DW.TextAlignment.Leading;

                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}
