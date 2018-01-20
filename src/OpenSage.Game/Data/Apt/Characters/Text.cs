using System;
using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Mathematics;

namespace OpenSage.Data.Apt.Characters
{
    public sealed class Text : Character
    {
        public Vector4 Bounds { get; private set; }
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
            var text = new Text();
            text.Bounds = reader.ReadVector4();
            text.Font = reader.ReadUInt32();
            text.Alignment = reader.ReadUInt32();
            text.Color = reader.ReadColorRgba();
            text.FontHeight = reader.ReadSingle();
            text.ReadOnly = Convert.ToBoolean(reader.ReadUInt32());
            text.Multiline = Convert.ToBoolean(reader.ReadUInt32());
            text.WordWrap = Convert.ToBoolean(reader.ReadUInt32());
            text.Content = reader.ReadStringAtOffset();
            text.Value = reader.ReadStringAtOffset();
            return text;
        }
    }
}
