namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Pops a bool from the stack. If the bool is true jump to the byte offset (parameter)
    /// </summary>
    public sealed class BranchIfTrue : InstructionBase
    {
        public override InstructionType Type => InstructionType.BranchIfTrue;
        public override uint Size => 4;

        public override void Execute(ActionContext context)
        {
            var cond = context.Stack.Pop().ToBoolean();

            //when the condition is true make the stream jump
            if (cond)
                context.Stream.Branch(Parameters[0].ToInteger());
        }
    }

    // <summary>
    /// Jump to the byte offset (parameter)
    /// </summary>
    public sealed class BranchAlways : InstructionBase
    {
        public override InstructionType Type => InstructionType.BranchAlways;
        public override uint Size => 4;

        public override void Execute(ActionContext context)
        {
            context.Stream.Branch(Parameters[0].ToInteger());
        }
    }
}
