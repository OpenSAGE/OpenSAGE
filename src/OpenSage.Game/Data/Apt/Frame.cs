using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public class Frame
    {
        public List<FrameItem> FrameItems { get; private set; }

        public Frame(BinaryReader br)
        {
            FrameItems = br.ReadListAtOffset<FrameItem>();
        }
    }
}
