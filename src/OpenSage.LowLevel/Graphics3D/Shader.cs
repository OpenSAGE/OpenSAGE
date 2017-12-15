namespace OpenSage.LowLevel.Graphics3D
{
    public sealed partial class Shader : GraphicsDeviceChild
    {
        public ShaderType ShaderType { get; }
        public ShaderResourceBinding[] ResourceBindings { get; }

        public Shader(GraphicsDevice graphicsDevice, string functionName, byte[] deviceBytecode)
            : base(graphicsDevice)
        {
            PlatformConstruct(
                graphicsDevice,
                functionName,
                deviceBytecode, 
                out var shaderType,
                out var resourceBindings);

            ShaderType = shaderType;
            ResourceBindings = resourceBindings;
        }
    }
}
