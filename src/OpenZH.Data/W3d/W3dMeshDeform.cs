using System.IO;
using System.Runtime.InteropServices;

namespace OpenZH.Data.W3d
{
    /// <summary>
    /// Deform information. Each mesh can have sets of keyframes of
    ///	deform info associated with it.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dMeshDeform
    {
        public uint SetCount;
        public uint AlphaPasses;

        public static W3dMeshDeform Parse(BinaryReader reader)
        {
            var result = new W3dMeshDeform
            {
                SetCount = reader.ReadUInt32(),
                AlphaPasses = reader.ReadUInt32()
            };

            reader.ReadBytes(3 * sizeof(uint)); // Reserved

            return result;
        }
    }
}
