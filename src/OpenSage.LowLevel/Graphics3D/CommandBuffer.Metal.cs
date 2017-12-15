using Metal;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class CommandBuffer
    {
        private readonly IMTLCommandBuffer _deviceCommandBuffer;

        internal override string PlatformGetDebugName() => _deviceCommandBuffer.Label;
        internal override void PlatformSetDebugName(string value) => _deviceCommandBuffer.Label = value;

        internal CommandBuffer(GraphicsDevice graphicsDevice, IMTLCommandBuffer deviceCommandBuffer)
            : base(graphicsDevice)
        {
            _deviceCommandBuffer = deviceCommandBuffer;
        }

        private CommandEncoder PlatformGetCommandEncoder(RenderPassDescriptor renderPassDescriptor)
        {
            return new CommandEncoder(
                GraphicsDevice,
                _deviceCommandBuffer.CreateRenderCommandEncoder(renderPassDescriptor.DeviceRenderPassDescriptor));
        }

        private void PlatformCommit()
        {
            _deviceCommandBuffer.Commit();
        }

        private void PlatformCommitAndPresent(SwapChain swapChain)
        {
            _deviceCommandBuffer.PresentDrawable(swapChain.CurrentDrawable);

            PlatformCommit();
        }
    }
}
