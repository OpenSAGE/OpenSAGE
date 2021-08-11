using System.IO;
using System;
using OpenSage.FileFormats.Apt.ActionScript;

namespace OpenSage.FileFormats.Apt.FrameItems
{
    public sealed class Action : FrameItem
    {
        public InstructionStorage Instructions { get; private set; }

        public static Action Parse(BinaryReader reader)
        {
            var action = new Action();
            var instructionsPosition = reader.ReadUInt32();
            action.Instructions = InstructionStorage.Parse(reader.BaseStream, instructionsPosition);
            return action;
        }

        public override void Write(BinaryWriter writer, MemoryPool pool)
        {
            writer.Write((UInt32) FrameItemType.Action);
            writer.WriteInstructions(Instructions, pool);
        }
    }
}
