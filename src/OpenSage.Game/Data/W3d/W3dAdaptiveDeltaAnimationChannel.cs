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

        private static float[] Table = CalculateTable();

        private static float[] CalculateTable()
        {
            var result = new float[256];

            for (int i = 0; i < 16; ++i)
            {
                result[i] = (float) Math.Pow(10, i - 8.0);
            }

            for (int i = 0; i < 240; ++i)
            {
                float num = i / 240.0f;
                result[i + 16] = (float) (1.0 - Math.Sin(90.0 * num * Math.PI / 180.0));
            }

            return result;
        }

        private static int[] ReadDeltaBlock(BinaryReader reader, int nBits)
        {
            var deltaBytes = reader.ReadBytes(nBits * 2);
            var deltas = new int[16];

            for (int i = 0; i < deltaBytes.Length; ++i)
            {
                if (nBits == 4)
                {
                    deltas[i * 2] = deltaBytes[i] & 0x0F;
                    if ((deltas[i * 2] & 0x08) > 0)
                        deltas[i * 2] = -(deltas[i * 2] - 8);

                    deltas[i * 2 + 1] = deltaBytes[i] >> 4;
                    if ((deltas[i * 2 + 1] & 0x08) > 0)
                        deltas[i * 2 + 1] = -(deltas[i * 2 + 1] - 8);
                }
                else if (nBits == 8)
                {
                    deltas[i] = (sbyte) deltaBytes[i];
                }
            }

            return deltas;
        }

        private static float GetValue(W3dAnimationChannelDatum datum, W3dAnimationChannelType type, int i = 0)
        {
            if (i > 0 && type != W3dAnimationChannelType.Quaternion)
            {
                throw new InvalidDataException();
            }

            switch (type)
            {
                case W3dAnimationChannelType.TranslationX:
                case W3dAnimationChannelType.TranslationY:
                case W3dAnimationChannelType.TranslationZ:
                case W3dAnimationChannelType.XR:
                case W3dAnimationChannelType.YR:
                case W3dAnimationChannelType.ZR:
                    return datum.FloatValue;
                case W3dAnimationChannelType.Quaternion:
                    switch (i)
                    {
                        case 0:
                            return datum.Quaternion.W;
                        case 1:
                            return datum.Quaternion.X;
                        case 2:
                            return datum.Quaternion.Y;                        
                        case 3:
                            return datum.Quaternion.Z;
                        default:
                            throw new InvalidOperationException();
                    }
                    break;
                case W3dAnimationChannelType.UnknownBfme:
                default:
                    throw new NotImplementedException();
            }
        }

        private static void UpdateDatum(ref W3dAnimationChannelDatum datum, float value, W3dAnimationChannelType type, int i = 0)
        {
            if (i > 0 && type != W3dAnimationChannelType.Quaternion)
            {
                throw new InvalidDataException();
            }

            switch (type)
            {
                case W3dAnimationChannelType.TranslationX:
                case W3dAnimationChannelType.TranslationY:
                case W3dAnimationChannelType.TranslationZ:
                case W3dAnimationChannelType.XR:
                case W3dAnimationChannelType.YR:
                case W3dAnimationChannelType.ZR:
                    datum.FloatValue = value;
                    break;
                case W3dAnimationChannelType.Quaternion:
                    switch (i)
                    {
                        case 0:
                            datum.Quaternion.W = value;
                            break;
                        case 1:
                            datum.Quaternion.X = value;
                            break;
                        case 2:
                            datum.Quaternion.Y = value;
                            break;
                        case 3:
                            datum.Quaternion.Z = value;
                            break;
                    }
                    break;
                case W3dAnimationChannelType.UnknownBfme:
                    throw new NotImplementedException();
            }
        }

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
                UpdateDatum(ref result.Data[0], initial, result.ChannelType, k);

                for (int i = 0; i < count; ++i)
                {
                    var blockIndex = reader.ReadByte();
                    var blockScale = Table[blockIndex];
                    var deltaScale = blockScale * result.Scale;

                    //read deltas for the next 
                    var deltas = ReadDeltaBlock(reader, nBits);

                    for (int j = 0; j < deltas.Length; ++j)
                    {
                        int idx = i * 16 + j + 1;

                        if (idx >= result.NumTimeCodes)
                            break;

                        var value = GetValue(result.Data[idx - 1],result.ChannelType,k) + deltaScale * deltas[j];
                        UpdateDatum(ref result.Data[idx], value, result.ChannelType,k);
                    }
                }
            }

            reader.ReadBytes(3);

            //reader.BaseStream.Seek(startPosition + chunkSize, SeekOrigin.Begin);

            return result;
        }
    }
}
