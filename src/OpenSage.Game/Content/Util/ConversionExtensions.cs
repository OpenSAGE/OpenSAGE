using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LLGfx;
using OpenSage.Data.Map;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Effects;
using OpenSage.Settings;
using OpenSage.Graphics.Animation;

namespace OpenSage.Content.Util
{
    internal static class ConversionExtensions
    {
        public static Blend ToBlend(this W3dShaderSrcBlendFunc value)
        {
            switch (value)
            {
                case W3dShaderSrcBlendFunc.Zero:
                    return Blend.Zero;

                case W3dShaderSrcBlendFunc.One:
                    return Blend.One;

                case W3dShaderSrcBlendFunc.SrcAlpha:
                    return Blend.SrcAlpha;

                case W3dShaderSrcBlendFunc.OneMinusSrcAlpha:
                    return Blend.OneMinusSrcAlpha;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Blend ToBlend(this W3dShaderDestBlendFunc value, bool alpha)
        {
            switch (value)
            {
                case W3dShaderDestBlendFunc.Zero:
                    return Blend.Zero;

                case W3dShaderDestBlendFunc.One:
                    return Blend.One;

                case W3dShaderDestBlendFunc.SrcAlpha:
                    return Blend.SrcAlpha;

                case W3dShaderDestBlendFunc.OneMinusSrcAlpha:
                    return Blend.OneMinusSrcAlpha;

                case W3dShaderDestBlendFunc.SrcColor:
                    return alpha ? Blend.SrcAlpha : Blend.SrcColor;

                case W3dShaderDestBlendFunc.OneMinusSrcColor:
                    return alpha ? Blend.OneMinusSrcAlpha : Blend.OneMinusSrcColor;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Vector3 ToVector3(this W3dRgb value)
        {
            return new Vector3(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f);
        }

        public static VertexMaterial ToVertexMaterial(this W3dVertexMaterial w3dVertexMaterial, W3dMaterial w3dMaterial)
        {
            return new VertexMaterial
            {
                Ambient = w3dVertexMaterial.Ambient.ToVector3(),
                Diffuse = w3dVertexMaterial.Diffuse.ToVector3(),
                Specular = w3dVertexMaterial.Specular.ToVector3(),
                Emissive = w3dVertexMaterial.Emissive.ToVector3(),
                Shininess = w3dVertexMaterial.Shininess,
                Opacity = w3dVertexMaterial.Opacity,
                TextureMappingStage0 = CreateTextureMapping(
                    w3dVertexMaterial.Stage0Mapping,
                    w3dMaterial.MapperArgs0),
                TextureMappingStage1 = CreateTextureMapping(
                    w3dVertexMaterial.Stage1Mapping,
                    w3dMaterial.MapperArgs1)
            };
        }

        private static TextureMapping CreateTextureMapping(W3dVertexMappingType mappingType, W3dVertexMapperArgs args)
        {
            return new TextureMapping
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
                Speed = args.Speed,
                Fps = args.FPS,
                Log2Width = (uint) args.Log2Width
            };
        }

        private static TextureMappingType ToTextureMappingType(this W3dVertexMappingType value)
        {
            switch (value)
            {
                case W3dVertexMappingType.Uv:
                    return TextureMappingType.Uv;

                case W3dVertexMappingType.Environment:
                case W3dVertexMappingType.CheapEnvironment:
                    return TextureMappingType.Environment;

                case W3dVertexMappingType.LinearOffset:
                    return TextureMappingType.LinearOffset;

                case W3dVertexMappingType.Rotate:
                    return TextureMappingType.Rotate;

                case W3dVertexMappingType.SineLinearOffset:
                    return TextureMappingType.SineLinearOffset;

                case W3dVertexMappingType.Screen:
                    return TextureMappingType.Screen;

                case W3dVertexMappingType.Scale:
                    return TextureMappingType.Scale;

                case W3dVertexMappingType.Grid:
                    return TextureMappingType.Grid;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static DiffuseLightingType ToDiffuseLightingType(this W3dShaderPrimaryGradient value)
        {
            switch (value)
            {
                case W3dShaderPrimaryGradient.Disable:
                    return DiffuseLightingType.Disable;

                case W3dShaderPrimaryGradient.Modulate:
                case W3dShaderPrimaryGradient.Enable:
                    return DiffuseLightingType.Modulate;

                case W3dShaderPrimaryGradient.Add:
                    return DiffuseLightingType.Add;

                case W3dShaderPrimaryGradient.BumpEnvMap:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static SecondaryTextureBlend ToSecondaryTextureBlend(this W3dShaderDetailColorFunc value)
        {
            switch (value)
            {
                case W3dShaderDetailColorFunc.Disable:
                    return SecondaryTextureBlend.Disable;
                    
                case W3dShaderDetailColorFunc.Detail:
                    return SecondaryTextureBlend.Detail;

                case W3dShaderDetailColorFunc.Scale:
                    return SecondaryTextureBlend.Scale;

                case W3dShaderDetailColorFunc.InvScale:
                    return SecondaryTextureBlend.InvScale;

                case W3dShaderDetailColorFunc.Add:
                case W3dShaderDetailColorFunc.Sub:
                case W3dShaderDetailColorFunc.SubR:
                case W3dShaderDetailColorFunc.Blend:
                    throw new NotImplementedException();

                case W3dShaderDetailColorFunc.DetailBlend:
                    return SecondaryTextureBlend.DetailBlend;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static SecondaryTextureBlend ToSecondaryTextureBlend(this W3dShaderDetailAlphaFunc value)
        {
            switch (value)
            {
                case W3dShaderDetailAlphaFunc.Disable:
                    return SecondaryTextureBlend.Disable;

                case W3dShaderDetailAlphaFunc.Detail:
                    return SecondaryTextureBlend.Detail;

                case W3dShaderDetailAlphaFunc.Scale:
                    return SecondaryTextureBlend.Scale;

                case W3dShaderDetailAlphaFunc.InvScale:
                    return SecondaryTextureBlend.InvScale;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Dictionary<TimeOfDay, LightSettings> ToLightSettingsDictionary(this Dictionary<TimeOfDay, GlobalLightingConfiguration> value)
        {
            return value.ToDictionary(
                x => x.Key,
                x => x.Value.ToLightSettings());
        }

        private static LightSettings ToLightSettings(this GlobalLightingConfiguration mapLightingConfiguration)
        {
            return new LightSettings
            {
                TerrainLights = new Lights
                {
                    Light0 = ToLight(mapLightingConfiguration.TerrainSun),
                    Light1 = ToLight(mapLightingConfiguration.TerrainAccent1),
                    Light2 = ToLight(mapLightingConfiguration.TerrainAccent2),
                },
                ObjectLights = new Lights
                {
                    Light0 = ToLight(mapLightingConfiguration.ObjectSun),
                    Light1 = ToLight(mapLightingConfiguration.ObjectAccent1),
                    Light2 = ToLight(mapLightingConfiguration.ObjectAccent2),
                }
            };
        }

        private static Light ToLight(GlobalLight mapLight)
        {
            return new Light
            {
                Ambient = mapLight.Ambient,
                Color = mapLight.Color,
                Direction = Vector3.Normalize(mapLight.Direction)
            };
        }

        public static Comparison ToComparison(this W3dShaderDepthCompare value)
        {
            switch (value)
            {
                case W3dShaderDepthCompare.PassNever:
                    return Comparison.Never;

                case W3dShaderDepthCompare.PassLess:
                    return Comparison.Less;

                case W3dShaderDepthCompare.PassEqual:
                    return Comparison.Equal;

                case W3dShaderDepthCompare.PassLEqual:
                    return Comparison.LessEqual;

                case W3dShaderDepthCompare.PassGreater:
                    return Comparison.Greater;

                case W3dShaderDepthCompare.PassNotEqual:
                    return Comparison.NotEqual;

                case W3dShaderDepthCompare.PassGEqual:
                    return Comparison.GreaterEqual;

                case W3dShaderDepthCompare.PassAlways:
                    return Comparison.Always;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
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
    }
}
