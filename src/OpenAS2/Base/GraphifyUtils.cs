using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAS2.Base
{
    using RawInstructionStorage = SortedList<uint, RawInstruction>;
    using InstKvp = KeyValuePair<uint, RawInstruction>;

    public class LogicalBranchInstruction : RawInstruction
    {
        public InstructionBlock TargetBlock { get; private set; }
        public LogicalBranchInstruction(InstructionType type, InstructionBlock targetBlock) : base(type, null, false)
        {
            TargetBlock = targetBlock;
        }
    }

    public class LogicalFunctionInstruction: RawInstruction
    {
        public uint CodeSize { get; private set; }
        public LogicalFunctionInstruction(LogicalFunctionContext lfc, uint codeSize) : base(lfc.Type, lfc.Parameters)
        {
            CodeSize = codeSize;
        }
    }

    public static class GraphifyUtils
    {

        public static bool IsStorageEquivalent(RawInstructionStorage s1, RawInstructionStorage s2, out string? msg)
        {
            StringBuilder sb = new StringBuilder();
            var ans = true;

            var c = s1.Count;
            if (s1.Count != s2.Count)
            {
                ans = false;
                c = Math.Min(c, s2.Count);
                sb.AppendLine("The count is not the same");
            }

            List<InstKvp> s1l = new(s1);
            List<InstKvp> s2l = new(s2);
            int i1 = 0, i2 = 0;
            for (int i = 0; i < c; ++i)
            {
                var (pos1, inst1) = s1l[i1];
                var (pos2, inst2) = s2l[i2];

                if (RawInstruction.ContentEquals(inst1, inst2))
                    continue;
                else
                {
                    if (i2 - 1 >= 0 && RawInstruction.ContentEquals(inst1, s2l[i2 - 1].Value))
                        --i2;
                    else if (i2 + 1 < s2.Count && RawInstruction.ContentEquals(inst1, s2l[i2 + 1].Value))
                        ++i2;
                    else if (i2 - 1 >= 0 && RawInstruction.ContentEquals(inst1, s2l[i2 - 2].Value))
                        i2 -= 2;
                    else if (i2 + 1 < s2.Count && RawInstruction.ContentEquals(inst1, s2l[i2 + 2].Value))
                        i2 += 2;
                    sb.AppendLine($"ContentEquals returns false at index #{i}(#{i1}, #{i2}) with position {pos1} and {pos2}");
                    ans = false;
                }

                ++i1;
                ++i2;
            }

            if (ans == true)
                msg = null;
            else
                msg = sb.ToString();
            return ans;
        }

        public static int GetRealParamLength(uint startPointer, InstructionType type)
        {
            int alignmentItem = 0;
            if (Definition.IsAlignmentRequired(type))
                alignmentItem = (4 - ((int)startPointer + 1) % 4) % 4;
            return 1 + alignmentItem + Definition.GetParamLength(type);
        }

        public static List<uint> CalculateOffset(IEnumerable<InstructionType> types, uint startPosition = 0)
        {
            List<uint> ans = new();
            uint curSP = startPosition;
            foreach (InstructionType type in types)
            {
                ans.Add(curSP);
                curSP += (uint) GetRealParamLength(curSP, type);
            }
            return ans;
        }

        public static RawInstructionStorage ReverseGraphify(InstructionGraph g, uint startPosition = 0)
        {
            Dictionary<InstructionBlock, uint> posDict = new();
            var ansInner = ReverseGraphifyInner(g, posDict, startPosition);
            ansInner.Add(new(InstructionType.End, null));
            var offset = CalculateOffset(ansInner.Select(x => x.Type), startPosition);
            RawInstructionStorage ans = new();
            for (int i = 0; i < ansInner.Count; ++i)
            {
                var curOffset = offset[i];
                var nextOffset = offset[(i + 1) % ansInner.Count];
                var inst = ansInner[i];
                if (inst is LogicalFunctionInstruction lfi)
                {
                    List<RawValue> p = new(inst.Parameters);
                    var nParams = p[1].Integer;
                    if (inst.Type == InstructionType.DefineFunction2)
                        p[4 + nParams * 2] = RawValue.FromUInteger(lfi.CodeSize);
                    else
                        p[2 + nParams] = RawValue.FromUInteger(lfi.CodeSize);
                    inst = new RawInstruction(inst.Type, p, false);
                }
                else if (inst is LogicalBranchInstruction lbi)
                {
                    var branchTarget = (int)(posDict[lbi.TargetBlock] - nextOffset);
                    List<RawValue> p = new() { RawValue.FromInteger(branchTarget) };
                    inst = new RawInstruction(inst.Type, p, false);
                }
                else if (inst is LogicalTaggedInstruction lti)
                    inst = lti.MostInner;
                ans[curOffset] = inst;
            }
            return ans;
        }

        public static List<RawInstruction> ReverseGraphifyInner(InstructionGraph g, Dictionary<InstructionBlock, uint> startPosDict, uint curPos, int depth = 0)
        {
            List<RawInstruction> res = new();
            for (var b = g.BaseBlock; b != null; b = b.NextBlockDefault)
            {
                // Console.WriteLine($"#{depth} {b}");
                startPosDict[b] = curPos;
                foreach (var (_, inst) in b.Items)
                {
                    // Console.WriteLine(curPos);
                    int offset = GetRealParamLength(curPos, inst.Type);
                    uint curPosWithOffset = curPos + (uint)offset;
                    if (inst is LogicalFunctionContext lfc)
                    {
                        var rgi = ReverseGraphifyInner(lfc.Instructions, startPosDict, curPosWithOffset, depth + 1);
                        uint codeSize = CalculateOffset(rgi.Select(x => x.Type).Append(InstructionType.End), curPosWithOffset).LastOrDefault(curPosWithOffset) - curPosWithOffset;
                        curPos = curPosWithOffset + codeSize;
                        res.Add(new LogicalFunctionInstruction(lfc, codeSize));
                        res.AddRange(rgi);
                    }
                    else if (inst.IsBranch)
                    {
                        if (inst != b.BranchCondition || b.NextBlockCondition == null)
                            throw new InvalidOperationException();
                        curPos = curPosWithOffset;
                        res.Add(new LogicalBranchInstruction(inst.Type, b.NextBlockCondition!));
                    }
                    else
                    {
                        if (inst.Type == InstructionType.End)
                            continue; // strip all ends

                        curPos = curPosWithOffset;
                        res.Add(inst);
                    }
                }
            }

            return res;
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
                if (newBlocks.TryGetValue(bm, out var nb))
                {
                    // do nothing
                    // continue;
                }
                else
                {
                    nb = new InstructionBlock(prev);
                    nb.BranchCondition = bm.BranchCondition;
                }
                nb.Labels.AddRange(b.Labels);
                var damnparsing = 0;
                foreach (var (pos, inst) in b.Items)
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
                            ++damnparsing;
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
