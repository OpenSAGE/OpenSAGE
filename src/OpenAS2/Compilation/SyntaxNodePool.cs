using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenAS2.Runtime;
using OpenAS2.Base;
using OpenAS2.Compilation.Syntax;

namespace OpenAS2.Compilation
{
    public class StatementCollection
    {
        public IEnumerable<SyntaxNode> Nodes { get; private set; }
        public IEnumerable<Value> Constants { get; }
        public Dictionary<int, string> RegNames { get; }
        public Dictionary<int, Value> Registers { get; }
        public Dictionary<string, Value?> NodeNames { get; }
        public Dictionary<Value, string> NodeNames2 { get; }

        private StatementCollection? _parent;

        public StatementCollection(NodePool pool)
        {
            Nodes = pool.PopNodes();
            Constants = pool.Constants;
            RegNames = new Dictionary<int, string>(pool.RegNames);
            Registers = new();
            NodeNames = new();
            NodeNames2 = new();
            foreach (var (reg, rname) in RegNames)
                NodeNames[rname] = null; // don't care overwriting
            _parent = null;
        }

        // nomination

        public bool HasValueName(Value val, out string? name, out bool canOverwrite)
        {
            var ans = NodeNames2.TryGetValue(val, out name);
            canOverwrite = ans;
            if (!ans && _parent != null)
                ans = _parent.HasValueName(val, out name, out var _);
            return ans;
        }

        public bool HasRegisterName(int id, out string? name, out bool canOverwrite)
        {
            var ans = RegNames.TryGetValue(id, out name);
            canOverwrite = ans;
            if (!ans && _parent != null)
                ans = _parent.HasRegisterName(id, out name, out var _);
            return ans;
        }

        public bool HasRegisterValue(int id, out Value? val)
        {
            var ans = Registers.TryGetValue(id, out val);
            if (!ans && _parent != null)
                ans = _parent.HasRegisterValue(id, out val);
            return ans;
        }

        public string NameRegister(int id, string? hint, bool forceOverwrite = false)
        {
            if (string.IsNullOrWhiteSpace(hint))
                hint = $"reg{id}";
            while ((!forceOverwrite) && NodeNames.ContainsKey(hint))
                hint = InstructionUtils.GetIncrementedName(hint);
            RegNames[id] = hint;
            if (!NodeNames.ContainsKey(hint))
                NodeNames[hint] = null;
            return hint;
        }

        public string NameVariable(Value val, string name, bool forceOverwrite = false)
        {
            while ((!forceOverwrite) && NodeNames.ContainsKey(name))
                name = InstructionUtils.GetIncrementedName(name);
            NodeNames[name] = val;
            NodeNames2[val] = name;
            return name;
        }

        // TODO fancier implementation of name-related things

        // code optimization

        public bool IsEmpty() { return Nodes.Count() == 0; } // TODO optimization may be needed

        // compilation
        public StringBuilder Compile(StringBuilder? sb = null,
            int startIndent = 0,
            int dIndent = 4,
            bool compileSubCollections = true,
            bool ignoreLastBranch = false,
            StatementCollection? parent = null)
        {
            sb = sb == null ? new() : sb;
            var curIndent = startIndent;

            var p = _parent;
            if (parent != null)
                _parent = parent;

            foreach (var node in Nodes)
            {
                if (node == null)
                    sb.Append("// null node\n".ToStringWithIndent(curIndent));
                else
                {
                    // TODO CSC
                    if (node is NodeControl nc && compileSubCollections)
                    {
                        nc.TryCompile2(this, sb, curIndent, dIndent, compileSubCollections);
                        continue;
                    }
                    node.TryCompose(this, !ignoreLastBranch || node != Nodes.Last());
                    if (node.Code == null)
                        sb.Append("// no compiling result\n".ToStringWithIndent(curIndent));
                    else
                    {
                        var code = node.Code.AddLabels(node.Labels);
                        if (node is SNExpression)
                        {
                            if (node.Instruction.Type == InstructionType.PushData || node.Instruction.Type.ToString().StartsWith("EA_Push"))
                                code = $"// __push__({code})@";
                        }
                        if (string.IsNullOrWhiteSpace(code))
                            continue;
                        code = code.Replace("@; //", ", ");
                        if (code.EndsWith("@"))
                            sb.Append(code.Substring(0, code.Length - 1).ToStringWithIndent(curIndent));
                        else
                        {
                            sb.Append(code.ToStringWithIndent(curIndent));
                            sb.Append(";");
                        }
                        sb.Append("\n");
                    }
                }
            }

            _parent = p;
            return sb;
        }

