using System.IO;
using System;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt
{
    public sealed class Export : IDataStorage
    {
        public string Name { get; private set; }
        public uint Character { get; private set; }

        public static Export Parse(BinaryReader reader)
        {
            return new Export
            {
                Name = reader.ReadStringAtOffset(),
                Character = reader.ReadUInt32()
            };
        }

        public void Write(BinaryWriter writer, MemoryPool memory)
        {
            writer.WriteStringAtOffset(Name, memory);
            writer.Write(Character);
        }

            public static Export Create(string name, int character)
        {
            return new Export
            {
                Name = name,
                Character = (uint) character
            };
        }
    }
}
