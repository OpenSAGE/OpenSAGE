using System.Collections.Generic;
using System.IO;
using OpenSage.FileFormats.Apt.FrameItems;
using OpenSage.FileFormats;
using OpenSage.FileFormats.Apt.ActionScript;

namespace OpenSage.FileFormats.Apt.Characters
{
    public sealed class Sprite : Playable
    {
        public InstructionStorage InitActions { get; set; }
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
