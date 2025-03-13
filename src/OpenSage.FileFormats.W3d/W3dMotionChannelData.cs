using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public interface IW3dMotionChannelData
{
    IEnumerable<W3dKeyframeWithValue> GetKeyframesWithValues(W3dMotionChannel channel);
    void WriteTo(BinaryWriter writer, W3dAnimationChannelType channelType);
}

public readonly record struct W3dKeyframeWithValue(ushort Keyframe, W3dAnimationChannelDatum Datum);
