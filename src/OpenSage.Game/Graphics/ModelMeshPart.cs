using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshPart
    {
        public uint StartIndex { get; }
        public uint IndexCount { get; }

        public MeshMaterial Material { get; }

        internal ModelMeshPart(
            uint startIndex, 
            uint indexCount, 
            MeshMaterial material)
        {
            StartIndex = startIndex;
            IndexCount = indexCount;

            Material = material;
        }
    }
}
