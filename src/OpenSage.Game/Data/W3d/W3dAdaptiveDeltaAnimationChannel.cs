using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Utilities;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dAdaptiveDeltaAnimationChannel
    {
        public uint NumTimeCodes { get; private set; }

        /// <summary>
        /// Pivot affected by this channel.
        /// </summary>
        public ushort Pivot { get; private set; }

        /// <summary>
        /// Length of each vector in this channel.
        /// </summary>
        public byte VectorLength { get; private set; }

        public W3dAnimationChannelType ChannelType { get; private set; }

        /// <summary>
        ///  Filter table scale.
        /// </summary>
        public float Scale { get; private set; }

        public W3dAnimationChannelDatum[] Data { get; private set; }

        public static W3dAdaptiveDeltaAnimationChannel Parse(BinaryReader reader, uint chunkSize, int nBits)
        {
            var startPosition = reader.BaseStream.Position;

            var result = new W3dAdaptiveDeltaAnimationChannel
            {
                NumTimeCodes = reader.ReadUInt32(),
                Pivot = reader.ReadUInt16(),
                VectorLength = reader.ReadByte(),
                ChannelType = EnumUtility.CastValueAsEnum<byte, W3dAnimationChannelType>(reader.ReadByte()),
                Scale = reader.ReadSingle(),
            };

            W3dAnimationChannel.ValidateChannelDataSize(result.ChannelType, result.VectorLength);

            uint count = (result.NumTimeCodes + 15) >> 4;
            result.Data = new W3dAnimationChannelDatum[result.NumTimeCodes];

            for (int k = 0; k < result.VectorLength; ++k)
            {
                var initial = reader.ReadSingle();
                W3dAdaptiveDelta.UpdateDatum(ref result.Data[0], initial, result.ChannelType, k);

                for (int i = 0; i < count; ++i)
                {
                    var blockIndex = reader.ReadByte();
                    var blockScale = W3dAdaptiveDelta.Table[blockIndex];
                    var deltaScale = blockScale * result.Scale;

                    //read deltas for the next 
                    var deltas = W3dAdaptiveDelta.ReadDeltaBlock(reader, nBits);

                    for (int j = 0; j < deltas.Length; ++j)
                    {
                        int idx = i * 16 + j + 1;

                        if (idx >= result.NumTimeCodes)
                            break;

                        var value = W3dAdaptiveDelta.GetValue(result.Data[idx - 1],result.ChannelType,k) + deltaScale * deltas[j];
                        W3dAdaptiveDelta.UpdateDatum(ref result.Data[idx], value, result.ChannelType,k);
                    }
                }
            }

            reader.ReadBytes(3);

            //reader.BaseStream.Seek(startPosition + chunkSize, SeekOrigin.Begin);

            return result;
        }
    }
}
