using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public abstract class W3dMotionChannelData
    {
        public abstract IEnumerable<W3dKeyframeWithValue> GetKeyframesWithValues(W3dMotionChannel channel);

        internal abstract void WriteTo(BinaryWriter writer, W3dAnimationChannelType channelType);
    }

    public readonly struct W3dKeyframeWithValue
    {
        public readonly ushort Keyframe;
        public readonly W3dAnimationChannelDatum Datum;

        public W3dKeyframeWithValue(ushort keyframe, in W3dAnimationChannelDatum datum)
        {
            Keyframe = keyframe;
            Datum = datum;
        }
    }
}
