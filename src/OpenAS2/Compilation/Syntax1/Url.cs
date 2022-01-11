using OpenAS2.Base;

namespace OpenAS2.Compilation.Syntax
{
    /// <summary>
    /// Used to get variables from the engine (?)
    /// </summary>
    public sealed class GetUrl : InstructionEvaluableMonoInput
    {
        public override InstructionType Type => InstructionType.GetURL;
        public override uint Size => 8;

        public override void Execute(ExecutionContext context)
        {
            var url = Parameters[0].ToString();
            var target = Parameters[1].ToString();

            context.Avm.Dom.Handle(context, url, target);
        }
        public override string ToString(string[] p)
        {
            return $"getUrl(\"{Parameters[0]}\", \"{Parameters[1]}\")";
        }
    }

    /// <summary>
    /// Used to get variables from the engine (?) (stack based)
    /// </summary>
    public sealed class GetUrl2 : InstructionEvaluableMonoInput
    {
        public override uint StackPop => 2;
        public override InstructionType Type => InstructionType.GetURL2;

        public override void Execute(ExecutionContext context)
        {
            var target = context.Pop();
            var url = context.Pop().ToString();

            context.Avm.Dom.Handle(context, url, target.ToString());
        }
        public override string ToString(string[] p)
        {
            return $"loadVariables({p[1]}, {p[0]})";
        }
    }
}
