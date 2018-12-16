using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.Utilities.Extensions;
using System;

namespace OpenSage.Data.W3d
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct W3dAnimationChannelDatum
    {
        [FieldOffset(0)]
        public readonly Quaternion Quaternion;

        [FieldOffset(0)]
        public readonly float FloatValue;

        public W3dAnimationChannelDatum(in Quaternion quaternion)
        {
            FloatValue = 0;
            Quaternion = quaternion;
        }

        public W3dAnimationChannelDatum(float floatValue)
        {
            Quaternion = new Quaternion();
            FloatValue = floatValue;
        }

        internal static W3dAnimationChannelDatum Parse(
            BinaryReader reader,
            W3dAnimationChannelType channelType)
        {
            switch (channelType)
            {
                case W3dAnimationChannelType.Quaternion:
                    return new W3dAnimationChannelDatum(reader.ReadQuaternion());

                case W3dAnimationChannelType.TranslationX:
                case W3dAnimationChannelType.TranslationY:
                case W3dAnimationChannelType.TranslationZ:
                    return new W3dAnimationChannelDatum(reader.ReadSingle());

                case W3dAnimationChannelType.UnknownBfme:
                    return new W3dAnimationChannelDatum(reader.ReadSingle());

                default:
                    throw new InvalidDataException();
            }
        }

        internal void WriteTo(
            BinaryWriter writer,
            W3dAnimationChannelType channelType)
        {
            switch (channelType)
            {
                case W3dAnimationChannelType.Quaternion:
                    writer.Write(Quaternion);
                    break;

                case W3dAnimationChannelType.TranslationX:
                case W3dAnimationChannelType.TranslationY:
                case W3dAnimationChannelType.TranslationZ:
                case W3dAnimationChannelType.UnknownBfme:
                    writer.Write(FloatValue);
                    break;

                default:
                    throw new InvalidDataException();
            }
        }

        public override string ToString()
        {
            return $"Quaterion = {Quaternion}, FloatValue = {FloatValue}";
        }

        public float GetValue(int index)
        {
            switch (index)
            {
                case 0:
                    return Quaternion.X;

                case 1:
                    return Quaternion.Y;

                case 2:
                    return Quaternion.Z;

                case 3:
                    return Quaternion.W;

                default:
                    throw new InvalidOperationException();
            }
        }

        public W3dAnimationChannelDatum WithValue(float value, int index)
        {
            switch (index)
            {
                case 0:
                    return new W3dAnimationChannelDatum(
                        new Quaternion(
                            value,
                            Quaternion.Y,
                            Quaternion.Z,
                            Quaternion.W));

                case 1:
                    return new W3dAnimationChannelDatum(
                        new Quaternion(
                            Quaternion.X,
                            value,
                            Quaternion.Z,
                            Quaternion.W));

                case 2:
                    return new W3dAnimationChannelDatum(
                        new Quaternion(
                            Quaternion.X,
                            Quaternion.Y,
                            value,
                            Quaternion.W));

                case 3:
                    return new W3dAnimationChannelDatum(
                        new Quaternion(
                            Quaternion.X,
                            Quaternion.Y,
                            Quaternion.Z,
                            value));

                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
