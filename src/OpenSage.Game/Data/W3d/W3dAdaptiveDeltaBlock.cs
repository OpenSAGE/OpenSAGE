using System;
using System.IO;

namespace OpenSage.Data.W3d
{

    public sealed class W3dAdaptiveDeltaBlock
    {
        public int VectorIndex { get; private set; }

        public byte BlockIndex { get; private set; }
        public sbyte[] DeltaBytes { get; private set; }

        internal static W3dAdaptiveDeltaBlock Parse(BinaryReader reader, int vectorIndex, int numBits)
        {
            var result = new W3dAdaptiveDeltaBlock
            {
                VectorIndex = vectorIndex,
                BlockIndex = reader.ReadByte()
            };

            var numDeltaBytes = numBits * 2;
            var deltaBytes = new sbyte[numDeltaBytes];
            for (var k = 0; k < numDeltaBytes; k++)
            {
                deltaBytes[k] = reader.ReadSByte();
            }
            result.DeltaBytes = deltaBytes;

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(BlockIndex);

            for (var i = 0; i < DeltaBytes.Length; i++)
            {
                writer.Write(DeltaBytes[i]);
            }
        }

        public sbyte[] GetDeltas(W3dAdaptiveDeltaBitCount bitCount)
        {
            var numBits = (int) bitCount;

            var deltas = new sbyte[16];

            for (var i = 0; i < DeltaBytes.Length; ++i)
            {
                switch (numBits)
                {
                    case 4:
                        deltas[i * 2] = DeltaBytes[i];
                        if ((deltas[i * 2] & 8) != 0)
                        {
                            deltas[i * 2] = (sbyte) (deltas[i * 2] | 0xF0);
                        }
                        else
                        {
                            deltas[i * 2] &= 0x0F;
                        }
                        deltas[i * 2 + 1] = (sbyte) (DeltaBytes[i] >> 4);
                        break;

                    case 8:
                        var val = (byte) DeltaBytes[i];
                        //do a bitflip
                        if ((val & 0x80) != 0)
                        {
                            val &= 0x7F;
                        }
                        else
                        {
                            val |= 0x80;
                        }
                        deltas[i] = (sbyte) val;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(bitCount));
                }
            }

            return deltas;
        }
    }
}
