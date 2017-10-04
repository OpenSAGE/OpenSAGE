using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    /// <summary>
    /// A triangle, occurs inside the W3D_CHUNK_TRIANGLES chunk
    /// This was introduced with version 3.0 of the file format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dTriangle
    {
        // vertex,vnormal,texcoord,color indices
        public uint VIndex0;
        public uint VIndex1;
        public uint VIndex2;

        public W3dSurfaceType SurfaceType;

        /// <summary>
        /// Plane normal.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Plane distance.
        /// </summary>
        public float Distance;

        public static W3dTriangle Parse(BinaryReader reader)
        {
            return new W3dTriangle
            {
                VIndex0 = reader.ReadUInt32(),
                VIndex1 = reader.ReadUInt32(),
                VIndex2 = reader.ReadUInt32(),

                SurfaceType = (W3dSurfaceType) reader.ReadUInt32(),

                Normal = reader.ReadVector3(),

                Distance = reader.ReadSingle()
            };
        }
    }
}
