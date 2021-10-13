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
        public static string ToStringWithIndent(this object obj, int indent = 0)
        {
            var s = new StringBuilder();
            for (int i = 0; i < indent; ++i)
                s.Append(" ");
            s.Append(obj.ToString());
            return s.ToString();
        }

        public static InstructionGraph? Graphify(
            InstructionCollection? c, 
            List<ConstantEntry>? constSource = null,
            List<Value>? constPool = null,
            Dictionary<int, string>? regNames = null
            )
        {
            if (c == null)
                return null;

            var g = new InstructionGraph(c, constSource, 0, constPool, regNames);

            if (constSource == null) return g;

            // wtf
            var p = StructurizedBlockChain.Parse(g.BaseBlock);
            Console.WriteLine("\nGan Si Huang Xu Dong");
            p.Print(g.ConstPool, g.RegNames);

            return g;
        }
    }

}
