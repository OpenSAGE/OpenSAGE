using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMotionChannelAdaptiveDeltaData : W3dMotionChannelData
    {
        public float Scale { get; private set; }
        public W3dAdaptiveDeltaData Data { get; private set; }

        internal static W3dMotionChannelAdaptiveDeltaData Parse(
            BinaryReader reader,
            uint numTimeCodes,
            W3dAnimationChannelType channelType,
            int vectorLen,
            W3dAdaptiveDeltaBitCount bitCount)
        {
            return new W3dMotionChannelAdaptiveDeltaData
            {
                Scale = reader.ReadSingle(),
                Data = W3dAdaptiveDeltaData.Parse(
                    reader,
                    numTimeCodes,
                    channelType,
                    vectorLen,
                    bitCount)
            };
        }

        public override IEnumerable<W3dKeyframeWithValue> GetKeyframesWithValues(W3dMotionChannel channel)
        {
            var decodedData = W3dAdaptiveDeltaCodec.Decode(
                Data,
                channel.NumTimeCodes,
                Scale);

            for (var i = (ushort) 0; i < decodedData.Length; i++)
            {
                yield return new W3dKeyframeWithValue(
                    i,
                    decodedData[i]);
            }
        }

        internal override void WriteTo(BinaryWriter writer, W3dAnimationChannelType channelType)
        {
            writer.Write(Scale);
            Data.WriteTo(writer, channelType);
        }
    }
}