        public string GetExpression(Value v)
        {
            var ret = string.Empty;
            if (v.Type == ValueType.Constant)
                if (Constants != null && v.ToInteger() >= 0 && v.ToInteger() < Constants.Count())
                {
                    v = Constants.ElementAt(v.ToInteger());
                }
                else
                {
                    ret = $"__const__[{v.ToInteger()}]";
                }

            if (v.Type == ValueType.Register)
                if (HasRegisterName(v.ToInteger(), out var reg, out var _))
                {
                    ret = reg;
                }
                else
                {
                    ret = $"__reg__[{v.ToInteger()}]";
                }

            if (string.IsNullOrEmpty(ret))
            {
                if (NodeNames2.TryGetValue(v, out var ret2))
                    ret = ret2;
                else
                    ret = v.ToString();
            }

            return ret;
        }

    }

    // AST
    public class NodePool
    {
        /*
         * This class is used to change StructurizedBlockChain's to some structure
         * that has the feature of AST. 
         * In principle, no calculation should be involved, but enumerate- and array-related 
         * actions are exceptions.
         * The following assumptions are used:
         * - ConstantPool is only executed at most once in one code piece.
         *   Consequences of violation is unclear yet. 
         * - Enumerate is only used in for...in statements.
         *   Consequences of violation is unclear yet. 
         * 
         */

        // change through process
        public List<SyntaxNode> NodeList { get; }
        private Dictionary<SyntaxNode, int> _special = new();
        public int ParentNodeDivision { get; private set; }

        // should not be modified
        public IList<Value> Constants { get; set; }
        public IList<ConstantEntry> GlobalPool { get; }

        // should not be modified
        public Dictionary<int, string> RegNames { get; }
        public Dictionary<int, SNExpression?> RegValues { get; }

        private readonly List<string> _labels;

        public override string ToString()
        {
            return $"NodePool({NodeList.Count} Nodes, {Constants.Count()} Constants, {RegNames.Count} Named Registers)";
        }


        // brand new pool
        public NodePool(IList<ConstantEntry> globalPool, IDictionary<int, string>? regNames = null)
        {
            GlobalPool = globalPool;
            _labels = new();
            NodeList = new();
            Constants = new List<Value>();
            
            if (regNames != null)
                RegNames = new(regNames);
            else
                RegNames = new();

            RegValues = new();
            foreach (var (i, n) in RegNames)
                RegValues[i] = new SNNominator(n);
        }

        // subpool of function
        public NodePool(LogicalFunctionContext defFunc, NodePool parent, IList<Value>? consts = null)
        {
            GlobalPool = parent.GlobalPool;
            NodeList = new();
            _labels = new();
            if (parent != null)
            {
                Constants = parent.Constants;
                RegNames = defFunc.Instructions.RegNames == null ? new() : new(defFunc.Instructions.RegNames!);
            }
            else
            {
                Constants = consts == null ? new List<Value>().AsReadOnly() : consts;
                RegNames = new();
            }
            RegValues = new();
            foreach (var (i, n) in RegNames)
                RegValues[i] = new SNNominator(n);
        }

