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

        public W3dAnimationChannelDatum[] Data { get; private set; }

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

            ValidateChannelDataSize(result.ChannelType, result.VectorLength);

            var numElements = result.LastFrame - result.FirstFrame + 1;
            var data = new W3dAnimationChannelDatum[numElements];

            for (var i = 0; i < numElements; i++)
            {
                data[i] = W3dAnimationChannelDatum.Parse(reader, result.ChannelType);
            }

            result.Data = data;

            // Pad
            var endPosition = startPosition + chunkSize;
            reader.ReadBytes((int) (endPosition - reader.BaseStream.Position));

            return result;
        }

        internal static void ValidateChannelDataSize(W3dAnimationChannelType channelType, int vectorLength)
        {
            switch (channelType)
            {
                case W3dAnimationChannelType.Quaternion:
                    if (vectorLength != 4)
                    {
                        throw new InvalidDataException();
                    }
                    break;

                case W3dAnimationChannelType.TranslationX:
                case W3dAnimationChannelType.TranslationY:
                case W3dAnimationChannelType.TranslationZ:
                    if (vectorLength != 1)
                    {
                        throw new InvalidDataException();
                    }
                    break;

                default:
                    throw new InvalidDataException();
            }
        }
    }
}
