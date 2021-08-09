using System.Collections.Generic;
using System.IO;
using OpenSage.FileFormats.Apt.FrameItems;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt
{
    public sealed class Frame
    {
        public List<FrameItem> FrameItems { get; private set; }

        public static Frame Parse(BinaryReader reader)
        {
            var frame = new Frame();
            frame.FrameItems = reader.ReadListAtOffset<FrameItem>(() => FrameItem.Create(reader), true);

            return frame;
        }

        public static Frame Create(List<FrameItem> frameItems)
        {
            var frame = new Frame();
            frame.FrameItems = frameItems;
            return frame;
        }
    }
}
