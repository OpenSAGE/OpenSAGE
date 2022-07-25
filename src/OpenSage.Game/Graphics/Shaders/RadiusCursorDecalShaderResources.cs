using System.Numerics;
using OpenSage.Core.Graphics;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class RadiusCursorDecalShaderResources : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;

        private readonly Sampler _aniso4xClampSampler;

        public readonly ResourceLayout RadiusCursorDecalsResourceLayout;

        public RadiusCursorDecalShaderResources(GraphicsDeviceManager graphicsDeviceManager)
        {
            _graphicsDevice = graphicsDeviceManager.GraphicsDevice;

            _aniso4xClampSampler = graphicsDeviceManager.Aniso4xClampSampler;

            RadiusCursorDecalsResourceLayout = AddDisposable(_graphicsDevice.ResourceFactory.CreateResourceLayout(
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
