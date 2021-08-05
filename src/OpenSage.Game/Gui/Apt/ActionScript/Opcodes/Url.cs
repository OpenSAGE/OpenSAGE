namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Used to get variables from the engine (?)
    /// </summary>
    public sealed class GetUrl : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.GetURL;
        public override uint Size => 8;

        public override void Execute(ActionContext context)
        {
            var url = Parameters[0].ToString();
            var target = Parameters[1].ToString();

            context.Apt.Avm.Handle(context, url, target);
        }
    }

    /// <summary>
    /// Used to get variables from the engine (?) (stack based)
    /// </summary>
    public sealed class GetUrl2 : InstructionMonoPushPop
    {
        public override uint StackPop => 2;
        public override InstructionType Type => InstructionType.GetURL2;

        public override void Execute(ActionContext context)
        {
            var target = context.Pop();
            var url = context.Pop().ToString();

            context.Apt.Avm.Handle(context, url, target.ToString());
        }
    }
}
