using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenAS2.Base;
using OpenAS2.Runtime;

namespace OpenAS2.Base
{
    using RawInstructionStorage = SortedList<int, RawInstruction>;

    public enum TagType
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

    public class LogicalTaggedInstruction : RawInstruction
    {
        public string Tag = "";
        public string? Label;
        public readonly TagType TagType;
        public TagType FinalTagType { get { return Inner is LogicalTaggedInstruction l ? l.FinalTagType : TagType; } }
        public RawInstruction Inner { get; protected set; }
        public RawInstruction MostInner { get { return Inner is LogicalTaggedInstruction l ? l.MostInner : Inner; } }
        public LogicalTaggedInstruction MostTaggedInner { get { return Inner is LogicalTaggedInstruction l ? l.MostTaggedInner : this; } }

        public override InstructionType Type => Inner.Type;
        public override IList<RawValue> Parameters => Inner.Parameters;

        public LogicalTaggedInstruction(RawInstruction instruction, string tag = "", TagType type = TagType.None): base(instruction.Type, null, false)
        {
            Inner = instruction;
            Tag = tag;
            TagType = type;
        }

        public IEnumerable<string> GetLabels()
        {
            var ans = new List<string>();
            for (RawInstruction it = this; it != null && it is LogicalTaggedInstruction itl; it = itl.Inner)
                if (itl.TagType == TagType.Label && !string.IsNullOrWhiteSpace(itl.Label))
                    ans.Add(itl.Label);
            return ans;
        }

        public override string ToString()
        {
            if (Tag == null || Tag == "") return Inner.ToString() ?? String.Empty;

            return $"// Tagged: {Tag}\n{Inner.ToString()}";
        }
    }

    public class LogicalFunctionContext : LogicalTaggedInstruction
    {
        public InstructionGraph Instructions { get; private set; }

        public LogicalFunctionContext(RawInstruction instruction,
            RawInstructionStorage? insts,
            int indexOffset = 0,
            RawInstruction? constPool = null,
            Dictionary<int, string>? regNames = null) : base(instruction, "", TagType.DefineFunction)
        {
            if (instruction.Type == InstructionType.DefineFunction2)
            {
                var flags = (FunctionPreloadFlags) instruction.Parameters[3].Integer;
                regNames = Preload(flags, instruction.Parameters);
            }
            Instructions = insts != null ? new (insts!, indexOffset, constPool, regNames) : new(new()) ;
            
        }

        public static LogicalFunctionContext OptimizeGraph(LogicalFunctionContext fc)
        {
            var ans = new LogicalFunctionContext(fc.Inner, null);
            ans.Instructions = InstructionGraph.OptimizeGraph(fc.Instructions);
            return ans;
        }

        public static Dictionary<int, string> Preload(FunctionPreloadFlags flags, IEnumerable<RawValue> parameters)
        {
            int reg = 1;
            var _registers = new Dictionary<int, string>();
            if (flags.HasFlag(FunctionPreloadFlags.PreloadThis))
            {
                _registers[reg] = "this";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadArguments)) // TODO sanity check
            {
                _registers[reg] = "arguments";
                ++reg;
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
            int nrArgs = parameters.ElementAt(1).Integer;
            for (int i = 0; i < nrArgs; ++i)
            {
                var pname = parameters.ElementAt(4 + i * 2 + 1).String;
                var preg = parameters.ElementAt(4 + i * 2).Integer;
                _registers[preg] = pname;
            }
            return _registers;
        }
        
    }

    public class InstructionBlock
    {
        public SortedDictionary<int, RawInstruction> Items { get; set; }

        public LogicalTaggedInstruction? BranchCondition { get; set; }
        public InstructionBlock? NextBlockCondition { get; set; }
        public InstructionBlock? NextBlockDefault { get; private set; } // set automatically in most cases
        public readonly List<string> Labels;

        // set automatically

        public readonly InstructionBlock? PreviousBlock;
        public readonly int Hierarchy;

        // judgements

        public bool IsFirstBlock => PreviousBlock == null;
        public bool IsLastBlock => NextBlockDefault == null;

        public bool HasConditionalBranch => BranchCondition != null && BranchCondition.IsConditionalBranch;
        public bool HasConstantBranch => BranchCondition != null && BranchCondition.IsBranchAlways;
        public bool HasBranch => BranchCondition != null && BranchCondition.IsBranch;

        public InstructionBlock(InstructionBlock? prev = null, int hierarchy = 0)
        {
            Items = new();
            Labels = new();
            PreviousBlock = prev;
            if (prev != null)
            {
                prev.NextBlockDefault = this;
                Hierarchy = prev.Hierarchy + 1;
            }
            else
                Hierarchy = hierarchy > 0 ? hierarchy : 0;
        }

