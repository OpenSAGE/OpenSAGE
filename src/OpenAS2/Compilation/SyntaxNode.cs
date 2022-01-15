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

        /// <summary>
        /// do not need to process indent, labels etc.
        /// return if ; is necessary
        /// </summary>
        /// <param name="sta"></param>
        /// <param name="sb"></param>
        public virtual bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            sb.Append(TryComposeRaw(sta));
            return true;
        }
        
    }


    public abstract class SNExpression : SyntaxNode
    {
        public virtual int LowestPrecendence => 24;
        public virtual bool doNotDeleteAfterPopped => false;

        private static Dictionary<InstructionType, int> NIE = new();

        public SNExpression() : base() { }

        public override abstract string TryComposeRaw(StatementCollection sta);

        public virtual string TryComposeWithPrecendence(StatementCollection sta, int targetPrec = 24)
        {
            var s = TryComposeRaw(sta);
            if (targetPrec > LowestPrecendence)
            {
                s = $"({s})";
            }
            return s;
        }

        public override bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            var s = TryComposeWithPrecendence(sta, 24);
            sb.Append(s);
            return true;
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
            return $"[[enumerate node: {Node.TryComposeRaw(sta)}]]";
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

        public string GetRawString() { return Value != null ? Value.String : string.Empty; }
        
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

        public static SNExpression Check(SNExpression sn) { return Check(sn, out var _); }

        public static SNExpression Check(SNExpression sn, out bool flag)
        {
            flag = sn is SNLiteral snl && snl.IsStringLiteral;
            if (flag)
                return new SNNominator(((SNLiteral) sn).GetRawString());
            else
                return sn;
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
            StringBuilder sb = new();
            TryCompose(sta, sb);
            return sb.ToString();
        }

        public override bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            for (int i = 0; i < Expressions.Count; ++i)
            {
                var node = Expressions[i];
                if (node is SNLiteralUndefined)
                {
                    sb.Append($"__args__[{i}]");
                }
                else
                {
                    var flag = node is SNArray;
                    if (flag)
                        sb.Append('[');
                    node.TryCompose(sta, sb);
                    if (flag)
                        sb.Append(']');
                }
                if (i != Expressions.Count - 1)
                    sb.Append(", ");
            }
            return true;
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
            return $"{_c.TryComposeWithPrecendence(sta, LowestPrecendence)} ? {_t.TryComposeWithPrecendence(sta, LowestPrecendence)} : {_f.TryComposeWithPrecendence(sta, LowestPrecendence)}"; 
        }
    }

    public class SNBinary : SNOperator
    {
        public readonly SNExpression E1, E2;
        public readonly string Pattern;

        public SNBinary(int p, Order o, string pat, SNExpression? e1, SNExpression? e2, bool checkPrecendence = true) : base(checkPrecendence ? p : 24, o) // seems okay
        {
            E1 = e1 ?? new SNLiteralUndefined();
            E2 = e2 ?? new SNLiteralUndefined();
            Pattern = pat;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            var s1 = E1.TryComposeWithPrecendence(sta, LowestPrecendence);
            var s2 = E2.TryComposeWithPrecendence(sta, LowestPrecendence);
            return string.Format(Pattern, s1, s2);
        }
    }

    public class SNUnary : SNOperator
    {
        public SNExpression E { get; protected set; }
        public string Pattern { get; protected set; }

        public SNUnary(int p, Order o, string pat, SNExpression? e1) : base(p, o)
        {
            E = e1 ?? new SNLiteralUndefined();
            Pattern = pat;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            var s1 = E.TryComposeWithPrecendence(sta, LowestPrecendence);
            return string.Format(Pattern, s1);
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
            var s = _e1.TryComposeWithPrecendence(sta, LowestPrecendence);
            if (s.StartsWith('/'))
                s = $"getTarget({s})";
            return s;
        }
    }

    public class SNMemberAccess : SNBinary
    {
        protected readonly string _pattern2 = "{0}.{1}";
        public SNMemberAccess(SNExpression? e1, SNExpression? e2) : base(18, Order.LeftToRight, "{0}[{1}]", e1, e2, false) { }

        public override string TryComposeRaw(StatementCollection sta)
        {
            var flag = E2 is SNLiteral e2l && e2l.IsStringLiteral;
            var s1 = E1.TryComposeWithPrecendence(sta, LowestPrecendence);
            var s2 = flag ? ((SNLiteral) E2).GetRawString() : E2.TryComposeWithPrecendence(sta, LowestPrecendence);
            var flag2 = string.IsNullOrEmpty(s1) || s1 == "undefined";
            if (flag2) {
                return flag ? s2 : $"this[{s2}]";
            }
            var p = flag ? _pattern2 : Pattern;
            return string.Format(p, s1, s2);
        }
    }

    public static class OprUtils
    {
        // defined
        public static SNOperator Cast(SNExpression? e1, SNExpression? e2) { return new SNBinary(24, SNOperator.Order.NotAcceptable, "{0} as {1}", e1, e2, false); }
        public static SNOperator KeyAssign(SNExpression? e1, SNExpression? e2) { return new SNBinary(24, SNOperator.Order.NotAcceptable, "{0}: {1}", e1, e2, false); }


        // reference: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Operator_Precedence

        public static SNOperator Grouping(SNExpression e) { return new SNUnary(19, SNOperator.Order.NotAcceptable, "( {0} )", e); }
        // public static SNOperator MemberAccess(SNExpression? e1, SNExpression? e2) { return new SNBinary(18, SNOperator.Order.LeftToRight, "{0} . {1}", e1, e2); }
        // public static SNOperator ComputedMemberAccess(SNExpression? e1, SNExpression? e2) { return new SNBinary(18, SNOperator.Order.LeftToRight, "{0} [ {1} ]", e1, e2); }
        public static SNOperator New(SNExpression? e1, SNExpression? e2) { return new SNBinary(18, SNOperator.Order.NotAcceptable, "new {0}( {1} )", e1, e2, false); }
        public static SNOperator FunctionCall(SNExpression? e1, SNExpression? e2) { return new SNBinary(18, SNOperator.Order.LeftToRight, "{0}({1})", e1, e2, false); }
        public static SNStatement FunctionCall2(SNExpression? e1, SNExpression? e2) { return new SNToStatement(new SNBinary(18, SNOperator.Order.LeftToRight, "{0} ({1})", e1, e2, false)); }
        public static SNOperator Optionalchaining(SNExpression e) { return new SNUnary(18, SNOperator.Order.LeftToRight, "?.", e); }
        public static SNOperator NewWithoutArgs(SNExpression e) { return new SNUnary(17, SNOperator.Order.RightToLeft, "new {0}", e); }
        public static SNOperator PostfixIncrement(SNExpression e) { return new SNUnary(16, SNOperator.Order.NotAcceptable, "{0}++", e); }
        public static SNOperator PostfixDecrement(SNExpression e) { return new SNUnary(16, SNOperator.Order.NotAcceptable, "{0}--", e); }
        public static SNExpression LogicalNot(SNExpression e) {
            if (e is SNUnary su && su.Pattern == "!{0}")
                return su.E;
            return new SNUnary(15, SNOperator.Order.RightToLeft, "!{0}", e);
        }
        public static SNOperator BitwiseNot(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "~{0}", e); }
        public static SNOperator UnaryPlus(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "+{0}", e); }
        public static SNOperator UnaryNegation(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "-{0}", e); }
        public static SNOperator PrefixIncrement(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "++{0}", e); }
        public static SNOperator PrefixDecrement(SNExpression e) { return new SNUnary(15, SNOperator.Order.RightToLeft, "--{0}", e); }
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

    public abstract class SNStatement : SyntaxNode
    {
        public SNStatement() : base() { }
    }

    public class SNToStatement : SNStatement
    {
        public SNExpression Exp { get; }
        public SNToStatement(SNExpression exp) { Exp = exp; }
        public override bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            return Exp.TryCompose(sta, sb);
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return Exp.TryComposeRaw(sta);
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
        private bool _varCheck;
        public SNValAssign(SNExpression? e1, SNExpression? e2, bool varCheck = false)
        {
            _e1 = e1 ?? new SNLiteralUndefined();
            _e1 = SNNominator.Check(_e1);
            _e2 = e2 ?? new SNLiteralUndefined();
            _varCheck = varCheck; // TODO
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return $"{_e1.TryComposeRaw(sta)} = {_e2.TryComposeRaw(sta)}";
        }

        public override bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            _e1.TryCompose(sta, sb);
            sb.Append(" = ");
            _e2.TryCompose(sta, sb);
            return true;
        }
    }

    public class SNKeyWord : SNStatement
    {
        public string Keyword { get; set; }
        public SNExpression Node { get; set; }

        public SNKeyWord(string keyWord, SNExpression exp) : base()
        {
            Keyword = keyWord;
            Node = exp;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return Keyword + " " + Node.TryComposeWithPrecendence(sta);
        }

        public override bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            sb.Append(Keyword + " ");
            Node.TryCompose(sta, sb);
            return true;
        }

    }

    public class SNMarkNomination : SNStatement
    {
        public SNExpression Node { get; set; }

        public SNMarkNomination(SNExpression exp) : base()
        {
            Node = exp;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            throw new InvalidOperationException();
        }

        public override bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            // throw new InvalidOperationException();
            return true;
        }

    }


    public abstract class SNControl : SNStatement
    {

        public SNControl() : base() { }
        public override string TryComposeRaw(StatementCollection sta)
        {
            StringBuilder sb = new();
            TryCompose(sta, sb);
            return sb.ToString();
        }

    }

    public class SNDefineFunction : SNControl
    {
        public StatementCollection Body;
        public RawInstruction Instruction { get; }
        private readonly (string, List<string>) _info;
        public SNDefineFunction(RawInstruction inst, StatementCollection body) : base()
        {
            Body = body;
            Instruction = inst;
            _info = InstructionUtils.GetNameAndArguments(Instruction);
        }
        
        public override bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            var (name, args) = _info;
            var head = $"function {name}({string.Join(", ", args.ToArray())})\n";
            sb.Append(head);
            sta.CallSubCollection(Body, sb);
            return false;
        }


    }

    public class SNFunctionBody : SNExpression
    {
        public StatementCollection Body;
        public RawInstruction Instruction { get; }
        private readonly (string, List<string>) _info;
        public SNFunctionBody(RawInstruction inst, StatementCollection body) : base()
        {
            Body = body;
            Instruction = inst;
            _info = InstructionUtils.GetNameAndArguments(Instruction);
        }
        public override string TryComposeRaw(StatementCollection sta)
        {
            StringBuilder sb = new();
            TryCompose(sta, sb);
            return sb.ToString();
        }

        public override bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            var (_, args) = _info;
            var head = $"function({string.Join(", ", args.ToArray())})\n";
            sb.Append(head);
            sta.CallSubCollection(Body, sb);
            return false;
        }

    }

    public class SNControlCase: SNControl
    {

        public SNExpression? Condition;
        public StatementCollection Unbranch;
        public StatementCollection Branch;

        public SNControlCase(
            SNExpression? condition,
            StatementCollection unbranch, 
            StatementCollection branch
            ) : base()
        {
            Condition = condition;
            Unbranch = unbranch;
            Branch = branch;
        }

        public static bool IsElseIfBranch(StatementCollection sta, out SNControlCase? c)
        {
            var ret = false;
            c = null;
            if (sta.IsEmpty())
                return ret;
            else if (sta.Nodes.Count() == 1)
            {
                ret = sta.Nodes.ElementAt(0) is SNControlCase;
                if (ret)
                    c = (SNControlCase) sta.Nodes.ElementAt(0);
            }
            else
            {
                var cases = 0;
                var noncases = 0;
                foreach (var n in sta.Nodes)
                {
                    if (n is SNControlCase)
                    {
                        c = (SNControlCase) n;
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

        public override bool TryCompose(StatementCollection sta, StringBuilder sb) { return TryCompose(sta, sb, false); }
        public bool TryCompose(StatementCollection sta, StringBuilder sb, bool elseIfBranch = false)
        {
            var tmp = Condition != null ? Condition.TryComposeRaw(sta) : "[[null condition]]";

            var b1 = Unbranch;
            var b2 = Branch;
            var ei1 = IsElseIfBranch(b1, out var nc1); 
            var ei2 = IsElseIfBranch(b2, out var nc2); // these are ensured to compile

            // do not need to reverse since it is already done in node pool
            var ifBranch = !elseIfBranch ? $"if ({tmp})\n" : ("\n" + $"else if ({tmp})\n".ToStringWithIndent(sta._currentIndent));
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
                sb.Append(ifBranch);
                sta.CallSubCollection(b1, sb);
                nc2!.TryCompose(sta, sb, true);
            }
            else
            {
                sb.Append(ifBranch);
                sta.CallSubCollection(b1, sb);
                if (!b2.IsEmpty())
                {
                    sb.Append("\n" + "else\n".ToStringWithIndent(sta._currentIndent));
                    sta.CallSubCollection(b1, sb);
                }
            }
            return false;
        }
    }

    public class SNControlLoop : SNControl
    {

        public SNExpression? Condition;
        public StatementCollection Maintain;
        public StatementCollection Branch;

        public SNControlLoop(
            SNExpression? condition,
            StatementCollection maintain,
            StatementCollection branch
            ) : base()
        {
            Condition = condition;
            Maintain = maintain;
            Branch = branch;
        }
        // TODO uncertain, need to check
        public override bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            var tmp = Condition != null ? Condition.TryComposeRaw(sta) : "[[null condition]]";
            sta.CallSubCollection(Maintain, sb, prefix: " // loop maintain condition");
            sb.Append($"while ({tmp})\n".ToStringWithIndent(sta._currentIndent));
            sta.CallSubCollection(Branch, sb);
            return false;
        }
    }

}
