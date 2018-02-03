using System;
using System.Diagnostics;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Used to get variables from the engine (?)
    /// </summary>
    public sealed class GetUrl : InstructionBase
    {
        public override InstructionType Type => InstructionType.GetURL;
        public override uint Size => 8;

        public override void Execute(ActionContext context)
        {
            var target = Parameters[0].ToString();
            var url = Parameters[1].ToString();

            Debug.WriteLine("[URL] Target: " + target + " URL: " + url);
        }
    }

    /// <summary>
    /// Used to get variables from the engine (?) (stack based)
    /// </summary>
    public sealed class GetUrl2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.GetURL2;

        public override void Execute(ActionContext context)
        {
            var target = context.Stack.Pop();
            var url = context.Stack.Pop().ToString();

            if(target.Type==ValueType.String)
                Debug.WriteLine("[URL2] Target: " + target + " URL: " + url);
            else
                Debug.WriteLine("[URL2] URL: " + url);
        }
    }
}
