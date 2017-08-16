using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.Ani
{
    public sealed class SequenceChunkContent : RiffChunkContent
    {
        public uint[] FrameIndices { get; private set; }

        internal static SequenceChunkContent Parse(BinaryReader reader, long endPosition)
        {
            var frameIndices = new List<uint>();

            while (reader.BaseStream.Position < endPosition)
            {
                frameIndices.Add(reader.ReadUInt32());
            }

            return new SequenceChunkContent
            {
                FrameIndices = frameIndices.ToArray()
            };
        }
    }
}
