using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAS2.Base;
using OpenAS2.Runtime;
using OpenAS2.Compilation.Syntax;
using Value = OpenAS2.Runtime.Value;
using ValueType = OpenAS2.Runtime.ValueType;

namespace OpenAS2.Compilation
{

    // Nodes

    public abstract class SyntaxNode
    {
        public readonly List<string> Labels;
        protected SyntaxNode()
        {
            Labels = new();
        }

        public abstract string TryComposeRaw(StatementCollection sta);

        public virtual string TryCompose(StatementCollection sta, bool compileBranches = false)
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
                else if (ncur.TryGetValue(InstructionUtils.ParseValueWrapped(sta), out var nval))
                {
                    valCode[i] = sta.GetExpression(nval!);
                    val[i] = nval;
                    // if (string.IsNullOrEmpty(valCode[i]))
                    // {
                    //     ncur.TryCompile(sta);
                    //     valCode[i] = ncur.Code == null ? $"__args__[{i}]" : ncur.Code; // do not care empty string
                    // }
                }
                else
                {
                    ncur.TryCompose(sta);
                    valCode[i] = ncur.Code == null ? $"__args__[{i}]" : ncur.Code;
                    val[i] = null;
                    // fix precendence
                    // only needed for compiled codes
                    if (ncur.Instruction != null && Instruction.LowestPrecendence > ncur.Instruction.LowestPrecendence)
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
                            itmp = itag.Inner;
                        }
                        if (itmp.Type == InstructionType.BranchAlways)
                            if (itmp.Parameters[0].ToInteger() > 0)
                                ret = $"break; // __jmp__({lbl!.ToCodingForm()})@";
                            else
                                ret = $"continue; // __jmp__({lbl!.ToCodingForm()})@";
                        else
                        {
                            tmp = valCode[0];
                            (tmp, ttmp) = InstructionUtils.SimplifyCondition(tmp, ttmp);
                            ret = $"__{(ttmp == InstructionType.BranchIfTrue ? "jz" : "jnz")}__({lbl!.ToCodingForm()}, {tmp})";
                        }

                    }
                    break;
                // case 2: value assignment statements
                case InstructionType.SetRegister:
                    var nrReg = Instruction.Parameters[0].ToInteger();
                    if (val[0] == null || !sta.NodeNames2.ContainsKey(val[0]!))
                    {
                        var regSet = sta.HasRegisterName(nrReg, out var nReg, out var co);
                        var c = (val[0] != null ? 2 : 1) + (co ? 2 : 0);
                        if (!regSet || co)
                        {
                            nReg = sta.NameRegister(nrReg, InstructionUtils.JustifyName(valCode[0]));
                            if (val[0] != null)
                                sta.NameVariable(val[0]!, nReg, true);
                            ret = $"var {nReg} = {valCode[0]}; // [[register #{nrReg}]], case {c}@";
                        }
                        else
                        {
                            ret = $"{nReg} = {valCode[0]}; // [[register #{nrReg}]], case {c + 4}@";
                        }
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

                //case InstructionType.SetVariable:

                //  break;
                // case 3: omitted cases
                case InstructionType.Pop:
                    ret = $"// __pop__({valCode[0]})@";
                    break;
                case InstructionType.End:
                    ret = "// __end__()@";
                    break;
                case InstructionType.PushDuplicate:
                    ret = valCode[0];
                    break;

                // case 0: unhandled cases | handling is not needed
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


    public abstract class SNExpression : SyntaxNode
    {
        public virtual int LowestPrecendence => 24;
        public virtual bool doNotDeleteAfterPopped => false;

        private static Dictionary<InstructionType, int> NIE = new();

        public SNExpression() : base() { }

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
                    if (val!.IsSpecialType())
                    {
                        if (parse == null)
                            return false;
                        else
                        {
                            val = parse(val);
                            if (val == null || val!.IsSpecialType())
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
                if (Instruction is InstructionEvaluable inst && inst.PushStack)
                {
                    // NIE optimization
                    if (NIE.TryGetValue(Instruction.Type, out var c) && c > 4)
                        return false;
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
                NIE[Instruction.Type] = NIE.TryGetValue(Instruction.Type, out var c) ? c + 1 : 1;
                return false;
            }
            return ret != null;
        }

        public override abstract string TryComposeRaw(StatementCollection sta);

        public override string TryCompose(StatementCollection sta, bool branch) { return TryCompose(sta, 24); }
        public virtual string TryCompose(StatementCollection sta, int targetPrec = 24)
        {
            var s = TryCompose(sta);
            if (targetPrec > LowestPrecendence)
            {
                s = $"({s})";
            }
            return s;
        }
    }

    // vars

    public class SNEnumerate : SNExpression
    {
        public override bool doNotDeleteAfterPopped => true;
        public SNExpression Node { get; protected set; }
        public SNEnumerate(SNExpression node) : base()
        {
            Node = node;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return $"[[enumerate node: {Node.TryCompose(sta, false)}]]";
        }
        public override string TryCompose(StatementCollection sta, bool branch = false)
        {
            return $"[[enumerate node: {Node.TryCompose(sta, branch)}]]";
        }
    }

    public class SNLiteral : SNExpression
    {
        public RawValue? Value { get; protected set; }
        public bool IsStringLiteral => Value != null && Value.Type == RawValueType.String;

        public SNLiteral(RawValue? v) : base()
        {
            Value = v;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            if (Value == null)
                return "null";
            switch (Value.Type)
            {
                case RawValueType.String:
                    return Value.String.ToCodingForm();
                case RawValueType.Integer:
                    return $"{Value.Integer}";
                case RawValueType.Float:
                    return $"{Value.Double}";
                case RawValueType.Boolean:
                    return Value.Boolean ? "true" : "false";
                case RawValueType.Constant:
                case RawValueType.Register:
                default:
                    throw new InvalidOperationException("Well...This situation is really weird to be reached.");
            }
        }

        public string GetRawString() { return Value.String; }
        
    }

    public class SNLiteralUndefined: SNLiteral
    {
        public SNLiteralUndefined(): base(null) { }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return "undefined";
        }
    }

    public class SNNominator: SNExpression
    {
        public string Name { get; set; }

        public SNNominator(string name): base()
        {
            Name = name;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return Name;
        }

    }

    public class SNArray : SNExpression
    {
        public readonly IList<SNExpression> Expressions;

        public SNArray(IList<SNExpression?> exprs) : base()
        {
            Expressions = exprs.Select(x => x ?? new SNLiteralUndefined()).ToList().AsReadOnly();
        }

        // TODO use sta
        public override string TryComposeRaw(StatementCollection sta)
        {
            var vals = new string[Expressions.Count];
            for (int i = 0; i < Expressions.Count; ++i)
            {
                var node = Expressions[i];
                if (node is SNLiteralUndefined)
                {
                    vals[i] = $"__args__[{i}]";
                }
                else
                {
                    vals[i] = node.TryCompose(sta);
                    if (node is SNArray)
                    {
                        vals[i] = $"[{vals[i]}]";
                    }
                }
            }
            return string.Join(", ", vals);
        }
    }

    public abstract class SNOperator : SNExpression
    {
        public enum Order
        {
            NotAcceptable = 0,
            LeftToRight = 1,
            RightToLeft = 2
        }
        protected readonly int _precendence;
        public readonly Order CalcOrder;
        public override int LowestPrecendence => _precendence;

        public SNOperator(int precendence, Order order)
        {
            _precendence = precendence;
            CalcOrder = order;
        }

        // definitions

    }

    public class SNTernary : SNOperator
    {
        private SNExpression _c, _t, _f;
        public SNTernary(SNExpression? c, SNExpression? t, SNExpression? f) : base(4, Order.RightToLeft)
        {
            _c = c ?? new SNLiteralUndefined();
            _t = t ?? new SNLiteralUndefined();
            _f = f ?? new SNLiteralUndefined();
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return $"{_c.TryCompose(sta, LowestPrecendence)} ? {_t.TryCompose(sta, LowestPrecendence)} : {_f.TryCompose(sta, LowestPrecendence)}"; 
        }
    }

    public class SNBinary : SNOperator
    {
        protected readonly SNExpression _e1, _e2;
        protected readonly string _pattern;

        public SNBinary(int p, Order o, string pat, SNExpression? e1, SNExpression? e2) : base(p, o)
        {
            _e1 = e1 ?? new SNLiteralUndefined();
            _e2 = e2 ?? new SNLiteralUndefined();
            _pattern = pat;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            var s1 = _e1.TryCompose(sta, LowestPrecendence);
            var s2 = _e2.TryCompose(sta, LowestPrecendence);
            return string.Format(_pattern, s1, s2);
        }
    }

    public class SNUnary : SNOperator
    {
        private SNExpression _e1;
        private string _pattern;

        public SNUnary(int p, Order o, string pat, SNExpression? e1) : base(p, o)
        {
            _e1 = e1 ?? new SNLiteralUndefined();
            _pattern = pat;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            var s1 = _e1.TryCompose(sta, LowestPrecendence);
            return string.Format(_pattern, s1);
        }
    }

    public class SNCheckTarget : SNOperator
    {
        private SNExpression _e1;

        public SNCheckTarget(SNExpression e) : base(18, Order.LeftToRight)
        {
            _e1 = e;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            var s = _e1.TryCompose(sta, LowestPrecendence);
            if (s.StartsWith('/'))
                s = $"getTarget({s})";
            return s;
        }
    }

    public class SNMemberAccess : SNBinary
    {
        protected readonly string _pattern2 = "{0}.{1}";
        public SNMemberAccess(SNExpression? e1, SNExpression? e2) : base(18, Order.LeftToRight, "{0}[{1}]", e1, e2) { }

        public override string TryComposeRaw(StatementCollection sta)
        {
            var flag = _e2 is SNLiteral e2l && e2l.IsStringLiteral;
            var s1 = _e1.TryCompose(sta, LowestPrecendence);
            var s2 = flag ? ((SNLiteral) _e2).GetRawString() : _e2.TryCompose(sta, LowestPrecendence);
            var flag2 = string.IsNullOrEmpty(s1) || s1 == "undefined";
            if (flag2) {
                return flag ? s2 : $"this[{s2}]";
            }
            var p = flag ? _pattern2 : _pattern;
            return string.Format(p, s1, s2);
        }
    }

    public static class OprUtils
    {
        // reference: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Operator_Precedence

        public static SNOperator Grouping(SNExpression e) { return new SNUnary(19, SNOperator.Order.NotAcceptable, "( {0} )", e); }
        // public static SNOperator MemberAccess(SNExpression? e1, SNExpression? e2) { return new SNBinary(18, SNOperator.Order.LeftToRight, "{0} . {1}", e1, e2); }
        // public static SNOperator ComputedMemberAccess(SNExpression? e1, SNExpression? e2) { return new SNBinary(18, SNOperator.Order.LeftToRight, "{0} [ {1} ]", e1, e2); }
        public static SNOperator New(SNExpression? e1, SNExpression? e2) { return new SNBinary(18, SNOperator.Order.NotAcceptable, "new {0} ( {1} )", e1, e2); }
        public static SNOperator FunctionCall(SNExpression? e1, SNExpression? e2) { return new SNBinary(18, SNOperator.Order.LeftToRight, "{0} ( {1} )", e1, e2); }
        public static SNStatement FunctionCall2(SNExpression? e1, SNExpression? e2) { return new SNToStatement(new SNBinary(18, SNOperator.Order.LeftToRight, "{0} ( {1} )", e1, e2)); }
        public static SNOperator Optionalchaining(SNExpression e) { return new SNUnary(18, SNOperator.Order.LeftToRight, "?.", e); }
        public static SNOperator NewWithoutArgs(SNExpression e) { return new SNUnary(17, SNOperator.Order.RightToLeft, "new {0}", e); }
        public static SNOperator PostfixIncrement(SNExpression e) { return new SNUnary(16, SNOperator.Order.NotAcceptable, "{0} ++", e); }
        public static SNOperator PostfixDecrement(SNExpression e) { return new SNUnary(16, SNOperator.Order.NotAcceptable, "{0} --", e); }
        public static SNOperator LogicalNot(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "! {0}", e); }
        public static SNOperator BitwiseNot(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "~ {0}", e); }
        public static SNOperator UnaryPlus(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "+ {0}", e); }
        public static SNOperator UnaryNegation(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "- {0}", e); }
        public static SNOperator PrefixIncrement(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "++ {0}", e); }
        public static SNOperator PrefixDecrement(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "-- {0}", e); }
        public static SNOperator TypeOf(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "typeof {0}", e); }
        public static SNOperator Void(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "void {0}", e); }
        public static SNOperator Delete(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "delete {0}", e); }
        public static SNOperator Await(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "await {0}", e); }
        public static SNOperator Exponentiation(SNExpression? e1, SNExpression? e2) { return new SNBinary(14, SNOperator.Order.RightToLeft, "{0} ** {1}", e1, e2); }
        public static SNOperator Multiplication(SNExpression? e1, SNExpression? e2) { return new SNBinary(13, SNOperator.Order.LeftToRight, "{0} * {1}", e1, e2); }
        public static SNOperator Division(SNExpression? e1, SNExpression? e2) { return new SNBinary(13, SNOperator.Order.LeftToRight, "{0} / {1}", e1, e2); }
        public static SNOperator Remainder(SNExpression? e1, SNExpression? e2) { return new SNBinary(13, SNOperator.Order.LeftToRight, "{0} % {1}", e1, e2); }
        public static SNOperator Addition(SNExpression? e1, SNExpression? e2) { return new SNBinary(12, SNOperator.Order.LeftToRight, "{0} + {1}", e1, e2); }
        public static SNOperator Subtraction(SNExpression? e1, SNExpression? e2) { return new SNBinary(12, SNOperator.Order.LeftToRight, "{0} - {1}", e1, e2); }
        public static SNOperator BitwiseLeftShift(SNExpression? e1, SNExpression? e2) { return new SNBinary(11, SNOperator.Order.LeftToRight, "{0} << {1}", e1, e2); }
        public static SNOperator BitwiseRightShift(SNExpression? e1, SNExpression? e2) { return new SNBinary(11, SNOperator.Order.LeftToRight, "{0} >> {1}", e1, e2); }
        public static SNOperator BitwiseUnsignedRightShift(SNExpression? e1, SNExpression? e2) { return new SNBinary(11, SNOperator.Order.LeftToRight, "{0} >>> {1}", e1, e2); }
        public static SNOperator LessThan(SNExpression? e1, SNExpression? e2) { return new SNBinary(10, SNOperator.Order.LeftToRight, "{0} < {1}", e1, e2); }
        public static SNOperator LessThanOrEqual(SNExpression? e1, SNExpression? e2) { return new SNBinary(10, SNOperator.Order.LeftToRight, "{0} <= {1}", e1, e2); }
        public static SNOperator GreaterThan(SNExpression? e1, SNExpression? e2) { return new SNBinary(10, SNOperator.Order.LeftToRight, "{0} > {1}", e1, e2); }
        public static SNOperator GreaterThanOrEqual(SNExpression? e1, SNExpression? e2) { return new SNBinary(10, SNOperator.Order.LeftToRight, "{0} >= {1}", e1, e2); }
        public static SNOperator In(SNExpression? e1, SNExpression? e2) { return new SNBinary(10, SNOperator.Order.LeftToRight, "{0} in {1}", e1, e2); }
        public static SNOperator InstanceOf(SNExpression? e1, SNExpression? e2) { return new SNBinary(10, SNOperator.Order.LeftToRight, "{0} instanceof {1}", e1, e2); }
        public static SNOperator Equality(SNExpression? e1, SNExpression? e2) { return new SNBinary(9, SNOperator.Order.LeftToRight, "{0} == {1}", e1, e2); }
        public static SNOperator Inequality(SNExpression? e1, SNExpression? e2) { return new SNBinary(9, SNOperator.Order.LeftToRight, "{0} != {1}", e1, e2); }
        public static SNOperator StrictEquality(SNExpression? e1, SNExpression? e2) { return new SNBinary(9, SNOperator.Order.LeftToRight, "{0} === {1}", e1, e2); }
        public static SNOperator StrictInequality(SNExpression? e1, SNExpression? e2) { return new SNBinary(9, SNOperator.Order.LeftToRight, "{0} !== {1}", e1, e2); }
        public static SNOperator BitwiseAnd(SNExpression? e1, SNExpression? e2) { return new SNBinary(8, SNOperator.Order.LeftToRight, "{0} & {1}", e1, e2); }
        public static SNOperator BitwiseXor(SNExpression? e1, SNExpression? e2) { return new SNBinary(7, SNOperator.Order.LeftToRight, "{0} ^ {1}", e1, e2); }
        public static SNOperator BitwiseOr(SNExpression? e1, SNExpression? e2) { return new SNBinary(6, SNOperator.Order.LeftToRight, "{0} | {1}", e1, e2); }
        public static SNOperator LogicalAnd(SNExpression? e1, SNExpression? e2) { return new SNBinary(5, SNOperator.Order.LeftToRight, "{0} && {1}", e1, e2); }
        public static SNOperator LogicalOr(SNExpression? e1, SNExpression? e2) { return new SNBinary(4, SNOperator.Order.LeftToRight, "{0} || {1}", e1, e2); }
        public static SNOperator Nullishcoalescingoperator(SNExpression? e1, SNExpression? e2) { return new SNBinary(4, SNOperator.Order.LeftToRight, "{0} ?? {1}", e1, e2); }
        // public static SNOperator Conditional (SNExpression? e1, SNExpression? e2) { return new SNBinary(3, SNOperator.Order.RightToLeft, "{0} ? {1} : {2}", e1, e2); }
        public static SNOperator ValAssign(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} = {1}", e1, e2); }
        public static SNOperator ValAssignAdd(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} += {1}", e1, e2); }
        public static SNOperator ValAssignSubtract(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} -= {1}", e1, e2); }
        public static SNOperator ValAssignExp(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} **= {1}", e1, e2); }
        public static SNOperator ValAssignMult(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} *= {1}", e1, e2); }
        public static SNOperator ValAssignDivide(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} /= {1}", e1, e2); }
        public static SNOperator ValAssignModulo(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} %= {1}", e1, e2); }
        public static SNOperator ValAssignShiftLeft(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} <<= {1}", e1, e2); }
        public static SNOperator ValAssignShiftRight(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} >>= {1}", e1, e2); }
        public static SNOperator ValAssignShiftRight2(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} >>>= {1}", e1, e2); }
        public static SNOperator ValAssignAnd(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} &= {1}", e1, e2); }
        public static SNOperator ValAssignXor(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} ^= {1}", e1, e2); }
        public static SNOperator ValAssignOr(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} |= {1}", e1, e2); }
        public static SNOperator ValAssignLogicalAnd(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} &&= {1}", e1, e2); }
        public static SNOperator ValAssignLogicalOr(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} ||= {1}", e1, e2); }
        public static SNOperator ValAssignNullCoalese(SNExpression? e1, SNExpression? e2) { return new SNBinary(2, SNOperator.Order.RightToLeft, "{0} ??= {1}", e1, e2); }
        public static SNOperator Yield(SNExpression e) { return new SNUnary(2, SNOperator.Order.RightToLeft, "yield {0}", e); }
        public static SNOperator Yield2(SNExpression e) { return new SNUnary(2, SNOperator.Order.RightToLeft, "yield* {0}", e); }
        public static SNOperator CommaSequence(SNExpression ? e1, SNExpression ? e2) { return new SNBinary(1, SNOperator.Order.LeftToRight, "{0} , {1}", e1, e2); }

    }


    public class NodeFunctionBody : SNExpression
    {
        public StatementCollection Body;
        public static readonly string NoIndentMark = "/*@([{@%@)]}@*/";
        public NodeFunctionBody(InstructionBase inst, StatementCollection body) : base(inst)
        {
            Body = body;
        }

        public override bool TryGetValue(Func<Value?, Value?>? parse, out Value? ret)
        {
            ret = _v;
            return true;
        }

        public override void TryCompose(StatementCollection sta, bool compileBranches = false)
        {
            StringBuilder sb = new();
            var (name, args) = InstructionUtils.GetNameAndArguments(Instruction);
            var head = $"function({string.Join(", ", args.ToArray())})\n";
            // sb.Append(NoIndentMark);
            sb.Append(head);
            // sb.Append(NoIndentMark);
            sb.Append("{\n");
            Body.Compile(sb, 1, 1, true, false);
            // sb.Append(NoIndentMark);
            sb.Append("}");
            Code = sb.ToString();
        }
    }


    public abstract class SNStatement : SyntaxNode
    {
        public SNStatement() : base() { }
        public override string TryCompose(StatementCollection sta, bool compileBranches = false) { return base.TryCompose(sta, compileBranches); }
    }

    public class SNToStatement : SNStatement
    {
        public SNExpression Exp { get; }
        public SNToStatement(SNExpression exp) { Exp = exp; }
        public override string TryCompose(StatementCollection sta, bool compileBranches = false) { return Exp.TryCompose(sta); }

        public override string TryComposeRaw(StatementCollection sta)
        {
            throw new NotImplementedException();
        }
    }

    public class SNForConvenience : SNStatement
    {
        public string String { get; set; }
        public SNForConvenience(string str)
        {
            String = str;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return String;
        }
    }

    public class SNValAssign : SNStatement
    {
        private SNExpression _e1, _e2;
        public SNValAssign(SNExpression? e1, SNExpression? e2)
        {
            _e1 = e1 ?? new SNLiteralUndefined();
            _e2 = e2 ?? new SNLiteralUndefined();
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return $"{_e1.TryCompose(sta)} = {_e2.TryCompose(sta)}";
        }
    }

    public class SNControl : SNExpression
    {
        public string Keyword { get; set; }
        public SNExpression Node { get; set; }

        public SNControl(string keyWord, SNExpression exp) : base()
        {
            Keyword = keyWord;
            Node = exp;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return Keyword + " " + Node.TryCompose(sta);
        }

    }


    public abstract class NodeControl : SNStatement
    {
        public NodeControl(InstructionBase inst) : base(inst) { }
        public override void TryCompose(StatementCollection sta, bool compileBranches = false) { base.TryCompose(sta, compileBranches); }

        public abstract void TryCompile2(StatementCollection sta, StringBuilder sb, int indent = 0, int dIndent = 4, bool compileSubCollections = true);
    }

    public class NodeIncludeFunction : NodeControl
    { 
        public readonly SyntaxNode n;
        public NodeIncludeFunction(SyntaxNode body) : base(body.Instruction)
        {
            n = body;
        }
        public override void TryCompose(StatementCollection sta, bool compileBranches = false) { n.TryCompose(sta, compileBranches); Code = n.Code; } 

        // using brute force ways to do so
        public override void TryCompile2(StatementCollection sta, StringBuilder sb, int indent = 0, int dIndent = 4, bool compileSubCollections = true)
        {
            TryCompose(sta, true);
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

        public override void TryCompose(StatementCollection sta, bool compileBranches = false) { throw new NotImplementedException(); }

        public override void TryCompile2(StatementCollection sta, StringBuilder sb, int indent = 0, int dIndent = 4, bool compileSubCollections = true)
        {
            var (name, args) = InstructionUtils.GetNameAndArguments(Instruction);
            var head = $"function {name}({string.Join(", ", args.ToArray())})\n";
            sb.Append(head.ToStringWithIndent(indent));
            sb.Append("{\n".ToStringWithIndent(indent));
            Body.Compile(sb, indent + dIndent, dIndent, compileSubCollections, false, sta);
            sb.Append("}\n".ToStringWithIndent(indent));
        }
    }

    public class NodeCase: NodeControl
    {

        public SNExpression? Condition;
        public StatementCollection Unbranch;
        public StatementCollection Branch;

        public NodeCase(
            InstructionBase inst,
            SNExpression? condition,
            StatementCollection unbranch, 
            StatementCollection branch
            ) : base(inst)
        {
            Condition = condition;
            Unbranch = unbranch;
            Branch = branch;
        }

        public static bool IsElseIfBranch(StatementCollection sta, out NodeCase? c)
        {
            var ret = false;
            c = null;
            if (sta.IsEmpty())
                return ret;
            else if (sta.Nodes.Count() == 1)
            {
                ret = sta.Nodes.ElementAt(0) is NodeCase;
                if (ret)
                    c = (NodeCase) sta.Nodes.ElementAt(0);
            }
            else
            {
                var cases = 0;
                var noncases = 0;
                foreach (var n in sta.Nodes)
                {
                    if (n is NodeCase)
                    {
                        c = (NodeCase) n;
                        ++cases;
                    }
                    else
                    {
                        // TODO may need fancier codelessness judgement
                        // n.TryCompile(sta);
                        // if (!string.IsNullOrEmpty(n.Code))
                        ++noncases;
                    }
                    if (cases > 1 || noncases > 0)
                    {
                        ret = false;
                        break;
                    }
                }
                return cases == 1 && noncases == 0;
            }
            return ret;
        }

        public override void TryCompile2(StatementCollection sta, StringBuilder sb, int indent = 0, int dIndent = 4, bool compileSubCollections = true)
        {
            TryCompile2(sta, sb, indent, dIndent, compileSubCollections, false);
        }

        public void TryCompile2(StatementCollection sta, StringBuilder sb, int indent, int dIndent, bool compileSubCollections, bool elseIfBranch)
        {
            var tmp = "[[null condition]]";
            if (Condition != null)
            {
                Condition.TryCompose(sta, true); // TODO turn the condition to real condition
                tmp = Condition.Code!;
            }

            var b1 = Unbranch;
            var b2 = Branch;
            var ei1 = IsElseIfBranch(b1, out var nc1); 
            var ei2 = IsElseIfBranch(b2, out var nc2); // these are ensured to compile

            if (Instruction.Type == InstructionType.BranchIfTrue)
                tmp = InstructionUtils.ReverseCondition(tmp);

            if (ei1 ^ ei2)
            {
                if (ei1)
                {
                    var b3 = b2;
                    b2 = b1;
                    b1 = b3;
                    var nc3 = nc2;
                    nc2 = nc1;
                    nc1 = nc3;
                    tmp = InstructionUtils.ReverseCondition(tmp);
                }
                var ifBranch = elseIfBranch ? $"if ({tmp})\n" : $"if ({tmp})\n".ToStringWithIndent(indent);
                sb.Append(ifBranch);
                sb.Append("{\n".ToStringWithIndent(indent));
                b1.Compile(sb, indent + dIndent, dIndent, compileSubCollections, false, sta);
                sb.Append("}\n".ToStringWithIndent(indent));
                sb.Append("else ".ToStringWithIndent(indent));
                nc2!.TryCompile2(sta, sb, indent, dIndent, compileSubCollections, true);
            }
            else
            {
                var ifBranch2 = elseIfBranch ? $"if ({tmp})\n" : $"if ({tmp})\n".ToStringWithIndent(indent);
                sb.Append(ifBranch2);
                sb.Append("{\n".ToStringWithIndent(indent));
                b1.Compile(sb, indent + dIndent, dIndent, compileSubCollections, false, sta);
                sb.Append("}\n".ToStringWithIndent(indent));
                if (!b2.IsEmpty())
                {
                    sb.Append("else\n".ToStringWithIndent(indent));
                    sb.Append("{\n".ToStringWithIndent(indent));
                    b2.Compile(sb, indent + dIndent, dIndent, compileSubCollections, false, sta);
                    sb.Append("}\n".ToStringWithIndent(indent));
                }
            }
        }
    }

    public class NodeLoop : NodeControl
    {

        public SNExpression? Condition;
        public StatementCollection Maintain;
        public StatementCollection Branch;

        public NodeLoop(
            InstructionBase inst,
            SNExpression? condition,
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
                Condition.TryCompose(sta, true); // TODO turn the condition to real condition
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
            Maintain.Compile(sb, indent + dIndent, dIndent, compileSubCollections, true, sta);
            sb.Append("}\n".ToStringWithIndent(indent));
            sb.Append($"while ({tmp})\n".ToStringWithIndent(indent));
            sb.Append("{\n".ToStringWithIndent(indent));
            Branch.Compile(sb, indent + dIndent, dIndent, compileSubCollections, false, sta);
            sb.Append("}\n".ToStringWithIndent(indent));
        }
    }

}