        private InstructionBlock(int h) { Hierarchy = h; Items = new(); Labels = new(); }
        public static InstructionBlock H(int h) { return new(h); }

        public override string ToString()
        {
            return $"IB({Items.Count} Instructions, Hierarchy={Hierarchy})";
        }

        public void CopyFrom(InstructionBlock b, bool copyNext)
        {
            Items = b.Items;
            Labels.AddRange(b.Labels);
            BranchCondition = b.BranchCondition;
            NextBlockDefault = copyNext ? b.NextBlockDefault : null;
            NextBlockCondition = b.NextBlockCondition;
        }

    }

    public class InstructionGraph
    {
        // public SortedDictionary<int, RawInstruction> Items { get; set; }
        public InstructionBlock BaseBlock { get; set; }
        public RawInstructionStorage Insts { get; private set; }
        public int IndexOffset;
        public RawInstruction? ConstPool;
        public Dictionary<int, string>? RegNames;

        // public

        // used to copy graphs
        public InstructionGraph(InstructionGraph g, InstructionBlock b)
        {
            BaseBlock = b;
            Insts = g.Insts;
            IndexOffset = g.IndexOffset;
            ConstPool = g.ConstPool;
            RegNames = g.RegNames;
        }

        public InstructionGraph(RawInstructionStorage insts,
            int indexOffset = 0,
            RawInstruction? constPool = null,
            Dictionary<int, string>? regNames = null)
        {
            // init vars

            Insts = insts;
            IndexOffset = indexOffset;
            var stream = new InstructionStream(insts, createEnd: true);

            ConstPool = constPool;
            RegNames = regNames;

            // sub graphs & mark items

            SortedDictionary<int, RawInstruction> tempItems = new();
            var mapBranchTagToIndex = new Dictionary<string, int>(); // position: label
            var labelCount = 0;
            while (!stream.IsFinished())
            {
                var index = stream.Index;
                var instruction = stream.GetCurrentAndToNext();
                var instType = instruction.Type;

                // create the constant pool
                // TODO what if multiple pools are created (especially not in the main path)?
                if (instType == InstructionType.ConstantPool)
                {
                    ConstPool = instruction;
                }

                // create the sub graph
                if (instType == InstructionType.DefineFunction || instType == InstructionType.DefineFunction2)
                {
                    var nParams = instruction.Parameters[1].Integer;
                    var size =
                        instType == InstructionType.DefineFunction ?
                        instruction.Parameters[2 + nParams].Integer :
                        instruction.Parameters[4 + nParams * 2].Integer;

                    var codes = stream.GetInstructions(size, true, true);
                    instruction = new LogicalFunctionContext(instruction, codes, IndexOffset + index + 1, ConstPool, RegNames);
                    ((LogicalFunctionContext) instruction).Instructions.ConstPool = ConstPool;
                }


                // create labels
                // TODO what if BranchIfFalse
                else if (instType == InstructionType.BranchIfTrue || instType == InstructionType.BranchAlways)
                {
                    var dest = instruction.Parameters[0].Integer;
                    ++labelCount;
                    var destIndex = stream.GetBranchDestination(dest, index + 1);
                    var tag = destIndex == -1 ? $"[[@{IndexOffset}#{index + 1}+({dest})]]" : $"label_#{destIndex + IndexOffset}";
                    if (destIndex == -1)
                    {
                        throw new InvalidOperationException();
                        // branch_dict[tag] = index + 1;
                    }
                    else
                        mapBranchTagToIndex[tag] = destIndex;
                    instruction = new LogicalTaggedInstruction(instruction, "goto " + tag, TagType.GotoLabel) { Label = tag };
                }

                tempItems.Add(index, instruction);
            }
            // branch destination tag
            foreach (var kvp in mapBranchTagToIndex)
            {
                var instOrig = tempItems[kvp.Value];
                if (instOrig is LogicalTaggedInstruction inst && inst.TagType == TagType.Label)
                    throw new InvalidOperationException();
                tempItems[kvp.Value] = new LogicalTaggedInstruction(instOrig, kvp.Key, TagType.Label) { Label = kvp.Key };
            }

            // block division
            BaseBlock = new InstructionBlock();
            var mapLabelToBlock = new Dictionary<string, InstructionBlock>();
            var currentBlock = BaseBlock;

            // first iter: maintain BranchCondition & NextBlockDefault
            foreach (var kvp in tempItems)
            {
                var pos = kvp.Key;
                var inst = kvp.Value;
                if (inst is LogicalTaggedInstruction tinst && tinst.TagType == TagType.Label)
                {
                    var newBlock = new InstructionBlock(currentBlock);
                    newBlock.Items[pos] = inst;
                    currentBlock = newBlock;

                    // mark all labels affiliated to this block
                    mapLabelToBlock[tinst.Label!] = newBlock;
                    var instTemp = tinst;
                    while (instTemp.Inner is LogicalTaggedInstruction tinst2)
                    {
                        instTemp = tinst2;
                        if (tinst2.TagType == TagType.Label)
                            mapLabelToBlock[tinst2.Label!] = newBlock;
                    }

                    if (tinst.FinalTagType == TagType.GotoLabel)
                    {
                        var newerBlock = new InstructionBlock(newBlock);
                        newBlock.BranchCondition = tinst.MostTaggedInner;
                        currentBlock = newerBlock;
                    }

                }
                else if (inst is LogicalTaggedInstruction lti3 && lti3.TagType == TagType.GotoLabel)
                {
                    var new_block = new InstructionBlock(currentBlock);
                    currentBlock.BranchCondition = lti3.MostTaggedInner;
                    currentBlock.Items[pos] = inst;
                    currentBlock = new_block;
                }
                else if (inst.IsEnumerate)
                {
                    // just do nothing?
                    currentBlock.Items[pos] = inst;
                }
                else
                {
                    currentBlock.Items[pos] = inst;
                }
            }

            // second iter: maintain Labels
            foreach (var kvp in mapLabelToBlock)
            {
                var lbl = kvp.Key;
                var blk = kvp.Value;
                blk.Labels.Add(lbl);
            }

            // third iter: maintain NextBlockCondition
            currentBlock = BaseBlock;
            while (currentBlock != null)
            {
                if (currentBlock.BranchCondition != null)
                    if (mapLabelToBlock.TryGetValue(currentBlock.BranchCondition.Label!, out var blk))
                        currentBlock.NextBlockCondition = blk;
                    else
                        throw new InvalidOperationException();
                currentBlock = currentBlock.NextBlockDefault;
            }

        }

