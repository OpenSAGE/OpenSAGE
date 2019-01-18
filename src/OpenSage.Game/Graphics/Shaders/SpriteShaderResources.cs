using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Mathematics;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class SpriteShaderResources : ShaderResourcesBase
    {
        private readonly Dictionary<int, Pipeline> _pipelines;
        private readonly Dictionary<Sampler, ResourceSet> _samplerResourceSets;
        private readonly ResourceLayout _spriteConstantsResourceLayout;
        private readonly ResourceLayout _samplerResourceLayout;
        private readonly ResourceLayout _textureResourceLayout;

        private static int GetPipelineKey(
            in BlendStateDescription blendStateDescription,
            in OutputDescription outputDescription)
        {
            return HashCode.Combine(
                blendStateDescription,
                outputDescription);
        }

        public SpriteShaderResources(GraphicsDevice graphicsDevice)
            : base(
                 graphicsDevice,
                 "Sprite",
                 new GlobalResourceSetIndices(null, LightingType.None, null, null, null, null),
                 SpriteVertex.VertexDescriptor)
        {
            _pipelines = new Dictionary<int, Pipeline>();
            _samplerResourceSets = new Dictionary<Sampler, ResourceSet>();

            _spriteConstantsResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("SpriteConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment))));

            _samplerResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment))));

            _textureResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment))));
        }

        public Pipeline GetCachedPipeline(
            in BlendStateDescription blendStateDescription,
            in OutputDescription outputDescription)
        {
            var key = GetPipelineKey(blendStateDescription, outputDescription);

            if (!_pipelines.TryGetValue(key, out var result))
            {
                _pipelines.Add(key, result = AddDisposable(GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        blendStateDescription,
                        DepthStencilStateDescription.Disabled,
                        RasterizerStateDescriptionUtility.CullNoneSolid, // TODO
                        PrimitiveTopology.TriangleList,
                        ShaderSet.Description,
                        ShaderSet.ResourceLayouts,
                        outputDescription))));
            }

            return result;
        }

        public ResourceSet CreateSpriteConstantsResourceSet(
            DeviceBuffer spriteConstantsVSBuffer,
            DeviceBuffer spriteConstantsPSBuffer)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    _spriteConstantsResourceLayout,
                    spriteConstantsVSBuffer,
                    spriteConstantsPSBuffer));
        }

        public ResourceSet GetCachedSamplerResourceSet(Sampler sampler)
        {
            if (!_samplerResourceSets.TryGetValue(sampler, out var result))
            {
                _samplerResourceSets.Add(sampler, result = AddDisposable(GraphicsDevice.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(
                        _samplerResourceLayout,
                        sampler))));
            }

            return result;
        }

        public ResourceSet CreateTextureResourceSet(Texture texture)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    _textureResourceLayout,
                    texture));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MaterialConstantsVS
        {
            public Matrix4x4 Projection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpriteConstantsPS
        {
            private readonly Vector3 _padding;
            public Bool32 IgnoreAlpha;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpriteVertex
        {
            public Vector3 Position;
            public Vector2 UV;
            public ColorRgbaF Color;

            public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
                new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("COLOR", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));
        }
    }
}
