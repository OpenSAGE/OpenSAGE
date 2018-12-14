using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.Utilities.Extensions;
using System.Diagnostics;

namespace OpenSage.Data.W3d
{
    [StructLayout(LayoutKind.Explicit)]
    public struct W3dAnimationChannelDatum
    {
        [FieldOffset(0)]
        public Quaternion Quaternion;

        [FieldOffset(0)]
        public float FloatValue;

        public static W3dAnimationChannelDatum Parse(BinaryReader reader, W3dAnimationChannelType channelType)
        {
            switch (channelType)
            {
                case W3dAnimationChannelType.Quaternion:
                    return new W3dAnimationChannelDatum
                    {
                        Quaternion = reader.ReadQuaternion()
                    };

                case W3dAnimationChannelType.TranslationX:
                case W3dAnimationChannelType.TranslationY:
                case W3dAnimationChannelType.TranslationZ:
                    return new W3dAnimationChannelDatum
                    {
                        FloatValue = reader.ReadSingle()
                    };

                case W3dAnimationChannelType.UnknownBfme:
                    return new W3dAnimationChannelDatum
                    {
                        FloatValue = reader.ReadSingle()
                    };

                default:
                    throw new InvalidDataException();
            }
        }

        internal void WriteTo(BinaryWriter writer, W3dAnimationChannelType channelType)
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
    }
}
