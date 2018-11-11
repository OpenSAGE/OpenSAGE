using System;
using System.IO;

namespace OpenSage.Data.W3d
{
    class W3dAdaptiveDelta
    {
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

        private static sbyte[] ReadDeltaBlock(BinaryReader reader, int nBits)
        {
            var deltaBytes = new sbyte[nBits * 2];
            for (int i = 0; i < nBits * 2; ++i)
            {
                deltaBytes[i] = reader.ReadSByte();
            }

            var deltas = new sbyte[16];

            for (int i = 0; i < deltaBytes.Length; ++i)
            {
                if (nBits == 4)
                {
                    deltas[i * 2] = deltaBytes[i];
                    if ((deltas[i * 2] & 8) != 0)
                    {
                        deltas[i * 2] = (sbyte) (deltas[i * 2] | 0xF0);
                    }
                    else
                    {
                        deltas[i * 2] &= 0x0F;
                    }
                    deltas[i * 2 + 1] = (sbyte) (deltaBytes[i] >> 4);
                }
                else if (nBits == 8)
                {
                    var val = (byte)deltaBytes[i];
                    //do a bitflip
                    if ((val & 0x80) != 0)
                    {
                        val &= 0x7F;
                    }
                    else
                    {
                        val |= 0x80;
                    }
                    deltas[i] = (sbyte)val;
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
                case W3dAnimationChannelType.UnknownBfme:
                    return datum.FloatValue;
                case W3dAnimationChannelType.Quaternion:
                    switch (i)
                    {
                        case 0:
                            return datum.Quaternion.X;
                        case 1:
                            return datum.Quaternion.Y;
                        case 2:
                            return datum.Quaternion.Z;
                        case 3:
                            return datum.Quaternion.W;
                        default:
                            throw new InvalidOperationException();
                    }
                    break;
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
                case W3dAnimationChannelType.UnknownBfme:
                    datum.FloatValue = value;
                    break;
                case W3dAnimationChannelType.Quaternion:
                    switch (i)
                    {
                        case 0:
                            datum.Quaternion.X = value;
                            break;
                        case 1:
                            datum.Quaternion.Y = value;
                            break;
                        case 2:
                            datum.Quaternion.Z = value;
                            break;
                        case 3:
                            datum.Quaternion.W = value;
                            break;
                    }
                    break;
            }
        }

        public static W3dAnimationChannelDatum[] ReadAdaptiveDelta(BinaryReader reader, uint numFrames,W3dAnimationChannelType type, int vectorLen, float scale, int nBits)
        {
            //when delta8 multiply the scale by 0.0625f
            float scaleFactor = (float)(Math.Pow(2, 4) / Math.Pow(2, nBits));
            uint count = (numFrames + 15) >> 4;
            var data = new W3dAnimationChannelDatum[numFrames];

            //First read all initial values
            for (int k = 0; k < vectorLen; ++k)
            {
                var initial = reader.ReadSingle();
                W3dAdaptiveDelta.UpdateDatum(ref data[0], initial, type, k);
            }

            //Then read the interleaved delta blocks
            for (int i = 0; i < count; ++i)
            {
                for (int k = 0; k < vectorLen; ++k)
                {
                    var blockIndex = reader.ReadByte();
                    var blockScale = W3dAdaptiveDelta.Table[blockIndex];
                    var deltaScale = blockScale * scale * scaleFactor;

                    //read deltas for the next 
                    var deltas = W3dAdaptiveDelta.ReadDeltaBlock(reader, nBits);

                    for (int j = 0; j < deltas.Length; ++j)
                    {
                        int idx = i * 16 + j + 1;

                        if (idx >= numFrames)
                            break;

                        var value = W3dAdaptiveDelta.GetValue(data[idx - 1], type, k) + deltaScale * deltas[j];
                        W3dAdaptiveDelta.UpdateDatum(ref data[idx], value, type, k);
                    }
                }             
            }

            return data;
        }
    }
}
