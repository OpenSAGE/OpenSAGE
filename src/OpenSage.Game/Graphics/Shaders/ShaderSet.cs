using OpenSage.Utilities.Extensions;
using Veldrid;
using Veldrid.SPIRV;

namespace OpenSage.Graphics.Shaders
{
    public sealed class ShaderSet : DisposableBase
    {
        private static byte _nextId = 0;

        public readonly byte Id;

        public readonly ShaderSetDescription Description;
        public readonly GlobalResourceSetIndices GlobalResourceSetIndices;

        public ShaderSet(
            GraphicsDevice graphicsDevice,
            string shaderName,
            GlobalResourceSetIndices globalResourceSetIndices,
            params VertexLayoutDescription[] vertexDescriptors)
        {
            GlobalResourceSetIndices = globalResourceSetIndices;

            Id = _nextId++;

#if DEBUG
            const bool debug = true;
#else
            const bool debug = false;
#endif

            var assembly = typeof(ShaderSet).Assembly;

            byte[] ReadShader(string shaderType)
            {
                var bytecodeShaderName = $"OpenSage.Assets.Shaders.{shaderName}.{shaderType}.spv";
                using (var shaderStream = assembly.GetManifestResourceStream(bytecodeShaderName))
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

    public sealed class GlobalResourceSetIndices
    {
        public readonly uint? GlobalConstants;
        public readonly LightingType LightingType;
        public readonly uint? GlobalLightingConstants;
        public readonly uint? CloudConstants;
        public readonly uint? ShadowConstants;
        public readonly uint? RenderItemConstants;

        public GlobalResourceSetIndices(
            uint? globalConstants,
            LightingType lightingType,
            uint? globalLightingConstants,
            uint? cloudConstants,
            uint? shadowConstants,
            uint? renderItemConstants)
        {
            GlobalConstants = globalConstants;
            LightingType = lightingType;
            GlobalLightingConstants = globalLightingConstants;
            CloudConstants = cloudConstants;
            ShadowConstants = shadowConstants;
            RenderItemConstants = renderItemConstants;
        }
    }
}
