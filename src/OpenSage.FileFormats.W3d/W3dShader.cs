using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dShader
    {
        public W3dShaderDepthCompare DepthCompare { get; private set; }
        public W3dShaderDepthMask DepthMask { get; private set; }

        /// <summary>
        /// Now obsolete and ignored
        /// </summary>
        public byte ColorMask { get; private set; }

        public W3dShaderDestBlendFunc DestBlend { get; private set; }

        /// <summary>
        /// Now obsolete and ignored
        /// </summary>
        public byte FogFunc { get; private set; }

        public W3dShaderPrimaryGradient PrimaryGradient { get; private set; }

        public W3dShaderSecondaryGradient SecondaryGradient { get; private set; }

        public W3dShaderSrcBlendFunc SrcBlend { get; private set; }

        public W3dShaderTexturing Texturing { get; private set; }

        public W3dShaderDetailColorFunc DetailColorFunc { get; private set; }

        public W3dShaderDetailAlphaFunc DetailAlphaFunc { get; private set; }

        /// <summary>
        /// Now obsolete and ignored
        /// </summary>
        public byte ShaderPreset { get; private set; }

        public W3dShaderAlphaTest AlphaTest { get; private set; }

        public W3dShaderDetailColorFunc PostDetailColorFunc { get; private set; }

        public W3dShaderDetailAlphaFunc PostDetailAlphaFunc { get; private set; }

        internal static W3dShader Parse(BinaryReader reader)
        {
            var result = new W3dShader
            {
                DepthCompare = reader.ReadByteAsEnum<W3dShaderDepthCompare>(),
                DepthMask = reader.ReadByteAsEnum<W3dShaderDepthMask>(),
                ColorMask = reader.ReadByte(),
                DestBlend = reader.ReadByteAsEnum<W3dShaderDestBlendFunc>(),
                FogFunc = reader.ReadByte(),
                PrimaryGradient = reader.ReadByteAsEnum<W3dShaderPrimaryGradient>(),
                SecondaryGradient = reader.ReadByteAsEnum<W3dShaderSecondaryGradient>(),
                SrcBlend = reader.ReadByteAsEnum<W3dShaderSrcBlendFunc>(),
                Texturing = reader.ReadByteAsEnum<W3dShaderTexturing>(),
                DetailColorFunc = reader.ReadByteAsEnum<W3dShaderDetailColorFunc>(),
                DetailAlphaFunc = reader.ReadByteAsEnum<W3dShaderDetailAlphaFunc>(),
                ShaderPreset = reader.ReadByte(),
                AlphaTest = reader.ReadByteAsEnum<W3dShaderAlphaTest>(),
                PostDetailColorFunc = reader.ReadByteAsEnum<W3dShaderDetailColorFunc>(),
                PostDetailAlphaFunc = reader.ReadByteAsEnum<W3dShaderDetailAlphaFunc>()
            };

            reader.ReadByte(); // padding

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write((byte) DepthCompare);
            writer.Write((byte) DepthMask);
            writer.Write(ColorMask);
            writer.Write((byte) DestBlend);
            writer.Write(FogFunc);
            writer.Write((byte) PrimaryGradient);
            writer.Write((byte) SecondaryGradient);
            writer.Write((byte) SrcBlend);
            writer.Write((byte) Texturing);
            writer.Write((byte) DetailColorFunc);
            writer.Write((byte) DetailAlphaFunc);
            writer.Write(ShaderPreset);
            writer.Write((byte) AlphaTest);
            writer.Write((byte) PostDetailColorFunc);
            writer.Write((byte) PostDetailAlphaFunc);

            writer.Write((byte) 0); // Padding
        }
    }
}
