using System.IO;
using System.Runtime.InteropServices;

namespace OpenSage.Data.W3d
{
    /// <summary>
    /// Deform keyframe information. Each keyframe is made up of a set of per-vert deform data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dDeformKeyframeInfo
    {
        public float DeformPercent;
        public uint DataCount;

        public static W3dDeformKeyframeInfo Parse(BinaryReader reader)
        {
            var result = new W3dDeformKeyframeInfo
            {
                DeformPercent = reader.ReadSingle(),
                DataCount = reader.ReadUInt32()
            };

            reader.ReadBytes(2 * sizeof(uint)); // Reserved

            return result;
        }
    }
}
