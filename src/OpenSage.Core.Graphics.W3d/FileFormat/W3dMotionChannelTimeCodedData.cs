using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dMotionChannelTimeCodedData : W3dMotionChannelData
    {
        public ushort[] TimeCodes { get; private set; }
        public W3dAnimationChannelDatum[] Values { get; private set; }

        public static W3dMotionChannelTimeCodedData Parse(BinaryReader reader, ushort numTimeCodes, W3dAnimationChannelType channelType)
        {
            var keyframes = new ushort[numTimeCodes];
            for (var i = 0; i < numTimeCodes; i++)
            {
                keyframes[i] = reader.ReadUInt16();
            }

            if (numTimeCodes % 2 != 0)
            {
                // Align to 4-byte boundary.
                reader.ReadUInt16();
            }

            var data = new W3dAnimationChannelDatum[numTimeCodes];
            for (var i = 0; i < numTimeCodes; i++)
            {
                data[i] = W3dAnimationChannelDatum.Parse(reader, channelType);
            }

            return new W3dMotionChannelTimeCodedData
            {
                TimeCodes = keyframes,
                Values = data
            };
        }

        public override IEnumerable<W3dKeyframeWithValue> GetKeyframesWithValues(W3dMotionChannel channel)
        {
            for (var i = 0; i < TimeCodes.Length; i++)
            {
                yield return new W3dKeyframeWithValue(TimeCodes[i], Values[i]);
            }
        }

        internal override void WriteTo(BinaryWriter writer, W3dAnimationChannelType channelType)
        {
            for (var i = 0; i < TimeCodes.Length; i++)
            {
                writer.Write(TimeCodes[i]);
            }

            if (TimeCodes.Length % 2 != 0)
            {
                // Align to 4-byte boundary.
                writer.Write((ushort) 0);
            }

            for (var i = 0; i < Values.Length; i++)
            {
                Values[i].WriteTo(writer, channelType);
            }
        }
    }
}
