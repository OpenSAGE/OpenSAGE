using System.Numerics;
using ShaderGen;

[assembly: ShaderSet("Sprite", "OpenSage.Graphics.Shaders.Sprite.VS", "OpenSage.Graphics.Shaders.Sprite.PS")]

namespace OpenSage.Graphics.Shaders
{
    public class Sprite
    {
        public struct VertexInput
        {
            [PositionSemantic] public Vector3 Position;
            [TextureCoordinateSemantic] public Vector2 UV;
            [ColorSemantic] public Vector4 Color;
        }

        public struct PixelInput
        {
            [SystemPositionSemantic] public Vector4 Position;
            [TextureCoordinateSemantic] public Vector2 UV;
            [ColorSemantic] public Vector4 Color;
        }

        public Matrix4x4 Projection;

        public Texture2DResource Texture;
        public SamplerResource Sampler;

        [VertexShader]
        public PixelInput VS(VertexInput input)
        {
            PixelInput output;

            output.Position = Vector4.Transform(new Vector4(input.Position, 1), Projection);
            output.UV = input.UV;
            output.Color = input.Color;

            return output;
        }

        [FragmentShader]
        public Vector4 PS(PixelInput input)
        {
            var textureColor = ShaderBuiltins.Sample(Texture, Sampler, input.UV);

            textureColor *= input.Color;

            return textureColor;
        }
    }
}
