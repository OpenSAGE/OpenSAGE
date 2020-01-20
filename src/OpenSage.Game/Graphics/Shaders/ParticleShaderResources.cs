using System;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Mathematics;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ParticleShaderResources : ShaderResourcesBase
    {
        private readonly Pipeline _alphaPipeline;
        private readonly Pipeline _additivePipeline;

        private readonly ResourceLayout _particleResourceLayout;

        public ParticleShaderResources(
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources)
            : base(
                graphicsDevice,
                "Particle",
                new GlobalResourceSetIndices(0u, LightingType.None, null, null, null, null),
                ParticleVertex.VertexDescriptor)
        {
            _particleResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("RenderItemConstants", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("ParticleConstants", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("ParticleTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment))));

            var resourceLayouts = new[]
            {
                globalShaderResources.GlobalConstantsResourceLayout,
                _particleResourceLayout
            };

            Pipeline CreatePipeline(in BlendStateDescription blendStateDescription)
            {
                return graphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        blendStateDescription,
                        DepthStencilStateDescription.DepthOnlyLessEqualRead,
                        RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                        PrimitiveTopology.TriangleList,
                        ShaderSet.Description,
                        resourceLayouts,
                        RenderPipeline.GameOutputDescription));
            }

            _alphaPipeline = AddDisposable(CreatePipeline(BlendStateDescription.SingleAlphaBlend));
            _additivePipeline = AddDisposable(CreatePipeline(BlendStateDescriptionUtility.SingleAdditiveBlendNoAlpha));
        }

        public Pipeline GetCachedPipeline(ParticleSystemShader shader)
        {
            switch (shader)
            {
                case ParticleSystemShader.Alpha:
                case ParticleSystemShader.AlphaTest: // TODO: proper implementation for AlphaTest
                    return _alphaPipeline;

                case ParticleSystemShader.Additive:
                    return _additivePipeline;

                default:
                    throw new ArgumentOutOfRangeException(nameof(shader));
            }
        }

        public ResourceSet CreateParticleResoureSet(
            DeviceBuffer renderItemConstantsBufferVS,
            DeviceBuffer particleConstantsBufferVS,
            Texture texture)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    _particleResourceLayout,
                    renderItemConstantsBufferVS,
                    particleConstantsBufferVS,
                    texture,
                    GraphicsDevice.LinearSampler));
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ParticleVertex
        {
            public Vector3 Position;
            public float Size;
            public Vector3 Color;
            public float Alpha;
            public float AngleZ;

            public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
                new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ParticleConstantsVS
        {
            private readonly Vector3 _padding;
            public Bool32 IsGroundAligned;
        }
    }
}
