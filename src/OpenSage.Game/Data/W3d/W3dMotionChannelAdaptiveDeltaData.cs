using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMotionChannelAdaptiveDeltaData : W3dMotionChannelData
    {
        internal static W3dMotionChannelAdaptiveDeltaData Parse(BinaryReader reader, uint numTimeCodes, W3dAnimationChannelType channelType, int vectorLen, int nBits)
        {
            float scale = reader.ReadSingle();

            var data = W3dAdaptiveDelta.ReadAdaptiveDelta(
                reader,
                numTimeCodes,
                channelType,
                vectorLen,
                scale,
                nBits);

            var keyframes = new ushort[numTimeCodes];
            for (ushort i = 0; i < numTimeCodes; i++)
            {
                keyframes[i] = i;
            }

            return new W3dMotionChannelAdaptiveDeltaData
            {
                TimeCodes = keyframes,
                Values = data
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            
        }
    }
}
