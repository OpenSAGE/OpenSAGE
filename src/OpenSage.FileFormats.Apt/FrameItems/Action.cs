using System.IO;
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
    }
}
