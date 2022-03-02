using System.IO;
using System;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt
{
    public sealed class Export : IMemoryStorage
    {
        public string Name { get; private set; }
        public uint Character { get; private set; }

        public Export()
        {
            Name = string.Empty;
        }

        public Export(string name, uint character)
        {
            Name = name;
            Character = character;
        }

        public static Export Parse(BinaryReader reader)
        {
            return new
            (
                reader.ReadStringAtOffset(),
                reader.ReadUInt32()
            );
        }

        public void Write(BinaryWriter writer, BinaryMemoryChain memory)
        {
            writer.WriteStringAtOffset(Name, memory);
            writer.Write(Character);
        }

    }
}
