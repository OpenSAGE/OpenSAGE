using LLGfx;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshPart
    {
        public uint StartIndex { get; }
        public uint IndexCount { get; }

        public ShadingConfiguration ShadingConfiguration { get; }

        public EffectPipelineStateHandle PipelineStateHandle { get; }

        public Texture Texture0 { get; }
        public Texture Texture1 { get; }

        internal ModelMeshPart(
            uint startIndex, 
            uint indexCount, 
            ShadingConfiguration shadingConfiguration,
            EffectPipelineStateHandle pipelineStateHandle,
            Texture texture0,
            Texture texture1)
        {
            StartIndex = startIndex;
            IndexCount = indexCount;

            ShadingConfiguration = shadingConfiguration;

            PipelineStateHandle = pipelineStateHandle;

            Texture0 = texture0;
            Texture1 = texture1;
        }
    }
}
