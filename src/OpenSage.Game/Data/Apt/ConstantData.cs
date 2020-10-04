using System.Collections.Generic;
using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Data.Apt
{
    public sealed class ConstantData
    {
        public List<ConstantEntry> Entries { get; internal set; }
        public uint AptDataEntryOffset { get; internal set; }

        public ConstantData()
        {
            Entries = new List<ConstantEntry>();
        }

        public static ConstantData FromFileSystemEntry(FileSystemEntry entry)
        {
            using var stream = entry.Open();
            using var reader = new BinaryReader(stream);
            var data = new ConstantData();

            //validate that this is a correct const
            var magic = reader.ReadFixedLengthString(17);
            if (magic != "Apt constant file")
            {
                throw new InvalidDataException($"Not a supported const file: {magic}");
            }

            reader.BaseStream.Seek(3, SeekOrigin.Current);

            data.AptDataEntryOffset = reader.ReadUInt32();
            var numEntries = reader.ReadUInt32();
            var headerSize = reader.ReadUInt32();

            if (headerSize != 32)
            {
                throw new InvalidDataException("Constant header must be 32 bytes");
            }

            for (var i = 0; i < numEntries; i++)
            {
                var constEntry = new ConstantEntry
                {
                    Type = reader.ReadUInt32AsEnum<ConstantEntryType>()
                };

                //read the number/ string offset


                switch (constEntry.Type)
                {
                    case ConstantEntryType.Undef:
                        throw new InvalidDataException("Undefined const entry");
                    case ConstantEntryType.String:
                        var strOffset = reader.ReadUInt32();
                        var pos = reader.BaseStream.Position;
                        reader.BaseStream.Seek(strOffset, SeekOrigin.Begin);
                        constEntry.Value = reader.ReadNullTerminatedString();
                        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                        break;
                    case ConstantEntryType.Register:
                        constEntry.Value = reader.ReadUInt32();
                        break;
                    case ConstantEntryType.Boolean:
                        constEntry.Value = reader.ReadBooleanUInt32Checked();
                        break;
                    case ConstantEntryType.Float:
                        constEntry.Value = reader.ReadSingle();
                        break;
                    case ConstantEntryType.Integer:
                        constEntry.Value = reader.ReadInt32();
                        break;
                    case ConstantEntryType.Lookup:
                        constEntry.Value = reader.ReadUInt32();
                        break;
                    case ConstantEntryType.None:
                        constEntry.Value = null;
                        break;
                    default:
                        throw new InvalidDataException();
                }

                data.Entries.Add(constEntry);
            }

            return data;
        }
    }

    public enum ConstantEntryType
    {
        //TODO: validate that all those types are correct
        Undef = 0,
        String = 1,
        Property = 2,
        None = 3,
        Register = 4,
        Boolean = 5,
        Float = 6,
        Integer = 7,
        Lookup = 8
    }

    public sealed class ConstantEntry
    {
        public ConstantEntryType Type { get; internal set; }
        public object Value { get; internal set; }
    }
}
