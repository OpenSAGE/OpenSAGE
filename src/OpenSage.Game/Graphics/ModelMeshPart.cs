using LLGfx.Effects;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshPart
    {
        public uint StartIndex { get; }
        public uint IndexCount { get; }

        public uint ShadingConfigurationID { get; }

        public EffectPipelineStateHandle PipelineStateHandle { get; }

        internal ModelMeshPart(
            uint startIndex, 
            uint indexCount, 
            uint shadingConfigurationID,
            EffectPipelineStateHandle pipelineStateHandle)
        {
            StartIndex = startIndex;
            IndexCount = indexCount;

            ShadingConfigurationID = shadingConfigurationID;

            PipelineStateHandle = pipelineStateHandle;
        }
    }
}
