using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="DepthCompare"></param>
/// <param name="DepthMask"></param>
/// <param name="ColorMask">Now obsolete and ignored</param>
/// <param name="DestBlend"></param>
/// <param name="FogFunc">Now obsolete and ignored</param>
/// <param name="PrimaryGradient"></param>
/// <param name="SecondaryGradient"></param>
/// <param name="SrcBlend"></param>
/// <param name="Texturing"></param>
/// <param name="DetailColorFunc"></param>
/// <param name="DetailAlphaFunc"></param>
/// <param name="ShaderPreset">Now obsolete and ignored</param>
/// <param name="AlphaTest"></param>
/// <param name="PostDetailColorFunc"></param>
/// <param name="PostDetailAlphaFunc"></param>
public sealed record W3dShader(
    W3dShaderDepthCompare DepthCompare,
    W3dShaderDepthMask DepthMask,
    byte ColorMask,
    W3dShaderDestBlendFunc DestBlend,
    byte FogFunc,
    W3dShaderPrimaryGradient PrimaryGradient,
    W3dShaderSecondaryGradient SecondaryGradient,
    W3dShaderSrcBlendFunc SrcBlend,
    W3dShaderTexturing Texturing,
    W3dShaderDetailColorFunc DetailColorFunc,
    W3dShaderDetailAlphaFunc DetailAlphaFunc,
    byte ShaderPreset,
    W3dShaderAlphaTest AlphaTest,
    W3dShaderDetailColorFunc PostDetailColorFunc,
    W3dShaderDetailAlphaFunc PostDetailAlphaFunc)
{
    internal static W3dShader Parse(BinaryReader reader)
    {
        var depthCompare = reader.ReadByteAsEnum<W3dShaderDepthCompare>();
        var depthMask = reader.ReadByteAsEnum<W3dShaderDepthMask>();
        var colorMask = reader.ReadByte();
        var destBlend = reader.ReadByteAsEnum<W3dShaderDestBlendFunc>();
        var fogFunc = reader.ReadByte();
        var primaryGradient = reader.ReadByteAsEnum<W3dShaderPrimaryGradient>();
        var secondaryGradient = reader.ReadByteAsEnum<W3dShaderSecondaryGradient>();
        var srcBlend = reader.ReadByteAsEnum<W3dShaderSrcBlendFunc>();
        var texturing = reader.ReadByteAsEnum<W3dShaderTexturing>();
        var detailColorFunc = reader.ReadByteAsEnum<W3dShaderDetailColorFunc>();
        var detailAlphaFunc = reader.ReadByteAsEnum<W3dShaderDetailAlphaFunc>();
        var shaderPreset = reader.ReadByte();
        var alphaTest = reader.ReadByteAsEnum<W3dShaderAlphaTest>();
        var postDetailColorFunc = reader.ReadByteAsEnum<W3dShaderDetailColorFunc>();
        var postDetailAlphaFunc = reader.ReadByteAsEnum<W3dShaderDetailAlphaFunc>();

        reader.ReadByte(); // padding

        return new W3dShader(depthCompare, depthMask, colorMask, destBlend, fogFunc, primaryGradient, secondaryGradient,
            srcBlend, texturing, detailColorFunc, detailAlphaFunc, shaderPreset, alphaTest, postDetailColorFunc,
            postDetailAlphaFunc);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write((byte)DepthCompare);
        writer.Write((byte)DepthMask);
        writer.Write(ColorMask);
        writer.Write((byte)DestBlend);
        writer.Write(FogFunc);
        writer.Write((byte)PrimaryGradient);
        writer.Write((byte)SecondaryGradient);
        writer.Write((byte)SrcBlend);
        writer.Write((byte)Texturing);
        writer.Write((byte)DetailColorFunc);
        writer.Write((byte)DetailAlphaFunc);
        writer.Write(ShaderPreset);
        writer.Write((byte)AlphaTest);
        writer.Write((byte)PostDetailColorFunc);
        writer.Write((byte)PostDetailAlphaFunc);

        writer.Write((byte)0); // Padding
    }
}
