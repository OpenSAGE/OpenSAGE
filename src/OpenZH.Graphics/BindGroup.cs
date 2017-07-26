namespace OpenZH.Graphics
{
    public sealed partial class BindGroup : GraphicsDeviceChild
    {
        public BindGroupLayout Layout { get; }

        public BindGroup(GraphicsDevice graphicsDevice, BindGroupLayout layout)
            : base(graphicsDevice)
        {
            Layout = layout;

            PlatformConstruct(graphicsDevice, layout);
        }

        public void SetBuffers(int startIndex, Buffer[] buffers)
        {
            for (var i = 0; i < buffers.Length; i++)
            {
                var buffer = buffers[i];

                // TODO: Validation.

                PlatformSetBuffer(startIndex + i, buffer);
            }
        }
    }
}
