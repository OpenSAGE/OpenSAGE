using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt.Characters
{
    public sealed class Movie : Playable
    {
        public uint Unknown;
        [DataStorageList]
        public List<Character> Characters { get; private set; }
        [DataStorageList]
        public List<Import> Imports { get; private set; }
        [DataStorageList]
        public List<Export> Exports { get; private set; }
        public uint ScreenWidth { get; private set; }
        public uint ScreenHeight { get; private set; }
        public uint MillisecondsPerFrame { get; private set; }

        public static Movie Parse(BinaryReader reader, AptFile container, Dictionary<long, Character> readCharacters)
        {
            var movie = new Movie();
            var startPos = reader.BaseStream.Position - 8;
            readCharacters[startPos] = movie;

            movie.Frames = reader.ReadListAtOffset<Frame>(() => Frame.Parse(reader));
            movie.Unknown = reader.ReadUInt32();

            movie.Characters = reader.ReadListAtOffset<Character>(() => Create(reader, container, readCharacters), true);

            movie.ScreenWidth = reader.ReadUInt32();
            movie.ScreenHeight = reader.ReadUInt32();
            movie.MillisecondsPerFrame = reader.ReadUInt32();

            movie.Imports = reader.ReadListAtOffset<Import>(() => Import.Parse(reader));
            movie.Exports = reader.ReadListAtOffset<Export>(() => Export.Parse(reader));

            return movie;
        }

        public override void Write(BinaryWriter writer, MemoryPool pool)
        {
            Write(writer, pool, false, null);
        }

        public void Write(BinaryWriter writer, MemoryPool poolPost, bool writeHead = false, Dictionary<Movie, (MemoryPool, uint)> additional = null)
        {
            if (writeHead)
            {
                writer.Write(Encoding.ASCII.GetBytes(Constants.AptFileHeader));
                writer.Write(Encoding.ASCII.GetBytes(Constants.OpenSageAptEditorCredits));
                writer.BaseStream.Seek(Constants.AptFileStartPos, SeekOrigin.Begin);
            }

            if (additional == null)
                additional = new();
            var startPos = writer.BaseStream.Position;
            additional[this] = (poolPost.Pre, (uint) startPos);

            writer.Write((UInt32) CharacterType.Movie);
            writer.Write((UInt32) Character.SIGNATURE);

            writer.WriteArrayAtOffsetWithSize(Frames, poolPost);
            writer.Write(Unknown);
            Func<int, BinaryWriter, MemoryPool, bool> f = (i, w, p) =>
            {
                var chr = Characters[i];
                var flag = true;
                if (chr == null) 
                    flag = false; // pointer remains 0
                else if (chr is Movie mv) // sometimes chr is already written, rewriting it will cause endless recursion, so just register a pointer
                {
                    if (additional.TryGetValue(mv, out var pool_pos))
                    {
                        var post_pool = p.Pre;
                        var this_pool = post_pool.Pre;
                        this_pool.RegisterPreOffset(pool_pos.Item1, pool_pos.Item2);
                        flag = false; // registered pointer for writing is automatically removed
                    }
                    else
                    {
                        mv.Write(w, p, false, additional);
                        flag = true;
                    }
                }
                else
                    chr.Write(w, p);
                return flag;
            };
            writer.WriteArrayAtOffsetWithSize(Characters.Count, f, poolPost, true);

            writer.Write((UInt32) ScreenWidth);
            writer.Write((UInt32) ScreenHeight);
            writer.Write((UInt32) MillisecondsPerFrame);

            writer.WriteArrayAtOffsetWithSize(Imports, poolPost);
            writer.WriteArrayAtOffsetWithSize(Exports, poolPost);
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
