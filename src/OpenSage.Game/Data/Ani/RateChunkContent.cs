using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.Ani
{
    public sealed class RateChunkContent : RiffChunkContent
    {
        public uint[] Durations { get; private set; }

        internal static RateChunkContent Parse(BinaryReader reader, long endPosition)
        {
            var durations = new List<uint>();

            while (reader.BaseStream.Position < endPosition)
            {
                durations.Add(reader.ReadUInt32());
            }

            return new RateChunkContent
            {
                Durations = durations.ToArray()
            };
        }
    }
}
