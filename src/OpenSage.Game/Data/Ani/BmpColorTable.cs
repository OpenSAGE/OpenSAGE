using System.IO;

namespace OpenSage.Data.Ani
{
    public sealed class BmpColorTable
    {
        public BmpColorTableEntry[] Entries { get; private set; }

        public static BmpColorTable Parse(BinaryReader reader, int numEntries)
        {
            var entries = new BmpColorTableEntry[numEntries];

            for (var i = 0; i < numEntries; i++)
            {
                entries[i] = new BmpColorTableEntry
                {
                    Red = reader.ReadByte(),
                    Green = reader.ReadByte(),
                    Blue = reader.ReadByte()
                };

                var padding = reader.ReadByte();
                if (padding != 0)
                {
                    throw new InvalidDataException();
                }
            }

            return new BmpColorTable
            {
                Entries = entries
            };
        }
    }

    public struct BmpColorTableEntry
    {
        public byte Red;
        public byte Green;
        public byte Blue;
    }
}
