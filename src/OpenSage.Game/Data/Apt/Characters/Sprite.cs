using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.FileFormats;

namespace OpenSage.Data.Apt.Characters
{
    public sealed class Sprite : Playable
    {
        public static Sprite Parse(BinaryReader reader)
        {
            return new Sprite
            {
                Frames = reader.ReadListAtOffset(() => Frame.Parse(reader))
            };
        }

        public static Sprite Create(AptFile container)
        {
            return new Sprite
            {
                Container = container,
                Frames = new List<Frame> { Frame.Create(new List<FrameItem>()) }
            };
        }
    }
}
