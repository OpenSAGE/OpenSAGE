using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Mathematics;
using OpenSage.Mathematics;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class SpriteShaderResources : ShaderResourcesBase
    {
        private readonly Dictionary<PipelineKey, Pipeline> _pipelines;
        private readonly Dictionary<Sampler, ResourceSet> _samplerResourceSets;
        private readonly ResourceLayout _spriteConstantsResourceLayout;
        private readonly ResourceLayout _samplerResourceLayout;
        private readonly ResourceLayout _textureResourceLayout;
        private readonly ResourceLayout _alphaMaskResourceLayout;
        private readonly ResourceLayout[] _resourceLayouts;

        public SpriteShaderResources(GraphicsDevice graphicsDevice)
            : base(
                 graphicsDevice,
                 "Sprite",
                 SpriteVertex.VertexDescriptor)
        {
            _pipelines = new Dictionary<PipelineKey, Pipeline>();
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

            _alphaMaskResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("AlphaMask", ResourceKind.TextureReadOnly, ShaderStages.Fragment))));

            _resourceLayouts = new[]
            {
                _spriteConstantsResourceLayout,
                _samplerResourceLayout,
                _textureResourceLayout,
                _alphaMaskResourceLayout,
            };
        }

        public Pipeline GetCachedPipeline(
            in BlendStateDescription blendStateDescription,
            in OutputDescription outputDescription)
        {
            var key = new PipelineKey(blendStateDescription, outputDescription);

            if (!_pipelines.TryGetValue(key, out var result))
            {
                _pipelines.Add(key, result = AddDisposable(GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        blendStateDescription,
                        DepthStencilStateDescription.Disabled,
                        RasterizerStateDescriptionUtility.CullNoneSolid, // TODO
                        PrimitiveTopology.TriangleList,
                        ShaderSet.Description,
                        _resourceLayouts,
                        outputDescription))));
            }

            return result;
        }

        private readonly struct PipelineKey : IEquatable<PipelineKey>
        {
            public readonly BlendStateDescription BlendState;
            public readonly OutputDescription Output;

            public PipelineKey(in BlendStateDescription blendState, in OutputDescription output)
            {
                BlendState = blendState;
                Output = output;
            }

            public override bool Equals(object obj)
            {
                return obj is PipelineKey a && Equals(a);
            }

            public bool Equals(PipelineKey other)
            {
                return
                    BlendState.Equals(other.BlendState) &&
                    Output.Equals(other.Output);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(BlendState, Output);
            }

            public static bool operator ==(in PipelineKey key1, in PipelineKey key2)
            {
                return key1.Equals(key2);
            }

            public static bool operator !=(in PipelineKey key1, in PipelineKey key2)
            {
                return !(key1 == key2);
            }
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

        public ResourceSet CreateAlphaMaskResourceSet(Texture alphaMask)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    _alphaMaskResourceLayout,
                    alphaMask));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MaterialConstantsVS
        {
            public Matrix4x4 Projection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpriteConstantsPS
        {
            public Vector2 OutputOffset;
            public Vector2 OutputSize;
            public Bool32 IgnoreAlpha;
            public SpriteFillMethod FillMethod;
            public float FillAmount;
            public Bool32 Grayscale;
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
