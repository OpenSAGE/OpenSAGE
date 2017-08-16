using System.Runtime.InteropServices;

namespace LLGfx
{
    public sealed partial class DynamicBuffer : Buffer
    {
        public static DynamicBuffer Create<T>(
            GraphicsDevice graphicsDevice)
            where T : struct
        {
            var sizeInBytes = (uint) Marshal.SizeOf<T>();

            var result = new DynamicBuffer(graphicsDevice, sizeInBytes);

            result.PlatformConstruct(
                graphicsDevice,
                sizeInBytes);

            return result;
        }

        private DynamicBuffer(GraphicsDevice graphicsDevice, uint sizeInBytes)
            : base(graphicsDevice, sizeInBytes)
        {
        }

        public void SetData<T>(ref T data)
            where T : struct
        {
            PlatformSetData(ref data);
        }
    }
}
