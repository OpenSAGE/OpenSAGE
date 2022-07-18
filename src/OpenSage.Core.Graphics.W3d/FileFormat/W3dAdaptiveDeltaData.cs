﻿using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dAdaptiveDeltaData
    {
        public W3dAdaptiveDeltaBitCount BitCount { get; private set; }
        public int VectorLength { get; private set; }

        public W3dAnimationChannelDatum InitialDatum { get; private set; }
        public W3dAdaptiveDeltaBlock[] DeltaBlocks { get; private set; }

        internal static W3dAdaptiveDeltaData Parse(
            BinaryReader reader,
            uint numFrames,
            W3dAnimationChannelType type,
            int vectorLength,
            W3dAdaptiveDeltaBitCount bitCount)
        {
            var count = (numFrames + 15) >> 4;

            // First read all initial values
            var result = new W3dAdaptiveDeltaData
            {
                BitCount = bitCount,
                VectorLength = vectorLength,
                InitialDatum = W3dAnimationChannelDatum.Parse(reader, type)
            };

            var numBits = (int) bitCount;

            // Then read the interleaved delta blocks
            var deltaBlocks = new W3dAdaptiveDeltaBlock[count * vectorLength];
            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < vectorLength; j++)
                {
                    deltaBlocks[(i * vectorLength) + j] = W3dAdaptiveDeltaBlock.Parse(
                        reader,
                        j,
                        numBits);
                }
            }
            result.DeltaBlocks = deltaBlocks;

            return result;
        }

        internal void WriteTo(BinaryWriter writer, W3dAnimationChannelType channelType)
        {
            InitialDatum.WriteTo(writer, channelType);

            for (var i = 0; i < DeltaBlocks.Length; i++)
            {
                DeltaBlocks[i].WriteTo(writer);
            }
        }
    }
}
