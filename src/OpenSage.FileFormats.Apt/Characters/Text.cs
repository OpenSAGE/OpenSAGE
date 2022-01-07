using System.IO;
using System;
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

        public override void Write(BinaryWriter writer, BinaryMemoryChain pool)
        {
            writer.Write((UInt32) CharacterType.Text);
            writer.Write((UInt32) Character.SIGNATURE);
            writer.Write((RectangleF) Bounds);
            writer.Write((UInt32) Font);
            writer.Write((UInt32) Alignment);
            writer.Write((ColorRgba) Color);
            writer.Write((Single) FontHeight);
            writer.WriteBooleanUInt32(ReadOnly);
            writer.WriteBooleanUInt32(Multiline);
            writer.WriteBooleanUInt32(WordWrap);

            writer.WriteStringAtOffset(Content, pool);
            writer.WriteStringAtOffset(Value, pool);
        }
    }
}
