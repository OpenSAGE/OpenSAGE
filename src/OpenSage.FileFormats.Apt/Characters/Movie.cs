using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt.Characters
{
    public sealed class Movie : Playable
    {
        public uint Unknown;
        public List<Character> Characters { get; private set; }
        public List<Import> Imports { get; private set; }
        public List<Export> Exports { get; private set; }
        public uint ScreenWidth { get; private set; }
        public uint ScreenHeight { get; private set; }
        public uint MillisecondsPerFrame { get; private set; }

        public static Movie Parse(BinaryReader reader, AptFile container)
        {
            var movie = new Movie();
            movie.Frames = reader.ReadListAtOffset<Frame>(() => Frame.Parse(reader));
            movie.Unknown = reader.ReadUInt32();

            movie.Characters = reader.ReadListAtOffset<Character>(() => Character.Create(reader, container), true);

            movie.ScreenWidth = reader.ReadUInt32();
            movie.ScreenHeight = reader.ReadUInt32();
            movie.MillisecondsPerFrame = reader.ReadUInt32();

            movie.Imports = reader.ReadListAtOffset<Import>(() => Import.Parse(reader));
            movie.Exports = reader.ReadListAtOffset<Export>(() => Export.Parse(reader));

            return movie;
        }

        public override void Write(BinaryWriter writer, MemoryPool pool)
        {
            writer.Write((UInt32) CharacterType.Movie);
            writer.Write((UInt32) Character.SIGNATURE);

            writer.WriteArrayAtOffsetWithSize(Frames, pool);
            writer.Write(Unknown);
            Func<int, BinaryWriter, MemoryPool, bool> f = (i, w, p) =>
            {
                var chr = Characters[i];
                if (chr == null || i == 0) // TODO i = 0 's address is itself; repair it after the dump is complete
                {
                    return false;
                }
                else
                    chr.Write(w, p);
                return true;
            };
            writer.WriteArrayAtOffsetWithSize(Characters.Count, f, pool, true);

            writer.Write((UInt32) ScreenWidth);
            writer.Write((UInt32) ScreenHeight);
            writer.Write((UInt32) MillisecondsPerFrame);

            writer.WriteArrayAtOffsetWithSize(Imports, pool);
            writer.WriteArrayAtOffsetWithSize(Exports, pool);
        }

        public static Movie CreateEmpty(AptFile container, int width, int height, int millisecondsPerFrame)
        {
            if (container.Movie != null)
            {
                throw new ArgumentException("AptFile already has a Movie");
            }
            var movie = new Movie
            {
                Frames = new List<Frame>(),
                Characters = new List<Character>(),
                Imports = new List<Import>(),
                Exports = new List<Export>(),
                ScreenWidth = (uint) width,
                ScreenHeight = (uint) height,
                MillisecondsPerFrame = (uint) millisecondsPerFrame
            };
            movie.Characters.Add(movie);
            movie.Container = container;
            return movie;
        }
    }
}
