using System;
using System.Collections.Generic;

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

    public sealed class Function: ObjectContext
    {
        public static Function FunctionConstructor => ObjectContext._ffc;
        public static Function ObjectConstructor => ObjectContext._foc;

        public Function(): base()
        {
            __proto__ = FunctionPrototype;
            var prt = new ObjectContext();
            prt.constructor = this;
            this.prototype = prt;
        }

        internal Function(bool JustUsedToCreateObjectPrototype): base(JustUsedToCreateObjectPrototype)
        {
            __proto__ = FunctionPrototype;
        }

        public InstructionCollection Instructions { get; set; }
        public List<Value> Parameters { get; set; }
        public int NumberRegisters { get; set; }
        public List<Value> Constants { get; set; }
        public FunctionPreloadFlags Flags { get; set; }
        public bool IsNewVersion { get; set; }
    }
}
