using System.IO;
using System;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.Apt.FrameItems
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
        public override void Write(BinaryWriter writer, BinaryMemoryChain pool)
        {
            writer.Write((UInt32) FrameItemType.BackgroundColor);
            writer.Write(Color);
        }
    }
}
