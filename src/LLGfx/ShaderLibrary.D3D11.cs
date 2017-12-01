using System.IO;

namespace LLGfx
{
    partial class ShaderLibrary
    {
        internal override string PlatformGetDebugName() => null;
        internal override void PlatformSetDebugName(string value) { }

        private void PlatformConstruct(GraphicsDevice graphicsDevice) { }

        internal byte[] GetShader(string shaderName)
        {
            var shaderPath = Path.Combine("Shaders", shaderName + ".cso");
            return File.ReadAllBytes(shaderPath);
        }
    }
}
