using System.Collections.Generic;
using System.IO;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Data.Apt.FrameItems
{
    //called when a sprite is created
    public sealed class InitAction : FrameItem
    {
        public uint Sprite { get; private set; }
        public InstructionCollection Instructions { get; private set; }

        public static InitAction Parse(BinaryReader reader)
        {
            var action = new InitAction();
            action.Sprite = reader.ReadUInt32();
            var instructions = new InstructionCollection(reader.BaseStream);
            instructions.Parse();
            action.Instructions = instructions;
            return action;
        }
    }
}
