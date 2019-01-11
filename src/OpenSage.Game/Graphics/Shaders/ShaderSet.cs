using OpenSage.Utilities.Extensions;
using Veldrid;
using Veldrid.SPIRV;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ShaderSet : DisposableBase
    {
        public readonly ShaderSetDescription Description;

        public ShaderSet(
            GraphicsDevice graphicsDevice,
            string shaderName,
            params VertexLayoutDescription[] vertexDescriptors)
        {
#if DEBUG
            const bool debug = true;
#else
            const bool debug = false;
#endif

            byte[] ReadShader(string shaderType)
            {
                var bytecodeShaderName = $"OpenSage.Assets.Shaders.{shaderName}.{shaderType}.spv";
                using (var shaderStream = typeof(ShaderSet).Assembly.GetManifestResourceStream(bytecodeShaderName))
                {
                    return shaderStream.ReadAllBytes();
                }
            }

            var vsBytes = ReadShader("vert");
            var fsBytes = ReadShader("frag");

            var shaders = graphicsDevice.ResourceFactory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, vsBytes, "main", debug),
                new ShaderDescription(ShaderStages.Fragment, fsBytes, "main", debug),
                new CrossCompileOptions());

            var vertexShader = AddDisposable(shaders[0]);
            var fragmentShader = AddDisposable(shaders[1]);

            vertexShader.Name = $"{shaderName}.vert";
            fragmentShader.Name = $"{shaderName}.frag";

            Description = new ShaderSetDescription(
                vertexDescriptors,
                new[] { vertexShader, fragmentShader });
        }
    }
}
