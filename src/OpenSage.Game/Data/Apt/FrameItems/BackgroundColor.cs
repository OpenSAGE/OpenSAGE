using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Mathematics;

namespace OpenSage.Data.Apt.FrameItems
{
    public sealed class BackgroundColor : FrameItem
    {
        public ColorRgba Color { get; private set; }

        public static BackgroundColor Parse(BinaryReader reader)
        {
            var backgroundColor = new BackgroundColor();
            backgroundColor.Color = reader.ReadColorRgba();
            return backgroundColor;
        }
    }
}
