using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSage.FileFormats.Apt;
using OpenSage.FileFormats.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using Value = OpenSage.Gui.Apt.ActionScript.Value;
using ValueType = OpenSage.Gui.Apt.ActionScript.ValueType;

namespace OpenSage.Tools.AptEditor.Apt.Editor
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
    }

    internal enum TagType
    {
        None, 
        Label,
        GotoLabel,
        DefineFunction, 
    }

    public enum CodeType
    {
        Sequential,
        Case,
        Loop
    }

    internal class LogicalTaggedInstruction : InstructionBase
    {
        public string Tag = "";
        public object? AdditionalData;
        public TagType TagType;
        public TagType FinalTagType { get { return InnerAction is LogicalTaggedInstruction l ? l.FinalTagType : TagType; } }
        public InstructionBase InnerAction { get; protected set; }
        public InstructionBase FinalInnerAction { get { return InnerAction is LogicalTaggedInstruction l ? l.FinalInnerAction : InnerAction; } }
        public LogicalTaggedInstruction FinalTaggedInnerAction { get { return InnerAction is LogicalTaggedInstruction l ? l.FinalTaggedInnerAction : this; } }

        public override InstructionType Type => InnerAction.Type;
        public override List<Value> Parameters => InnerAction.Parameters;
        public override bool IsStatement => InnerAction.IsStatement;
        public override int Precendence => InnerAction.Precendence;

        public override void Execute(ActionContext context) => throw new InvalidOperationException();
        public override bool Breakpoint
        {
            get { return InnerAction.Breakpoint; }
            set { InnerAction.Breakpoint = value; }
        }
        public LogicalTaggedInstruction(InstructionBase instruction, string tag = "", TagType type = TagType.None)
        {
            InnerAction = instruction;
            Tag = tag;
            TagType = type;
        }
        public override string GetParameterDesc(ActionContext context)
        {
            return InnerAction.GetParameterDesc(context);
        }
        public override string ToString(ActionContext context)
        {
            if (Tag == null || Tag == "") return InnerAction.ToString(context);

            return $"// Tagged: {Tag}\n{InnerAction.ToString(context)}";
        }
        public override string ToString(string[] p)
        {
            return InnerAction.ToString(p);
        }
    }

    internal class LogicalFunctionContext : LogicalTaggedInstruction
    {
        public LogicalInstructions Instructions { get; private set; }

        public LogicalFunctionContext(InstructionBase instruction,
            InstructionCollection insts,
            List<ConstantEntry>? constSource,
            int indexOffset = 0,
            List<Value>? constPool = null,
            Dictionary<int, string>? regNames = null) : base(instruction, "", TagType.DefineFunction)
        {   if (instruction is DefineFunction2 df2)
            {
                var flags = (FunctionPreloadFlags) df2.Parameters[3].ToInteger();
                regNames = Preload(flags);
            }
            Instructions = new LogicalInstructions(insts, constSource, indexOffset, constPool, regNames);
            
        }
        public static Dictionary<int, string> Preload(FunctionPreloadFlags flags)
        {
            int reg = 1;
            var _registers = new Dictionary<int, string>();
            if (flags.HasFlag(FunctionPreloadFlags.PreloadThis))
            {
                _registers[reg] = "this";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadArguments))
            {
                throw new NotImplementedException();
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadSuper))
            {
                _registers[reg] = "super";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadRoot))
            {
                _registers[reg] = "_root";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadParent))
            {
                _registers[reg] = "_parent";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadGlobal))
            {
                _registers[reg] = "_global";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadExtern))
            {
                _registers[reg] = "extern";
                ++reg;
            }
            return _registers;
        }

        // Probably it would be better to update InnerFunction.Parameter each time this.Parameter changes
        public List<Value> CreateRealParameters(int functionSize)
        {
            throw new NotImplementedException();
        }
        
    }

    internal class InstructionBlock
    {
        public SortedDictionary<int, InstructionBase> Items { get; set; }

        public LogicalTaggedInstruction? BranchCondition;
        public InstructionBlock? NextBlockCondition;
        public InstructionBlock? NextBlockDefault;

        public readonly InstructionBlock? PreviousBlock;
        public readonly int Hierarchy;

        public string? Label;

        public InstructionBlock(InstructionBlock? prev = null) {
            Items = new();
            PreviousBlock = prev;
            if (prev != null)
            {
                prev.NextBlockDefault = this;
                Hierarchy = prev.Hierarchy + 1;
            }
            else
                Hierarchy = 0;
        }

        public InstructionBlock(InstructionBlock b, bool copyNext)
        {
            Items = b.Items;
            BranchCondition = b.BranchCondition;
            NextBlockDefault = copyNext ? b.NextBlockDefault : null;
            NextBlockCondition = b.NextBlockCondition;
            PreviousBlock = b.PreviousBlock;
            Hierarchy = b.Hierarchy;
        }
    }

    internal class StructurizedBlockChain
    {
        
        public InstructionBlock StartBlock;
        public InstructionBlock EndBlock;
        public StructurizedBlockChain? SubChainStart;
        public StructurizedBlockChain? Next;
        public CodeType Type = CodeType.Sequential;
        public List<StructurizedBlockChain> AdditionalData = new();
        public bool Empty { get; set; }

        public override string ToString()
        {
            var inf = "\\infty";
            var emp = "Empty, ";
            return $"SBC(Type={Type}, {(Empty ? emp : string.Empty)}Range=[{StartBlock.Hierarchy}, {(EndBlock == null ? inf : EndBlock.Hierarchy)}])";
        }

        
        public StructurizedBlockChain(InstructionBlock start, InstructionBlock? end = null)
        {
            StartBlock = start;
            if (end == null)
            {
                EnsureEndBlock();
                if (EndBlock == null)
                    EndBlock = StartBlock;
            }
            else
                EndBlock = end;
            
        }

        public void PrintRaw(
            List<Value>? constPool = null,
            Dictionary<int, string>? regNames = null,
            int layer = 0,
            CodeType type = CodeType.Sequential
            )
        {
            // Console.WriteLine($"RAW {this}");
            if (Empty)
                return;
            var c = this;
            var currentBlock = c.StartBlock;
            while (currentBlock != null && (c.EndBlock == null || currentBlock.Hierarchy <= c.EndBlock!.Hierarchy))
            {
                //foreach (var a in currentBlock.Items)
                //    Console.WriteLine(a.Value);
                var tree = CodeTree.DecompileToTree(currentBlock, constPool, regNames);
                Console.WriteLine(tree.GetCode(layer * 4, type));
                currentBlock = currentBlock.NextBlockDefault;
            }
            c = c.Next;

        }

        public void Print(
            List<Value>? constPool = null,
            Dictionary<int, string>? regNames = null,
            int layer = 0,
            CodeType type = CodeType.Sequential
            )
        {
            // Console.WriteLine(this);
            if (Type == CodeType.Case)
            {
                //Console.WriteLine("If Statement {".ToStringWithIndent(layer * 4));
                AdditionalData[0].PrintRaw(constPool, regNames, layer, Type);
                Console.WriteLine("{".ToStringWithIndent(layer * 4));
                AdditionalData[1].Print(constPool, regNames, layer + 1);
                Console.WriteLine("} else {".ToStringWithIndent(layer * 4));
                AdditionalData[2].Print(constPool, regNames, layer + 1);
                Console.WriteLine("}".ToStringWithIndent(layer * 4));
            }
            else if (Type == CodeType.Loop)
            {
                // Console.WriteLine("While Statement {".ToStringWithIndent(layer * 4));
                AdditionalData[0].Print(constPool, regNames, layer, Type);
                Console.WriteLine("{".ToStringWithIndent(layer * 4));
                AdditionalData[1].Print(constPool, regNames, layer + 1);
                Console.WriteLine("}".ToStringWithIndent(layer * 4));
            }
            else if (SubChainStart != null)
            {
                var c = SubChainStart;
                while (c != null)
                {
                    c.Print(constPool, regNames, layer);
                    c = c.Next;
                }
            }
            else
                PrintRaw(constPool, regNames, layer, type);
        } 

        public static StructurizedBlockChain Parse(InstructionBlock root)
        {
            StructurizedBlockChain ret = new(root);

            // parse loop
            ret.ParseLoop();
            if (ret.SubChainStart == null)
            {
                ret.ParseCase();
            }


            return ret;
        }

        public void EnsureEndBlock()
        {
            if (EndBlock == null)
            {
                var b = StartBlock;
                while (b.NextBlockDefault != null)
                    b = b.NextBlockDefault;
                EndBlock = b;
            }
        }

        public void ParseCase()
        {
            // ensure all loops in the block is parsed!
            var currentChain = new StructurizedBlockChain(StartBlock, EndBlock) { Next = Next, Type = CodeType.Sequential };
            var subChainCache = currentChain;
            var containsCase = false;
            var b = StartBlock;

            while (b != null && (EndBlock == null || b.Hierarchy <= EndBlock.Hierarchy))
            {
                // identify if structures
                if (b.BranchCondition != null &&
                    (b.BranchCondition.Type == InstructionType.BranchIfTrue ||
                     b.BranchCondition.Type == InstructionType.EA_BranchIfFalse))
                {
                    containsCase = true;

                    var startBlock = b;
                    var endBlock = b.NextBlockCondition!.PreviousBlock!; // assume jump when if-statement is not executed
                    
                    // find the real end block of if structure
                    while (b!.Hierarchy < endBlock!.Hierarchy)
                    {
                        if (b.BranchCondition != null)
                        {
                            var bn = b.NextBlockCondition!;
                            var bt = b.BranchCondition.Type;
                            if (bt == InstructionType.BranchAlways && bn.Hierarchy > endBlock.Hierarchy)
                            {
                                endBlock = bn.PreviousBlock;
                            }
                        }
                        b = b.NextBlockDefault;
                    }

                    List<StructurizedBlockChain> structures = new();

                    StructurizedBlockChain sc = new(startBlock, startBlock);
                    StructurizedBlockChain st = new(startBlock.NextBlockDefault!, startBlock.NextBlockCondition!.PreviousBlock!);
                    StructurizedBlockChain sf = new(startBlock.NextBlockCondition!, endBlock);
                    structures.Add(sc);
                    structures.Add(st);
                    structures.Add(sf);

                    // cascaded parse
                    st.ParseCase();
                    sf.ParseCase();

                    // split

                    var sb2 = endBlock.NextBlockDefault != null ?
                        new StructurizedBlockChain(endBlock.NextBlockDefault, currentChain!.EndBlock) { Next = currentChain.Next } :
                        null;

                    var sb = new StructurizedBlockChain(startBlock, endBlock) { Next = sb2, Type = CodeType.Case, AdditionalData = structures };

                    if (startBlock.PreviousBlock == null)
                        currentChain!.Empty = true;
                    else
                        currentChain!.EndBlock = startBlock.PreviousBlock;
                    currentChain.Next = sb;
                    currentChain = sb2;

                }
                
                b = b.NextBlockDefault;
            }

            if (containsCase)
            {
                SubChainStart = subChainCache;
            }
        }

        public void ParseLoop()
        {
            var b = EndBlock;

            var currentChain = new StructurizedBlockChain(StartBlock) { EndBlock = EndBlock, Next = Next, Type = CodeType.Sequential };
            var subChainCache = currentChain;
            var containsLoop = false;
            while (b != null && (b.Hierarchy >= StartBlock.Hierarchy))
            {
                // identify for/while structures
                if (b.BranchCondition != null &&
                    b.BranchCondition.Type == InstructionType.BranchAlways &&
                    b.NextBlockCondition!.Hierarchy >= StartBlock.Hierarchy && // be not the block's loop itself
                    b.NextBlockCondition!.Hierarchy < b.Hierarchy) // could be a loop
                {
                    containsLoop = true;
                    // mark range
                    var startBlock = b.NextBlockCondition;
                    var endBlock = b;

                    InstructionBlock? outerBlock = null;
                    for (var i = startBlock; i != null && i.Hierarchy <= endBlock.Hierarchy; i = i.NextBlockDefault)
                    {
                        if (outerBlock == null ||
                            (i.BranchCondition != null &&
                             (i.BranchCondition!.Type == InstructionType.BranchIfTrue ||
                              i.BranchCondition!.Type == InstructionType.EA_BranchIfFalse ||
                              i.BranchCondition!.Type == InstructionType.BranchAlways) &&
                             i.NextBlockCondition!.Hierarchy > endBlock.Hierarchy))
                        {
                            if (outerBlock != null && i.NextBlockCondition!.Hierarchy != outerBlock.Hierarchy)
                                throw new InvalidOperationException();
                            else if (i.NextBlockCondition != null && i.NextBlockCondition.Hierarchy > endBlock.Hierarchy)
                            {
                                outerBlock = i.NextBlockCondition;
                            }
                        }
                    }

                    // TODO outer block sanity check?

                    List<StructurizedBlockChain> structures = new();

                    StructurizedBlockChain sc = new(startBlock, startBlock);
                    StructurizedBlockChain st = new(startBlock.NextBlockDefault!, endBlock);
                    structures.Add(sc);
                    structures.Add(st);

                    // split blocks
                    // TODO theoretically outerBlock should be the same as endBlock + 1

                    var sb2 = endBlock.NextBlockDefault != null ?
                        new StructurizedBlockChain(endBlock.NextBlockDefault, currentChain!.EndBlock) { Next = currentChain.Next } :
                        null;

                    if (sb2 != null)
                        sb2.ParseCase();

                    var sb = new StructurizedBlockChain(startBlock, endBlock) { Next = sb2, Type = CodeType.Loop, AdditionalData = structures };

                    if (startBlock.PreviousBlock == null)
                        currentChain!.Empty = true;
                    else
                        currentChain!.EndBlock = startBlock.PreviousBlock;
                    currentChain.Next = sb;

                    // cascaded parsing
                    st.ParseLoop();

                    // TODO judgement block parsing

                    // update b
                    b = startBlock;
                }

                b = b.PreviousBlock;
            }
            if (containsLoop)
            {
                currentChain.ParseCase();
                SubChainStart = subChainCache;
            }
        }
    }

    internal class CodeTree
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
                        
                } catch (NotImplementedException)
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
        public class NodeValue: NodeExpression
        {
            public Value Value;
            public NodeValue (InstructionBase inst, bool iss = false, Value? v = null) : base(inst)
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
        public class NodeArray: NodeExpression
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

    internal class LogicalInstructions
    {
        public SortedDictionary<int, InstructionBase> Items { get; set; }
        public InstructionBlock BaseBlock { get; set; }
        public InstructionCollection Insts { get; private set; }
        public int IndexOffset;
        public List<Value>? ConstPool;
        public Dictionary<int, string>? RegNames;

        // public 
        public LogicalInstructions(InstructionStorage insts) : this(InstructionCollection.Parse(insts)) { }
        public LogicalInstructions(InstructionCollection insts,
            List<ConstantEntry>? constSource = null,
            int indexOffset = 0,
            List<Value>? constPool = null,
            Dictionary<int, string>? regNames = null)
        {
            Insts = insts;
            IndexOffset = indexOffset;
            var stream = new InstructionStream(insts.AddEnd());

            ConstPool = constPool;
            RegNames = regNames;

            Items = new SortedDictionary<int, InstructionBase>();
            var branch_dict = new Dictionary<string, int>(); // position: label
            var label_number = 0;
            while (!stream.IsFinished())
            {
                var index = stream.Index;
                var instruction = stream.GetInstruction();

                if (instruction is ConstantPool cp && constSource != null)
                {
                    ActionContext ac = new ActionContext(null, null, null, 4) { GlobalConstantPool = constSource };
                    ac.ReformConstantPool(cp.Parameters);
                    ConstPool = ac.Constants;
                }
                if (instruction is DefineFunction || instruction is DefineFunction2)
                {
                    var nParams = instruction.Parameters[1].ToInteger();
                    var size =
                        instruction is DefineFunction ?
                        instruction.Parameters[2 + nParams].ToInteger() :
                        instruction.Parameters[4 + nParams * 2].ToInteger();
                    
                    var codes = stream.GetInstructions(size, true, true);
                    instruction = new LogicalFunctionContext(instruction, codes, constSource, IndexOffset + index + 1, ConstPool, RegNames);
                    ((LogicalFunctionContext) instruction).Instructions.ConstPool = ConstPool;
                }

                else if (instruction is BranchIfTrue || instruction is BranchAlways)
                {
                    var tag = $"label{++label_number}";
                    var index_dest = stream.GetBranchDestination(instruction.Parameters[0].ToInteger(), index + 1);
                    if (index_dest == -1)
                        branch_dict[tag + $": {index + 1}+({instruction.Parameters[0].ToInteger()})"] = index;
                    else
                        branch_dict[tag] = index_dest;
                    instruction = new LogicalTaggedInstruction(instruction, "Goto " + tag, TagType.GotoLabel) { AdditionalData = tag };
                }

                Items.Add(index, instruction);
            }
            // branch destination tag
            foreach (var kvp in branch_dict)
                Items[kvp.Value] = new LogicalTaggedInstruction(Items[kvp.Value], kvp.Key, TagType.Label) { AdditionalData = kvp.Key };

            // block division
            BaseBlock = new InstructionBlock();
            var mapLabelToBlock = new Dictionary<string, InstructionBlock>();
            var currentBlock = BaseBlock;
            // first iter: maintain BranchCondition & NextBlockDefault
            foreach (var kvp in Items)
            {
                var pos = kvp.Key;
                var inst = kvp.Value;
                if ((inst is LogicalTaggedInstruction lti && lti.TagType == TagType.Label) || inst is Enumerate2)// TODO need some instances to determine how to deal with Enumerate2
                {
                    var newBlock = new InstructionBlock(currentBlock);
                    newBlock.Items[pos] = inst;
                    currentBlock = newBlock;
                    if (inst is LogicalTaggedInstruction lti2) {

                        mapLabelToBlock[(string) lti2.AdditionalData!] = newBlock;
                        var instTemp = lti2;
                        while (instTemp.InnerAction is LogicalTaggedInstruction lti3)
                        {
                            instTemp = lti3;
                            mapLabelToBlock[(string) lti3.AdditionalData!] = newBlock;
                        }
                        
                        if (lti2.FinalTagType == TagType.GotoLabel)
                        {
                            var newerBlock = new InstructionBlock(newBlock);
                            newBlock.BranchCondition = lti2.FinalTaggedInnerAction;
                            currentBlock = newerBlock;
                        }
                    }
                }
                else if (inst is LogicalTaggedInstruction lti3 && lti3.TagType == TagType.GotoLabel)
                {
                    var new_block = new InstructionBlock(currentBlock);
                    currentBlock.BranchCondition = lti3.FinalTaggedInnerAction;
                    currentBlock.Items[pos] = inst;
                    currentBlock = new_block;
                }
                else
                {
                    currentBlock.Items[pos] = inst;
                }
            }
            // second iter: maintain Label
            foreach (var kvp in mapLabelToBlock)
            {
                var lbl = kvp.Key;
                var blk = kvp.Value;
                if (string.IsNullOrEmpty(blk.Label))
                    blk.Label = lbl;
                else
                    blk.Label += $", {lbl}";
            }
            // third iter: maintain NextBlockCondition
            currentBlock = BaseBlock;
            while (currentBlock != null)
            {
                if (currentBlock.BranchCondition != null)
                    if (mapLabelToBlock.TryGetValue((string) currentBlock.BranchCondition.AdditionalData!, out var blk))
                        currentBlock.NextBlockCondition = blk;
                    else
                        throw new InvalidOperationException();
                        // currentBlock.NextBlockCondition = new InstructionBlock() { Label = currentBlock.BranchCondition.Tag };
                currentBlock = currentBlock.NextBlockDefault;
            }

            // decompilation
            // will be eventually removed
            if (constSource == null) return;

            // structurize
            
            /*
            currentBlock = BaseBlock;
            while (currentBlock != null)
            {
                foreach (var a in currentBlock.Items)
                    Console.WriteLine(a);
                var tree = CodeTree.DecompileToTree(currentBlock, ConstPool, RegNames);
                Console.WriteLine(tree.GetCode());
                currentBlock = currentBlock.NextBlockDefault;
            }
            */
            var p = StructurizedBlockChain.Parse(BaseBlock);
            Console.WriteLine("Gan Si Huang Xu Dong");
            p.Print(ConstPool, RegNames);
            var gshxd = 1;

        }

        private int IndexOfNextRealInstruction(int currentIndex)
        {
            for (var i = currentIndex + 1; i < Items.Count; ++i)
            {
                switch (Items[i])
                {
                    case LogicalDestination _:
                    case LogicalEndOfFunction _:
                        continue;
                    default:
                        return i;
                }
            }
            throw new IndexOutOfRangeException();
        }

        public InstructionCollection ConvertToRealInstructions()
        {
            // A pretty stupid methodnull
            var positionOfBranches = new Dictionary<LogicalBranch, int>();
            var positionOfDefineFunctions = new Dictionary<LogicalDefineFunction, int>();
            var sortedList = new SortedList<int, InstructionBase>();
            for (var i = 0; i < Items.Count; ++i)
            {
                var current = Items[i];
                switch (current)
                {
                    case LogicalDestination logicalDestination:
                        {
                            var sourceIndex = positionOfBranches[logicalDestination.LogicalBranch];
                            var sourceNext = IndexOfNextRealInstruction(sourceIndex);
                            var destination = IndexOfNextRealInstruction(i);
                            var offset = destination - sourceNext;
                            var parameters = logicalDestination.LogicalBranch.InnerInstruction.Parameters;
                            parameters[0] = Value.FromInteger(offset);
                        }
                        break;
                    case LogicalEndOfFunction logicalEndOfFunction:
                        {
                            var sourceIndex = positionOfDefineFunctions[logicalEndOfFunction.LogicalDefineFunction];
                            var sourceNext = IndexOfNextRealInstruction(sourceIndex);
                            var destination = IndexOfNextRealInstruction(i);
                            var size = destination - sourceNext;
                            var parameters = logicalEndOfFunction.LogicalDefineFunction.CreateRealParameters(size);
                            logicalEndOfFunction.LogicalDefineFunction.InnerInstruction.Parameters = parameters;
                        }
                        break;
                    case LogicalBranch logicalBranch:
                        positionOfBranches.Add(logicalBranch, i);
                        sortedList.Add(i, logicalBranch.InnerInstruction);
                        break;
                    case LogicalDefineFunction defineFunction:
                        positionOfDefineFunctions.Add(defineFunction, i);
                        sortedList.Add(i, defineFunction.InnerInstruction);
                        break;
                    default:
                        sortedList.Add(i, current);
                        break;
                }
            }
            return new InstructionCollection(sortedList);
        }
    }

    internal class LogicalDestination : InstructionBase
    {
        public override InstructionType Type => throw new InvalidOperationException();
        public override void Execute(ActionContext context) => throw new InvalidOperationException();
        public string Name { get; set; }
        public LogicalBranch LogicalBranch { get; private set; }

        public LogicalDestination(LogicalBranch logicalBranch, string labelName)
        {
            Parameters = new List<Value>();
            LogicalBranch = logicalBranch;
            Name = labelName;
        }
        public override string ToString(ActionContext context) { return ToString(); }
        public override string ToString()
        {
            return "LogicalDestination";
        }
    }


    internal class LogicalEndOfFunction : InstructionBase
    {
        public override InstructionType Type => throw new InvalidOperationException();
        public override void Execute(ActionContext context) => throw new InvalidOperationException();
        public override List<Value> Parameters => throw new NotImplementedException();
        public LogicalDefineFunction LogicalDefineFunction { get; private set; }

        public LogicalEndOfFunction(LogicalDefineFunction logicalDefineFunction)
        {
            // Parameters = new List<Value>();
            LogicalDefineFunction = logicalDefineFunction;
        }

        public override string ToString(ActionContext context) { return ToString(); }
        public override string ToString()
        {
            return "LogicalEndOfFunction";
        }
    }

    internal class LogicalBranch : InstructionBase
    {
        public override InstructionType Type => InnerInstruction.Type;
        public override void Execute(ActionContext context) => throw new InvalidOperationException();
        public InstructionBase InnerInstruction { get; private set; }
        public LogicalDestination Destination { get; private set; }

        public LogicalBranch(InstructionBase instruction, string labelName)
        {
            Parameters = new List<Value>();
            InnerInstruction = instruction;
            Destination = new LogicalDestination(this, labelName);
        }
    }

    internal class LogicalDefineFunction : InstructionBase
    {
        public override InstructionType Type => InnerInstruction.Type;
        public override void Execute(ActionContext context) => throw new InvalidOperationException();
        public InstructionBase InnerInstruction { get; private set; }
        public LogicalEndOfFunction? EndOfFunction { get; private set; }

        public LogicalDefineFunction(InstructionBase instruction)
        {
            InnerInstruction = instruction;
            Parameters = InnerInstruction.Parameters.SkipLast(1).ToList();
            EndOfFunction = null;
        }

        public LogicalEndOfFunction CreateEndOfFunction()
        {
            if (EndOfFunction != null)
            {
                throw new InvalidOperationException();
            }
            EndOfFunction = new LogicalEndOfFunction(this);
            return EndOfFunction;
        }

        // Probably it would be better to update InnerFunction.Parameter each time this.Parameter changes
        public List<Value> CreateRealParameters(int functionSize)
        {
            throw new NotImplementedException();
        }
    }


    // old ones?

    internal sealed class ValueTypePattern
    {
        public readonly ValueType[] FirstTypes;
        public readonly ValueType[] RepeatingTypes;
        public readonly ValueType[] LastTypes;

        public ValueTypePattern(ValueType[] firstTypes, ValueType[] repeatingTypes, ValueType[] lastTypes)
        {
            FirstTypes = firstTypes;
            RepeatingTypes = repeatingTypes;
            LastTypes = lastTypes;
        }
    }

    internal static class InstructionUtility
    {
        public static IReadOnlyList<string> InstructionNames => InstructionNamesInner.AsReadOnly();

        private static Dictionary<Type, ValueType[]> FixedInstructionsParameterTypes;
        private static Dictionary<Type, ValueTypePattern> NonFixedInstructionsParameterPattern;
        private static Dictionary<string, Type> InstructionTypes;
        private static List<string> InstructionNamesInner;
        private static int DefaultLabelName = 0;
        private static string _getName => "location_" + (++DefaultLabelName);

        static InstructionUtility()
        {
            FixedInstructionsParameterTypes = new Dictionary<Type, ValueType[]>();
            NonFixedInstructionsParameterPattern = new Dictionary<Type, ValueTypePattern>();
            InstructionTypes = new Dictionary<string, Type>();
            InstructionNamesInner = new List<string>();

            var fixeds = RetrieveFixedSizeInstructionMetaData();
            foreach (var (name, type, valueTypes) in fixeds)
            {
                FixedInstructionsParameterTypes.Add(type, valueTypes);
                InstructionTypes.Add(name, type);
                InstructionNamesInner.Add(name);
            }

            var variables = RetrieveNonFixedSizeInstructionMetaData();
            foreach (var (name, type, pattern) in variables)
            {
                NonFixedInstructionsParameterPattern.Add(type, pattern);
                InstructionTypes.Add(name, type);
                InstructionNamesInner.Add(name);
            }

            InstructionNamesInner.Sort();
        }

        public static InstructionBase NewLogicalInstruction(Type type, List<Value> parameters,
                                                            out InstructionBase? pairedInstruction)
        {
            var instruction = NewInstruction(type, parameters);
            pairedInstruction = null;
            switch (true)
            {
                case var _ when type == typeof(BranchAlways):
                case var _ when type == typeof(BranchIfTrue):
                    var logicalBranch = new LogicalBranch(instruction, _getName);
                    pairedInstruction = logicalBranch.Destination;
                    return logicalBranch;
                case var _ when type == typeof(DefineFunction):
                case var _ when type == typeof(DefineFunction2):
                    var logicalDefineFunction = new LogicalDefineFunction(instruction);
                    pairedInstruction = logicalDefineFunction.CreateEndOfFunction();
                    return logicalDefineFunction;
            }

            return instruction;
        }

        private static InstructionBase NewInstruction(Type type, List<Value> parameters)
        {
            var instance = Activator.CreateInstance(type);
            if(instance is InstructionBase instruction)
            {
                if (!CheckParameterType(instruction, parameters))
                {
                    throw new InvalidOperationException("Parameter type mismatch");
                }

                instruction.Parameters = parameters.Select(value => DeepCopyInstructionParameters(value)).ToList();
                return instruction;
            }
            throw new InvalidCastException(type.Name);
        }

        private static Value DeepCopyInstructionParameters(Value existing)
        {
            switch (existing.Type)
            {
                case ValueType.Constant:
                    return Value.FromConstant(existing.GetIDValue());
                case ValueType.Float:
                    return Value.FromFloat((float)existing.ToReal());
                case ValueType.Integer:
                    return Value.FromInteger(existing.ToInteger());
                case ValueType.Register:
                    return Value.FromRegister(existing.GetIDValue());
                case ValueType.String:
                    return Value.FromString(existing.ToString());
                default:
                    throw new InvalidOperationException();
            }
        }

        private static bool CheckParameterType(InstructionBase instruction, List<Value> parameters)
        {
            switch (instruction)
            {
                case LogicalBranch _:
                case LogicalDestination _:
                case LogicalEndOfFunction _:
                    return parameters.Count == 0;
                case LogicalDefineFunction logicalDefineFunction:
                    return CheckParameterType(logicalDefineFunction.InnerInstruction,
                                              logicalDefineFunction.Parameters.Append(Value.FromInteger(0)).ToList());
            }

            if (NonFixedInstructionsParameterPattern.TryGetValue(instruction.GetType(), out var pattern))
            {
                var repeatingParametersCount = parameters.Count - (pattern.FirstTypes.Length + pattern.LastTypes.Length);
                if (repeatingParametersCount < 0)
                {
                    return false;
                }

                if (repeatingParametersCount % pattern.RepeatingTypes.Length != 0)
                {
                    return false;
                }

                for (var i = 0; i < repeatingParametersCount; ++i)
                {
                    if (parameters[i + pattern.FirstTypes.Length].Type != pattern.RepeatingTypes[i % pattern.RepeatingTypes.Length])
                    {
                        return false;
                    }
                }

                return true;
            }

            if (FixedInstructionsParameterTypes.TryGetValue(instruction.GetType(), out var parameterTypes))
            {
                if (parameters.Count != parameterTypes.Length)
                {
                    return false;
                }

                for (var i = 0; i < parameters.Count; ++i)
                {
                    if (parameters[i].Type != parameterTypes[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            throw new NotImplementedException(instruction.GetType().ToString());
        }

        private static (string, Type, ValueType[])[] RetrieveFixedSizeInstructionMetaData()
        {
            var none = Array.Empty<ValueType>();
            return new (string, Type, ValueType[])[]
            {
                ("ToNumber",            typeof(ToNumber),           none),
                ("NextFrame",           typeof(NextFrame),          none),
                ("Play",                typeof(Play),               none),
                ("Stop",                typeof(Stop),               none),
                ("Add",                 typeof(Add),                none),
                ("Subtract",            typeof(Subtract),           none),
                ("Multiply",            typeof(Multiply),           none),
                ("Divide",              typeof(Divide),             none),
                ("Modulo",              typeof(Modulo),             none),
                ("LogicalAnd",          typeof(LogicalAnd),         none),
                ("LogicalOr",           typeof(LogicalOr),          none),
                ("LogicalNot",          typeof(LogicalNot),         none),
                ("BitwiseAnd",          typeof(BitwiseAnd),         none),
                ("BitwiseOr",           typeof(BitwiseOr),          none),
                ("BitwiseXOr",          typeof(BitwiseXOr),         none),
                ("ShiftLeft",           typeof(ShiftLeft),          none),
                ("ShiftRight",          typeof(ShiftRight),         none),
                ("ShiftRight2",         typeof(ShiftRight2),        none),
                ("StringEquals",        typeof(StringEquals),       none),
                ("Pop",                 typeof(Pop),                none),
                ("ToInteger",           typeof(ToInteger),          none),
                ("GetVariable",         typeof(GetVariable),        none),
                ("SetVariable",         typeof(SetVariable),        none),
                ("StringConcat",        typeof(StringConcat),       none),
                ("GetProperty",         typeof(GetProperty),        none),
                ("SetProperty",         typeof(SetProperty),        none),
                ("RemoveSprite",        typeof(RemoveSprite),       none),
                ("CloneSprite",         typeof(CloneSprite),        none),
                ("Trace",               typeof(Trace),              none),
                ("Delete",              typeof(Delete),             none),
                ("Delete2",             typeof(Delete2),            none),

                ("DefineLocal",         typeof(DefineLocal),        none),
                ("DefineLocal2",        typeof(DefineLocal2),       none),
                ("CallFunction",        typeof(CallFunction),       none),
                ("Return",              typeof(Return),             none),
                ("NewObject",           typeof(NewObject),          none),
                ("NewMethod",           typeof(NewMethod),          none),
                ("InitArray",           typeof(InitArray),          none),
                ("InitObject",          typeof(InitObject),         none),

                ("TypeOf",              typeof(TypeOf),             none),
                ("Add2",                typeof(Add2),               none),
                ("LessThan2",           typeof(LessThan2),          none),
                ("Equals2",             typeof(Equals2),            none),
                ("ToString",            typeof(ToStringOpCode),           none),
                ("PushDuplicate",       typeof(PushDuplicate),      none),
                ("GetMember",           typeof(GetMember),          none),
                ("SetMember",           typeof(SetMember),          none),
                ("Increment",           typeof(Increment),          none),
                ("Decrement",           typeof(Decrement),          none),
                ("CallMethod",          typeof(CallMethod),         none),
                ("EACallMethod",        typeof(EACallMethod),       none),
                ("Enumerate2",          typeof(Enumerate2),         none),
                ("PushThis",            typeof(PushThis),           none),
                ("PushZero",            typeof(PushZero),           none),
                ("PushOne",             typeof(PushOne),            none),
                ("CallFunc",            typeof(CallFunc),           none),
                ("CallMethodPop",       typeof(CallMethodPop),      none),
                ("Greater",             typeof(Greater),            none),
                ("PushThisVar",         typeof(PushThisVar),        none),
                ("PushGlobalVar",       typeof(PushGlobalVar),      none),
                ("ZeroVar",             typeof(ZeroVar),            none),
                ("PushTrue",            typeof(PushTrue),           none),
                ("PushFalse",           typeof(PushFalse),          none),
                ("PushNull",            typeof(PushNull),           none),
                ("PushUndefined",       typeof(PushUndefined),      none),
                ("CastOp",              typeof(CastOp),             none),
                ("ImplememtsOp",        typeof(ImplementsOp),       none),
                ("GetTime",             typeof(GetTime),            none),
                // with parameters
                ("GotoFrame",           typeof(GotoFrame),          new[] { ValueType.Integer }),
                ("GetUrl",              typeof(GetUrl),             new[] { ValueType.String, ValueType.String }),
                ("SetRegister",         typeof(SetRegister),        new[] { ValueType.Register }),
                // ("ConstantPool",        typeof(ConstantPool),       new[] { (ValueType.) }),
                ("GotoLabel",           typeof(GotoLabel),          new[] { ValueType.String }),
                // ("DefineFunction2",     typeof(DefineFunction2),    new[] { (ValueType) }),
                // ("PushData",            typeof(PushData),           new[] { () }),
                ("BranchAlways",        typeof(BranchAlways),       new[] { ValueType.Integer }),
                ("GetUrl2",             typeof(GetUrl2),            none),
                // ("DefineFunction",      typeof(DefineFunction),     new[] { () }),
                ("BranchIfTrue",        typeof(BranchIfTrue),       new[] { ValueType.Integer }),
                ("GotoFrame2",          typeof(GotoFrame2),         new[] { ValueType.Integer }),
                ("PushString",          typeof(PushString),         new[] { ValueType.String }),
                ("PushConstantByte",    typeof(PushConstantByte),   new[] { ValueType.Constant }),
                ("GetStringVar",        typeof(GetStringVar),       new[] { ValueType.String }),
                ("SetStringVar",        typeof(SetStringVar),       new[] { ValueType.String }),
                ("GetStringMember",     typeof(GetStringMember),    new[] { ValueType.String }),
                ("SetStringMember",     typeof(SetStringMember),    new[] { ValueType.String }),
                ("PushValueOfVar",      typeof(PushValueOfVar),     new[] { ValueType.Constant }),
                ("GetNamedMember",      typeof(GetNamedMember),     new[] { ValueType.Constant }),
                ("CallNamedFuncPop",    typeof(CallNamedFuncPop),   new[] { ValueType.Constant }),
                ("CallNamedFunc",       typeof(CallNamedFunc),      new[] { ValueType.Constant }),
                ("CallNamedMethodPop",  typeof(CallNamedMethodPop), new[] { ValueType.Constant }),
                ("PushFloat",           typeof(PushFloat),          new[] { ValueType.Float }),
                ("PushByte",            typeof(PushByte),           new[] { ValueType.Integer }),
                ("PushShort",           typeof(PushShort),          new[] { ValueType.Integer }),
                ("PushLong",            typeof(PushLong),           new[] { ValueType.Integer }), // TODO need reconstruction
                ("End",                 typeof(End),                none),
                ("CallNamedMethod",     typeof(CallNamedMethod),    new[] { ValueType.Constant }),
                ("PushRegister",        typeof(PushRegister),       new[] { ValueType.Register }),
                ("PushConstantWord",    typeof(PushConstantWord),   new[] { ValueType.Constant }),
                ("CallFunctionPop",     typeof(CallFunctionPop),    none),
                ("StrictEquals",        typeof(StrictEquals),       none),
                ("Extends",             typeof(Extends),            none),
                ("InstanceOf",          typeof(InstanceOf),         none),
            };
        }

        private static (string, Type, ValueTypePattern)[] RetrieveNonFixedSizeInstructionMetaData()
        {
            var none = Array.Empty<ValueType>();

            var defineFunctionPattern = new ValueTypePattern(
                new[] { ValueType.String, ValueType.Integer },
                new[] { ValueType.String },
                new[] { ValueType.Integer }
            );

            var defineFunction2Pattern = new ValueTypePattern(
                new[] { ValueType.String, ValueType.Integer, ValueType.Integer, ValueType.Integer },
                new[] { ValueType.Integer, ValueType.String },
                new[] { ValueType.Integer }
            );

            return new[]
            {
                ("DefineFunction", typeof(DefineFunction), defineFunctionPattern),
                ("DefineFunction2", typeof(DefineFunction2), defineFunction2Pattern),
                ("ConstantPool", typeof(ConstantPool), new ValueTypePattern(none, new[] { ValueType.Constant }, none)),
                ("PushData", typeof(PushData), new ValueTypePattern(none, new[] { ValueType.Constant }, none)),
            };
        }
    }

    internal static class UtilityMethodsExtensions
    {
        public static string InstructionName(this InstructionBase instruction)
        {
            switch (instruction)
            {
                case LogicalDestination destination:
                    return "BranchDestination";
                case LogicalEndOfFunction endOfFunction:
                    return "EndOfFunction";
                default:
                    return instruction.Type.ToString();
            }
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TKey : notnull where TValue : new()
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, new TValue());
            }

            return dict[key];
        }
    }
}
