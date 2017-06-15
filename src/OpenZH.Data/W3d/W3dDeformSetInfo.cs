using System.IO;
using System.Runtime.InteropServices;

namespace OpenZH.Data.W3d
{
    /// <summary>
    /// Deform set information. Each set is made up of a series of keyframes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dDeformSetInfo
    {
        public uint KeyframeCount;
        public W3dDeformSetFlags Flags;

        public static W3dDeformSetInfo Parse(BinaryReader reader)
        {
            var result = new W3dDeformSetInfo
            {
                KeyframeCount = reader.ReadUInt32(),
                Flags = (W3dDeformSetFlags) reader.ReadUInt32()
            };

            reader.ReadBytes(sizeof(uint)); // Reserved

            return result;
        }
    }
}
