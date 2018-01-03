using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
{
    public sealed class Movie : Character, IPlayable
    {
        public List<Frame> Frames { get; private set; }
        public List<Character> Characters { get; private set; }
        public List<Import> Imports { get; private set; }
        public List<Export> Exports { get; private set; }
        public uint ScreenWidth { get; private set; }
        public uint ScreenHeight { get; private set; }

        public static Movie Parse(BinaryReader reader, AptFile container)
        {
            var movie = new Movie();
            movie.Frames = reader.ReadListAtOffset<Frame>(() => Frame.Parse(reader));
            var unknown = reader.ReadUInt32();

            movie.Characters = reader.ReadListAtOffset<Character>(() => Character.Create(reader, container), true);

            movie.ScreenWidth = reader.ReadUInt32();
            movie.ScreenHeight = reader.ReadUInt32();
            var unknown2 = reader.ReadUInt32();

            movie.Imports = reader.ReadListAtOffset<Import>(() => Import.Parse(reader));
            movie.Exports = reader.ReadListAtOffset<Export>(() => Export.Parse(reader));

            return movie;
        }
    }
}
