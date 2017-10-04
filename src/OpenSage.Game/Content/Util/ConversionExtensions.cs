using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LLGfx;
using OpenSage.Data.Map;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Effects;
using OpenSage.Settings;

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

        public static Blend ToBlend(this W3dShaderDestBlendFunc value)
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
                }
            };
        }

        private static TextureMappingType ToTextureMappingType(this W3dVertexMappingType value)
        {
            switch (value)
            {
                case W3dVertexMappingType.Uv:
                    return TextureMappingType.Uv;

                case W3dVertexMappingType.Environment:
                    return TextureMappingType.Environment;

                case W3dVertexMappingType.LinearOffset:
                    return TextureMappingType.LinearOffset;

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
    }
}
