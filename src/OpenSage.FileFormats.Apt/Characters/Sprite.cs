using System.Collections.Generic;
using System.IO;
using System;
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

        public override void Write(BinaryWriter writer, BinaryMemoryChain pool)
        {
            writer.Write((UInt32) CharacterType.Sprite);
            writer.Write((UInt32) Character.SIGNATURE);

            writer.WriteArrayAtOffsetWithSize(Frames, pool);
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
