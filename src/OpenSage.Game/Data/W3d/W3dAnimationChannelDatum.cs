using System.IO;
using System.Runtime.InteropServices;

namespace OpenSage.Data.W3d
{
    [StructLayout(LayoutKind.Explicit)]
    public struct W3dAnimationChannelDatum
    {
        [FieldOffset(0)]
        public W3dQuaternion Quaternion;

        [FieldOffset(0)]
        public float FloatValue;

        public static W3dAnimationChannelDatum Parse(BinaryReader reader, W3dAnimationChannelType channelType)
        {
            switch (channelType)
            {
                case W3dAnimationChannelType.Quaternion:
                    return new W3dAnimationChannelDatum
                    {
                        Quaternion = W3dQuaternion.Parse(reader)
                    };

                case W3dAnimationChannelType.TranslationX:
                case W3dAnimationChannelType.TranslationY:
                case W3dAnimationChannelType.TranslationZ:
                    return new W3dAnimationChannelDatum
                    {
                        FloatValue = reader.ReadSingle()
                    };

                default:
                    throw new InvalidDataException();
            }
        }
    }
}