        // subpool of control
        public NodePool(NodePool parent, IList<Value>? consts = null)
        {
            GlobalPool = parent.GlobalPool;
            _labels = new();
            if (parent != null)
            {
                NodeList = new(parent.NodeList.Where(x => x is SNExpression));
                ParentNodeDivision = NodeList.Count;
                Constants = parent.Constants;
                RegNames = new();
                RegValues = new(parent.RegValues);
            }
            else
            {
                NodeList = new();
                Constants = consts == null ? new List<Value>().AsReadOnly() : consts;
                RegNames = new();
                RegValues = new();
            }
            
        }

        public SNExpression PopExpression(bool deleteIfPossible = true)
        {
            var ind = NodeList.FindLastIndex(n => n is SNExpression);
            SNExpression? ret = null;
            if (ind == -1)
                ret = null;
            else
            {
                var node = (SNExpression) NodeList[ind];

                // some special nodes shouldn't be deleted like Enumerate
                var ableToDelete = !node.doNotDeleteAfterPopped;

                if (ableToDelete && deleteIfPossible)
                {
                    NodeList.RemoveAt(ind);
                    if (ind < ParentNodeDivision)
                        ParentNodeDivision = ind;
                    // --ParentNodeDivision;
                }
                ret = node;
            }
            if (ret != null)
            {
                _labels.AddRange(ret.Labels);
                return ret;
            }
            return new SNLiteralUndefined();
        }
        public SNArray PopArray(bool readPair = false, bool ensureCount = true) // TODO remove ensurecount?
        {
            
            List<SNExpression?> expressions = new();
            var nexp = PopExpression();
            var count = 0;
            if (nexp is SNLiteral s && s.Value is RawValue v)
                count = v.Integer == 0 ? (int) v.Double : v.Integer;
            for (int i = 0; i < count; ++i)
            {
                if (readPair)
                {
                    var exp1 = PopExpression();
                    var exp2 = PopExpression();
                    expressions.Add(OprUtils.KeyAssign(exp1, exp2));
                }
                else
                {
                    var exp = PopExpression();
                    expressions.Add(exp);
                }
            }
            SNArray ans = new(expressions);
            return ans;
        }

        public IEnumerable<SNStatement> PopStatements()
        {
            return NodeList.Skip(ParentNodeDivision).Where(x => x is SNStatement && !_special.ContainsKey(x)).Cast<SNStatement>();
        }
        public IEnumerable<SyntaxNode> PopNodes()
        {
            return NodeList.Skip(ParentNodeDivision).Where(x => !_special.ContainsKey(x));
        }

        public static IEnumerable<SNStatement> GatherNodes(NodePool np1, NodePool np2)
        {
            var ans = new List<SNStatement>();
            throw new NotImplementedException();
            return ans;
        }

        public void PushNode(SyntaxNode n)
        {
            n.Labels.AddRange(_labels);
            _labels.Clear();
            NodeList.Add(n);
        }

        public void PushNodeConstant(int id, Func<SNExpression, SNExpression?>? f = null)
        {
            SNExpression n = new SNLiteral(RawValue.FromString(Constants[id].ToString()));
            if (f != null)
            {
                var fn = f(n);
                if (fn != null)
                    PushNode(fn);
            }
            else
                PushNode(n);
        }

        public void PushNodeRegister(int id)
        {
            PushNode(RegValues.TryGetValue(id, out var r) ? r! : new SNLiteralUndefined());
        }

        public void PushInstruction(RawInstruction inst, StructurizedBlockChain chain)
        {
            if (inst is LogicalTaggedInstruction ltag)
                _labels.AddRange(ltag.GetLabels());
            if (inst is LogicalFunctionContext fc)
            {
                var subpool = new NodePool(fc, this);
                if (chain.ChainRecord.TryGetValue(fc, out var cfc))
                    subpool.PushChain(cfc);
                else
                    throw new InvalidOperationException();
                StatementCollection sc = new(subpool);
                var n = new NodeDefineFunction(fc, sc);
            }
            
            else
            {
                var flag =
                    Control.Parse(this, inst) ||
                    General.Parse(this, inst) ||
                    ObjectOriented.Parse(this, inst);
                if (!flag)
                    throw new NotImplementedException();
            }
            // maintain labels

        }


