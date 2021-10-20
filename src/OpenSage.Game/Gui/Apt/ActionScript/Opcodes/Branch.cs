using OpenSage.FileFormats.Apt.ActionScript;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Pops a bool from the stack. If the bool is true jump to the byte offset (parameter)
    /// </summary>
    public sealed class BranchIfTrue : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.BranchIfTrue;
        public override uint Size => 4;
        public override bool PopStack => true;

        public override void Execute(ExecutionContext context)
        {
            var cond = context.Pop().ToBoolean();

            //when the condition is true make the stream jump
            if (cond)
                context.Stream.Branch(Parameters[0].ToInteger());
        }
    }

    // <summary>
    /// Jump to the byte offset (parameter)
    /// </summary>
    public sealed class BranchAlways : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.BranchAlways;
        public override uint Size => 4;

        public override void Execute(ExecutionContext context)
        {
            context.Stream.Branch(Parameters[0].ToInteger());
        }
    }
}
