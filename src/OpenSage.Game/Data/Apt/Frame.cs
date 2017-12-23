using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
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
    }
}
