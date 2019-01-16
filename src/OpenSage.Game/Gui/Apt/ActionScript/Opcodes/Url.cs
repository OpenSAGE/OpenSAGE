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
            var url = Parameters[0].ToString();
            var target  = Parameters[1].ToString();

            context.Apt.AVM.UrlHandler.Handle(url, target);
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

            context.Apt.AVM.UrlHandler.Handle(url, target.ToString());
        }
    }
}
