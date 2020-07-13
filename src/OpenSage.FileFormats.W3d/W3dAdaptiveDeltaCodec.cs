using System;

namespace OpenSage.FileFormats.W3d
{
    public static class W3dAdaptiveDeltaCodec
    {
        private static readonly float[] Table = CalculateTable();

        private static float[] CalculateTable()
        {
            var result = new float[256];

            for (var i = 0; i < 16; i++)
            {
                result[i] = MathF.Pow(10, i - 8.0f);
            }

            for (var i = 0; i < 240; i++)
            {
                var num = i / 240.0f;
                result[i + 16] = 1.0f - MathF.Sin(90.0f * num * MathF.PI / 180.0f);
            }

            return result;
        }

        public static W3dAnimationChannelDatum[] Decode(
            W3dAdaptiveDeltaData data,
            uint numFrames,
            float scale)
        {
            var scaleFactor = 1.0f;
            switch (data.BitCount)
            {
                // Do nothing for 4 bit deltas since they already map to [-16;16]
                case W3dAdaptiveDeltaBitCount.FourBits:
                    break;

                // When deltas are 8 bits large we need to scale them to the range [-16;16]
                case W3dAdaptiveDeltaBitCount.EightBits:
                    scaleFactor = 1 / 16.0f;
                    break;

                default:
                    throw new InvalidOperationException("Adaptive delta only supported 4 bit & 8 bit!");
            }

            var result = new W3dAnimationChannelDatum[numFrames];

            result[0] = data.InitialDatum;

            for (var i = 0; i < data.DeltaBlocks.Length; i++)
            {
                var deltaBlock = data.DeltaBlocks[i];

                var blockIndex = deltaBlock.BlockIndex;
                var blockScale = Table[blockIndex];
                var deltaScale = blockScale * scale * scaleFactor;

                var vectorIndex = deltaBlock.VectorIndex;

                var deltas = deltaBlock.GetDeltas(data.BitCount);

                for (var j = 0; j < deltas.Length; j++)
                {
                    var idx = ((i / data.VectorLength) * 16) + j + 1;
                    if (idx >= numFrames)
                    {
                        break;
                    }

                    var value = result[idx - 1].GetValue(vectorIndex) + deltaScale * deltas[j];
                    result[idx] = result[idx].WithValue(value, vectorIndex);

                    idx++;
                }
            }

            return result;
        }
    }
}
