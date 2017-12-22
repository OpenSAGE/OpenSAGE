using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
{
    public class Sprite : Character
    {
        public List<Frame> Frames { get; private set; }

        public static Sprite Parse(BinaryReader br)
        {
            var sp = new Sprite();
            sp.Frames = br.ReadListAtOffset<Frame>(() => Frame.Parse(br));
            return sp;
        }
    }
}
