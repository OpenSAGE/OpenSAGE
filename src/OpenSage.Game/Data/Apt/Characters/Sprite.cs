using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
{
    public class Sprite : Character
    {
        public List<Frame> Frames { get; private set; }

        public static Sprite Parse(BinaryReader reader)
        {
            var sp = new Sprite();
            sp.Frames = reader.ReadListAtOffset<Frame>(() => Frame.Parse(reader));
            return sp;
        }
    }
}
