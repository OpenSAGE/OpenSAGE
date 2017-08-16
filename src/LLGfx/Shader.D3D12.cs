using SharpDX.Direct3D12;

namespace LLGfx
{
    partial class Shader
    {
        internal ShaderBytecode DeviceBytecode { get; private set; }

        private void PlatformConstruct(ShaderLibrary shaderLibrary, string shaderName)
        {
            DeviceBytecode = shaderLibrary.GetShader(shaderName);
        }
    }
}
