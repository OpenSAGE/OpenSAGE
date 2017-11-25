namespace LLGfx
{
    public sealed partial class VertexShader : Shader
    {
        public VertexShader(ShaderLibrary shaderLibrary, string shaderName)
            : base(shaderLibrary.GraphicsDevice)
        {
            PlatformConstruct(shaderLibrary, shaderName);
        }
    }
}
