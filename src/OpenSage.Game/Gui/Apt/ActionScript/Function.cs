using System;
using System.Collections.Generic;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    [Flags]
    public enum FunctionPreloadFlags
    {
        PreloadExtern = 0x010000,   //this seems to be added by EA
        PreloadParent = 0x008000,
        PreloadRoot = 0x004000,
        SupressSuper = 0x002000,
        PreloadSuper = 0x001000,
        SupressArguments = 0x000800,
        PreloadArguments = 0x000400,
        SupressThis = 0x000200,
        PreloadThis = 0x000100,
        PreloadGlobal = 0x000001
    }

    public class FunctionArgument
    {
        public int Register;
        public string Parameter;
    }

    public sealed class Function
    {
        public InstructionCollection Instructions { get; set; }
        public List<Value> Parameters { get; set; }
        public int NumberRegisters { get; set; }
        public FunctionPreloadFlags Flags { get; set; }
        public bool IsNewVersion { get; set; }
    }
}
