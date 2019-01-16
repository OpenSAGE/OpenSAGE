using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshPart
    {
        public uint StartIndex { get; }
        public uint IndexCount { get; }

        internal readonly bool BlendEnabled;
        internal readonly Pipeline Pipeline;
        internal readonly ResourceSet MaterialResourceSet;
        internal readonly Pipeline DepthPipeline;

        internal ModelMeshPart(
            uint startIndex, 
            uint indexCount,
            bool blendEnabled,
            Pipeline pipeline,
            ResourceSet materialResourceSet,
            Pipeline depthPipeline)
        {
            StartIndex = startIndex;
            IndexCount = indexCount;

            BlendEnabled = blendEnabled;
            Pipeline = pipeline;
            MaterialResourceSet = materialResourceSet;

            DepthPipeline = depthPipeline;
        }
    }
}
