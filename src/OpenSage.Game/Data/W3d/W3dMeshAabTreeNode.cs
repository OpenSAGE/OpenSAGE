using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMeshAabTreeNode
    {
        /// <summary>
        /// min corner of the box
        /// </summary>
        public Vector3 Min { get; private set; }

        /// <summary>
        /// max corner of the box
        /// </summary>
        public Vector3 Max { get; private set; }

        /// <summary>
        /// index of the front child or poly0 (if MSB is set, then leaf and poly0 is valid)
        /// </summary>
        public uint FrontOrPoly0 { get; private set; }

        /// <summary>
        /// index of the back child or polycount
        /// </summary>
        public uint BackOrPolyCount { get; private set; }

        internal static W3dMeshAabTreeNode Parse(BinaryReader reader)
        {
            return new W3dMeshAabTreeNode
            {
                Min = reader.ReadVector3(),
                Max = reader.ReadVector3(),

                FrontOrPoly0 = reader.ReadUInt32(),
                BackOrPolyCount = reader.ReadUInt32()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(Min);
            writer.Write(Max);
            writer.Write(FrontOrPoly0);
            writer.Write(BackOrPolyCount);
        }
    }
}
