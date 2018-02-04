using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Data.W3d;
using OpenSage.Data.Wnd;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;
using OpenSage.Settings;
using Veldrid;

namespace OpenSage.Content.Util
{
    internal static class ConversionExtensions
    {
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

        public static Vector3 ToVector3(this W3dRgb value)
        {
            return new Vector3(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f);
        }

        public static FixedFunctionMaterial.VertexMaterial ToVertexMaterial(this W3dVertexMaterial w3dVertexMaterial, W3dMaterial w3dMaterial)
        {
            return new FixedFunctionMaterial.VertexMaterial
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

        private static FixedFunctionMaterial.TextureMapping CreateTextureMapping(W3dVertexMappingType mappingType, W3dVertexMapperArgs args)
        {
            return new FixedFunctionMaterial.TextureMapping
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

        private static FixedFunctionMaterial.TextureMappingType ToTextureMappingType(this W3dVertexMappingType value)
        {
            switch (value)
            {
                case W3dVertexMappingType.Uv:
                    return FixedFunctionMaterial.TextureMappingType.Uv;

                case W3dVertexMappingType.Environment:
                case W3dVertexMappingType.CheapEnvironment:
                    return FixedFunctionMaterial.TextureMappingType.Environment;

                case W3dVertexMappingType.LinearOffset:
                    return FixedFunctionMaterial.TextureMappingType.LinearOffset;

                case W3dVertexMappingType.Rotate:
                    return FixedFunctionMaterial.TextureMappingType.Rotate;

                case W3dVertexMappingType.SineLinearOffset:
                    return FixedFunctionMaterial.TextureMappingType.SineLinearOffset;

                case W3dVertexMappingType.Screen:
                    return FixedFunctionMaterial.TextureMappingType.Screen;

                case W3dVertexMappingType.Scale:
                    return FixedFunctionMaterial.TextureMappingType.Scale;

                case W3dVertexMappingType.Grid:
                    return FixedFunctionMaterial.TextureMappingType.Grid;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static FixedFunctionMaterial.DiffuseLightingType ToDiffuseLightingType(this W3dShaderPrimaryGradient value)
        {
            switch (value)
            {
                case W3dShaderPrimaryGradient.Disable:
                    return FixedFunctionMaterial.DiffuseLightingType.Disable;

                case W3dShaderPrimaryGradient.Modulate:
                case W3dShaderPrimaryGradient.Enable:
                    return FixedFunctionMaterial.DiffuseLightingType.Modulate;

                case W3dShaderPrimaryGradient.Add:
                    return FixedFunctionMaterial.DiffuseLightingType.Add;

                case W3dShaderPrimaryGradient.BumpEnvMap:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static FixedFunctionMaterial.SecondaryTextureBlend ToSecondaryTextureBlend(this W3dShaderDetailColorFunc value)
        {
            switch (value)
            {
                case W3dShaderDetailColorFunc.Disable:
                    return FixedFunctionMaterial.SecondaryTextureBlend.Disable;
                    
                case W3dShaderDetailColorFunc.Detail:
                    return FixedFunctionMaterial.SecondaryTextureBlend.Detail;

                case W3dShaderDetailColorFunc.Scale:
                    return FixedFunctionMaterial.SecondaryTextureBlend.Scale;

                case W3dShaderDetailColorFunc.InvScale:
                    return FixedFunctionMaterial.SecondaryTextureBlend.InvScale;

                case W3dShaderDetailColorFunc.Add:
                case W3dShaderDetailColorFunc.Sub:
                case W3dShaderDetailColorFunc.SubR:
                case W3dShaderDetailColorFunc.Blend:
                    throw new NotImplementedException();

                case W3dShaderDetailColorFunc.DetailBlend:
                    return FixedFunctionMaterial.SecondaryTextureBlend.DetailBlend;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static FixedFunctionMaterial.SecondaryTextureBlend ToSecondaryTextureBlend(this W3dShaderDetailAlphaFunc value)
        {
            switch (value)
            {
                case W3dShaderDetailAlphaFunc.Disable:
                    return FixedFunctionMaterial.SecondaryTextureBlend.Disable;

                case W3dShaderDetailAlphaFunc.Detail:
                    return FixedFunctionMaterial.SecondaryTextureBlend.Detail;

                case W3dShaderDetailAlphaFunc.Scale:
                    return FixedFunctionMaterial.SecondaryTextureBlend.Scale;

                case W3dShaderDetailAlphaFunc.InvScale:
                    return FixedFunctionMaterial.SecondaryTextureBlend.InvScale;

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
                TerrainLights = new LightingConstants
                {
                    Light0 = ToLight(mapLightingConfiguration.TerrainSun),
                    Light1 = ToLight(mapLightingConfiguration.TerrainAccent1),
                    Light2 = ToLight(mapLightingConfiguration.TerrainAccent2),
                },
                ObjectLights = new LightingConstants
                {
                    // RA3 and later only had one light defined per time of day.
                    Light0 = ToLight(mapLightingConfiguration.ObjectSun ?? mapLightingConfiguration.TerrainSun),
                    Light1 = ToLight(mapLightingConfiguration.ObjectAccent1 ?? mapLightingConfiguration.TerrainAccent1),
                    Light2 = ToLight(mapLightingConfiguration.ObjectAccent2 ?? mapLightingConfiguration.TerrainAccent2),
                }
                // TODO: Infantry lights
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

        public static Rectangle ToRectangle(this WndScreenRect value)
        {
            return new Rectangle(
                value.UpperLeft.X,
                value.UpperLeft.Y,
                value.BottomRight.X - value.UpperLeft.X,
                value.BottomRight.Y - value.UpperLeft.Y);
        }

        public static Rectangle ToRectangle(this MappedImageCoords value)
        {
            return new Rectangle(
                value.Left,
                value.Top,
                value.Right - value.Left,
                value.Bottom - value.Top);
        }

        public static ColorRgbaF ToColorRgbaF(this ColorRgba value)
        {
            return new ColorRgbaF(
                value.R / 255.0f,
                value.G / 255.0f,
                value.B / 255.0f,
                value.A / 255.0f);
        }
    }
}
