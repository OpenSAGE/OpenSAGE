using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.FrameItems
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
    }
}
