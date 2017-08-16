using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Ani
{
    public sealed class RiffChunkList : RiffChunkContent
    {
        public string ListType { get; private set; }
        public RiffChunk[] Chunks { get; private set; }

        internal static RiffChunkList Parse(BinaryReader reader, long endPosition)
        {
            var listType = reader.ReadUInt32().ToFourCcString();

            var chunks = new List<RiffChunk>();
            while (reader.BaseStream.Position < endPosition)
            {
                chunks.Add(RiffChunk.Parse(reader));
            }

            return new RiffChunkList
            {
                ListType = listType,
                Chunks = chunks.ToArray()
            };
        }
    }
}
