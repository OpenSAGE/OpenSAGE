using System.IO;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterProperties(
    uint ColorKeyframesCount,
    uint OpacityKeyframesCount,
    uint SizeKeyframesCount,
    ColorRgba ColorRandom,
    float OpacityRandom,
    float SizeRandom,
    W3dEmitterColorKeyframe[] ColorKeyframes,
    W3dEmitterOpacityKeyframe[] OpacityKeyframes,
    W3dEmitterSizeKeyframe[] SizeKeyframes) : W3dChunk(W3dChunkType.W3D_CHUNK_EMITTER_PROPS)
{
    internal static W3dEmitterProperties Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var colorKeyframesCount = reader.ReadUInt32();
            var opacityKeyframesCount = reader.ReadUInt32();
            var sizeKeyframesCount = reader.ReadUInt32();
            var colorRandom = reader.ReadColorRgba();
            var opacityRandom = reader.ReadSingle();
            var sizeRandom = reader.ReadSingle();

            reader.ReadBytes(4 * sizeof(uint)); // Pad

            var colorKeyframes = new W3dEmitterColorKeyframe[colorKeyframesCount];
            for (var i = 0; i < colorKeyframesCount; i++)
            {
                colorKeyframes[i] = W3dEmitterColorKeyframe.Parse(reader);
            }

            var opacityKeyframes = new W3dEmitterOpacityKeyframe[opacityKeyframesCount];
            for (var i = 0; i < opacityKeyframesCount; i++)
            {
                opacityKeyframes[i] = W3dEmitterOpacityKeyframe.Parse(reader);
            }

            var sizeKeyframes = new W3dEmitterSizeKeyframe[sizeKeyframesCount];
            for (var i = 0; i < sizeKeyframesCount; i++)
            {
                sizeKeyframes[i] = W3dEmitterSizeKeyframe.Parse(reader);
            }

            return new W3dEmitterProperties(colorKeyframesCount, opacityKeyframesCount, sizeKeyframesCount, colorRandom,
                opacityRandom, sizeRandom, colorKeyframes, opacityKeyframes, sizeKeyframes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(ColorKeyframesCount);
        writer.Write(OpacityKeyframesCount);
        writer.Write(SizeKeyframesCount);
        writer.Write(ColorRandom);
        writer.Write(OpacityRandom);
        writer.Write(SizeRandom);

        for (var i = 0; i < 4; i++) // Pad
        {
            writer.Write((uint)0);
        }

        foreach (var keyframe in ColorKeyframes)
        {
            keyframe.WriteTo(writer);
        }

        foreach (var keyframe in OpacityKeyframes)
        {
            keyframe.WriteTo(writer);
        }

        foreach (var keyframe in SizeKeyframes)
        {
            keyframe.WriteTo(writer);
        }
    }
}
