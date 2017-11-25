namespace LLGfx
{
    partial class PixelShader
    {
        internal byte[] DeviceBytecode { get; private set; }

        private void PlatformConstruct(ShaderLibrary shaderLibrary, string shaderName)
        {
            DeviceBytecode = shaderLibrary.GetShader(shaderName);
        }
    }
}
