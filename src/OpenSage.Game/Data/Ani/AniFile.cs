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

        public uint IconWidth { get; private set; }
        public uint IconHeight { get; private set; }

        public uint HotspotX { get; private set; }
        public uint HotspotY { get; private set; }

        public RateChunkContent Rates { get; private set; }
        public SequenceChunkContent Sequence { get; private set; }

        public AniCursorImage[] Images { get; private set; }

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
                                    var iconDirEntry = iconChunkContent.IconDirEntries[0];

                                    if (result.IconWidth == 0)
                                    {
                                        result.IconWidth = iconDirEntry.Width;
                                        result.IconHeight = iconDirEntry.Height;

                                        result.HotspotX = iconDirEntry.HotspotX;
                                        result.HotspotY = iconDirEntry.HotspotY;
                                    }
                                    else
                                    {
                                        if (result.IconWidth != iconDirEntry.Width || result.IconHeight != iconDirEntry.Height)
                                        {
                                            throw new InvalidDataException();
                                        }
                                    }

                                    var icon = iconChunkContent.Images[0];

                                    var pixels = new byte[result.IconWidth * result.IconHeight * 4];

                                    var index = 0;
                                    for (var y = 0; y < result.IconHeight; y++)
                                    {
                                        for (var x = 0; x < result.IconWidth; x++)
                                        {
                                            var pixelIndex = (y * result.IconWidth) + x;

                                            var isTransparent = icon.AndMask.Pixels[pixelIndex] == 1;

                                            var color = icon.ColorTable.Entries[icon.XorMask.Pixels[pixelIndex]];

                                            pixels[index++] = color.Blue;
                                            pixels[index++] = color.Green;
                                            pixels[index++] = color.Red;
                                            pixels[index++] = isTransparent ? (byte) 0 : (byte) 255;
                                        }
                                    }

                                    result.Images[iconIndex] = new AniCursorImage(pixels);

                                    iconIndex++;
                                }
                                break;
                        }
                        break;

                    case AniHeaderChunkContent anih:
                        result.Images = new AniCursorImage[anih.NumFrames];
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
