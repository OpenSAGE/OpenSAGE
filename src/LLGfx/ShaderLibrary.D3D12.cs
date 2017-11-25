using System.IO;

namespace LLGfx
{
    partial class ShaderLibrary
    {
        private void PlatformConstruct(GraphicsDevice graphicsDevice) { }

        internal byte[] GetShader(string shaderName)
        {
            var shaderPath = Path.Combine("Shaders", shaderName + ".cso");
            return File.ReadAllBytes(shaderPath);
        }
    }
}
