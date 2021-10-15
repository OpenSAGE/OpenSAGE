using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.FileFormats.Apt;
using OpenSage.FileFormats.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using Value = OpenSage.Gui.Apt.ActionScript.Value;
using ValueType = OpenSage.Gui.Apt.ActionScript.ValueType;

namespace OpenSage.Tools.AptEditor.ActionScript
{
    public static class InstUtils
    {
        public static Value? ParseValue(Value? v, IEnumerable<Value?> consts, IDictionary<int, Value?>? regs)
        {
            if (v == null)
                return v;
            while (v != null && (v.Type == ValueType.Constant || v.Type == ValueType.Register))
            {
                if (v.Type == ValueType.Constant)
                {
                    v = consts.ElementAt(v.ToInteger());
                    if (v.Type == ValueType.Constant)
                        throw new InvalidOperationException();
                }
                else
                {
                    if (regs != null && regs.TryGetValue(v.ToInteger(), out var v_))
                        v = v_;
                    else
                        break;
                }
            }
            return v;
        }

        public static string? ToStringWithIndent(this object obj, int indent = 0)
        {
            if (indent <= 0)
                return obj.ToString();
            var s = new StringBuilder();
            for (int i = 0; i < indent; ++i)
                s.Append(" ");
            s.Append(obj.ToString());
            return s.ToString();
        }

        public static string ToCodingForm(this string str)
        {
            return $"\"{str.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\"", "\\\"")}\"";
        }

        public static InstructionGraph? Graphify(
            InstructionCollection? ci, 
            List<ConstantEntry>? constSource = null,
            List<Value>? constPool = null,
            Dictionary<int, string>? regNames = null
            )
        {
            if (ci == null)
                return null;

            var g = new InstructionGraph(ci, constSource, 0, constPool, regNames);

            if (constSource == null) return g;

            // wtf
            var c = StructurizedBlockChain.Parse(g.BaseBlock);
            Console.WriteLine("\nGan Si Huang Xu Dong");
            c.Print(g.ConstPool, g.RegNames);
            // change to:
            var p = NodePool.ConvertToAST(c, g.ConstPool, g.RegNames);
            var sc = new StatementCollection(p);
            // var code = some recursive function of sc to print
            // console.write(code)

            return g;
        }
    }

}
