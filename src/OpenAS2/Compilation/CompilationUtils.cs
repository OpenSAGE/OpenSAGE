using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAS2.Base;
using OpenAS2.Runtime;
using OpenAS2.Compilation.Syntax;
using Value = OpenAS2.Runtime.Value;
using ValueType = OpenAS2.Runtime.ValueType;
using System.Text.Json;

namespace OpenAS2.Compilation
{
    using RawInstructionStorage = SortedList<int, RawInstruction>;

    public static class CompilationUtils
    {
        public static IList<Value> CreateConstantPool(IList<RawValue> parameters, IList<ConstantEntry> globalPool)
        {
            var pool = new List<Value>();

            // The first parameter is the constant count, omit it
            for (var i = 1; i < parameters.Count; ++i)
            {
                Value result;
                var entry = globalPool[parameters[i].Integer];
                switch (entry.Type)
                {
                    case ConstantType.String:
                        result = Value.FromString((string) entry.Value);
                        break;
                        // to solve it, consider expanding wrapped value in EC to wider usages
                        /*
                    case ConstantType.Register:
                        result = Value.FromRegister((uint) entry.Value);
                        break;
                        */
                    default:
                        throw new NotImplementedException();
                }
                pool.Add(result);
            }

            return pool.AsReadOnly();
        }
        
        public static (string, List<string>) GetNameAndArguments(RawInstruction function)
        {
            string name = function.Parameters[0].String;
            List<string> args = new();
            int nrArgs = function.Parameters[1].Integer;
            for (int i = 0; i < nrArgs; ++i)
            {
                if (function.Type == InstructionType.DefineFunction2)
                    args.Add(function.Parameters[4 + i * 2 + 1].String);
                else
                    args.Add(function.Parameters[2 + i].String);
            }
            return (name, args);
        }
        public static string? ToStringWithIndent(this object obj, int indent = 0)
        {
            string ots = obj.ToString()!;
            if (ots == null)
                ots = "null";
            if (indent <= 0)
                return ots;
            var s = new StringBuilder();
            for (int i = 0; i < indent; ++i)
                s.Append(" ");
            s.Append(ots);
            return s.ToString();
        }

        public static string FormLabels(IEnumerable<string> l)
        {
            return string.Join(", ", l.ToArray());
        }

        public static string GetIncrementedName(string s, bool startFromZero = false)
        {
            var u = s.LastIndexOf('_');
            var add = u < 1 || u == s.Length - 1;
            int curNr = startFromZero ? -1 : 0;
            if (!add)
                add = !int.TryParse(s.Substring(u + 1), out curNr);
            return (add ? s + "_" : s.Substring(0, u + 1)) + (curNr + 1).ToString();
        }

        public static string JustifyName(
            string s,
            int maxLength = 20,
            string illegal = " ,./\\()[]<>`~+-*^=&|%$#@!'\":\t",
            string split = "{};\n",
            bool striptUnderscore = true
            )
        {
            foreach (var c in split)
                s = s.Split(c)[0];
            foreach (var c in illegal)
                s = s.Replace(c, '_');
            for (int i = 0; i < 16; ++i)
            {
                var s1 = s.Replace("__", "_");
                if (s1.Length == s.Length)
                    break;
                else
                    s = s1;
            }
            if (striptUnderscore)
            {
                if (s.StartsWith('_'))
                    s = s.Substring(1);
                if (s.EndsWith('_'))
                    s = s.Substring(0, s.Length - 1);
            }

            s = s.Replace("function", "func").Replace("prototype", "proto").Replace("system", "sys").Replace("object", "obj").Replace("variable", "var").Replace("initialize", "init");
            s = s.Replace("Function", "Func").Replace("Prototype", "Proto").Replace("System", "Sys").Replace("Object", "Obj").Replace("Variable", "Var").Replace("Initialize", "Init");

            if (int.TryParse(s.Substring(0, 1), out var _))
                s = "num_" + s;
            if (s.Length > maxLength)
                s = s.Substring(0, maxLength);

            if (s == "True" || s == "true" || s == "False" || s == "false")
                return "boolval";
            else if (s == "null")
                return "nullval";
            else if (s == "undefined")
                return "undefval";

            return s;
        }

        public static string ToCodingForm(this string str)
        {
            return JsonSerializer.Serialize(str);
        }




        public static string Decompile(
            RawInstructionStorage? ci,
            IList<ConstantEntry>? constSource = null,
            Dictionary<int, string>? regNames = null
            )
        {
            if (ci == null)
                return "// null instruction storage";

            var g_ = new InstructionGraph(ci, 0, null, regNames);
            var g = g_; // InstructionGraph.OptimizeGraph(g_);

            var c = StructurizedBlockChain.Parse(g.BaseBlock);

            var p = SyntaxNodePool.ConvertToAST(c, constSource, g.RegNames);

            var sc = new StatementCollection(p);
            var code = sc.Compile().ToString();
            return code;
        }
    }

}
