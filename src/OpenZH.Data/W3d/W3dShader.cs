using System.IO;

namespace OpenZH.Data.W3d
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

        public static W3dShader Parse(BinaryReader reader)
        {
            var result = new W3dShader
            {
                DepthCompare = (W3dShaderDepthCompare) reader.ReadByte(),
                DepthMask = (W3dShaderDepthMask) reader.ReadByte(),
                ColorMask = reader.ReadByte(),
                DestBlend = (W3dShaderDestBlendFunc) reader.ReadByte(),
                FogFunc = reader.ReadByte(),
                PrimaryGradient = (W3dShaderPrimaryGradient) reader.ReadByte(),
                SecondaryGradient = (W3dShaderSecondaryGradient) reader.ReadByte(),
                SrcBlend = (W3dShaderSrcBlendFunc) reader.ReadByte(),
                Texturing = (W3dShaderTexturing) reader.ReadByte(),
                DetailColorFunc = (W3dShaderDetailColorFunc) reader.ReadByte(),
                DetailAlphaFunc = (W3dShaderDetailAlphaFunc) reader.ReadByte(),
                ShaderPreset = reader.ReadByte(),
                AlphaTest = (W3dShaderAlphaTest) reader.ReadByte(),
                PostDetailColorFunc = (W3dShaderDetailColorFunc) reader.ReadByte(),
                PostDetailAlphaFunc = (W3dShaderDetailAlphaFunc) reader.ReadByte()
            };

            reader.ReadByte(); // padding

            return result;
        }
    }
}
