using System.IO;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class ShaderLibrary
    {
        private void PlatformConstruct(GraphicsDevice graphicsDevice) { }

        internal ShaderBytecode GetShader(string shaderName)
        {
            var shaderPath = Path.Combine("Shaders", shaderName + ".cso");
            return File.ReadAllBytes(shaderPath);
        }
    }
}
