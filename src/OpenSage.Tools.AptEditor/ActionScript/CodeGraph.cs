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

namespace OpenSage.Tools.AptEditor.ActionScript
{
    public enum TagType
    {
        None, 
        Label,
        GotoLabel,
        DefineFunction, 
    }


    public class LogicalTaggedInstruction : InstructionBase
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

    public class LogicalFunctionContext : LogicalTaggedInstruction
    {
        public InstructionGraph Instructions { get; private set; }

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
            Instructions = new InstructionGraph(insts, constSource, indexOffset, constPool, regNames);
            
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

    public class InstructionGraph
    {
        public SortedDictionary<int, InstructionBase> Items { get; set; }
        public InstructionBlock BaseBlock { get; set; }
        public InstructionCollection Insts { get; private set; }
        public int IndexOffset;
        public List<Value>? ConstPool;
        public Dictionary<int, string>? RegNames;

        // public 
        public InstructionGraph(InstructionStorage insts) : this(InstructionCollection.Parse(insts)) { }
        public InstructionGraph(InstructionCollection insts,
            List<ConstantEntry>? constSource = null,
            int indexOffset = 0,
            List<Value>? constPool = null,
            Dictionary<int, string>? regNames = null)
        {
            // init vars

            Insts = insts;
            IndexOffset = indexOffset;
            var stream = new InstructionStream(insts.AddEnd());

            ConstPool = constPool;
            RegNames = regNames;

            // sub graphs & mark items

            Items = new SortedDictionary<int, InstructionBase>();
            var branch_dict = new Dictionary<string, int>(); // position: label
            var label_number = 0;
            while (!stream.IsFinished())
            {
                var index = stream.Index;
                var instruction = stream.GetInstruction();

                // create the constant pool
                if (instruction is ConstantPool cp && constSource != null)
                {
                    ActionContext ac = new ActionContext(null, null, null, 4) { GlobalConstantPool = constSource };
                    ac.ReformConstantPool(cp.Parameters);
                    ConstPool = ac.Constants;
                }

                // create the sub graph
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


                // create labels
                else if (instruction is BranchIfTrue || instruction is BranchAlways)
                {
                    var dest = instruction.Parameters[0].ToInteger();
                    ++label_number;
                    var destIndex = stream.GetBranchDestination(dest, index + 1);
                    var tag = destIndex == -1 ? $"[[#{index + 1}+({dest})]]" : $"label_#{destIndex}";
                    if (destIndex == -1)
                    {
                        throw new InvalidOperationException();
                        branch_dict[tag] = index;
                    }
                    else
                        branch_dict[tag] = destIndex;
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
                    if (inst is LogicalTaggedInstruction lti2)
                    {

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

    public class InstructionBlock
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

    public class StructurizedBlockChain
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

}
