using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dMotionChannelAdaptiveDeltaData(float Scale, W3dAdaptiveDeltaData Data) : IW3dMotionChannelData
{
    internal static W3dMotionChannelAdaptiveDeltaData Parse(
        BinaryReader reader,
        uint numTimeCodes,
        W3dAnimationChannelType channelType,
        int vectorLen,
        W3dAdaptiveDeltaBitCount bitCount)
    {
        var scale = reader.ReadSingle();
        var data = W3dAdaptiveDeltaData.Parse(
            reader,
            numTimeCodes,
            channelType,
            vectorLen,
            bitCount);

        return new W3dMotionChannelAdaptiveDeltaData(scale, data);
    }

    public IEnumerable<W3dKeyframeWithValue> GetKeyframesWithValues(W3dMotionChannel channel)
    {
        var decodedData = W3dAdaptiveDeltaCodec.Decode(
            Data,
            channel.NumTimeCodes,
            Scale);

        for (var i = (ushort)0; i < decodedData.Length; i++)
        {
            yield return new W3dKeyframeWithValue(
                i,
                decodedData[i]);
        }
    }

    public void WriteTo(BinaryWriter writer, W3dAnimationChannelType channelType)
    {
        writer.Write(Scale);
        Data.WriteTo(writer, channelType);
    }
}
