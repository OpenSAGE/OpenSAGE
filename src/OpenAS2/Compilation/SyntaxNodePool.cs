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
        public Dictionary<int, string> RegNames { get; }
        public Dictionary<int, Value> Registers { get; }
        public Dictionary<string, Value?> NodeNames { get; }
        public Dictionary<Value, string> NodeNames2 { get; }

        public StatementCollection? Parent { get; private set; }
        public StringBuilder? CurrentBuilder { get; private set; }
        public int CurrentIndent { get; private set; }
        public int DeltaIndent { get; private set; }

        public StatementCollection(SyntaxNodePool pool)
        {
            Nodes = pool.PopStatements(); // TODO check
            RegNames = new Dictionary<int, string>(pool.RegNames);
            Registers = new();
            NodeNames = new();
            NodeNames2 = new();
            foreach (var (reg, rname) in RegNames)
                NodeNames[rname] = null; // don't care overwriting

            Parent = null;
            CurrentBuilder = null;
            CurrentIndent = 0;
            DeltaIndent = 4;

        }

        // nomination

        public bool HasValueName(Value val, out string? name, out bool canOverwrite)
        {
            var ans = NodeNames2.TryGetValue(val, out name);
            canOverwrite = ans;
            if (!ans && Parent != null)
                ans = Parent.HasValueName(val, out name, out var _);
            return ans;
        }

        public bool HasRegisterName(int id, out string? name, out bool canOverwrite)
        {
            var ans = RegNames.TryGetValue(id, out name);
            canOverwrite = ans;
            if (!ans && Parent != null)
                ans = Parent.HasRegisterName(id, out name, out var _);
            return ans;
        }

        public bool HasRegisterValue(int id, out Value? val)
        {
            var ans = Registers.TryGetValue(id, out val);
            if (!ans && Parent != null)
                ans = Parent.HasRegisterValue(id, out val);
            return ans;
        }

        public string NameRegister(int id, string? hint, bool forceOverwrite = false)
        {
            if (string.IsNullOrWhiteSpace(hint))
                hint = $"reg{id}";
            while ((!forceOverwrite) && NodeNames.ContainsKey(hint))
                hint = CompilationUtils.GetIncrementedName(hint);
            RegNames[id] = hint;
            if (!NodeNames.ContainsKey(hint))
                NodeNames[hint] = null;
            return hint;
        }

        public string NameVariable(Value val, string name, bool forceOverwrite = false)
        {
            while ((!forceOverwrite) && NodeNames.ContainsKey(name))
                name = CompilationUtils.GetIncrementedName(name);
            NodeNames[name] = val;
            NodeNames2[val] = name;
            return name;
        }

        // TODO fancier implementation of name-related things

        // code optimization

        public bool IsEmpty() { return Nodes.Count() == 0; } // TODO optimization may be needed


        private void CompileInner(bool compileSubcollections)
        {
            var sb = CurrentBuilder!;
            var curIndent = CurrentIndent;
            var dIndent = DeltaIndent;
            var indentStr = "".ToStringWithIndent(curIndent);
            foreach (var node in Nodes)
            {
                sb.Append(indentStr);
                var flag = node.TryCompose(this, sb);
                if (flag)
                    sb.Append(';');


                var labels = CompilationUtils.FormLabels(node.Labels);
                if (!string.IsNullOrWhiteSpace(labels))
                {
                    sb.Append(" // ");
                    sb.Append(labels);
                }
                
                sb.Append("\n");

            }
        }

        // compilation
        public StringBuilder Compile(StringBuilder? sb = null,
            int startIndent = 0,
            int dIndent = 4,
            bool compileSubCollections = true,
            StatementCollection? parent = null)
        {
            sb = sb == null ? new() : sb;
            var osb = CurrentBuilder;
            var oci = CurrentIndent;
            var odi = DeltaIndent;
            var opr = Parent;

            CurrentBuilder = sb;
            CurrentIndent = startIndent;
            DeltaIndent = dIndent;
            Parent = parent;

            CompileInner(compileSubCollections);

            CurrentBuilder = osb;
            CurrentIndent = oci;
            DeltaIndent = odi;
            Parent = opr;

            return sb;
        }

        public void CallSubCollection(StatementCollection sub, StringBuilder? sb = null, string? prefix = null, string? suffix = null)
        {
            sb = sb ?? CurrentBuilder!;
            sb.Append(("{" + (prefix ?? string.Empty) + '\n').ToStringWithIndent(CurrentIndent));
            sub.Compile(sb, CurrentIndent + DeltaIndent, DeltaIndent, true, this);
            sb.Append(("}" + (suffix ?? string.Empty)).ToStringWithIndent(CurrentIndent));
        }

    }

    // AST
    public class SyntaxNodePool
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
        public int ParentExprssionsPopped { get; private set; }

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
        public SyntaxNodePool(IList<ConstantEntry> globalPool, IDictionary<int, string>? regNames = null)
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
        public SyntaxNodePool(LogicalFunctionContext defFunc, SyntaxNodePool parent, IList<Value>? consts = null)
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
        public SyntaxNodePool(SyntaxNodePool parent, IList<Value>? consts = null)
        {
            GlobalPool = parent.GlobalPool;
            _labels = new();
            if (parent != null)
            {
                NodeList = new(parent.NodeList.Where(x => x is SNExpression));
                ParentNodeDivision = NodeList.Count;
                ParentExprssionsPopped = 0;
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
            var ableToDelete = false;
            if (ind == -1)
                ret = null;
            else
            {
                var node = (SNExpression) NodeList[ind];

                // some special nodes shouldn't be deleted like Enumerate
                ableToDelete = !node.doNotDeleteAfterPopped;

                if (ableToDelete && deleteIfPossible)
                {
                    NodeList.RemoveAt(ind);
                    _labels.AddRange(node.Labels);
                    node.Labels.Clear();
                    if (ind < ParentNodeDivision)
                    {
                        ParentNodeDivision = ind;
                        ++ParentExprssionsPopped;
                    }
                }
                ret = node;
            }
            if (ret != null)
            {
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
            return NodeList.Skip(ParentNodeDivision)
                .Where(x => x is SNStatement || x is SNFunctionCall)
                .Select(x => x is SNStatement ? x : new SNToStatement((SNExpression) x))
                .Where(x => !_special.ContainsKey(x))
                .Cast<SNStatement>();
        }
        public IEnumerable<SyntaxNode> PopNodes()
        {
            return NodeList.Skip(ParentNodeDivision).Where(x => !_special.ContainsKey(x));
        }

        public static IEnumerable<SNStatement> GatherNodes(SyntaxNodePool np1, SyntaxNodePool np2)
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

        public void PushNodeConstant(int id, Func<SNExpression, SyntaxNode?>? f = null)
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

        public SNExpression NominateRegister(int reg, SNExpression? hintVal = null)
        {
            var nameHint = $"reg{reg}";
            try
            {
                nameHint = hintVal!.TryComposeRaw(null!);
            }
            catch (Exception e)
            {

            }
            nameHint = CompilationUtils.JustifyName(nameHint);
            while (RegNames.ContainsValue(nameHint))
                nameHint = CompilationUtils.GetIncrementedName(nameHint);
            var newNameNode = new SNNominator(nameHint);
            RegNames[reg] = nameHint;
            return newNameNode;
        }

        public void SetRegister(int reg, SNExpression val)
        {
            _labels.AddRange(val.Labels);
            val.Labels.Clear();
            if (RegValues.TryGetValue(reg, out var rv))
            {
                PushNode(new SNValAssign(rv, val)); // TODO check reference
            }
            else if (val is SNNominator sv)
            {
                RegValues[reg] = val;
                RegNames[reg] = sv.Name;
            }
            else
            {
                var newNameNode = NominateRegister(reg, val);
                RegValues[reg] = newNameNode;
                PushNode(new SNValAssign(newNameNode, val, true));
            }
        }

        public void PushNodeRegister(int id)
        {
            if (RegValues.TryGetValue(id, out var r))
            {
                PushNode(r!);
            }
            else
            {
                var newNameNode = NominateRegister(id);
                RegValues[id] = newNameNode;
                PushNode(new SNValAssign(newNameNode, new SNLiteralUndefined(), true));
                PushNode(newNameNode);
            }
        }

        // chain-based
        public void PushInstruction(RawInstruction inst, StructurizedBlockChain chain)
        {
            if (inst is LogicalTaggedInstruction ltag)
                _labels.AddRange(ltag.GetLabels());

            if (inst is LogicalFunctionContext fc)
            {
                var subpool = new SyntaxNodePool(fc, this);
                if (chain.ChainRecord.TryGetValue(fc, out var cfc))
                    subpool.PushChain(cfc);
                else
                    throw new InvalidOperationException();
                StatementCollection sc = new(subpool);
                var (name, _) = CompilationUtils.GetNameAndArguments(fc);
                SyntaxNode n = string.IsNullOrWhiteSpace(name) ? new SNFunctionBody(fc, sc) : new SNDefineFunction(fc, sc);
                PushNode(n);
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
                // var branch = chain.AdditionalData[0].EndBlock.BranchCondition;
                var bexp = NodeList.Last();
                if (bexp != null)
                {
                    NodeList.RemoveAt(NodeList.Count - 1);
                }

                // create node expression
                SyntaxNodePool sub1 = new(this);
                SyntaxNodePool sub2 = new(this);
                sub1.PushChain(chain.AdditionalData[1]);
                sub2.PushChain(chain.AdditionalData[2]);
                SNControlCase n = new(bexp as SNExpression, new(sub1), new(sub2));
                PushNode(n);
                var mostpep = Math.Max(sub1.ParentExprssionsPopped, sub2.ParentExprssionsPopped);
                var np1 = sub1.PopNodes().Where(x => x is SNExpression).Cast<SNExpression>();
                var np2 = sub2.PopNodes().Where(x => x is SNExpression).Cast<SNExpression>();
                var d1 = mostpep - sub1.ParentExprssionsPopped;
                var d2 = mostpep - sub2.ParentExprssionsPopped;
                if (d1 != 0)
                {
                    np1 = sub1.PopNodes().Where(x => x is SNExpression).Cast<SNExpression>().TakeLast(d1).Concat(np1);
                }
                if (d2 != 0)
                {
                    np2 = sub2.PopNodes().Where(x => x is SNExpression).Cast<SNExpression>().TakeLast(d1).Concat(np2);
                }
                var pns = PopNodes().Where(x => x is SNExpression).TakeLast(mostpep).Cast<SNExpression>().ToList();
                for (var _ = 0; _ < pns.Count(); ++_)
                    PopExpression();
                var maxnpc = Math.Max(np1.Count(), np2.Count());
                np1 = pns.Concat(np1).TakeLast(maxnpc);
                np2 = pns.Concat(np2).TakeLast(maxnpc);
                var d12 = np1.Count() - np2.Count();
                if (d12 > 0)
                    np2 = Enumerable.Repeat<SNExpression>(new SNLiteralUndefined(), d12).Concat(np2);
                else if (d12 < 0)
                    np1 = Enumerable.Repeat<SNExpression>(new SNLiteralUndefined(), -d12).Concat(np1);
                foreach (var (c1, c2) in np1.Zip(np2, (x, y) => (x, y)))
                {
                    var ns2 = new SNTernary(bexp as SNExpression, c1, c2);
                        // _special[ns2] = 1;
                        PushNode(ns2);
                    
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
                SyntaxNodePool sub1 = new(this);
                SyntaxNodePool sub2 = new(this);
                sub1.PushChain(chain.AdditionalData[0]);
                sub2.PushChain(chain.AdditionalData[1]);
                SNControlLoop n = new(bexp as SNExpression, new(sub1), new(sub2));
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

        public static SyntaxNodePool ConvertToAST(StructurizedBlockChain chain, IList<ConstantEntry>? constants, IDictionary<int, string>? regNames)
        {
            SyntaxNodePool pool = new(constants ?? new List<ConstantEntry>().AsReadOnly(), regNames);
            pool.PushChain(chain);
            return pool;
        }

        // graph-based
    }

}
