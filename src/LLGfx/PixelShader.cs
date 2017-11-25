namespace LLGfx
{
    public sealed partial class PixelShader : Shader
    {
        public PixelShader(ShaderLibrary shaderLibrary, string shaderName)
            : base(shaderLibrary.GraphicsDevice)
        {
            PlatformConstruct(shaderLibrary, shaderName);
        }
    }
}
