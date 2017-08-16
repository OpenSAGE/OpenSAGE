using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMeshAabTreeNode
    {
        /// <summary>
        /// min corner of the box
        /// </summary>
        public W3dVector Min { get; private set; }

        /// <summary>
        /// max corner of the box
        /// </summary>
        public W3dVector Max { get; private set; }

        /// <summary>
        /// index of the front child or poly0 (if MSB is set, then leaf and poly0 is valid)
        /// </summary>
        public uint FrontOrPoly0 { get; private set; }

        /// <summary>
        /// index of the back child or polycount
        /// </summary>
        public uint BackOrPolyCount { get; private set; }

        public static W3dMeshAabTreeNode Parse(BinaryReader reader)
        {
            return new W3dMeshAabTreeNode
            {
                Min = W3dVector.Parse(reader),
                Max = W3dVector.Parse(reader),

                FrontOrPoly0 = reader.ReadUInt32(),
                BackOrPolyCount = reader.ReadUInt32()
            };
        }
    }
}
