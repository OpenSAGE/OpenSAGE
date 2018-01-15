﻿using System.Collections.Generic;
using System.IO;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Data.Apt.FrameItems
{
    public sealed class Action : FrameItem
    {
        public List<InstructionBase> Instructions { get; private set; }

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
