using System.Runtime.InteropServices;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class ConstantBuffer<T> : DisposableBase
        where T : unmanaged
    {
        private readonly GraphicsDevice _graphicsDevice;

        public DeviceBuffer Buffer { get; }

        public T Value;

        public unsafe ConstantBuffer(GraphicsDevice graphicsDevice, string name = null)
        {
            _graphicsDevice = graphicsDevice;

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
    }
}
