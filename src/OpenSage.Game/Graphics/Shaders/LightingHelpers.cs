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

        public struct Light
        {
            public Vector3 Ambient;
            float _Padding1;
            public Vector3 Color;
            float _Padding2;
            public Vector3 Direction;
            float _Padding3;
        }

        public struct Global_LightingConstantsPS
        {
            [ArraySize(3)] // NumLights
            public Light[] Lights;
        }

        //public struct LightingParameters
        //{
        //    public Vector3 WorldPosition;
        //    public Vector3 WorldNormal;
        //    public Vector3 MaterialAmbient;
        //    public Vector3 MaterialDiffuse;
        //    public Vector3 MaterialSpecular;
        //    public float MaterialShininess;
        //}

        public static Vector3 CalculateViewVector(Vector3 cameraPosition, Vector3 worldPosition)
        {
            return Vector3.Normalize(cameraPosition - worldPosition);
        }

        internal static void DoLighting(
            Global_LightingConstantsPS lightingConstantsPS,
            // TODO: Replace this with LightingParameters struct, once "internal" structs are supported
            Vector3 worldPosition,
            Vector3 worldNormal,
            Vector3 materialAmbient,
            Vector3 materialDiffuse,
            Vector3 materialSpecular,
            float materialShininess,
            Vector3 cameraPosition,
            bool specularEnabled,
            out Vector3 diffuseColor,
            out Vector3 specularColor)
        {
            diffuseColor = Vector3.Zero;
            specularColor = Vector3.Zero;

            for (var i = 0; i < 3 /* NumLights */; i++)
            {
                var light = lightingConstantsPS.Lights[i];

                var ambient = light.Ambient * materialAmbient;

                var diffuse =
                    Saturate(Vector3.Dot(worldNormal, -light.Direction)) *
                    materialDiffuse;

                if (specularEnabled)
                {
                    var v = CalculateViewVector(cameraPosition, worldPosition);
                    var h = Vector3.Normalize(v - light.Direction);
                    specularColor +=
                        Saturate(Vector3.Dot(worldNormal, h)) *
                        materialShininess *
                        materialSpecular *
                        light.Color;
                }

                diffuseColor += ambient + (diffuse * light.Color);
            }

            diffuseColor = Saturate(diffuseColor);
        }
    }
}
