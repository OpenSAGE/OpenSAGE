using System;
using System.Numerics;
using OpenSage.FileFormats.W3d;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Graphics.W3d;

internal static class ConversionExtensions
{
    public static FixedFunctionShaderResources.VertexMaterial ToVertexMaterial(this W3dVertexMaterialInfo w3dVertexMaterialInfo, W3dVertexMaterial w3dVertexMaterial)
    {
        return new FixedFunctionShaderResources.VertexMaterial
        {
            Ambient = w3dVertexMaterialInfo.Ambient.ToVector3(),
            Diffuse = w3dVertexMaterialInfo.Diffuse.ToVector3(),
            Specular = w3dVertexMaterialInfo.Specular.ToVector3(),
            Emissive = w3dVertexMaterialInfo.Emissive.ToVector3(),
            Shininess = w3dVertexMaterialInfo.Shininess,
            Opacity = w3dVertexMaterialInfo.Opacity,
            TextureMappingStage0 = CreateTextureMapping(
                w3dVertexMaterialInfo.Stage0Mapping,
                w3dVertexMaterial.MapperArgs0 ?? new W3dVertexMapperArgs(W3dChunkType.W3D_CHUNK_VERTEX_MAPPER_ARGS0)),
            TextureMappingStage1 = CreateTextureMapping(
                w3dVertexMaterialInfo.Stage1Mapping,
                w3dVertexMaterial.MapperArgs1 ?? new W3dVertexMapperArgs(W3dChunkType.W3D_CHUNK_VERTEX_MAPPER_ARGS1))
        };
    }

    private static FixedFunctionShaderResources.TextureMapping CreateTextureMapping(W3dVertexMappingType mappingType, W3dVertexMapperArgs args)
    {
        return new FixedFunctionShaderResources.TextureMapping
        {
            MappingType = mappingType.ToTextureMappingType(),
            UVPerSec = new Vector2
            {
                X = args.UPerSec,
                Y = args.VPerSec
            },
            UVScale = new Vector2
            {
                X = args.UScale,
                Y = args.VScale,
            },
            UVCenter = new Vector2
            {
                X = args.UCenter,
                Y = args.VCenter
            },
            UVAmplitude = new Vector2
            {
                X = args.UAmp,
                Y = args.VAmp
            },
            UVFrequency = new Vector2
            {
                X = args.UFreq,
                Y = args.VFreq
            },
            UVPhase = new Vector2
            {
                X = args.UPhase,
                Y = args.VPhase
            },
            UVStep = new Vector2
            {
                X = args.UStep,
                Y = args.VStep,
            },
            Speed = args.Speed,
            Fps = args.FPS,
            Log2Width = args.Log2Width,
            StepsPerSecond = args.StepsPerSecond
        };
    }

