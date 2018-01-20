using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
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
            var glyphCount = reader.ReadUInt32();
            var glyphOffset = reader.ReadUInt32();

            var currentPos = reader.BaseStream.Position;
            reader.BaseStream.Seek(glyphOffset, SeekOrigin.Begin);

            font.Glyphs = new List<uint>();
            for (int i=0;i<glyphCount;++i)
            {
                font.Glyphs.Add(reader.ReadUInt32()); 
            }

            reader.BaseStream.Seek(currentPos, SeekOrigin.Begin);
           

            return font;
        }
    }
}
