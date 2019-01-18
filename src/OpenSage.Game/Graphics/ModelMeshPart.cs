using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshPart
    {
        public readonly uint StartIndex;
        public readonly uint IndexCount;

        public readonly DeviceBuffer TexCoordVertexBuffer;

        public readonly bool BlendEnabled;
        public readonly Pipeline Pipeline;
        public readonly ResourceSet MaterialResourceSet;

        internal ModelMeshPart(
            DeviceBuffer texCoordVertexBuffer,
            uint startIndex, 
            uint indexCount,
            bool blendEnabled,
            Pipeline pipeline,
            ResourceSet materialResourceSet)
        {
            TexCoordVertexBuffer = texCoordVertexBuffer;

            StartIndex = startIndex;
            IndexCount = indexCount;

            BlendEnabled = blendEnabled;
            Pipeline = pipeline;
            MaterialResourceSet = materialResourceSet;
        }
    }
}