    private static FixedFunctionShaderResources.TextureMappingType ToTextureMappingType(this W3dVertexMappingType value)
    {
        switch (value)
        {
            case W3dVertexMappingType.Uv:
                return FixedFunctionShaderResources.TextureMappingType.Uv;

            case W3dVertexMappingType.Environment:
            case W3dVertexMappingType.CheapEnvironment:
                return FixedFunctionShaderResources.TextureMappingType.Environment;

            case W3dVertexMappingType.LinearOffset:
                return FixedFunctionShaderResources.TextureMappingType.LinearOffset;

            case W3dVertexMappingType.Rotate:
                return FixedFunctionShaderResources.TextureMappingType.Rotate;

            case W3dVertexMappingType.SineLinearOffset:
                return FixedFunctionShaderResources.TextureMappingType.SineLinearOffset;

            case W3dVertexMappingType.StepLinearOffset:
                return FixedFunctionShaderResources.TextureMappingType.StepLinearOffset;

            case W3dVertexMappingType.Screen:
                return FixedFunctionShaderResources.TextureMappingType.Screen;

            case W3dVertexMappingType.Scale:
                return FixedFunctionShaderResources.TextureMappingType.Scale;

            case W3dVertexMappingType.Grid:
                return FixedFunctionShaderResources.TextureMappingType.Grid;

            case W3dVertexMappingType.Random:
                return FixedFunctionShaderResources.TextureMappingType.Random;

            case W3dVertexMappingType.BumpEnv:
                return FixedFunctionShaderResources.TextureMappingType.BumpEnv;

            case W3dVertexMappingType.WsEnvironment:
                return FixedFunctionShaderResources.TextureMappingType.WsEnvironment;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static FixedFunctionShaderResources.DiffuseLightingType ToDiffuseLightingType(this W3dShaderPrimaryGradient value)
    {
        switch (value)
        {
            case W3dShaderPrimaryGradient.Disable:
                return FixedFunctionShaderResources.DiffuseLightingType.Disable;

            case W3dShaderPrimaryGradient.Modulate:
            case W3dShaderPrimaryGradient.Enable:
                return FixedFunctionShaderResources.DiffuseLightingType.Modulate;

            case W3dShaderPrimaryGradient.Add:
                return FixedFunctionShaderResources.DiffuseLightingType.Add;

            case W3dShaderPrimaryGradient.BumpEnvMap:
                return FixedFunctionShaderResources.DiffuseLightingType.BumpEnvMap;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static FixedFunctionShaderResources.SecondaryTextureBlend ToSecondaryTextureBlend(this W3dShaderDetailColorFunc value)
    {
        switch (value)
        {
            case W3dShaderDetailColorFunc.Disable:
                return FixedFunctionShaderResources.SecondaryTextureBlend.Disable;

            case W3dShaderDetailColorFunc.Detail:
                return FixedFunctionShaderResources.SecondaryTextureBlend.Detail;

            case W3dShaderDetailColorFunc.Scale:
            case W3dShaderDetailColorFunc.ScaleAlt:
                return FixedFunctionShaderResources.SecondaryTextureBlend.Scale;

            case W3dShaderDetailColorFunc.InvScale:
            case W3dShaderDetailColorFunc.InvScaleAlt:
                return FixedFunctionShaderResources.SecondaryTextureBlend.InvScale;

            case W3dShaderDetailColorFunc.Add:
                return FixedFunctionShaderResources.SecondaryTextureBlend.Add;

            case W3dShaderDetailColorFunc.Sub:
            case W3dShaderDetailColorFunc.SubR:
            case W3dShaderDetailColorFunc.Blend:
                throw new NotImplementedException();

            case W3dShaderDetailColorFunc.DetailBlend:
                return FixedFunctionShaderResources.SecondaryTextureBlend.DetailBlend;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static FixedFunctionShaderResources.SecondaryTextureBlend ToSecondaryTextureBlend(this W3dShaderDetailAlphaFunc value)
    {
        switch (value)
        {
            case W3dShaderDetailAlphaFunc.Disable:
                return FixedFunctionShaderResources.SecondaryTextureBlend.Disable;

            case W3dShaderDetailAlphaFunc.Detail:
                return FixedFunctionShaderResources.SecondaryTextureBlend.Detail;

            case W3dShaderDetailAlphaFunc.Scale:
                return FixedFunctionShaderResources.SecondaryTextureBlend.Scale;

            case W3dShaderDetailAlphaFunc.InvScale:
                return FixedFunctionShaderResources.SecondaryTextureBlend.InvScale;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static AnimationClipType ToAnimationClipType(this W3dAnimationChannelType value)
    {
        switch (value)
        {
            case W3dAnimationChannelType.TranslationX:
                return AnimationClipType.TranslationX;

            case W3dAnimationChannelType.TranslationY:
                return AnimationClipType.TranslationY;

            case W3dAnimationChannelType.TranslationZ:
                return AnimationClipType.TranslationZ;

            case W3dAnimationChannelType.Quaternion:
                return AnimationClipType.Quaternion;

            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    public static ComparisonKind ToComparison(this W3dShaderDepthCompare value)
    {
        switch (value)
        {
            case W3dShaderDepthCompare.PassNever:
                return ComparisonKind.Never;

            case W3dShaderDepthCompare.PassLess:
                return ComparisonKind.Less;

            case W3dShaderDepthCompare.PassEqual:
                return ComparisonKind.Equal;

            case W3dShaderDepthCompare.PassLEqual:
                return ComparisonKind.LessEqual;

            case W3dShaderDepthCompare.PassGreater:
                return ComparisonKind.Greater;

            case W3dShaderDepthCompare.PassNotEqual:
                return ComparisonKind.NotEqual;

            case W3dShaderDepthCompare.PassGEqual:
                return ComparisonKind.GreaterEqual;

            case W3dShaderDepthCompare.PassAlways:
                return ComparisonKind.Always;

            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    public static BlendFactor ToBlend(this W3dShaderSrcBlendFunc value)
    {
        switch (value)
        {
            case W3dShaderSrcBlendFunc.Zero:
                return BlendFactor.Zero;

            case W3dShaderSrcBlendFunc.One:
                return BlendFactor.One;

            case W3dShaderSrcBlendFunc.SrcAlpha:
                return BlendFactor.SourceAlpha;

            case W3dShaderSrcBlendFunc.OneMinusSrcAlpha:
                return BlendFactor.InverseSourceAlpha;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static BlendFactor ToBlend(this W3dShaderDestBlendFunc value, bool alpha)
    {
        switch (value)
        {
            case W3dShaderDestBlendFunc.Zero:
                return BlendFactor.Zero;

            case W3dShaderDestBlendFunc.One:
                return BlendFactor.One;

            case W3dShaderDestBlendFunc.SrcAlpha:
                return BlendFactor.SourceAlpha;

            case W3dShaderDestBlendFunc.OneMinusSrcAlpha:
                return BlendFactor.InverseSourceAlpha;

            case W3dShaderDestBlendFunc.SrcColor:
                return alpha ? BlendFactor.SourceAlpha : BlendFactor.SourceColor;

            case W3dShaderDestBlendFunc.OneMinusSrcColor:
                return alpha ? BlendFactor.InverseSourceAlpha : BlendFactor.InverseSourceColor;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
