using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Ani
{
    public sealed class RiffChunkList : RiffChunkContent
    {
        public string ListType { get; private set; }
        public RiffChunk[] Chunks { get; private set; }

        internal static RiffChunkList Parse(BinaryReader reader, long endPosition)
        {
            var listType = reader.ReadFourCc();

            var chunks = new List<RiffChunk>();

            // Skip to the end of unknown list chunks.
            switch (listType)
            {
                case "ACON":
                case "fram":
                case "INFO":
                    while (reader.BaseStream.Position < endPosition)
                    {
                        chunks.Add(RiffChunk.Parse(reader));
                    }
                    break;

                default:
                    reader.ReadBytes((int) (endPosition - reader.BaseStream.Position));
                    break;
            }

            return new RiffChunkList
            {
                ListType = listType,
                Chunks = chunks.ToArray()
            };
        }
    }
}
