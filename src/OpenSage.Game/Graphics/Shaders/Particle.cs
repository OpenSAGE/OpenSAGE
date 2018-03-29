using System.Numerics;
using ShaderGen;
using static OpenSage.Graphics.Shaders.CommonShaderHelpers;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("Particle", "OpenSage.Graphics.Shaders.Particle.VS", "OpenSage.Graphics.Shaders.Particle.PS")]

namespace OpenSage.Graphics.Shaders
{
    public class Particle
    {
        public struct VertexInput
        {
            [PositionSemantic] public Vector3 Position;

            [TextureCoordinateSemantic] public float Size;
            [TextureCoordinateSemantic] public Vector3 Color;
            [TextureCoordinateSemantic] public float Alpha;
            [TextureCoordinateSemantic] public float AngleZ;
        }

        public struct PixelInput
        {
            [SystemPositionSemantic] public Vector4 Position;

            [TextureCoordinateSemantic] public Vector2 TexCoords;
            [TextureCoordinateSemantic] public Vector3 Color;
            [TextureCoordinateSemantic] public float Alpha;
        }

        public GlobalConstantsShared GlobalConstantsShared;

        public GlobalConstantsVS GlobalConstantsVS;

        public struct RenderItemConstantsVSType
        {
            public Matrix4x4 World;
        }

        public RenderItemConstantsVSType RenderItemConstantsVS;

        public Texture2DResource ParticleTexture;
        public SamplerResource Sampler;

        private Vector4 ComputePosition(Vector3 particlePosition, float size, float angle, Vector2 quadPosition)
        {
            var particlePosWS = Vector4.Transform(new Vector4(particlePosition, 1), RenderItemConstantsVS.World).XYZ();

            var toEye = Vector3.Normalize(GlobalConstantsShared.CameraPosition - particlePosWS);
            var up = new Vector3(Cos(angle), 0, Sin(angle));
            var right = Vector3.Cross(toEye, up);
            up = Vector3.Cross(toEye, right);

            particlePosWS += (right * size * quadPosition.X) + (up * size * quadPosition.Y);

            return Vector4.Transform(new Vector4(particlePosWS, 1), GlobalConstantsVS.ViewProjection);
        }

        [VertexShader]
        public PixelInput VS(VertexInput input)
        {
            PixelInput output;

            // Vertex layout:
            // 0 - 1
            // | / |
            // 2 - 3

            var quadVertexID = Mod(VertexID, 4);

            // TODO: This workaround is only because ShaderGen doesn't yet support const arrays.
            var vertexUVPos = Vector4.Zero;
            switch (quadVertexID)
            {
                case 0:
                    vertexUVPos = new Vector4(0, 1, -1, -1);
                    break;

                case 1:
                    vertexUVPos = new Vector4(0, 0, -1, 1);
                    break;

                case 2:
                    vertexUVPos = new Vector4(1, 1, 1, -1);
                    break;

                case 3:
                    vertexUVPos = new Vector4(1, 0, 1, 1);
                    break;
            }

            output.Position = ComputePosition(
                input.Position,
                input.Size,
                input.AngleZ,
                vertexUVPos.ZW());

            output.TexCoords = vertexUVPos.XY();

            output.Color = input.Color;
            output.Alpha = input.Alpha;

            return output;
        }

        [FragmentShader]
        public Vector4 PS(PixelInput input)
        {
            var texColor = Sample(ParticleTexture, Sampler, input.TexCoords);

            texColor = new Vector4(
                texColor.XYZ() * input.Color,
                texColor.W * input.Alpha);

            // TODO: Alpha test

            return texColor;
        }
    }
}
