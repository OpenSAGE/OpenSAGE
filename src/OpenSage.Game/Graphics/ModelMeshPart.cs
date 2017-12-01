using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshPart
    {
        public uint StartIndex { get; }
        public uint IndexCount { get; }

        public MeshMaterial Material { get; }

        public EffectPipelineStateHandle PipelineStateHandle { get; }

        internal ModelMeshPart(
            uint startIndex, 
            uint indexCount, 
            MeshMaterial material,
            EffectPipelineStateHandle pipelineStateHandle)
        {
            StartIndex = startIndex;
            IndexCount = indexCount;

            Material = material;

            PipelineStateHandle = pipelineStateHandle;
        }
    }
}
