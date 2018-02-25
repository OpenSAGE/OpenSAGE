using System.Numerics;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;

namespace OpenSage.Graphics.Shaders
{
    public static class LightingHelpers
    {
        public struct Global_LightingConstantsVS
        {
            public Matrix4x4 CloudShadowMatrix;
        }

        public const int NumLights = 3;

        public struct Light
        {
            public Vector3 Ambient;
            public Vector3 Color;
            public Vector3 Direction;
            float _Padding;
        }

        public struct Global_LightingConstantsPS
        {
            [ArraySize(3)] // NumLights
            public Light[] Lights;
        }

        internal struct LightingParameters
        {
            public Vector3 WorldPosition;
            public Vector3 WorldNormal;
            public Vector3 MaterialAmbient;
            public Vector3 MaterialDiffuse;
            public Vector3 MaterialSpecular;
            public float MaterialShininess;
        }

        public static Vector3 CalculateViewVector(Vector3 cameraPosition, Vector3 worldPosition)
        {
            return Vector3.Normalize(cameraPosition - worldPosition);
        }

        internal static void DoLighting(
            Global_LightingConstantsPS lightingConstantsPS,
            LightingParameters lightingParams,
            Vector3 cameraPosition,
            bool specularEnabled,
            out Vector3 diffuseColor,
            out Vector3 specularColor)
        {
            diffuseColor = Vector3.Zero;
            specularColor = Vector3.Zero;

            for (var i = 0; i < NumLights; i++)
            {
                var light = lightingConstantsPS.Lights[i];

                var ambient = light.Ambient * lightingParams.MaterialAmbient;

                var diffuse =
                    Saturate(Vector3.Dot(lightingParams.WorldNormal, -light.Direction)) *
                    lightingParams.MaterialDiffuse;

                if (specularEnabled)
                {
                    var v = CalculateViewVector(cameraPosition, lightingParams.WorldPosition);
                    var h = Vector3.Normalize(v - light.Direction);
                    specularColor +=
                        Saturate(Vector3.Dot(lightingParams.WorldNormal, h)) *
                        lightingParams.MaterialShininess *
                        lightingParams.MaterialSpecular *
                        light.Color;
                }

                diffuseColor += ambient + (diffuse * light.Color);
            }

            diffuseColor = Saturate(diffuseColor);
        }
    }
}
