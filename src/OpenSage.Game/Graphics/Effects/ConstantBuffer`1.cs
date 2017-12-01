using LLGfx;

namespace OpenSage.Graphics.Effects
{
    public sealed class ConstantBuffer<T> : GraphicsObject
        where T : struct
    {
        public Buffer<T> Buffer { get; }

        public T Value;

        public ConstantBuffer(GraphicsDevice graphicsDevice)
        {
            Buffer = Buffer<T>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer);
        }

        public void Update()
        {
            Buffer.SetData(ref Value);
        }
    }
}
