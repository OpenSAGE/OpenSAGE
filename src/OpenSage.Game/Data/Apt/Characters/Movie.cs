using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
{
    public class Movie : Character
    {
        public List<Frame> Frames { get; private set; }
        public List<Character> Characters { get; private set; }
        public List<Import> Imports { get; private set; }
        public List<Export> Exports { get; private set; }
        public uint ScreenWidth { get; private set; }
        public uint ScreenHeight { get; private set; }

        public static Movie Parse(BinaryReader reader, AptFile container)
        {
            var m = new Movie();
            m.Frames = reader.ReadListAtOffset<Frame>(() => Frame.Parse(reader));
            var unknown = reader.ReadUInt32();

            m.Characters = reader.ReadListAtOffset<Character>(() => Character.Create(reader, container), true);

            m.ScreenWidth = reader.ReadUInt32();
            m.ScreenHeight = reader.ReadUInt32();
            var unknown2 = reader.ReadUInt32();

            m.Imports = reader.ReadListAtOffset<Import>(() => Import.Parse(reader));
            m.Exports = reader.ReadListAtOffset<Export>(() => Export.Parse(reader));

            return m;
        }
    }
}
