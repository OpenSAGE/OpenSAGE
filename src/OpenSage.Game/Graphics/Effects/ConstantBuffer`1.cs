using System.Runtime.InteropServices;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class ConstantBuffer<T> : DisposableBase
        where T : struct
    {
        private readonly GraphicsDevice _graphicsDevice;

        public DeviceBuffer Buffer { get; }

        public T Value;

        public ConstantBuffer(GraphicsDevice graphicsDevice, string name = null)
        {
            _graphicsDevice = graphicsDevice;

            Buffer = AddDisposable(graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(
                    (uint) Marshal.SizeOf<T>(),
                    BufferUsage.UniformBuffer | BufferUsage.Dynamic)));

            Buffer.Name = name;
        }

        public void Update(CommandList commandList)
        {
            commandList.UpdateBuffer(Buffer, 0, ref Value);
        }
    }
}
