using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Utilities.Extensions;

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
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream))
            {
                var data = new ConstantData();

                //validate that this is a correct const              
                var magic = reader.ReadFixedLengthString(17);
                if (magic!="Apt constant file")
                    throw new InvalidDataException($"Not a supported const file: {magic}");

                reader.BaseStream.Seek(3, SeekOrigin.Current);

                data.AptDataEntryOffset = reader.ReadUInt32();
                var numEntries = reader.ReadUInt32();

                reader.BaseStream.Seek(4, SeekOrigin.Current);

                for (var i = 0; i < numEntries; i++)
                {
                    var constEntry = new ConstantEntry
                    {
                        Type = (ConstantEntryType)reader.ReadUInt32()
                    };

                    //read the number/ string offset
                    var entryValue = reader.ReadUInt32();

                    switch (constEntry.Type)
                    {
                        case ConstantEntryType.UNDEF:
                            throw new InvalidDataException("Undefined const entry");
                        case ConstantEntryType.STRING:
                            var pos = reader.BaseStream.Position;
                            reader.BaseStream.Seek(entryValue, SeekOrigin.Begin);
                            constEntry.Value =  reader.ReadNullTerminatedString();
                            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                            break;
                        case ConstantEntryType.NUMBER:
                            constEntry.Value = entryValue;
                            break;
                    }

                    data.Entries.Add(constEntry);
                }

                return data;
            }
        }
    }


    public enum ConstantEntryType
    {
        UNDEF   = 0,
        STRING  = 1,
        NUMBER  = 4,
    }

    public sealed class ConstantEntry
    {
        public ConstantEntryType Type { get; internal set; }
        public object Value { get; internal set; }
    }
}
