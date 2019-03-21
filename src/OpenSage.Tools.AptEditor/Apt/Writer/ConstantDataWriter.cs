using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenSage.FileFormats;
using OpenSage.Data.Apt;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.Apt.Writer
{
    public static class ConstantDataWriter
    {
        public static byte[] Write(UInt32 AptDataEntryOffset, ConstantData constantData)
        {
            var memory = new MemoryPool();
            memory.WriteDataToNewChunk(Encoding.ASCII.GetBytes(Constants.ConstFileMagic));
            memory.AllocateBytesForPadding(4);

            // write entryOffset, entriesCount and 32
            using (var header = new MemoryStream(memory.Allocate(Constants.IntPtrSize * 3).Memory, true))
            using (var headerWriter = new BinaryWriter(header))
            {
                headerWriter.Write((UInt32)AptDataEntryOffset);
                headerWriter.Write((UInt32)constantData.Entries.Count);
                headerWriter.Write((UInt32)32);
            }

            // Assuming sizeof(ConstantEntry) is always 8 bytes...
            const uint entrySize = Constants.IntPtrSize + Constants.IntPtrSize;
            var entriesChunk = memory.Allocate((uint)constantData.Entries.Count * entrySize);
            using (var entriesOutput = new MemoryStream(entriesChunk.Memory))
            using (var entriesWriter = new BinaryWriter(entriesOutput))
            {
                foreach (var entry in constantData.Entries)
                {
                    entriesWriter.Write((UInt32)entry.Type);

                    switch (entry.Type)
                    {
                        case ConstantEntryType.Undef:
                            throw new InvalidDataException("Undefined const entry");
                        case ConstantEntryType.String:
                            var address = DataWriter.Write(memory, (String)entry.Value);
                            entriesWriter.Write((UInt32)address);
                            break;
                        case ConstantEntryType.Register:
                            entriesWriter.Write((UInt32)entry.Value);
                            break;
                        case ConstantEntryType.Boolean:
                            entriesWriter.WriteBooleanUInt32((Boolean)entry.Value);
                            break;
                        case ConstantEntryType.Float:
                            entriesWriter.Write((Single)entry.Value);
                            break;
                        case ConstantEntryType.Integer:
                            entriesWriter.Write((Int32)entry.Value);
                            break;
                        case ConstantEntryType.Lookup:
                            entriesWriter.Write((UInt32)entry.Value);
                            break;
                        default:
                            throw new InvalidDataException();
                    }
                }
            }

            return memory.GetMemoryStream().ToArray();
        }
    }
}