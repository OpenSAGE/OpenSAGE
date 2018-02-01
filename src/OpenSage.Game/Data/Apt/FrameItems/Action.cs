﻿using System.Collections.Generic;
using System.IO;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Data.Apt.FrameItems
{
    public sealed class Action : FrameItem
    {
        public InstructionCollection Instructions { get; private set; }

        public static Action Parse(BinaryReader reader)
        {
            var action = new Action();
            action.Instructions = new InstructionCollection(reader.BaseStream);
            action.Instructions.Parse();
            return action;
        }
    }
}
