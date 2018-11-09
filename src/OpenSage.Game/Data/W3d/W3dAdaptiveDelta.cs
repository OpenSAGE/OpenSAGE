using System;
using System.IO;

namespace OpenSage.Data.W3d
{
    class W3dAdaptiveDelta
    {
        public static float[] Table = CalculateTable();

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

        public static sbyte[] ReadDeltaBlock(BinaryReader reader, int nBits)
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
                    deltas[i] = (sbyte) deltaBytes[i];
                }
            }

            return deltas;
        }

        public static float GetValue(W3dAnimationChannelDatum datum, W3dAnimationChannelType type, int i = 0)
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
                case W3dAnimationChannelType.UnknownBfme:
                default:
                    throw new NotImplementedException();
            }
        }

        public static void UpdateDatum(ref W3dAnimationChannelDatum datum, float value, W3dAnimationChannelType type, int i = 0)
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
                case W3dAnimationChannelType.UnknownBfme:
                    throw new NotImplementedException();
            }
        }
    }
}
