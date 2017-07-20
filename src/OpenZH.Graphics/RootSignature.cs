namespace OpenZH.Graphics
{
    public sealed partial class RootSignature : GraphicsDeviceChild
    {
        public RootSignature(GraphicsDevice graphicsDevice, RootSignatureDescriptor descriptor)
            : base(graphicsDevice)
        {
            PlatformConstruct(graphicsDevice, descriptor);
        }
    }
}
