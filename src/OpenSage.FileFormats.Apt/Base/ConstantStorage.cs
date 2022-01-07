using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using OpenSage.FileFormats.Apt.ActionScript;

namespace OpenSage.FileFormats.Apt
{
    public sealed class ConstantStorage : IMemoryStorage
    {
        public List<ConstantEntry> Entries { get; internal set; }
        public uint AptDataEntryOffset { get; internal set; }

        public ConstantStorage()
        {
            Entries = new List<ConstantEntry>();
        }

        public static ConstantStorage Parse(BinaryReader reader)
        {
            var data = new ConstantStorage();

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
                    Type = reader.ReadUInt32AsEnum<ConstantType>()
                };

                //read the number/ string offset
                switch (constEntry.Type)
                {
                    case ConstantType.Undef:
                        throw new InvalidDataException("Undefined const entry");
                    case ConstantType.String:
                        constEntry.Value = reader.ReadStringAtOffset();
                        break;
                    case ConstantType.Register:
                        constEntry.Value = reader.ReadUInt32();
                        break;
                    case ConstantType.Boolean:
                        constEntry.Value = reader.ReadBooleanUInt32Checked();
                        break;
                    case ConstantType.Float:
                        constEntry.Value = reader.ReadSingle();
                        break;
                    case ConstantType.Integer:
                        constEntry.Value = reader.ReadInt32();
                        break;
                    case ConstantType.Lookup:
                        constEntry.Value = reader.ReadUInt32();
                        break;
                    case ConstantType.None:
                        constEntry.Value = null;
                        break;
                    default:
                        throw new InvalidDataException();
                }

                data.Entries.Add(constEntry);
            }

            return data;
        }
        public void Write(BinaryWriter writer, BinaryMemoryChain memory)
        {
            writer.Write(Encoding.ASCII.GetBytes(Constants.ConstFileHeader));
            writer.Write((UInt32) AptDataEntryOffset); 
            // var ___ = 0 / 0;
            writer.Write((UInt32) Entries.Count);
            writer.Write((UInt32) 32);

            for (var i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];

                writer.Write((UInt32) entry.Type);

                switch (entry.Type)
                {
                    case ConstantType.Undef:
                        throw new InvalidDataException("Undefined const entry");
                    case ConstantType.String:
                        writer.WriteStringAtOffset((string) entry.Value, memory);
                        break;
                    case ConstantType.Register:
                        writer.Write((UInt32) entry.Value);
                        break;
                    case ConstantType.Boolean:
                        writer.WriteBooleanUInt32((Boolean) entry.Value);
                        break;
                    case ConstantType.Float:
                        writer.Write((Single) entry.Value);
                        break;
                    case ConstantType.Integer:
                        writer.Write((Int32) entry.Value);
                        break;
                    case ConstantType.Lookup:
                        writer.Write((UInt32) entry.Value);
                        break;
                    default:
                        throw new InvalidDataException();
                }
            }
        }
    }

}
