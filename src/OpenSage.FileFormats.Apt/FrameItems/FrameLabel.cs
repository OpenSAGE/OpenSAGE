using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt.FrameItems
{
    public sealed class FrameLabel : FrameItem
    {
        public string Name { get; private set; }
        public uint Flags { get; private set; }
        public uint FrameId { get; private set; }

        public static FrameLabel Parse(BinaryReader reader)
        {
            var label = new FrameLabel();
            label.Name = reader.ReadStringAtOffset();
            label.Flags = reader.ReadUInt32();
            label.FrameId = reader.ReadUInt32();
            return label;
        }
        public override void Write(BinaryWriter writer, MemoryPool pool)
        {
            writer.Write((UInt32) FrameItemType.FrameLabel);
            writer.WriteStringAtOffset(Name, pool);
            writer.Write(Flags);
            writer.Write(FrameId);
        }
    }
}