        /// <summary>
        /// replace all blocks with no meaningful operations inside it
        /// strip all labels and tags
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static InstructionGraph OptimizeGraph(InstructionGraph g, bool optimizeSubGraphs = false, bool stripTags = false)
        {
            Dictionary<InstructionBlock, bool> empty = new();
            Dictionary<InstructionBlock, (InstructionBlock?, InstructionBlock?)> jump = new();
            Dictionary<InstructionBlock, InstructionBlock> map = new();

            // init dicts
            for (var b = g.BaseBlock; b != null; b = b.NextBlockDefault)
            {
                // judge empty
                var flagEmpty = true;
                foreach (var (pos, inst) in b.Items)
                {
                    var it = inst.Type;
                    flagEmpty = flagEmpty && (it == InstructionType.End || it == InstructionType.Padding || (
                            it == InstructionType.BranchAlways && b.NextBlockCondition == b.NextBlockDefault
                        ));
                }
                empty[b] = flagEmpty;

                // store jump
                jump[b] = (b.NextBlockDefault, b.NextBlockCondition);

                // init map dict
                map[b] = b;
            }

            // fix map array
            foreach (var (b, e) in empty)
            {
                if (!e) continue;
                var bo = b;
                while (bo.NextBlockDefault != null && empty[bo])
                    bo = bo.NextBlockDefault;
                map[b] = bo;
            }

            // create main block chain
            InstructionBlock? prev = null;
            Dictionary<InstructionBlock, InstructionBlock> newBlocks = new();
            for (var b = g.BaseBlock; b != null; b = b.NextBlockDefault)
            {
                var bm = map[b];
                if (newBlocks.TryGetValue(bm, out var nbm))
                {
                    prev = nbm;
                    nbm.Labels.AddRange(b.Labels);
                    continue;
                }
                var nb = new InstructionBlock(prev);
                nb.BranchCondition = bm.BranchCondition;
                nb.Labels.AddRange(b.Labels);
                foreach (var (pos, inst) in bm.Items)
                {
                    if (inst is LogicalTaggedInstruction lti)
                        if (lti is LogicalFunctionContext lfc)
                            if (optimizeSubGraphs)
                                nb.Items.Add(pos, LogicalFunctionContext.OptimizeGraph(lfc));
                            else
                                nb.Items.Add(pos, lfc); // TODO strip
                        else if (!(inst.Type == InstructionType.End || inst.Type == InstructionType.Padding))
                            nb.Items.Add(pos, stripTags ? lti.MostInner : lti);
                    else
                        nb.Items.Add(pos, inst);
                }
                newBlocks[bm] = nb;
                prev = nb;
            }

            // fix branches
            for (var b = g.BaseBlock; b != null; b = b.NextBlockDefault)
            {
                var nb = newBlocks[map[b]];
                var (bd, bc) = jump[b];
                var nbc = bc == null ? null : newBlocks[map[bc]];
                nb.NextBlockCondition = nbc;
            }

            return new(g, newBlocks[map[g.BaseBlock]]);
        }

    }

    

}
