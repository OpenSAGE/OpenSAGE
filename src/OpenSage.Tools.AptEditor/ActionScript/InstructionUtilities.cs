using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.FileFormats.Apt;
using OpenSage.FileFormats.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using ValueType = OpenSage.Gui.Apt.ActionScript.ValueType;
using Value = OpenSage.Gui.Apt.ActionScript.Value;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    internal enum TagType
    {
        None, 
        Label,
        GotoLabel,
        DefineFunction, 
    }

    internal class LogicalTaggedInstruction : InstructionBase
    {
        public string Tag = "";
        public object AdditionalData;
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

        public LogicalFunctionContext(InstructionBase instruction, InstructionCollection insts, List<ConstantEntry> consts, int index_offset = 0, List<Value> constpool = null, Dictionary<int, string> regnames = null) : base(instruction, "", TagType.DefineFunction)
        {   if (instruction is DefineFunction2 df2)
            {
                var flags = (FunctionPreloadFlags) df2.Parameters[3].ToInteger();
                regnames = Preload(flags);
            }
            Instructions = new LogicalInstructions(insts, consts, index_offset, constpool, regnames);
            
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

    internal class StructurizedInstructionBlock
    {
        public InstructionBlock StartBlock;
        public InstructionBlock? EndBlock;
        public StructurizedInstructionBlock? Next;
        public List<StructurizedInstructionBlock> Loops = new();
        public List<StructurizedInstructionBlock> Cases = new();
        public int BlockType = 0;

        public static StructurizedInstructionBlock Parse(InstructionBlock root)
        {
            StructurizedInstructionBlock? currentSBlock = new() { StartBlock = root };

            // parse loop
            currentSBlock.ParseLoop();

            // parse case
            currentSBlock.ParseCase();

            return currentSBlock;
        }

        public void ParseCase()
        {
            var currentSBlock = new StructurizedInstructionBlock() { StartBlock = StartBlock, EndBlock = EndBlock, Next = Next, BlockType = BlockType };
            var b = StartBlock!;

            while (b != null && (EndBlock == null || b.Hierarchy <= EndBlock.Hierarchy))
            {
                // identify if structures
                if (b.BranchCondition != null &&
                    (b.BranchCondition!.Type == InstructionType.BranchIfTrue ||
                     b.BranchCondition!.Type == InstructionType.EA_BranchIfFalse))
                {
                    var startBlock = b;
                    var endBlock = b.NextBlockCondition!;
                    // find the end block of if structure
                    while (b.Hierarchy < endBlock.Hierarchy)
                    {
                        if (b.BranchCondition != null &&
                            (b.BranchCondition!.Type == InstructionType.BranchIfTrue ||
                             b.BranchCondition!.Type == InstructionType.EA_BranchIfFalse))
                        {
                            var bn = b.NextBlockCondition!;
                            if (EndBlock != null && bn.Hierarchy <= EndBlock.Hierarchy && bn.Hierarchy > endBlock.Hierarchy)
                            {
                                endBlock = bn;
                            } // else inner if structure
                        }
                        b = b.NextBlockDefault;
                    }
                    b = b.PreviousBlock!;

                    // split
                    currentSBlock!.EndBlock = startBlock.PreviousBlock;
                    var sb2 = endBlock.NextBlockDefault != null ?
                        new StructurizedInstructionBlock() { StartBlock = endBlock.NextBlockDefault! } :
                        null;

                    var sb = new StructurizedInstructionBlock() { StartBlock = startBlock, EndBlock = endBlock, Next = sb2, BlockType = 1 };
                    currentSBlock.Next = sb;
                    currentSBlock = sb2;
                    Cases.Add(sb);
                    // cascaded parsing
                    // sb.ParseCase();
                }
                b = b.NextBlockDefault;
            }

            if (currentSBlock != null)
                currentSBlock.EndBlock = EndBlock;
        }

        public void ParseLoop() {

            var currentSBlock = new StructurizedInstructionBlock() { StartBlock = StartBlock, EndBlock = EndBlock, Next = Next, BlockType = BlockType };
            
            Dictionary<int, InstructionBlock> blocks = new();
            var b = StartBlock;
            while (b != null && (EndBlock == null || b.Hierarchy <= EndBlock.Hierarchy))
            {
                // build dictionary
                blocks[b.Hierarchy] = b;

                // identify for/while structures
                if (b.BranchCondition != null &&
                    b.BranchCondition.Type == InstructionType.BranchAlways &&
                    b.NextBlockCondition!.Hierarchy != StartBlock.Hierarchy && // be not the block's loop itself
                    b.NextBlockCondition!.Hierarchy < b.Hierarchy) // could be a loop
                {
                    // mark range
                    int startBlock = b.NextBlockCondition.Hierarchy;
                    int endBlock = b.Hierarchy;
                    int outerBlock = -1;
                    for (int i = startBlock; i <= endBlock; ++i)
                    {
                        if (outerBlock == -1 ||
                            (blocks[i].BranchCondition != null &&
                             (blocks[i].BranchCondition!.Type == InstructionType.BranchIfTrue ||
                              blocks[i].BranchCondition!.Type == InstructionType.EA_BranchIfFalse ||
                              blocks[i].BranchCondition!.Type == InstructionType.BranchAlways) &&
                             blocks[i].NextBlockCondition!.Hierarchy > endBlock))
                        {
                            if (outerBlock >= 0 && blocks[i].NextBlockCondition!.Hierarchy != outerBlock)
                                throw new InvalidOperationException();
                            else if (blocks[i].NextBlockCondition!.Hierarchy > endBlock)
                            {
                                outerBlock = blocks[i].NextBlockCondition!.Hierarchy;
                                blocks[outerBlock] = blocks[i].NextBlockCondition!;
                            }
                        }
                    }
                    // split blocks
                    // TODO theoretically outerBlock should be the same as endBlock + 1
                    currentSBlock!.EndBlock = blocks[startBlock - 1];
                    var sb2 = blocks[endBlock].NextBlockDefault != null ?
                        new StructurizedInstructionBlock() { StartBlock = blocks[endBlock].NextBlockDefault! } :
                        null;

                    var sb = new StructurizedInstructionBlock() { StartBlock = blocks[startBlock], EndBlock = blocks[endBlock], Next = sb2, BlockType = 2 };
                    currentSBlock.Next = sb;
                    currentSBlock = sb2;
                    Loops.Add(sb);
                    // cascaded parsing
                    sb.ParseLoop();

                    // TODO judgement block parsing
                }

                b = b.NextBlockDefault;
            }

            if (currentSBlock != null)
                currentSBlock.EndBlock = EndBlock;
        }
    }

    internal class CodeTree
    {
        public List<Node> NodeList;

        // The decompilation depends on the assumption that
        // ConstantPool() is only executed at most once in one segment.
        public List<Value> Constants;
        public Dictionary<int, string> RegNames;

        public CodeTree(IEnumerable<Value>? constants = null, IDictionary<int, string>? regNames = null)
        {
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
            ret = null;
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

            if (ret == null) ret = v.ToString();
            return ans;
        }
        public NodeExpression FindFirstNodeExpression(bool deleteIfPossible = true)
        {
            var ind = NodeList.FindLastIndex(n => n is NodeExpression);
            NodeExpression val = null;
            if (ind == -1)
                val = null;
            else
            {
                var node = (NodeExpression) NodeList[ind];
                var able_to_delete = true; // TODO some special nodes shouldn't be deleted like Enumerate
                if (able_to_delete && deleteIfPossible)
                    NodeList.RemoveAt(ind);
                val = node;
            }
            return val;
        }
        public NodeArray ReadArray(bool readPair = false, bool ensureCount = true)
        {
            var flag = FindFirstNodeExpression().GetValue(this, out var countVal);
            if (!flag)
                return null;
            NodeArray ans = new();
            var count = countVal.ToInteger();
            if (readPair) count *= 2;
            for (int i = 0; i < count; ++i)
            {
                var exp = FindFirstNodeExpression();
                if (exp != null || (exp == null && ensureCount))
                    ans.Expressions.Add(exp);
            }
            return ans;
        }
        
        public abstract class Node
        {
            public List<NodeExpression> Expressions = new();
            public InstructionBase Instruction;

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
                        {
                            var flag2 = tree.ToStringInCodingForm(val, out vals[i]);
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
                                (val.Type == ValueType.String || val.Type == ValueType.Constant))
                                vals[i] = $"\"{vals[i]}\"";
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
                string ret = null;
                try
                {
                    ret = Instruction.ToString(vals);
                }
                catch
                {
                    ret = Instruction.ToString2(vals);
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
                var spec_proc_flag = true;
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
                        Expressions.Add(new NodeValue(instruction.Parameters[0]));
                        Expressions.Add(tree.ReadArray());
                        break;
                    case InstructionType.EA_CallNamedMethodPop:
                        Expressions.Add(new NodeValue(instruction.Parameters[0]));
                        Expressions.Add(tree.FindFirstNodeExpression());
                        Expressions.Add(tree.ReadArray());
                        break;
                    case InstructionType.EA_CallNamedMethod:
                        Expressions.Add(new NodeValue(instruction.Parameters[0])); // TODO resolve constant
                        Expressions.Add(tree.FindFirstNodeExpression());
                        Expressions.Add(tree.ReadArray());
                        Expressions.Add(tree.FindFirstNodeExpression());
                        break;
                    case InstructionType.InitObject:
                        Expressions.Add(tree.ReadArray(true));
                        break;

                    // type 3: constant resolve needed
                    case InstructionType.EA_GetNamedMember:
                        Expressions.Add(new NodeValue(instruction.Parameters[0])); // TODO resolve constant
                        Expressions.Add(tree.FindFirstNodeExpression());
                        break;
                    case InstructionType.EA_PushValueOfVar:
                        Expressions.Add(new NodeValue(instruction.Parameters[0])); // TODO resolve constant; incode form?
                        break;
                    case InstructionType.EA_PushGlobalVar:
                    case InstructionType.EA_PushThisVar:
                    case InstructionType.EA_PushGlobal:
                    case InstructionType.EA_PushThis:
                        break; // nothing needed
                    case InstructionType.EA_PushConstantByte:
                    case InstructionType.EA_PushConstantWord:
                        Expressions.Add(new NodeValue(instruction.Parameters[0], true)); // TODO resolve constant
                        break;
                    case InstructionType.EA_PushRegister:
                        Expressions.Add(new NodeValue(instruction.Parameters[0], true)); // TODO resolve register
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
                        spec_proc_flag = false;
                        break;
                }
                if ((!spec_proc_flag) && instruction is InstructionMonoPush inst)
                {
                    // TODO string output
                    for (int i = 0; i < inst.StackPop; ++i)
                        Expressions.Add(tree.FindFirstNodeExpression());
                }
                else if (!spec_proc_flag) // not implemented instructions
                {
                    throw new NotImplementedException(instruction.Type.ToString());
                }

                // filter string

            }
        }
        public class NodeExpression : Node
        {
            public virtual bool GetValue(CodeTree tree, out Value ret)
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
                        if (val.Type == ValueType.Constant || val.Type == ValueType.Register)
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
                string ret = null;
                if (GetValue(tree, out var val_))
                {
                    tree.ToStringInCodingForm(val_, out ret);
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
            public NodeValue (Value v, bool iss = false)
            {
                Value = v;
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
            public override bool GetValue(CodeTree tree, out Value ret)
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
                        tree.ToStringInCodingForm(val, out vals[i]);
                        if (val.Type == ValueType.String || val.Type == ValueType.Constant)
                            vals[i] = $"\"{vals[i]}\"";
            }
                }
                return $"{string.Join(", ", vals)}";
            }
        }
        public class NodeStatement: Node
        {
            public override string GetCode(CodeTree tree)
            {
                return base.GetCode(tree);
            }
        }
        public class NodeControl: NodeStatement
        {
            public override string GetCode(CodeTree tree)
            {
                throw new NotImplementedException();
            }
        }

        public static CodeTree DecompileToTree(InstructionBlock root, IEnumerable<Value> constants, IDictionary<int, string> regNames)
        {
            CodeTree Tree = new(constants, regNames);
            foreach (var kvp in root.Items)
            {
                var inst = kvp.Value;
                CodeTree.Node node = inst.IsStatement ? new CodeTree.NodeStatement() : new CodeTree.NodeExpression();
                node.Instruction = inst;
                node.GetExpressions(Tree);
                Tree.NodeList.Add(node);
            }
            return Tree;
        }

        public string GetCode()
        {
            var ans = "";
            foreach (var node in NodeList)
                if (node == null)
                    ans += "[[null node]];\n";
                else
                    ans += node.GetCode(this) + ";\n";
            return ans;
        }
    }

    internal class LogicalInstructions
    {
        public SortedDictionary<int, InstructionBase> Items { get; set; }
        public InstructionBlock BaseBlock { get; set; }
        public InstructionCollection Insts { get; private set; }
        public int IndexOffset;
        public List<Value> Constants;
        public Dictionary<int, string> RegNames;

        // public 
        public LogicalInstructions(InstructionStorage insts) : this(InstructionCollection.Parse(insts)) { }
        public LogicalInstructions(InstructionCollection insts, List<ConstantEntry> consts = null, int index_offset = 0, List<Value> constpool = null, Dictionary<int, string> regnames = null)
        {
            Insts = insts;
            IndexOffset = index_offset;
            var stream = new InstructionStream(insts.AddEnd());

            Constants = constpool;
            RegNames = regnames;

            Items = new SortedDictionary<int, InstructionBase>();
            var branch_dict = new Dictionary<string, int>(); // position: label
            var label_number = 0;
            while (!stream.IsFinished())
            {
                var index = stream.Index;
                var instruction = stream.GetInstruction();

                if (instruction is ConstantPool cp && consts != null)
                {
                    ActionContext ac = new ActionContext(null, null, null, 4) { GlobalConstantPool = consts };
                    ac.ReformConstantPool(cp.Parameters);
                    Constants = ac.Constants;
                }
                if (instruction is DefineFunction || instruction is DefineFunction2)
                {
                    var nParams = instruction.Parameters[1].ToInteger();
                    var size =
                        instruction is DefineFunction ?
                        instruction.Parameters[2 + nParams].ToInteger() :
                        instruction.Parameters[4 + nParams * 2].ToInteger();
                    
                    var codes = stream.GetInstructions(size, true, true);
                    instruction = new LogicalFunctionContext(instruction, codes, consts, IndexOffset + index + 1, Constants, RegNames);
                    ((LogicalFunctionContext) instruction).Instructions.Constants = Constants;
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
            var block_branch_dict = new Dictionary<string, InstructionBlock>();
            var currentBlock = BaseBlock;
            // first iter: maintain BranchCondition & NextBlockDefault
            foreach (var kvp in Items)
            {
                var new_block_needed = false;
                Dictionary<int, InstructionBase> new_block_items = null;
                var pos = kvp.Key;
                var inst = kvp.Value;
                if ((inst is LogicalTaggedInstruction lti && lti.TagType == TagType.Label) || inst is Enumerate2)// TODO need some instances to determine how to deal with Enumerate2
                {
                    var new_block = new InstructionBlock(currentBlock);
                    new_block.Items[pos] = inst;
                    currentBlock = new_block;
                    if (inst is LogicalTaggedInstruction lti2) {

                        block_branch_dict[(string) lti2.AdditionalData] = new_block;
                        var instTemp = lti2;
                        while (instTemp.InnerAction is LogicalTaggedInstruction lti3)
                        {
                            instTemp = lti3;
                            block_branch_dict[(string) lti3.AdditionalData] = new_block;
                        }
                        
                        if (lti2.FinalTagType == TagType.GotoLabel)
                        {
                            var new_new_block = new InstructionBlock(new_block);
                            new_block.BranchCondition = lti2.FinalTaggedInnerAction;
                            currentBlock = new_new_block;
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
            foreach (var kvp in block_branch_dict)
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
                    if (block_branch_dict.TryGetValue((string) currentBlock.BranchCondition.AdditionalData, out var blk))
                        currentBlock.NextBlockCondition = blk;
                    else
                        currentBlock.NextBlockCondition = new InstructionBlock() { Label = currentBlock.BranchCondition.Tag };
                currentBlock = currentBlock.NextBlockDefault;
            }

            // decompilation
            // will be eventually removed
            if (consts == null) return;

            // structurize
            

            currentBlock = BaseBlock;
            while (currentBlock != null)
            {
                foreach (var a in currentBlock.Items)
                    Console.WriteLine(a);
                var tree = CodeTree.DecompileToTree(currentBlock, Constants, RegNames);
                Console.WriteLine(tree.GetCode());
                currentBlock = currentBlock.NextBlockDefault;
            }

            var p = StructurizedInstructionBlock.Parse(BaseBlock);
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
        public static IReadOnlyList<string> InstructionNames => _instructionNames;

        private static Dictionary<Type, ValueType[]> _fixedInstructionsParameterTypes;
        private static Dictionary<Type, ValueTypePattern> _nonFixedInstructionsParameterPattern;
        private static Dictionary<string, Type> _instructionTypes;
        private static List<string> _instructionNames;
        private static int _defaultLabelName = 0;
        private static string _getName => "location_" + (++_defaultLabelName);

        static InstructionUtility()
        {
            _fixedInstructionsParameterTypes = new Dictionary<Type, ValueType[]>();
            _nonFixedInstructionsParameterPattern = new Dictionary<Type, ValueTypePattern>();
            _instructionTypes = new Dictionary<string, Type>();
            _instructionNames = new List<string>();

            var fixeds = RetrieveFixedSizeInstructionMetaData();
            foreach (var (name, type, valueTypes) in fixeds)
            {
                _fixedInstructionsParameterTypes.Add(type, valueTypes);
                _instructionTypes.Add(name, type);
                _instructionNames.Add(name);
            }

            var variables = RetrieveNonFixedSizeInstructionMetaData();
            foreach (var (name, type, pattern) in variables)
            {
                _nonFixedInstructionsParameterPattern.Add(type, pattern);
                _instructionTypes.Add(name, type);
                _instructionNames.Add(name);
            }

            _instructionNames.Sort();
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

            if (_nonFixedInstructionsParameterPattern.TryGetValue(instruction.GetType(), out var pattern))
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

            if (_fixedInstructionsParameterTypes.TryGetValue(instruction.GetType(), out var parameterTypes))
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
