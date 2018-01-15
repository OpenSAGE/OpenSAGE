using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Data.Apt.FrameItems
{
    public sealed class Action : FrameItem
    {
        public List<IInstruction> Instructions { get; private set; }

        public static Action Parse(BinaryReader reader)
        {
            var action = new Action();
            var instructionReader = new InstructionReader(reader.BaseStream);
            instructionReader.Parse();
            action.Instructions = instructionReader.Instructions;
            return action;
        }
    }
}
