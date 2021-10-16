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

    public class LogicalValue : Value
    {
        public readonly Node N;
        public LogicalValue(Node n) : base(ValueType.Undefined)
        {
            N = n;
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }

    public class StatementCollection
    {
        public IEnumerable<NodeStatement> Statements { get; private set; }
        public IEnumerable<Value> Constants { get; }
        public Dictionary<int, string> RegNames { get; }
        public Dictionary<int, Value> Registers { get; }
        public Dictionary<string, Value?> NodeNames { get; }
        public Dictionary<Value, string> NodeNames2 { get; }

        public StatementCollection(NodePool pool) {
            Statements = pool.PopStatements();
            Constants = pool.Constants;
            RegNames = new Dictionary<int, string>(pool.RegNames);
            Registers = new();
            NodeNames = new();
            NodeNames2 = new();
            foreach (var (reg, rname) in RegNames)
                NodeNames[rname] = null; // don't care overwriting
        }

        // nomination
        public string NameRegister(int id, string? hint, bool forceOverwrite = false)
        {
            if (string.IsNullOrWhiteSpace(hint))
                hint = $"reg{id}";
            while ((!forceOverwrite) && NodeNames.ContainsKey(hint))
                hint = InstUtils.GetIncrementedName(hint);
            RegNames[id] = hint;
            if (!NodeNames.ContainsKey(hint))
                NodeNames[hint] = null;
            return hint;
        }

        public string NameVariable(Value val, string name, bool forceOverwrite = false)
        {
            while ((!forceOverwrite) && NodeNames.ContainsKey(name))
                name = InstUtils.GetIncrementedName(name);
            NodeNames[name] = val;
            NodeNames2[val] = name;
            return name;
        }

        // compilation
        public StringBuilder Compile(StringBuilder? sb = null, int startIndent = 0, int dIndent = 4, bool compileSubCollections = true, bool ignoreLastBranch = false)
        {
            sb = sb == null ? new() : sb;
            var curIndent = startIndent;

            foreach (var node in Statements)
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
                    node.TryCompile(this, !ignoreLastBranch || node != Statements.Last());
                    if (node.Code == null)
                        sb.Append("// no compiling result\n".ToStringWithIndent(curIndent));
                    else
                        
                        {
                        var code = node.Code;
                            if (string.IsNullOrWhiteSpace(code))
                                continue;
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
                if (RegNames != null && RegNames.TryGetValue(v.ToInteger(), out var reg))
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
        public List<Node> NodeList { get; }

        // should not be modified
        public IEnumerable<Value> Constants { get; }

        // change through function
        public Dictionary<int, string> RegNames { get; }


        // will be removed
        public CodeType Type { get; private set; }

        // brand new pool
        public NodePool(IEnumerable<Value>? consts = null, IDictionary<int, string>? regNames = null)
        {
            Type = CodeType.Sequential;
            NodeList = new();
            Constants = consts == null ? Enumerable.Empty<Value>() : new List<Value>(consts);
            if (regNames != null)
                RegNames = new(regNames);
            else
                RegNames = new();
        }

        // subpool of function
        public NodePool(LogicalFunctionContext defFunc, NodePool parent, IEnumerable<Value>? consts = null)
        {
            NodeList = new();
            if (parent != null)
            {
                Constants = parent.Constants;
                RegNames = defFunc.Instructions.RegNames == null ? new() : new(defFunc.Instructions.RegNames!);
            }
            else
            {
                Constants = consts == null ? Enumerable.Empty<Value>() : new List<Value>(consts);
                RegNames = new();
            }
        }

        // subpool of control
        public NodePool(NodePool parent, IEnumerable<Value>? consts = null)
        {
            if (parent != null)
            {
                NodeList = new(parent.NodeList.Where(x => x is NodeExpression));
                Constants = parent.Constants;
                RegNames = new(parent.RegNames);
            }
            else
            {
                NodeList = new();
                Constants = consts == null ? Enumerable.Empty<Value>() : new List<Value>(consts);
                RegNames = new();
            }
        }

        public void PushInstruction(InstructionBase inst)
        {
            Node n;
            if (inst is LogicalFunctionContext fc)
            {
                var subpool = new NodePool(fc, this);
                subpool.PushChain(fc.Chain!);
                StatementCollection sc = new(subpool);
                n = fc.IsStatement ? new NodeDefineFunction(fc, sc) : new NodeFunctionBody(fc, sc);
            }
            else if (inst.Type == InstructionType.Enumerate || inst.Type == InstructionType.Enumerate2)
            {
                var obj = PopExpression();
                // a null object and the enumerated objects are pushed
                // but due to the mechanism of this class, only the latter
                // is needed
                n = new NodeEnumerate(inst);
                n.Expressions.Add(obj);
            }
            else
            {
                n = inst.IsStatement ? new NodeStatement(inst) : new NodeExpression(inst);
                n.GetExpressions(this);
                // find if there are any functions, if there are functions, wrap n
                if (n.Expressions.Any(x => x is NodeFunctionBody || x is NodeIncludeFunction))
                    n = new NodeIncludeFunction(n);
            }
            NodeList.Add(n);
        }
        
        public NodeExpression? PopExpression(bool deleteIfPossible = true)
        {
            var ind = NodeList.FindLastIndex(n => n is NodeExpression);
            NodeExpression? ret = null;
            if (ind == -1)
                ret = null;
            else
            {
                var node = (NodeExpression) NodeList[ind];

                // some special nodes shouldn't be deleted like Enumerate
                var ableToDelete = true; 
                if (node.Instruction.Type == InstructionType.Enumerate2 ||
                    node.Instruction.Type == InstructionType.Enumerate)
                    ableToDelete = false;

                if (ableToDelete && deleteIfPossible)
                    NodeList.RemoveAt(ind);
                ret = node;
            }
            return ret;
        }
        public NodeArray PopArray(bool readPair = false, bool ensureCount = true)
        {
            NodeArray ans = new();
            var nexp = PopExpression();
            Value? countVal = null;
            // another trygetvalue()

            var flag = nexp == null ? false : nexp.TryGetValue(x => InstUtils.ParseValue(x, Constants, null), out countVal);
            if (flag)
            {
                var count = countVal!.ToInteger();
                if (readPair) count *= 2;
                for (int i = 0; i < count; ++i)
                {
                    var exp = PopExpression();
                    if (exp != null || (exp == null && ensureCount))
                        ans.Expressions.Add(exp);
                }
            }
            return ans;
        }

        public IEnumerable<NodeStatement> PopStatements()
        {
            return NodeList.Where(x => x is NodeStatement).Cast<NodeStatement>();
        }

        
        public static NodePool PushBlock(InstructionBlock root, IEnumerable<Value>? constants, IDictionary<int, string>? regNames)
        {
            NodePool tree = new(constants, regNames);
            foreach (var kvp in root.Items)
            {
                var inst = kvp.Value;
                tree.PushInstruction(inst);
            }
            return tree;
        }

        public void PushChainRaw(StructurizedBlockChain chain, bool ignoreBranch)
        {
            if (chain.Empty)
                return;
            var c = chain;
            var currentBlock = c.StartBlock;
            while (currentBlock != null && (c.EndBlock == null || currentBlock.Hierarchy <= c.EndBlock!.Hierarchy))
            {
                foreach (var (pos, inst) in currentBlock.Items)
                {
                    // TODO how to ignore branch?
                    PushInstruction(inst);
                }

                currentBlock = currentBlock.NextBlockDefault;
            }
            c = c.Next;
        }

        public void PushChain(
            StructurizedBlockChain chain, 
            List<Value>? constPool = null,
            Dictionary<int, string>? regNames = null,
            int layer = 0,
            CodeType type = CodeType.Sequential
            )
        {
            // Console.WriteLine(this);
            if (chain.Type == CodeType.Case)
            {
                //Console.WriteLine("If Statement {".ToStringWithIndent(layer * 4));
                PushChain(chain.AdditionalData[0]);
                var branch = chain.AdditionalData[0].EndBlock.BranchCondition;
                var bexp = NodeList.Last();
                if (bexp != null)
                    bexp = bexp.Expressions[0];

                // create node expression
                NodePool sub1 = new(this);
                NodePool sub2 = new(this);
                sub1.PushChain(chain.AdditionalData[1]);
                sub2.PushChain(chain.AdditionalData[2]);
                NodeCase n = new(branch!, bexp as NodeExpression, new(sub1), new(sub2));
                NodeList.Add(n);
            }
            else if (chain.Type == CodeType.Loop)
            {
                PushChain(chain.AdditionalData[0]);
                var branch = chain.AdditionalData[0].EndBlock.BranchCondition;
                var bexp = NodeList.Last();
                if (bexp != null)
                    bexp = bexp.Expressions[0];
                // create node expression
                // this one needs more than condition!!!
                NodePool sub1 = new(this);
                NodePool sub2 = new(this);
                sub1.PushChain(chain.AdditionalData[0]);
                sub2.PushChain(chain.AdditionalData[1]);
                NodeLoop n = new(branch!, bexp as NodeExpression, new(sub1), new(sub2));
                NodeList.Add(n);

            }
            else if (chain.SubChainStart != null)
            {
                var c = chain.SubChainStart;
                while (c != null)
                {
                    PushChain(c, constPool, regNames, layer);
                    c = c.Next;
                }
            }
            else
                PushChainRaw(chain, false);
        }

        public static NodePool ConvertToAST(StructurizedBlockChain chain, IEnumerable<Value>? constants, IDictionary<int, string>? regNames)
        {
            NodePool pool = new(constants, regNames);
            pool.PushChain(chain);
            return pool;
        }

    }

    // Nodes


    public abstract class Node
    {
        public readonly List<NodeExpression?> Expressions;
        public readonly InstructionBase Instruction;
        public string? Code { get; protected set; }

        protected Node(InstructionBase inst)
        {
            Instruction = inst;
            Expressions = new();
        }

        public void GetExpressions(NodePool pool)
        {
            Expressions.Clear();
            var instruction = Instruction;
            if (Instruction is LogicalTaggedInstruction itag)
                instruction = itag.FinalInnerAction;
            // special process and overriding regular process
            var flagSpecialProc = true;
            switch (instruction.Type)
            {
                // type 1: peek but no pop
                case InstructionType.SetRegister:
                    Expressions.Add(pool.PopExpression(false));
                    // TODO resolve register
                    break;
                case InstructionType.PushDuplicate:
                    Expressions.Add(pool.PopExpression(false));
                    break;

                // type 2: need to read args
                case InstructionType.InitArray:
                    Expressions.Add(pool.PopArray());
                    break;
                case InstructionType.ImplementsOp:
                case InstructionType.CallFunction:
                case InstructionType.EA_CallFuncPop:
                case InstructionType.NewObject:
                    Expressions.Add(pool.PopExpression());
                    Expressions.Add(pool.PopArray());
                    break;
                case InstructionType.CallMethod:
                case InstructionType.EA_CallMethod:
                case InstructionType.EA_CallMethodPop:
                case InstructionType.NewMethod:
                    Expressions.Add(pool.PopExpression());
                    Expressions.Add(pool.PopExpression());
                    Expressions.Add(pool.PopArray());
                    break;
                case InstructionType.EA_CallNamedFuncPop:
                case InstructionType.EA_CallNamedFunc:
                    Expressions.Add(new NodeValue(instruction));
                    Expressions.Add(pool.PopArray());
                    break;
                case InstructionType.EA_CallNamedMethodPop:
                    Expressions.Add(new NodeValue(instruction));
                    Expressions.Add(pool.PopExpression());
                    Expressions.Add(pool.PopArray());
                    break;
                case InstructionType.EA_CallNamedMethod:
                    Expressions.Add(new NodeValue(instruction)); // TODO resolve constant
                    Expressions.Add(pool.PopExpression());
                    Expressions.Add(pool.PopArray());
                    Expressions.Add(pool.PopExpression());
                    break;
                case InstructionType.InitObject:
                    Expressions.Add(pool.PopArray(true));
                    break;

                // type 3: constant resolve needed
                case InstructionType.EA_GetNamedMember:
                    Expressions.Add(new NodeValue(instruction)); // TODO resolve constant
                    Expressions.Add(pool.PopExpression());
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
                    Expressions.Add(pool.PopExpression());
            }
            else if (!flagSpecialProc) // not implemented instructions
            {
                throw new NotImplementedException(instruction.Type.ToString());
            }

            // filter string

        }

        public virtual void TryCompile(StatementCollection sta, bool compileBranches = false)
        {
            // get all values
            var valCode = new string[Expressions.Count];
            var val = new Value?[Expressions.Count]; // should never be constant or register type
            for (int i = 0; i < Expressions.Count; ++i)
            {
                // get value
                var ncur = Expressions[i];
                if (ncur == null)
                {
                    valCode[i] = $"__args__[{i}]";
                    val[i] = null;
                    continue;
                }
                else if (ncur.TryGetValue(x => InstUtils.ParseValue(x, sta.Constants, sta.Registers!), out var nval))
                {
                    valCode[i] = sta.GetExpression(nval!);
                    val[i] = nval;
                    if (string.IsNullOrEmpty(valCode[i]))
                    {
                        ncur.TryCompile(sta);
                        valCode[i] = string.IsNullOrEmpty(ncur.Code) ? $"__args__[{i}]" : ncur.Code;
                    }
                }
                else
                {
                    ncur.TryCompile(sta);
                    valCode[i] = string.IsNullOrEmpty(ncur.Code) ? $"__args__[{i}]" : ncur.Code;
                    val[i] = null;
                    // fix precendence
                    // only needed for compiled codes
                    if (ncur.Instruction != null && Instruction.Precendence > ncur.Instruction.Precendence)
                        valCode[i] = $"({valCode[i]})";
                }
            }

            // fix string values
            // case 1: all strings are needed to fix
            if (Instruction.Type == InstructionType.Add2 ||
                Instruction.Type == InstructionType.GetURL ||
                Instruction.Type == InstructionType.GetURL2 ||
                Instruction.Type == InstructionType.StringConcat ||
                Instruction.Type == InstructionType.StringEquals)
            {
                for (int i = 0; i < Expressions.Count; ++i)
                {
                    if (val[i] != null && val[i]!.Type == ValueType.String)
                        valCode[i] = valCode[i].ToCodingForm();
                }
            }
            // case 2: only the first one
            else if (Instruction.Type == InstructionType.DefineLocal ||
                     // Instruction.Type == InstructionType.Var ||
                     Instruction.Type == InstructionType.ToInteger ||
                     Instruction.Type == InstructionType.ToString ||
                     Instruction.Type == InstructionType.SetMember ||
                     Instruction.Type == InstructionType.SetVariable ||
                     Instruction.Type == InstructionType.SetProperty ||
                     Instruction.Type == InstructionType.EA_PushString ||
                     // Instruction.Type == InstructionType.EA_SetStringMember ||
                     // Instruction.Type == InstructionType.EA_SetStringVar ||
                     Instruction.Type == InstructionType.EA_PushConstantByte ||
                     Instruction.Type == InstructionType.EA_PushConstantWord ||
                     Instruction.Type == InstructionType.Trace)
            {
                if (val[0] != null && val[0]!.Type == ValueType.String)
                    valCode[0] = valCode[0].ToCodingForm();
            }
            // case 3 special handling

            // start compile
            string ret = string.Empty;
            string tmp = string.Empty;
            switch (Instruction.Type)
            {
                // case 1: branches (break(1); continue(2); non-standatd codes(3))
                case InstructionType.BranchAlways:
                case InstructionType.BranchIfTrue:
                case InstructionType.EA_BranchIfFalse:
                    if (compileBranches)
                    {
                        var itmp = Instruction;
                        var ttmp = itmp.Type;
                        var lbl = $"[[{itmp.Parameters[0]}]]";
                        while (itmp is LogicalTaggedInstruction itag)
                        {
                            lbl = string.IsNullOrEmpty(itag.Label) && itag.TagType == TagType.GotoLabel ? lbl : itag.Label;
                            itmp = itag.InnerAction;
                        }
                        if (itmp.Type == InstructionType.BranchAlways)
                            if (itmp.Parameters[0].ToInteger() > 0)
                                ret = $"break; // __jmp__({lbl!.ToCodingForm()})@";
                            else
                                ret = $"continue; // __jmp__({lbl!.ToCodingForm()})@";
                        else
                        {
                            tmp = valCode[0];
                            while (tmp.StartsWith("!"))
                            {
                                tmp = tmp.Substring(1);
                                ttmp = ttmp == InstructionType.BranchIfTrue ? InstructionType.EA_BranchIfFalse : InstructionType.BranchIfTrue;
                                if (tmp.StartsWith('(') && tmp.EndsWith(')'))
                                    tmp = tmp.Substring(1, tmp.Length - 2);
                            }
                            ret = $"__{(ttmp == InstructionType.BranchIfTrue ? "jz" : "jnz")}__({lbl!.ToCodingForm()}, {tmp})";
                        }

                    }
                    break;
                // case 2: value assignment statements
                case InstructionType.SetRegister:
                    var nrReg = Instruction.Parameters[0].ToInteger();
                    if (val[0] == null)
                    {
                        var regSet = sta.RegNames.ContainsKey(nrReg);
                        var nReg = sta.NameRegister(nrReg, InstUtils.JustifyName(valCode[0]));
                        ret = $"var {nReg} = {valCode[0]}; // [[register #{nrReg}]], case 1@";
                    }
                    else if (!sta.NodeNames2.ContainsKey(val[0]!))
                    {
                        var regSet = sta.RegNames.ContainsKey(nrReg);
                        var nReg = sta.NameRegister(nrReg, InstUtils.JustifyName(valCode[0]));
                        sta.NameVariable(val[0]!, nReg, true);
                        ret = $"var {nReg} = {valCode[0]}; // [[register #{nrReg}]], case 2@";
                    }
                    else
                    {
                        sta.NameRegister(nrReg, sta.NodeNames2[val[0]!]);
                        ret = $"// [[register #{nrReg}]] <- {sta.NodeNames2[val[0]!]}@"; // do nothing
                    }
                    break;
                // NodeNames should be updated
                case InstructionType.SetMember: // val[1] is integer: [] else: .
                    if (val[1] == null || val[1]!.Type == ValueType.Integer)
                        ret = $"{valCode[2]}[{valCode[1]}] = {valCode[0]}";
                    else
                        ret = Instruction.ToString(valCode);
                    break;

                case InstructionType.SetVariable:

                    break;
                // case 3: unhandled cases | handling is not needed
                default:
                    try
                    {
                        ret = Instruction.ToString(valCode);
                    }
                    catch
                    {
                        ret = Instruction.ToString2(valCode);
                    }
                    break;
            }
            Code = ret;
        }
        
    }
    public class NodeExpression : Node
    {
        public Value? Value { get; protected set; }

        public NodeExpression(InstructionBase inst) : base(inst)
        {
        }

        public virtual bool TryGetValue(Func<Value?, Value?>? parse, out Value? ret)
        {
            Value = ret = null;
            var vals = new Value[Expressions.Count];
            for (int i = 0; i < Expressions.Count; ++i)
            {
                var node = Expressions[i];
                if (node == null)
                    return false;
                if (node.TryGetValue(parse, out var val))
                {
                    if (val!.Type == ValueType.Constant || val.Type == ValueType.Register)
                    {
                        if (parse == null)
                            return false;
                        else
                        {
                            val = parse(val);
                            if (val == null)
                                return false;
                        }
                    }
                    vals[i] = val;
                }
                else
                    return false;
            }
            try
            {
                if (Instruction is InstructionMonoPush inst && inst.PushStack)
                {
                    ret = inst.ExecuteWithArgs2(vals);
                    if (ret!.Type == ValueType.Constant || ret.Type == ValueType.Register)
                    {
                        if (parse == null)
                            return false;
                        else
                            ret = parse(ret);
                    }
                    Value = ret;
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
            return ret != null;
        }

        public override void TryCompile(StatementCollection sta, bool compileBranches = false)
        {
            string ret = string.Empty;
            if (TryGetValue(x => InstUtils.ParseValue(x, sta.Constants, sta.Registers!), out var val))
            {
                if (val == null)
                    ret = "null";
                else if (val.Type == ValueType.String)
                    ret = val.ToString().ToCodingForm();
                else
                    ret = val.ToString();
                Code = ret;
            }
            else // if (Instruction is InstructionMonoPush inst)
            {
                base.TryCompile(sta, compileBranches);
            }
        }
    }

    public class NodeEnumerate : NodeExpression
    {
        public NodeEnumerate(InstructionBase inst) : base(inst)
        {

        }

        public override bool TryGetValue(Func<Value?, Value?>? parse, out Value? ret)
        {
            ret = null;
            return false;
        }

        public override void TryCompile(StatementCollection sta, bool compileBranches = false)
        {
            Code = "[[enumerate node]]";
        }
    }

    public class NodeValue : NodeExpression
    {
        public NodeValue(InstructionBase inst, bool iss = false, Value? v = null) : base(inst)
        {
            Value = v == null ? inst.Parameters[0] : v;
        }
        public override bool TryGetValue(Func<Value?, Value?>? parse, out Value? ret)
        {
            // Better not use FromArray()
            ret = Value;
            if (parse != null)
                ret = parse(ret);
            return ret == null ? false : true;
        }
    }
    public class NodeArray : NodeExpression
    {
        private readonly LogicalValue _v;
        public NodeArray() : base(new InitArray())
        {
            _v = new(this);
        }

        public override bool TryGetValue(Func<Value?, Value?>? parse, out Value? ret)
        {
            // Better not use FromArray()
            ret = _v;
            return true;
        }

        public override void TryCompile(StatementCollection sta, bool compileBranches = false)
        {
            var vals = new string[Expressions.Count];
            for (int i = 0; i < Expressions.Count; ++i)
            {
                var node = Expressions[i];
                if (node == null)
                {
                    vals[i] = $"__args__[{i}]";
                    continue;
                }
                var flag = node.TryGetValue(x => InstUtils.ParseValue(x, sta.Constants, sta.Registers!), out var val);
                if (!flag)
                {
                    node.TryCompile(sta, compileBranches);
                    vals[i] = node.Code == null ? $"__args__[{i}]" : node.Code;
                }
                else
                {
                    vals[i] = val!.ToString();
                    if (val!.Type == ValueType.String)
                        vals[i] = vals[i].ToCodingForm();
                }
            }
            Code = $"[{string.Join(", ", vals)}]";
        }
    }

    public class NodeStatement : Node
    {
        public NodeStatement(InstructionBase inst) : base(inst) { }
        public override void TryCompile(StatementCollection sta, bool compileBranches = false) { base.TryCompile(sta, compileBranches); }
    }

    public abstract class NodeControl : NodeStatement
    {
        public NodeControl(InstructionBase inst) : base(inst) { }
        public override void TryCompile(StatementCollection sta, bool compileBranches = false) { base.TryCompile(sta, compileBranches); }

        public abstract void TryCompile2(StatementCollection sta, StringBuilder sb, int indent = 0, int dIndent = 4, bool compileSubCollections = true);
    }

    public class NodeFunctionBody : NodeExpression
    {
        public StatementCollection Body;
        public static readonly string NoIndentMark = "/*@([{@%@)]}@*/";
        private readonly LogicalValue _v;
        public NodeFunctionBody(InstructionBase inst, StatementCollection body) : base(inst)
        {
            Body = body;
            _v = new(this);
        }

        public override bool TryGetValue(Func<Value?, Value?>? parse, out Value? ret)
        {
            ret = _v;
            return true;
        }

        public override void TryCompile(StatementCollection sta, bool compileBranches = false)
        {
            StringBuilder sb = new();
            var (name, args) = InstUtils.GetNameAndArguments(Instruction);
            var head = $"function({string.Join(", ", args.ToArray())})\n";
            // sb.Append(NoIndentMark);
            sb.Append(head);
            // sb.Append(NoIndentMark);
            sb.Append("{\n");
            Body.Compile(sb, 1, 2, true, false);
            // sb.Append(NoIndentMark);
            sb.Append("}");
            Code = sb.ToString();
        } 
    }

    public class NodeIncludeFunction : NodeControl
    { 
        public readonly Node n;
        public NodeIncludeFunction(Node body) : base(body.Instruction)
        {
            n = body;
        }
        public override void TryCompile(StatementCollection sta, bool compileBranches = false) { n.TryCompile(sta, compileBranches); Code = n.Code; } 

        // using brute force ways to do so
        public override void TryCompile2(StatementCollection sta, StringBuilder sb, int indent = 0, int dIndent = 4, bool compileSubCollections = true)
        {
            TryCompile(sta, true);
            var lines = Code!.Split("\n");
            for (var i = 0; i < lines.Count(); ++i)
            {
                var l = lines[i];
                if (string.IsNullOrWhiteSpace(l))
                    continue;
                var tmpindent = 0;
                while (l.ElementAt(tmpindent) == ' ')
                    ++tmpindent;
                if (i == lines.Count() - 1)
                    if (l.EndsWith("@"))
                        l = l.Substring(0, l.Length - 1);
                    else if (!l.EndsWith(";"))
                        l = l + ";";
                sb.Append(l.Substring(tmpindent).ToStringWithIndent(indent + tmpindent * dIndent));
                sb.Append("\n");
            }
        }
    }

    public class NodeDefineFunction : NodeControl
    {
        public StatementCollection Body;
        public NodeDefineFunction(InstructionBase inst, StatementCollection body) : base(inst)
        {
            Body = body;
        }

        public override void TryCompile(StatementCollection sta, bool compileBranches = false) { throw new NotImplementedException(); }

        public override void TryCompile2(StatementCollection sta, StringBuilder sb, int indent = 0, int dIndent = 4, bool compileSubCollections = true)
        {
            var (name, args) = InstUtils.GetNameAndArguments(Instruction);
            var head = $"function {name}({string.Join(", ", args.ToArray())})\n";
            sb.Append(head.ToStringWithIndent(indent));
            sb.Append("{\n".ToStringWithIndent(indent));
            Body.Compile(sb, indent + dIndent, dIndent, compileSubCollections, false);
            sb.Append("}\n".ToStringWithIndent(indent));
        }
    }

    public class NodeCase: NodeControl
    {

        public NodeExpression Condition;
        public StatementCollection Unbranch;
        public StatementCollection Branch;

        public NodeCase(
            InstructionBase inst,
            NodeExpression condition,
            StatementCollection unbranch, 
            StatementCollection branch
            ) : base(inst)
        {
            Condition = condition;
            Unbranch = unbranch;
            Branch = branch;
        }

        public override void TryCompile2(StatementCollection sta, StringBuilder sb, int indent = 0, int dIndent = 4, bool compileSubCollections = true)
        {
            var tmp = "[[null condition]]";
            if (Condition != null)
            {
                Condition.TryCompile(sta, true); // TODO turn the condition to real condition
                tmp = Condition.Code!;
            }
            var ttmp = Instruction.Type;
            if (ttmp == InstructionType.BranchIfTrue)
            {
                if (tmp.StartsWith("!"))
                {
                    tmp = tmp.Substring(1);
                    if (tmp.StartsWith('(') && tmp.EndsWith(')'))
                        tmp = tmp.Substring(1, tmp.Length - 2);
                }
                else
                {
                    tmp = $"!({tmp})";
                }
            }
            sb.Append($"if ({tmp})\n".ToStringWithIndent(indent));
            sb.Append("{\n".ToStringWithIndent(indent));
            Unbranch.Compile(sb, indent + dIndent, dIndent, compileSubCollections, false);
            sb.Append("}\n".ToStringWithIndent(indent));
            sb.Append("else\n".ToStringWithIndent(indent));
            sb.Append("{\n".ToStringWithIndent(indent));
            Branch.Compile(sb, indent + dIndent, dIndent, compileSubCollections, false);
            sb.Append("}\n".ToStringWithIndent(indent));
        }
    }

    public class NodeLoop : NodeControl
    {

        public NodeExpression Condition;
        public StatementCollection Maintain;
        public StatementCollection Branch;

        public NodeLoop(
            InstructionBase inst,
            NodeExpression condition,
            StatementCollection maintain,
            StatementCollection branch
            ) : base(inst)
        {
            Condition = condition;
            Maintain = maintain;
            Branch = branch;
        }

        public override void TryCompile2(StatementCollection sta, StringBuilder sb, int indent = 0, int dIndent = 4, bool compileSubCollections = true)
        {
            var tmp = "[[null condition]]";
            if (Condition != null)
            {
                Condition.TryCompile(sta, true); // TODO turn the condition to real condition
                tmp = Condition.Code!;
            }
            var ttmp = Instruction.Type;
            if (ttmp == InstructionType.BranchIfTrue)
            {
                if (tmp.StartsWith("!"))
                {
                    tmp = tmp.Substring(1);
                    if (tmp.StartsWith('(') && tmp.EndsWith(')'))
                        tmp = tmp.Substring(1, tmp.Length - 2);
                }
                else
                {
                    tmp = $"!({tmp})";
                }
            }
            sb.Append("{ // loop maintain condition\n".ToStringWithIndent(indent));
            Maintain.Compile(sb, indent + dIndent, dIndent, compileSubCollections, true);
            sb.Append("}\n".ToStringWithIndent(indent));
            sb.Append($"while ({tmp})\n".ToStringWithIndent(indent));
            sb.Append("{\n".ToStringWithIndent(indent));
            Branch.Compile(sb, indent + dIndent, dIndent, compileSubCollections, false);
            sb.Append("}\n".ToStringWithIndent(indent));
        }
    }

}
