using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dChunkHeader
    {
        public const int SizeInBytes = sizeof(uint);

        /// <summary>
        /// Size of the chunk, (not including the chunk header)
        /// </summary>
        public uint ChunkSize { get; private set; }

        public bool HasSubChunks { get; private set; }

        internal static W3dChunkHeader Parse(BinaryReader reader)
        {
            var result = new W3dChunkHeader();

            var chunkSize = reader.ReadUInt32();
            result.ChunkSize = chunkSize & 0x7FFFFFFF;
            result.HasSubChunks = (chunkSize >> 31) == 1;

            return result;
        }

        internal W3dChunkHeader(uint chunkSize, bool hasSubChunks)
        {
            ChunkSize = chunkSize;
            HasSubChunks = hasSubChunks;
        }

        private W3dChunkHeader() { }

        internal void WriteTo(BinaryWriter writer)
        {
            var chunkSize = ChunkSize;

            if (HasSubChunks)
            {
                chunkSize |= (1u << 31);
            }

            writer.Write(chunkSize);
        }
    }
}
