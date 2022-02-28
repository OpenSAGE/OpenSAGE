using System.IO;
using System;
using OpenAS2.Base;

namespace OpenSage.FileFormats.Apt.FrameItems
{
    public sealed class Action : FrameItem
    {
        public InstructionStorage Instructions { get; private set; }

        public static new Action Parse(BinaryReader reader)
        {
            var action = new Action();
            var instructionsPosition = reader.ReadUInt32();
            action.Instructions = InstructionStorage.Parse(reader.BaseStream, instructionsPosition);
            return action;
        }

        public override void Write(BinaryWriter writer, BinaryMemoryChain pool)
        {
            writer.Write((UInt32) FrameItemType.Action);
            writer.WriteInstructions(Instructions, pool);
        }
    }
}
