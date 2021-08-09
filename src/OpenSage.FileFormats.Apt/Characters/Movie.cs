using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt.Characters
{
    public sealed class Movie : Playable
    {
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
            var unknown = reader.ReadUInt32();

            movie.Characters = reader.ReadListAtOffset<Character>(() => Character.Create(reader, container), true);

            movie.ScreenWidth = reader.ReadUInt32();
            movie.ScreenHeight = reader.ReadUInt32();
            movie.MillisecondsPerFrame = reader.ReadUInt32();

            movie.Imports = reader.ReadListAtOffset<Import>(() => Import.Parse(reader));
            movie.Exports = reader.ReadListAtOffset<Export>(() => Export.Parse(reader));

            return movie;
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
