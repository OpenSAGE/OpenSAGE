namespace OpenSage.LowLevel.Graphics3D
{
    public sealed partial class Shader : GraphicsDeviceChild
    {
        public ShaderType ShaderType { get; }
        public ShaderResourceBinding[] ResourceBindings { get; }

        public Shader(GraphicsDevice graphicsDevice, byte[] deviceBytecode)
            : base(graphicsDevice)
        {
            PlatformConstruct(
                graphicsDevice,
                deviceBytecode, 
                out var shaderType,
                out var resourceBindings);

            ShaderType = shaderType;
            ResourceBindings = resourceBindings;
        }
    }
}