        public void PushChainRaw(StructurizedBlockChain chain, bool ignoreBranch)
        {
            if (chain.Empty)
                return;
            var c = chain;
            var currentBlock = c.StartBlock;
            while (currentBlock != null && (c.EndBlock == null || currentBlock.Hierarchy <= c.EndBlock!.Hierarchy))
            {
                if (currentBlock.Labels.Count > 0 && currentBlock.Items.Count == 0)
                    // if this NIE is really triggered, consider adding a NodeTag: NodeStatement
                    // yielding Code = "" while receiving no input
                    throw new NotImplementedException();
                foreach (var (pos, inst) in currentBlock.Items)
                    PushInstruction(inst, chain);
                // a temporary solution to the BranchAlways codes.
                if (currentBlock.HasConstantBranch &&
                    currentBlock.BranchCondition!.Parameters[0].Integer >= 0 &&
                    (currentBlock.NextBlockDefault == null || currentBlock.NextBlockCondition!.Hierarchy > currentBlock.NextBlockDefault.Hierarchy))
                    currentBlock = currentBlock.NextBlockCondition;
                else
                    currentBlock = currentBlock.NextBlockDefault;
            }
        }

        // TODO clear judgement conditions
        public void PushChain(
            StructurizedBlockChain chain
            )
        {
            // TODO clear judgement conditions
            if (chain.Type == CodeType.Case)
            {
                PushChain(chain.AdditionalData[0]);
                var branch = chain.AdditionalData[0].EndBlock.BranchCondition;
                var bexp = NodeList.Last();
                if (bexp != null)
                {
                    NodeList.RemoveAt(NodeList.Count - 1);
                }

                // create node expression
                NodePool sub1 = new(this);
                NodePool sub2 = new(this);
                sub1.PushChain(chain.AdditionalData[1]);
                sub2.PushChain(chain.AdditionalData[2]);
                NodeCase n = new(bexp as SNExpression, new(sub1), new(sub2));
                PushNode(n);
                // add expressions inside the loop
                // TODO more judgements
                foreach (var ns2 in sub2.PopNodes())
                {
                    if (ns2 is SNExpression)
                    {
                        _special[ns2] = 1;
                        PushNode(ns2);
                    }
                }
            }
            else if (chain.Type == CodeType.Loop)
            {
                PushChain(chain.AdditionalData[0]);
                var branch = chain.AdditionalData[0].EndBlock.BranchCondition;
                var bexp = NodeList.Last();
                if (bexp != null)
                {
                    NodeList.RemoveAt(NodeList.Count - 1);
                }
                // create node expression
                // this one needs more than condition!!!
                NodePool sub1 = new(this);
                NodePool sub2 = new(this);
                sub1.PushChain(chain.AdditionalData[0]);
                sub2.PushChain(chain.AdditionalData[1]);
                NodeLoop n = new(bexp as SNExpression, new(sub1), new(sub2));
                PushNode(n);
                // add expressions inside the loop?
                foreach (var ns2 in sub2.PopNodes())
                {
                    if (ns2 is SNExpression)
                    {
                        _special[ns2] = 1;
                        PushNode(ns2);
                    }
                }
            }
            else if (chain.SubChainStart != null)
            {
                var c = chain.SubChainStart;
                while (c != null)
                {
                    PushChain(c); // bug
                    c = c.Next;
                }
            }
            else
                PushChainRaw(chain, false);
        }

        public static NodePool ConvertToAST(StructurizedBlockChain chain, IList<ConstantEntry>? constants, IDictionary<int, string>? regNames)
        {
            NodePool pool = new(constants ?? new List<ConstantEntry>().AsReadOnly(), regNames);
            pool.PushChain(chain);
            return pool;
        }

        internal void SetRegister(int reg, SNExpression val)
        {
            RegValues[reg] = val;
        }
    }

}
