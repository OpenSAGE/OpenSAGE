using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAS2.Base;

namespace OpenAS2.Compilation
{
    public class StructurizedBlockChain
    {

        public InstructionBlock StartBlock;
        public InstructionBlock EndBlock; // to be removed
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

        public static StructurizedBlockChain Parse(InstructionBlock root)
        {
            StructurizedBlockChain ret = new(root);

            // parse sub graphs inside defined functions
            var b = root;
            while (b != null)
            {
                foreach (var (p, i) in b.Items)
                    if (i is LogicalFunctionContext fc)
                        throw new NotImplementedException();
                        // fc.Chain = Parse(fc.Instructions.BaseBlock);
                b = b.NextBlockDefault;
            }

            ret.Parse();

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

        public void Parse()
        {

            var b = EndBlock;

            // scan all top-class(nit included by other loops in the current segment) loop structures
            Dictionary<InstructionBlock, InstructionBlock> loops = new(); // start, end(included)
            while (b != null && (b.Hierarchy >= StartBlock.Hierarchy))
            {
                // identify for/while structures
                if (b.HasConstantBranch &&
                    b.NextBlockCondition!.Hierarchy >= StartBlock.Hierarchy && // otherwise this b is a part of a larger loop
                    b.NextBlockCondition!.Hierarchy < b.Hierarchy)
                {
                    // the current b is a loop ending
                    loops[b.NextBlockCondition!] = b;
                    // jump the loops between them
                    b = b.NextBlockCondition!;
                }
                b = b.PreviousBlock;
            }

            // start parsing

            var currentChain = new StructurizedBlockChain(StartBlock, EndBlock) { Next = Next, Type = CodeType.Sequential };
            var subChainCache = currentChain;
            var parsed = false;
            b = StartBlock; // reuse b

            while (b != null && (EndBlock == null || b.Hierarchy <= EndBlock.Hierarchy))
            {
                // b is the start of a loop structure
                if (loops.ContainsKey(b))
                {
                    parsed = true;
                    var startBlock = b;
                    var endBlock = loops[b];

                    InstructionBlock? outerBlock = null;
                    for (var i = startBlock; i != null && i.Hierarchy <= endBlock!.Hierarchy; i = i.NextBlockDefault)
                    {
                        if (!i.HasBranch)
                            continue;

                        var branchBlock = i.NextBlockCondition!;
                        if (branchBlock == null)
                            throw new InvalidOperationException();

                        if (branchBlock.Hierarchy < startBlock.Hierarchy)
                            throw new NotImplementedException();
                        if (branchBlock.Hierarchy > endBlock.Hierarchy)
                        {
                            if (outerBlock != null && branchBlock.Hierarchy != outerBlock.Hierarchy)
                                throw new InvalidOperationException();
                            else
                            {
                                outerBlock = branchBlock;
                                endBlock = branchBlock.PreviousBlock; // TODO is this right?
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

                    var sb = new StructurizedBlockChain(startBlock, endBlock) { Next = sb2, Type = CodeType.Loop, AdditionalData = structures };

                    if (startBlock.PreviousBlock == null)
                        currentChain!.Empty = true;
                    else
                        currentChain!.EndBlock = startBlock.PreviousBlock;
                    currentChain.Next = sb;

                    // cascaded parsing
                    st.Parse();

                    // update b
                    b = endBlock;

                    // update b
                    b = b.NextBlockDefault;
                }
                // b is the start of a case structure
                else if (b.HasConditionalBranch)
                {
                    parsed = true;
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
                    st.Parse();
                    sf.Parse();

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

                    // update b
                    b = b.NextBlockDefault;

                }
                else
                {
                    // halt parsing if jumping out
                    if (b.HasConstantBranch &&
                    b.BranchCondition!.Parameters[0].Integer >= 0 &&
                    (b.NextBlockDefault == null || b.NextBlockCondition!.Hierarchy > b.NextBlockDefault.Hierarchy))
                        b = b.NextBlockCondition;
                    else
                        b = b.NextBlockDefault;
                }

            }
            if (parsed)
            {
                SubChainStart = subChainCache;
            }
        }


    }
}
