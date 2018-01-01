using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Ani
{
    public sealed class RiffChunk
    {
        public string ChunkType { get; private set; }
        public uint Size { get; private set; }

        public RiffChunkContent Content { get; private set; }

        internal static RiffChunk Parse(BinaryReader reader)
        {
            var chunkType = reader.ReadFourCc();
            var chunkSize = reader.ReadUInt32();

            var endPosition = reader.BaseStream.Position + chunkSize;

            // If this is the RIFF chunk, the chunk size includes the chunk type and chunk size values.
            if (chunkType == "RIFF")
            {
                endPosition -= 8;
            }

            RiffChunkContent content;
            switch (chunkType)
            {
                case "RIFF":
                case "LIST":
                    content = RiffChunkList.Parse(reader, endPosition);
                    break;

                case "INAM":
                case "IART":
                    content = InfoChunkContent.Parse(reader);
                    break;

                case "anih":
                    content = AniHeaderChunkContent.Parse(reader);
                    break;

                case "rate":
                    content = RateChunkContent.Parse(reader, endPosition);
                    break;

                case "seq ":
                    content = SequenceChunkContent.Parse(reader, endPosition);
                    break;

                case "icon":
                    content = IconChunkContent.Parse(reader, endPosition);
                    break;

                default:
                    throw new InvalidDataException($"Chunk type not supported: {chunkType}");
            }

            if (reader.BaseStream.Position != endPosition)
            {
                throw new InvalidDataException();
            }

            if (chunkSize % 2 != 0)
            {
                // Pad byte.
                reader.ReadByte();
            }

            return new RiffChunk
            {
                ChunkType = chunkType,
                Size = chunkSize,
                Content = content
            };
        }
    }
}
