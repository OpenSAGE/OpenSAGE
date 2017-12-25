using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics.Rendering
{
    //internal readonly struct DrawItem
    //{
    //    public readonly Matrix4x4 World;

    //    public readonly Buffer VertexBuffer0;
    //    public readonly Buffer VertexBuffer1;

    //    public readonly EffectMaterial Material;

    //    public readonly uint IndexCount;
    //    public readonly uint NumInstances;
    //    public readonly Buffer<ushort> IndexBuffer;
    //    public readonly uint StartIndex;
    //}

    internal delegate void RenderCallback(
        CommandEncoder commandEncoder, 
        Effect effect, 
        EffectPipelineStateHandle pipelineStateHandle,
        CameraComponent camera);

    internal sealed class RenderItem
    {
        public readonly Effect Effect;
        public readonly EffectPipelineStateHandle PipelineStateHandle;
        public readonly RenderCallback RenderCallback;

        public readonly EffectMaterial Material;
        public readonly RenderableComponent Renderable;

        // Set by RenderPipeline.
        public bool Visible;

        public RenderItem(
            RenderableComponent renderable,
            EffectMaterial material,
            EffectPipelineStateHandle pipelineStateHandle,
            RenderCallback renderCallback)
        {
            Effect = material.Effect;
            PipelineStateHandle = pipelineStateHandle;
            RenderCallback = renderCallback;

            Material = material;
            Renderable = renderable;
        }
    }
}
