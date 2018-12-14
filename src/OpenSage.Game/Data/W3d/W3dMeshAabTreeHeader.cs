using System.IO;

namespace OpenSage.Data.W3d
{
    /// <summary>
    /// AABTree header. Each mesh can have an associated Axis-Aligned-Bounding-Box tree
    /// which is used for collision detection and certain rendering algorithms (like 
    /// texture projection.
    /// </summary>
    public sealed class W3dMeshAabTreeHeader
    {
        public uint NodeCount { get; private set; }
        public uint PolyCount { get; private set; }

        internal static W3dMeshAabTreeHeader Parse(BinaryReader reader)
        {
            var result = new W3dMeshAabTreeHeader
            {
                NodeCount = reader.ReadUInt32(),
                PolyCount = reader.ReadUInt32()
            };

            reader.ReadBytes(6 * sizeof(uint)); // Padding

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(NodeCount);
            writer.Write(PolyCount);

            for (var i = 0; i < 6; i++) // Padding
            {
                writer.Write(0u);
            }
        }
    }
}
