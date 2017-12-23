using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public class Frame
    {
        public List<FrameItem> FrameItems { get; private set; }

        public static Frame Parse(BinaryReader reader)
        {
            var f = new Frame();
            f.FrameItems = reader.ReadListAtOffset<FrameItem>(() => FrameItem.Create(reader), true);

            return f;
        }
    }
}
