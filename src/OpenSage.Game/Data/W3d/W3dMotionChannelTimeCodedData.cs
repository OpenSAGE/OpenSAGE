using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMotionChannelTimeCodedData : W3dMotionChannelData
    {
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
    }
}
