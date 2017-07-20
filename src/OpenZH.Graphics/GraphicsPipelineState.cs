namespace OpenZH.Graphics
{
    public sealed partial class GraphicsPipelineState : GraphicsDeviceChild
    {
        public GraphicsPipelineStateDescriptor Descriptor { get; }

        public GraphicsPipelineState(GraphicsDevice graphicsDevice, GraphicsPipelineStateDescriptor descriptor)
            : base(graphicsDevice)
        {
            Descriptor = descriptor;

            PlatformConstruct(graphicsDevice, descriptor);
        }
    }
}
