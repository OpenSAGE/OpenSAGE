using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public class Frame
    {
        public List<FrameItem> FrameItems { get; private set; }

        public static Frame Parse(BinaryReader br)
        {
            var f = new Frame();
            f.FrameItems = br.ReadListAtOffset<FrameItem>(() => FrameItem.Create(br),true);

            return f;
        }
    }
}
