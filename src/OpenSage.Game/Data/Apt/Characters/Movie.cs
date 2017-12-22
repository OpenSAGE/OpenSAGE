using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
{
    public class Movie : Character
    {
        public List<Frame> Frames           { get; private set; }
        public List<Character> Characters   { get; private set; }
        public List<Import> Imports         { get; private set; }
        public List<Export> Exports         { get; private set; }
        public uint ScreenWidth      { get; private set; }
        public uint ScreenHeight     { get; private set; }

        public static Movie Parse(BinaryReader br,AptFile c)
        {
            var m = new Movie();
            m.Frames = br.ReadListAtOffset<Frame>(() => Frame.Parse(br));
            var unknown = br.ReadUInt32();

            m.Characters = br.ReadListAtOffset<Character>(() => Character.Create(br,c),true);

            m.ScreenWidth = br.ReadUInt32();
            m.ScreenHeight = br.ReadUInt32();
            var unknown2 = br.ReadUInt32();

            m.Imports = br.ReadListAtOffset<Import>(() => Import.Parse(br));
            m.Exports = br.ReadListAtOffset<Export>(() => Export.Parse(br));

            return m;
        }
    }
}
