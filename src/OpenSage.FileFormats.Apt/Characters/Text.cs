using System.IO;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.Apt.Characters
{
    public sealed class Text : Character
    {
        public RectangleF Bounds { get; private set; }
        public uint Font { get; private set; }
        public uint Alignment { get; private set; }
        public ColorRgba Color { get; private set; }
        public float FontHeight { get; private set; }
        public bool ReadOnly { get; private set; }
        public bool Multiline { get; private set; }
        public bool WordWrap { get; private set; }
        public string Content { get; private set; }
        public string Value { get; private set; }

        public static Text Parse(BinaryReader reader)
        {
            var text = new Text
            {
                Bounds = reader.ReadRectangleF(),
                Font = reader.ReadUInt32(),
                Alignment = reader.ReadUInt32(),
                Color = reader.ReadColorRgba(),
                FontHeight = reader.ReadSingle(),
                ReadOnly = reader.ReadBooleanUInt32Checked(),
                Multiline = reader.ReadBooleanUInt32Checked(),
                WordWrap = reader.ReadBooleanUInt32Checked(),
                Content = reader.ReadStringAtOffset(),
                Value = reader.ReadStringAtOffset()
            };
            return text;
        }
    }
}
