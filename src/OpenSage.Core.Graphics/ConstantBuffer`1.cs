using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ConstantBuffer<T> : DisposableBase
        where T : unmanaged
    {
        public DeviceBuffer Buffer { get; }

        public T Value;

        public unsafe ConstantBuffer(GraphicsDevice graphicsDevice, string name = null)
        {
            Buffer = AddDisposable(graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(
                    (uint) sizeof(T),
                    BufferUsage.UniformBuffer | BufferUsage.Dynamic)));

            if (name != null)
            {
                Buffer.Name = name;
            }
        }

        public void Update(CommandList commandList)
        {
            commandList.UpdateBuffer(Buffer, 0, ref Value);
        }

        public void Update(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.UpdateBuffer(Buffer, 0, ref Value);
        }
    }
}
