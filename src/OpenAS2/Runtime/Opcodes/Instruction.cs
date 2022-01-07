using System;
using System.Collections.Generic;
using System.Linq;
using OpenAS2.Base;

namespace OpenAS2.Runtime
{


    /// <summary>
    /// The baseclass used for all Instructions. Set default variables accordingly
    /// </summary>
    public abstract class Instruction
    {
        public InstructionType Type { get; }
        public IReadOnlyList<Value> Parameters { get; }
        public bool Breakpoint { get; set; }

        public Instruction(RawInstruction ri)
        {
            Type = ri.Type;
            Parameters = new List<Value>(from x in ri.Parameters select Value.FromStorage(x)).AsReadOnly();
        }

        public Instruction(Instruction i)
        {
            Type = i.Type;
            Parameters = i.Parameters;
        }

        public override string ToString()
        {
            return ToString((ExecutionContext) null);
        }
        public virtual string GetParameterDesc(ExecutionContext context)
        {
            if (Parameters == null) return "";
            string[] pv;// = { };
            var param_val = Parameters.Take(5).ToArray();
            pv = param_val.Select(x => x.ToStringWithType(context)).ToArray();
            return string.Join(", ", pv);
        }
        public virtual string ToString(ExecutionContext context)
        {
            return $"{Type}({GetParameterDesc(context)})";//: {Size}";
        }

        public virtual string ToString2(string[] parameters)
        {
            var t = (Parameters == null || Parameters.Count == 0) ? Type.ToString() : ToString();
            return $"{t}({string.Join(", ", parameters)})";
        }
        public virtual string ToString(string[] p) { return ToString2(p); }
    }

}
