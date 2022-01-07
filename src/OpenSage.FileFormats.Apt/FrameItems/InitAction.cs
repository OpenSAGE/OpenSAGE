using System;
using System.IO;
using OpenSage.FileFormats.Apt.ActionScript;

namespace OpenSage.FileFormats.Apt.FrameItems
{
    //called when a sprite is created
    public sealed class InitAction : FrameItem
    {
        public uint Sprite { get; private set; }
        public InstructionStorage Instructions { get; private set; }

        public static new InitAction Parse(BinaryReader reader)
        {
            var action = new InitAction();
            action.Sprite = reader.ReadUInt32();
            var instructionsPosition = reader.ReadUInt32();
            action.Instructions = InstructionStorage.Parse(reader.BaseStream, instructionsPosition);
            return action;
        }

        public override void Write(BinaryWriter writer, BinaryMemoryChain pool)
        {
            writer.Write((UInt32) FrameItemType.InitAction);
            writer.Write(Sprite);
            writer.WriteInstructions(Instructions, pool);
        }
    }
}
