using System;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenSage.Data.Ani
{
    public sealed class AniFile
    {
        public string IconName { get; private set; }
        public string ArtistName { get; private set; }

        /// <summary>
        /// Default frame display rate (measured in 1/60th-of-a-second units)
        /// </summary>
        public uint DefaultFrameDisplayRate { get; private set; }

        public RateChunkContent Rates { get; private set; }
        public SequenceChunkContent Sequence { get; private set; }

        public CursorImage[] Images { get; private set; }

        public static AniFile FromFileSystemEntry(FileSystemEntry entry)
        {
            RiffChunk rootChunk;
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                rootChunk = RiffChunk.Parse(reader);
            }

            var result = new AniFile();

            foreach (var chunk in ((RiffChunkList) rootChunk.Content).Chunks)
            {
                switch (chunk.Content)
                {
                    case RiffChunkList list:
                        switch (list.ListType)
                        {
                            case "INFO":
                                var iconNameChunk = list.Chunks.FirstOrDefault(x => x.ChunkType == "INAM");
                                if (iconNameChunk != null)
                                {
                                    result.IconName = ((InfoChunkContent) iconNameChunk.Content).Value;
                                }

                                var iconArtistChunk = list.Chunks.FirstOrDefault(x => x.ChunkType == "IART");
                                if (iconArtistChunk != null)
                                {
                                    result.ArtistName = ((InfoChunkContent) iconArtistChunk.Content).Value;
                                }

                                break;

                            case "fram":
                                var iconIndex = 0;
                                foreach (var iconChunk in list.Chunks)
                                {
                                    if (iconChunk.ChunkType != "icon")
                                    {
                                        throw new InvalidDataException();
                                    }

                                    var iconChunkContent = (IconChunkContent) iconChunk.Content;

                                    result.Images[iconIndex++] = iconChunkContent.GetImage(0);
                                }
                                break;
                        }
                        break;

                    case AniHeaderChunkContent anih:
                        result.Images = new CursorImage[anih.NumFrames];
                        result.DefaultFrameDisplayRate = anih.DefaultFrameDisplayRate;
                        break;

                    case RateChunkContent rate:
                        result.Rates = rate;
                        break;

                    case SequenceChunkContent seq:
                        result.Sequence = seq;
                        break;

                    default:
                        throw new InvalidDataException();
                }
            }

            return result;
        }

        private static RiffChunk FindChunkRecursive(RiffChunk chunk, string chunkType)
        {
            if (chunk.ChunkType == chunkType)
            {
                return chunk;
            }

            if (chunk.Content is RiffChunkList list)
            {
                foreach (var subchunk in list.Chunks)
                {
                    var result = FindChunkRecursive(subchunk, chunkType);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}
