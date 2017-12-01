namespace LLGfx
{
    partial class CommandQueue
    {
        private CommandBuffer _commandBuffer;

        internal override string PlatformGetDebugName() => null;
        internal override void PlatformSetDebugName(string value) { }

        private void PlatformConstruct(GraphicsDevice graphicsDevice)
        {
            _commandBuffer = new CommandBuffer(graphicsDevice, this);
        }

        private CommandBuffer PlatformGetCommandBuffer()
        {
            return _commandBuffer;
        }
    }
}
