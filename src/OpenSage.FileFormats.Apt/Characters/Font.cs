using System.Collections.Generic;
using System.IO;
using System;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt.Characters
{
    public struct FontGlyph
    {

    }

    public sealed class Font : Character
    {
        public string Name { get; private set; }
        //A reference to the shapes used by this character
        public List<uint> Glyphs { get; private set; }

        public static Font Parse(BinaryReader reader)
        {
            var font = new Font();
            font.Name = reader.ReadStringAtOffset();
            font.Glyphs = reader.ReadListAtOffset<uint>(() => reader.ReadUInt32());
            return font;
        }
        public override void Write(BinaryWriter writer, MemoryPool memory)
        {
            writer.Write((UInt32) CharacterType.Font);
            writer.Write((UInt32) Character.SIGNATURE);

            writer.WriteStringAtOffset(Name, memory);
            writer.WriteArrayAtOffsetWithSize(Glyphs.Count, (i, w, p) => writer.Write(Glyphs[i]), memory);
        }
    }
}
