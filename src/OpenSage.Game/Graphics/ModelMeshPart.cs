using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshPart
    {
        public uint StartIndex { get; }
        public uint IndexCount { get; }

        public bool AlphaTest { get; }
        public bool Texturing { get; }

        public EffectPipelineStateHandle PipelineStateHandle { get; }

        internal ModelMeshPart(
            uint startIndex, 
            uint indexCount, 
            bool alphaTest,
            bool texturing,
            EffectPipelineStateHandle pipelineStateHandle)
        {
            StartIndex = startIndex;
            IndexCount = indexCount;

            AlphaTest = alphaTest;
            Texturing = texturing;

            PipelineStateHandle = pipelineStateHandle;
        }
    }
}
