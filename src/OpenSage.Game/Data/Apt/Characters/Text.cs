using System;
using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Mathematics;

namespace OpenSage.Data.Apt.Characters
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
        public string Content { get; internal set; }
        public string Value { get; private set; }

        public static Text Parse(BinaryReader reader)
        {
            var text = new Text();
            text.Bounds = reader.ReadRectangleF();
            text.Font = reader.ReadUInt32();
            text.Alignment = reader.ReadUInt32();
            text.Color = reader.ReadColorRgba();
            text.FontHeight = reader.ReadSingle();
            text.ReadOnly = reader.ReadBooleanUInt32Checked();
            text.Multiline = reader.ReadBooleanUInt32Checked();
            text.WordWrap = reader.ReadBooleanUInt32Checked();
            text.Content = reader.ReadStringAtOffset();
            text.Value = reader.ReadStringAtOffset();
            return text;
        }
    }
}
