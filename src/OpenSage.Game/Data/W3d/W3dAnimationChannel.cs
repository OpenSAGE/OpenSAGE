using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dAnimationChannel
    {
        public ushort FirstFrame { get; private set; }

        public ushort LastFrame { get; private set; }

        /// <summary>
        /// Length of each vector in this channel.
        /// </summary>
        public ushort VectorLength { get; private set; }

        public W3dAnimationChannelType ChannelType { get; private set; }

        /// <summary>
        /// Pivot affected by this channel.
        /// </summary>
        public ushort Pivot { get; private set; }

        public float[,] Data { get; private set; }

        public static W3dAnimationChannel Parse(BinaryReader reader, uint chunkSize)
        {
            var startPosition = reader.BaseStream.Position;

            var result = new W3dAnimationChannel
            {
                FirstFrame = reader.ReadUInt16(),
                LastFrame = reader.ReadUInt16(),
                VectorLength = reader.ReadUInt16(),
                ChannelType = reader.ReadUInt16AsEnum<W3dAnimationChannelType>(),
                Pivot = reader.ReadUInt16()
            };

            reader.ReadUInt16(); // Pad

            var numElements = result.LastFrame - result.FirstFrame + 1;
            var data = new float[numElements, result.VectorLength];

            for (var i = 0; i < numElements; i++)
            {
                for (var j = 0; j < result.VectorLength; j++)
                {
                    data[i, j] = reader.ReadSingle();
                }
            }

            result.Data = data;

            // Pad
            var endPosition = startPosition + chunkSize;
            reader.ReadBytes((int) (endPosition - reader.BaseStream.Position));

            return result;
        }
    }
}
