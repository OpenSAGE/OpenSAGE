using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Mathematics;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ParticleShaderResources : ShaderSetBase
    {
        private readonly Pipeline _alphaPipeline;
        private readonly Pipeline _additivePipeline;

        private readonly ConstantBuffer<ParticleConstantsVS> _particleConstantsBufferIsGroundAlignedFalse;
        private readonly ConstantBuffer<ParticleConstantsVS> _particleConstantsBufferIsGroundAlignedTrue;

        private readonly Dictionary<ParticleMaterialKey, Material> _cachedMaterials = new();

        public ParticleShaderResources(ShaderSetStore store)
            : base(store, "Particle", ParticleVertex.VertexDescriptor)
        {
            Pipeline CreatePipeline(in BlendStateDescription blendStateDescription)
            {
                return AddDisposable(
                    store.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                        new GraphicsPipelineDescription(
                            blendStateDescription,
                            DepthStencilStateDescription.DepthOnlyLessEqualRead,
                            RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                            PrimitiveTopology.TriangleList,
                            Description,
                            ResourceLayouts,
                            RenderPipeline.GameOutputDescription)));
            }

            _alphaPipeline = CreatePipeline(BlendStateDescription.SingleAlphaBlend);
            _additivePipeline = CreatePipeline(BlendStateDescriptionUtility.SingleAdditiveBlendNoAlpha);

            ConstantBuffer<ParticleConstantsVS> CreateParticleConstantsBuffer(bool isGroundAligned)
            {
                var particleConstantsBufferVS = AddDisposable(new ConstantBuffer<ParticleConstantsVS>(store.GraphicsDevice));
                particleConstantsBufferVS.Value.IsGroundAligned = isGroundAligned;
                particleConstantsBufferVS.Update(store.GraphicsDevice);
                return particleConstantsBufferVS;
            }

            _particleConstantsBufferIsGroundAlignedFalse = CreateParticleConstantsBuffer(false);
            _particleConstantsBufferIsGroundAlignedTrue = CreateParticleConstantsBuffer(true);
        }

        public Material GetMaterial(FXParticleSystemTemplate template)
        {
            var key = new ParticleMaterialKey(
                template.Shader,
                template.IsGroundAligned,
                template.ParticleTexture.Value.Texture);

            if (!_cachedMaterials.TryGetValue(key, out var result))
            {
                var particleConstantsBuffer = template.IsGroundAligned
                    ? _particleConstantsBufferIsGroundAlignedTrue
                    : _particleConstantsBufferIsGroundAlignedFalse;

                var materialResourceSet = AddDisposable(
                    GraphicsDevice.ResourceFactory.CreateResourceSet(
                        new ResourceSetDescription(
                            ResourceLayouts[2],
                            particleConstantsBuffer.Buffer,
                            template.ParticleTexture.Value.Texture,
                            GraphicsDevice.LinearSampler)));

                var pipeline = template.Shader switch
                {
                    ParticleSystemShader.Alpha => _alphaPipeline,
                    ParticleSystemShader.AlphaTest => _alphaPipeline, // TODO: Proper implementation
                    ParticleSystemShader.Additive => _additivePipeline,
                    _ => throw new ArgumentOutOfRangeException(nameof(template)),
                };

                result = AddDisposable(
                    new Material(
                        this,
                        pipeline,
                        materialResourceSet,
                        SurfaceType.Transparent));

                _cachedMaterials.Add(key, result);
            }

            return result;
        }

        private readonly record struct ParticleMaterialKey(
            ParticleSystemShader Shader,
            bool IsGroundAligned,
            Texture Texture);

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

        private struct ParticleConstantsVS
        {
#pragma warning disable IDE1006, CS0169
            private readonly Vector3 _padding;
#pragma warning restore IDE1006, CS0169
            public Bool32 IsGroundAligned;
        }
    }
}
