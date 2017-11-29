namespace LLGfx
{
    public sealed partial class Shader : GraphicsDeviceChild
    {
        public ShaderType ShaderType { get; }
        public ShaderResourceBinding[] ResourceBindings { get; }

        public Shader(ShaderLibrary shaderLibrary, string shaderName)
            : base(shaderLibrary.GraphicsDevice)
        {
            PlatformConstruct(
                shaderLibrary, 
                shaderName, 
                out var shaderType,
                out var resourceBindings);

            ShaderType = shaderType;
            ResourceBindings = resourceBindings;
        }
    }
}
