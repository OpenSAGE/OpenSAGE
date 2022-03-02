using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.FileFormats.Apt.FrameItems;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt
{
    public sealed class Frame : IMemoryStorage
    {
        [DataStorageList(typeof(FrameItem), new[] { typeof(Action), typeof(BackgroundColor), typeof(FrameLabel), typeof(InitAction), typeof(PlaceObject), typeof(RemoveObject) })]
        public List<FrameItem> FrameItems { get; private set; }

        public Frame()
        {
            FrameItems = new();
        }

        public Frame(IEnumerable<FrameItem> frameItems)
        {
            FrameItems = frameItems.ToList();
        }

        private Frame(List<FrameItem> fi, bool mark)
        {
            FrameItems = fi;
        }

        public static Frame Parse(BinaryReader reader)
        {
            var fi = reader.ReadListAtOffset<FrameItem>(() => FrameItem.Parse(reader), true);
            return new(fi, true);
        }

        public void Write(BinaryWriter writer, BinaryMemoryChain memory)
        {
            writer.WriteArrayAtOffsetWithSize(FrameItems, memory, true);
        }

        public static Frame Create(List<FrameItem> frameItems)
        {
            var frame = new Frame(frameItems, true);
            return frame;
        }
    }
}
