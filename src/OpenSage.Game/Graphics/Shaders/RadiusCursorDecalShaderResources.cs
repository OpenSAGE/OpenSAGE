using System.Numerics;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class RadiusCursorDecalShaderResources : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;

        private readonly Sampler _aniso4xClampSampler;

        public readonly ResourceLayout RadiusCursorDecalsResourceLayout;

        public RadiusCursorDecalShaderResources(
            GraphicsDevice graphicsDevice,
            Sampler aniso4xClampSampler)
        {
            _graphicsDevice = graphicsDevice;

            _aniso4xClampSampler = aniso4xClampSampler;

            RadiusCursorDecalsResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("RadiusCursorDecalTextures", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RadiusCursorDecalSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RadiusCursorDecalConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RadiusCursorDecals", ResourceKind.StructuredBufferReadOnly, ShaderStages.Fragment))));
        }

        public ResourceSet CreateRadiusCursorDecalsResourceSet(
            Texture radiusCursorDecalTextureArray,
            DeviceBuffer radiusCursorDecalConstants,
            DeviceBuffer radiusCursorDecalsBuffer)
        {
            return _graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    RadiusCursorDecalsResourceLayout,
                    radiusCursorDecalTextureArray,
                    _aniso4xClampSampler,
                    radiusCursorDecalConstants,
                    radiusCursorDecalsBuffer));
        }

        public struct RadiusCursorDecalConstants
        {
            public Vector3 _Padding;
            public uint NumRadiusCursorDecals;
        }
    }
}
