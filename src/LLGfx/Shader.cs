namespace LLGfx
{
    public sealed partial class Shader : GraphicsDeviceChild
    {
        public Shader(ShaderLibrary shaderLibrary, string shaderName)
            : base(shaderLibrary.GraphicsDevice)
        {
            PlatformConstruct(shaderLibrary, shaderName);
        }
    }
}
