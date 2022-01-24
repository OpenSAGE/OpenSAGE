using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Mathematics;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.ParticleSystems;

internal sealed class ParticleMaterialDefinition : MaterialDefinition
{
    private readonly Pipeline _alphaPipeline;
    private readonly Pipeline _additivePipeline;

    private readonly Dictionary<ParticleMaterialKey, ParticleMaterial> _cachedMaterials = new();

    public ParticleMaterialDefinition(MaterialDefinitionStore store)
        : base(store, "Particle", ParticleVertex.VertexDescriptor)
    {
        Pipeline CreatePipeline(in BlendStateDescription blendStateDescription)
        {
            return AddDisposable(store.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    blendStateDescription,
                    DepthStencilStateDescription.DepthOnlyLessEqualRead,
                    RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                    PrimitiveTopology.TriangleList,
                    ShaderSet.Description,
                    ShaderSet.ResourceLayouts,
                    store.OutputDescription)));
        }

        _alphaPipeline = CreatePipeline(BlendStateDescription.SingleAlphaBlend);
        _additivePipeline = CreatePipeline(BlendStateDescriptionUtility.SingleAdditiveBlendNoAlpha);
    }

    public ParticleMaterial GetMaterial(FXParticleSystemTemplate template)
    {
        var key = new ParticleMaterialKey(template.Shader, template.IsGroundAligned, template.ParticleTexture.Value.Texture);

        if (!_cachedMaterials.TryGetValue(key, out var result))
        {
            result = AddDisposable(
                new ParticleMaterial(
                    this,
                    template.Shader switch
                    {
                        ParticleSystemShader.Alpha => _alphaPipeline,
                        ParticleSystemShader.AlphaTest => _alphaPipeline, // TODO: Proper implementation
                        ParticleSystemShader.Additive => _additivePipeline,
                        _ => throw new ArgumentOutOfRangeException(nameof(template)),
                    },
                    template));

            _cachedMaterials.Add(key, result);
        }

        return result;
    }

    private readonly record struct ParticleMaterialKey(ParticleSystemShader Shader, bool IsGroundAligned, Texture Texture);
}

internal sealed class ParticleMaterial : Material
{
    private readonly ResourceSet _particleResourceSet;

    public ParticleMaterial(
        ParticleMaterialDefinition definition,
        Pipeline pipeline,
        FXParticleSystemTemplate template)
        : base(definition, pipeline)
    {
        var particleConstantsBufferVS = AddDisposable(new ConstantBuffer<ParticleConstantsVS>(GraphicsDevice));
        particleConstantsBufferVS.Value.IsGroundAligned = template.IsGroundAligned;
        particleConstantsBufferVS.Update(GraphicsDevice);

        _particleResourceSet = AddDisposable(GraphicsDevice.ResourceFactory.CreateResourceSet(
            new ResourceSetDescription(
                definition.ShaderSet.ResourceLayouts[2],
                particleConstantsBufferVS.Buffer,
                template.ParticleTexture.Value.Texture,
                GraphicsDevice.LinearSampler)));
    }

    protected override void ApplyCore(CommandList commandList, RenderContext context)
    {
        commandList.SetGraphicsResourceSet(2, _particleResourceSet);
    }
}

internal static class MaterialDefinitionStoreExtensions
{
    public static ParticleMaterialDefinition GetParticleMaterialDefinition(this MaterialDefinitionStore store)
    {
        return store.GetMaterialDefinition(() => new ParticleMaterialDefinition(store));
    }
}

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

public struct ParticleConstantsVS
{
    private readonly Vector3 _padding;
    public Bool32 IsGroundAligned;
}

public struct ParticleRenderItemConstantsVS
{
    public Matrix4x4 WorldMatrix;
}
