using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
{
    public sealed class Sprite : Character
    {
        public List<Frame> Frames { get; private set; }

        public static Sprite Parse(BinaryReader reader)
        {
            var sprite = new Sprite();
            sprite.Frames = reader.ReadListAtOffset<Frame>(() => Frame.Parse(reader));
            return sprite;
        }
    }
}
