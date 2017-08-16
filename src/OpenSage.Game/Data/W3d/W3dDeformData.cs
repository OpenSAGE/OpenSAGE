using System.IO;
using System.Runtime.InteropServices;

namespace OpenSage.Data.W3d
{
    /// <summary>
    /// Deform data. Contains deform information about a vertex in the mesh.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dDeformData
    {
        public uint VertexIndex { get; private set; }
        public W3dVector Position { get; private set; }
        public W3dRgba Color { get; private set; }

        public static W3dDeformData Parse(BinaryReader reader)
        {
            var result = new W3dDeformData
            {
                VertexIndex = reader.ReadUInt32(),
                Position = W3dVector.Parse(reader),
                Color = W3dRgba.Parse(reader)
            };

            reader.ReadBytes(2 * sizeof(uint)); // Reserved

            return result;
        }
    }
}
