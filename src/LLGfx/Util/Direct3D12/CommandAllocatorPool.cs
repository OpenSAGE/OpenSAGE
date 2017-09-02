using SharpDX.Direct3D12;

namespace LLGfx.Util
{
    internal sealed class CommandAllocatorPool : GraphicsResourcePool<CommandAllocator>
    {
        private readonly CommandListType _commandListType;

        public CommandAllocatorPool(GraphicsDevice graphicsDevice, CommandListType commandListType)
            : base(graphicsDevice)
        {
            _commandListType = commandListType;
        }

        protected override CommandAllocator CreateResource()
        {
            return GraphicsDevice.Device.CreateCommandAllocator(_commandListType);
        }

        protected override void ResetResource(CommandAllocator resource)
        {
            resource.Reset();
        }
    }
}
