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
    public enum CodeType
    {
        Sequential,
        Case,
        Loop
    }


    // AST
    public class CodeTree
    {
        public List<Node> NodeList;

        // The decompilation depends on the assumption that
        // ConstantPool() is only executed at most once in one segment.
        public List<Value> Constants;
        public Dictionary<int, string> RegNames;
        public CodeType Type { get; private set; }

        public CodeTree(IEnumerable<Value>? constants = null, IDictionary<int, string>? regNames = null)
        {
            Type = CodeType.Sequential;
            NodeList = new();
            Constants = new();
            if (constants != null)
                Constants.AddRange(constants);
            if (regNames != null)
                RegNames = new(regNames);
            else
                RegNames = new();
        }

        public bool ToStringInCodingForm(Value v, out string ret)
        {
            ret = string.Empty;
            var ans = true;
            if (v.Type == ValueType.Constant)
                if (Constants != null && v.ToInteger() >= 0 && v.ToInteger() < Constants.Count)
                {
                    v = Constants[v.ToInteger()];
                    if (v.Type == ValueType.String)
                        ret = v.ToString();
                }
                else
                {
                    ret = $"const[{v.ToInteger()}]";
                    ans = false;
                }

            if (v.Type == ValueType.Register)
                if (RegNames != null && RegNames.TryGetValue(v.ToInteger(), out var reg))
                {
                    ret = reg;
                    ans = false;
                }
                else
                {
                    ret = $"reg[{v.ToInteger()}]";
                    ans = false;
                }

            if (string.IsNullOrEmpty(ret))
                ret = v.ToString();
            return ans;
        }
        public NodeExpression? FindFirstNodeExpression(bool deleteIfPossible = true)
        {
            var ind = NodeList.FindLastIndex(n => n is NodeExpression);
            NodeExpression? ret = null;
            if (ind == -1)
                ret = null;
            else
            {
                var node = (NodeExpression) NodeList[ind];
                var ableToDelete = true; // TODO some special nodes shouldn't be deleted like Enumerate
                if (ableToDelete && deleteIfPossible)
                    NodeList.RemoveAt(ind);
                ret = node;
            }
            return ret;
        }
        public NodeArray ReadArray(bool readPair = false, bool ensureCount = true)
        {
            NodeArray ans = new();
            var nexp = FindFirstNodeExpression();
            Value? countVal = null;
            var flag = nexp == null ? false : nexp.GetValue(this, out countVal);
            if (flag)
            {
                var count = countVal!.ToInteger();
                if (readPair) count *= 2;
                for (int i = 0; i < count; ++i)
                {
                    var exp = FindFirstNodeExpression();
                    if (exp != null || (exp == null && ensureCount))
                        ans.Expressions.Add(exp);
                }
            }
            return ans;
        }

        public static CodeTree DecompileToTree(InstructionBlock root, IEnumerable<Value>? constants, IDictionary<int, string>? regNames)
        {
            CodeTree Tree = new(constants, regNames);
            foreach (var kvp in root.Items)
            {
                var inst = kvp.Value;
                Node node = inst.IsStatement ? new NodeStatement(inst) : new NodeExpression(inst);
                node.GetExpressions(Tree);
                Tree.NodeList.Add(node);
            }
            return Tree;
        }

        public string GetCode(int indent = 0, CodeType type = CodeType.Sequential)
        {
            var tc = Type;
            Type = type;
            StringBuilder ans = new();
            var returnLine = false;
            foreach (var node in NodeList)
            {
                if (returnLine == false)
                    returnLine = true;
                else
                    ans.Append('\n');

                var addDiv = true;
                var code = "[[null node]]";
                if (node != null)
                {
                    code = node.GetCode(this);
                }

                if (code.EndsWith("@"))
                {
                    code = code.Substring(0, code.Length - 1);
                    addDiv = false;
                }
                if (string.IsNullOrWhiteSpace(code))
                    returnLine = false;
                else
                    ans.Append(code.ToStringWithIndent(indent));
                if (addDiv)
                    ans.Append(';');
            }
            Type = tc;
            return ans.ToString();
        }
    }

    // Nodes

    public abstract class Node
    {
        public List<NodeExpression?> Expressions = new();
        public readonly InstructionBase Instruction;

        public Node(InstructionBase inst) { Instruction = inst; }

        public virtual string GetCode(CodeTree tree)
        {
            var vals = new string[Expressions.Count];
            for (int i = 0; i < Expressions.Count; ++i)
            {
                var node = Expressions[i];
                if (node == null)
                {
                    vals[i] = $"args[{i}]";
                }
                else
                {
                    var flag = node.GetValue(tree, out var val);
                    if (flag)
                    { // Formatize strings
                        var flag2 = tree.ToStringInCodingForm(val!, out vals[i]);
                        // Special judge to flexible argument type opcodes
                        if (flag2 && ((
                                Instruction.Type == InstructionType.Add2 ||
                                Instruction.Type == InstructionType.GetURL ||
                                Instruction.Type == InstructionType.GetURL2 ||
                                Instruction.Type == InstructionType.StringConcat ||
                                Instruction.Type == InstructionType.StringEquals
                            ) || (i == 0 && (
                                Instruction.Type == InstructionType.DefineLocal ||
                                Instruction.Type == InstructionType.Var ||
                                Instruction.Type == InstructionType.ToInteger ||
                                Instruction.Type == InstructionType.ToString ||
                                Instruction.Type == InstructionType.SetMember ||
                                Instruction.Type == InstructionType.SetVariable ||
                                Instruction.Type == InstructionType.SetProperty ||
                                Instruction.Type == InstructionType.EA_PushString ||
                                Instruction.Type == InstructionType.EA_SetStringMember ||
                                Instruction.Type == InstructionType.EA_SetStringVar ||
                                Instruction.Type == InstructionType.EA_PushConstantByte ||
                                Instruction.Type == InstructionType.EA_PushConstantWord ||
                                Instruction.Type == InstructionType.Trace
                            ))) &&
                            (val!.Type == ValueType.String || val.Type == ValueType.Constant))
                            vals[i] = $"\"{vals[i].Replace("\n", "\\n").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\"", "\\\"")}\"";
                    }
                    else
                    {
                        vals[i] = node.GetCode(tree);
                        // if ins pre higher than v pre, ()
                        if (node.Instruction != null && Instruction.Precendence > node.Instruction.Precendence)
                            vals[i] = $"({vals[i]})";
                    }
                }

            }
            string ret = string.Empty;
            string tmp = string.Empty;
            switch (Instruction.Type)
            {
                case InstructionType.BranchAlways:
                    if (tree.Type == CodeType.Case)
                        ret = string.Empty;
                    else// if (tree.Type == CodeType.Loop)
                    {
                        var itmp = Instruction;
                        if (itmp is LogicalTaggedInstruction itag)
                            itmp = itag.FinalInnerAction;

                        if (itmp.Parameters[0].ToInteger() > 0)
                            ret = "break";
                        else
                            ret = "continue";
                    }
                    /*
                    else
                    {
                        if (Instruction is LogicalTaggedInstruction itag)
                            tmp = itag.AdditionalData == null ? itag.Tag : (string) itag.AdditionalData;
                        else
                            tmp = $"[[{Instruction.Parameters[0]}]]";
                        ret = $"goto {tmp}; // {Instruction.ToString2(vals)}@";
                    }
                    */
                    break;
                case InstructionType.BranchIfTrue:
                case InstructionType.EA_BranchIfFalse:
                    tmp = vals[0];
                    if (Instruction.Type == InstructionType.BranchIfTrue)
                        if (tmp.StartsWith("!"))
                            tmp = tmp.Substring(1);
                        else
                            tmp = $"!({tmp})";
                    if (!(tmp.StartsWith('(') && tmp.EndsWith(')')))
                        tmp = $"({tmp})";

                    if (tree.Type == CodeType.Case)
                        ret = $"if {tmp}@";
                    else if (tree.Type == CodeType.Loop)
                        ret = $"while {tmp}@";
                    else
                        ret = $"goto [[{Instruction.Parameters[0]}]]; // {Instruction.ToString2(vals)}@";
                    break;
                case InstructionType.DefineFunction:
                case InstructionType.DefineFunction2:
                    string name = Instruction.Parameters[0].ToString();
                    List<string> args = new();
                    int nrArgs = Instruction.Parameters[1].ToInteger();
                    for (int i = 0; i < nrArgs; ++i)
                    {
                        if (Instruction.Type == InstructionType.DefineFunction2)
                            args.Add(Instruction.Parameters[4 + i * 2 + 1].ToString());
                        else
                            args.Add(Instruction.Parameters[2 + i].ToString());
                    }
                    ret = $"{name}{(!string.IsNullOrEmpty(name) ? " = " : "")}function({string.Join(", ", args.ToArray())})";
                    break;
                default:
                    try
                    {
                        ret = Instruction.ToString(vals);
                    }
                    catch
                    {
                        ret = Instruction.ToString2(vals);
                    }
                    break;
            }
            return ret;
        }
        public void GetExpressions(CodeTree tree)
        {
            Expressions = new();
            var instruction = Instruction;
            if (Instruction is LogicalTaggedInstruction itag)
                instruction = itag.FinalInnerAction;
            // special process and overriding regular process
            var flagSpecialProc = true;
            switch (instruction.Type)
            {
                // type 1: peek but no pop
                case InstructionType.SetRegister:
                    Expressions.Add(tree.FindFirstNodeExpression(false));
                    // TODO resolve register
                    break;
                case InstructionType.PushDuplicate:
                    Expressions.Add(tree.FindFirstNodeExpression(false));
                    break;

                // type 2: need to read args
                case InstructionType.InitArray:
                    Expressions.Add(tree.ReadArray());
                    break;
                case InstructionType.ImplementsOp:
                case InstructionType.CallFunction:
                case InstructionType.EA_CallFuncPop:
                case InstructionType.NewObject:
                    Expressions.Add(tree.FindFirstNodeExpression());
                    Expressions.Add(tree.ReadArray());
                    break;
                case InstructionType.CallMethod:
                case InstructionType.EA_CallMethod:
                case InstructionType.EA_CallMethodPop:
                case InstructionType.NewMethod:
                    Expressions.Add(tree.FindFirstNodeExpression());
                    Expressions.Add(tree.FindFirstNodeExpression());
                    Expressions.Add(tree.ReadArray());
                    break;
                case InstructionType.EA_CallNamedFuncPop:
                case InstructionType.EA_CallNamedFunc:
                    Expressions.Add(new NodeValue(instruction));
                    Expressions.Add(tree.ReadArray());
                    break;
                case InstructionType.EA_CallNamedMethodPop:
                    Expressions.Add(new NodeValue(instruction));
                    Expressions.Add(tree.FindFirstNodeExpression());
                    Expressions.Add(tree.ReadArray());
                    break;
                case InstructionType.EA_CallNamedMethod:
                    Expressions.Add(new NodeValue(instruction)); // TODO resolve constant
                    Expressions.Add(tree.FindFirstNodeExpression());
                    Expressions.Add(tree.ReadArray());
                    Expressions.Add(tree.FindFirstNodeExpression());
                    break;
                case InstructionType.InitObject:
                    Expressions.Add(tree.ReadArray(true));
                    break;

                // type 3: constant resolve needed
                case InstructionType.EA_GetNamedMember:
                    Expressions.Add(new NodeValue(instruction)); // TODO resolve constant
                    Expressions.Add(tree.FindFirstNodeExpression());
                    break;
                case InstructionType.EA_PushValueOfVar:
                    Expressions.Add(new NodeValue(instruction)); // TODO resolve constant; incode form?
                    break;
                case InstructionType.EA_PushGlobalVar:
                case InstructionType.EA_PushThisVar:
                case InstructionType.EA_PushGlobal:
                case InstructionType.EA_PushThis:
                    break; // nothing needed
                case InstructionType.EA_PushConstantByte:
                case InstructionType.EA_PushConstantWord:
                    Expressions.Add(new NodeValue(instruction, true)); // TODO resolve constant
                    break;
                case InstructionType.EA_PushRegister:
                    Expressions.Add(new NodeValue(instruction, true)); // TODO resolve register
                    break;

                // type 4: variable output count
                case InstructionType.PushData:
                    // TODO
                    break;
                case InstructionType.Enumerate:
                    // TODO
                    break;
                case InstructionType.Enumerate2:
                    // TODO
                    break;

                // no hits
                default:
                    flagSpecialProc = false;
                    break;
            }
            if ((!flagSpecialProc) && instruction is InstructionMonoPush inst)
            {
                // TODO string output
                for (int i = 0; i < inst.StackPop; ++i)
                    Expressions.Add(tree.FindFirstNodeExpression());
            }
            else if (!flagSpecialProc) // not implemented instructions
            {
                throw new NotImplementedException(instruction.Type.ToString());
            }

            // filter string

        }
    }
    public class NodeExpression : Node
    {
        public NodeExpression(InstructionBase inst) : base(inst)
        {
        }

        public virtual bool GetValue(CodeTree tree, out Value? ret)
        {
            ret = null;
            var vals = new Value[Expressions.Count];
            for (int i = 0; i < Expressions.Count; ++i)
            {
                var node = Expressions[i];
                if (node == null)
                    return false;
                var flag = node.GetValue(tree, out var val);
                if (!flag)
                    return false;
                else
                {
                    if (val!.Type == ValueType.Constant || val.Type == ValueType.Register)
                    {
                        var flag2 = tree.ToStringInCodingForm(val, out var str);
                        if (!flag2)
                            return false;
                        else
                            val = Value.FromString(str);
                    }
                    vals[i] = val;
                }

            }
            try
            {
                if (Instruction is InstructionMonoPush inst && inst.PushStack)
                {
                    ret = inst.ExecuteWithArgs2(vals);
                }
                else
                {
                    //TODO
                    return false;
                }

            }
            catch (NotImplementedException)
            {
                return false;
            }
            return true;
        }

        public override string GetCode(CodeTree tree)
        {
            string ret = string.Empty;
            if (GetValue(tree, out var val))
            {
                tree.ToStringInCodingForm(val!, out ret);
            }
            else // if (Instruction is InstructionMonoPush inst)
            {
                ret = base.GetCode(tree);
            }
            return ret;
        }
    }
    public class NodeValue : NodeExpression
    {
        public Value Value;
        public NodeValue(InstructionBase inst, bool iss = false, Value? v = null) : base(inst)
        {
            Value = v == null ? inst.Parameters[0] : v;
        }
        public override bool GetValue(CodeTree tree, out Value ret)
        {
            // Better not use FromArray()
            ret = Value;
            return true;
        }
    }
    public class NodeArray : NodeExpression
    {
        public NodeArray() : base(new InitArray())
        {

        }

        public override bool GetValue(CodeTree tree, out Value? ret)
        {
            // Better not use FromArray()
            ret = null;
            return false;
        }

        public override string GetCode(CodeTree tree)
        {
            var vals = new string[Expressions.Count];
            for (int i = 0; i < Expressions.Count; ++i)
            {
                var node = Expressions[i];
                if (node == null)
                {
                    vals[i] = $"args[{i}]";
                    continue;
                }
                var flag = node.GetValue(tree, out var val);
                if (!flag)
                    vals[i] = node.GetCode(tree);
                else
                {
                    tree.ToStringInCodingForm(val!, out vals[i]);
                    if (val!.Type == ValueType.String || val.Type == ValueType.Constant)
                        vals[i] = $"\"{vals[i]}\"";
                }
            }
            return $"{string.Join(", ", vals)}";
        }
    }
    public class NodeStatement : Node
    {
        public NodeStatement(InstructionBase inst) : base(inst)
        {
        }

        public override string GetCode(CodeTree tree)
        {
            return base.GetCode(tree);
        }
    }
    public class NodeControl : NodeStatement
    {
        public NodeControl(InstructionBase inst) : base(inst)
        {
        }

        public override string GetCode(CodeTree tree)
        {
            throw new NotImplementedException();
        }
    }

}
