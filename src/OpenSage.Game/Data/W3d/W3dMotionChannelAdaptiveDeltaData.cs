using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMotionChannelAdaptiveDeltaData : W3dMotionChannelData
    {
        public static W3dMotionChannelAdaptiveDeltaData Parse(BinaryReader reader, uint numTimeCodes, W3dAnimationChannelType channelType, int vectorLen, int nBits)
        {
            float scale = reader.ReadSingle();
            var data = new W3dAnimationChannelDatum[numTimeCodes];

            uint count = (numTimeCodes + 15) >> 4;

            for (int k = 0; k < vectorLen; ++k)
            {
                var initial = reader.ReadSingle();
                W3dAdaptiveDelta.UpdateDatum(ref data[0], initial, channelType, k);

                for (int i = 0; i < count; ++i)
                {
                    var blockIndex = reader.ReadByte();
                    var blockScale = W3dAdaptiveDelta.Table[blockIndex];
                    var deltaScale = blockScale * scale;

                    //read deltas for the next 
                    var deltas = W3dAdaptiveDelta.ReadDeltaBlock(reader, nBits);

                    for (int j = 0; j < deltas.Length; ++j)
                    {
                        int idx = i * 16 + j + 1;

                        if (idx >= numTimeCodes)
                            break;

                        var value = W3dAdaptiveDelta.GetValue(data[idx - 1], channelType, k) + deltaScale * deltas[j];
                        W3dAdaptiveDelta.UpdateDatum(ref data[idx], value, channelType, k);
                    }
                }
            }

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
    }
}
