using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenAS2.Base;
using Value = OpenAS2.Runtime.Value;

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
        public const int MaxPrecendence = 24;
        public const int MinPrecendence = -1;
        public virtual int LowestPrecendenceAllowed => MaxPrecendence;
        public enum Order
        {
            NotAcceptable = 0,
            LeftToRight = 1,
            RightToLeft = 2
        }
        public virtual Order CalcOrder => Order.NotAcceptable;

        public virtual bool MutableWhenEvaluated => true;
        public virtual bool DoNotDeleteAfterPopped => false;

        private static Dictionary<InstructionType, int> NIE = new();

        public SNExpression() : base() { }

        public override abstract string TryComposeRaw(StatementCollection sta);

        public virtual string TryComposeWithPrecendence(StatementCollection sta, int targetLPA = MaxPrecendence, bool orderIssue = false)
        {
            var s = TryComposeRaw(sta);
            if (targetLPA > LowestPrecendenceAllowed || (orderIssue && targetLPA == LowestPrecendenceAllowed))
            {
                s = $"({s})";
            }
            return s;
        }

        public override bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            var s = TryComposeWithPrecendence(sta, MinPrecendence);
            sb.Append(s);
            return true;
        }
    }

    // vars

    public class SNEnumerate : SNExpression
    {
        public override bool DoNotDeleteAfterPopped => true;
        public override bool MutableWhenEvaluated => Node.MutableWhenEvaluated;
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
        public override bool MutableWhenEvaluated => false;

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

    public class SNLiteralUndefined : SNLiteral
    {
        public SNLiteralUndefined() : base(null) { }
        public override bool MutableWhenEvaluated => false;

        public override string TryComposeRaw(StatementCollection sta)
        {
            return "undefined";
        }
    }

    public class SNNominator : SNExpression
    {
        public string Name { get; set; }
        public override bool MutableWhenEvaluated => false;
        public SNNominator(string name) : base()
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
        public override bool MutableWhenEvaluated => _mwe;
        private bool _mwe;

        public SNArray(IList<SNExpression?> exprs) : base()
        {
            Expressions = exprs.Select(x => x ?? new SNLiteralUndefined()).ToList().AsReadOnly();
            _mwe = false;
            foreach (var exp in Expressions)
                if (exp.MutableWhenEvaluated)
                {
                    _mwe = true;
                    break;
                }
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
        public override bool MutableWhenEvaluated => _mwe;
        protected bool _mwe;

        protected bool _forceIgnorePrecendence = false;

        public override int LowestPrecendenceAllowed => _precendence;
        protected readonly int _precendence;

        public override Order CalcOrder => _order;
        protected readonly Order _order;

        public SNOperator(int precendence, Order order, bool mwe)
        {
            _precendence = precendence;
            _order = order;
            _mwe = mwe;
        }

        // definitions

    }

    public class SNTernary : SNOperator
    {
        private SNExpression _c, _t, _f;
        public SNTernary(SNExpression? c, SNExpression? t, SNExpression? f) :
            base(4, Order.RightToLeft, false)
        {
            _c = c ?? new SNLiteralUndefined();
            _t = t ?? new SNLiteralUndefined();
            _f = f ?? new SNLiteralUndefined();
            _mwe = c!.MutableWhenEvaluated || t!.MutableWhenEvaluated || f!.MutableWhenEvaluated;
        }

        public static SNOperator Check(SNExpression? c, SNExpression? t, SNExpression? f)
        {
            if (c is SNBinary cb)
            {
                if (c.Equals(f))
                {
                    return OprUtils.LogicalOr(c, t);
                }
                else if (f != null && c.Equals(OprUtils.LogicalNot(f)))
                {
                    return OprUtils.LogicalAnd(c, OprUtils.LogicalNot(f));
                }
                else
                    return new SNTernary(c, t, f);
            }
            else
                return new SNTernary(c, t, f);
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return $"{_c.TryComposeWithPrecendence(sta, LowestPrecendenceAllowed)} ? {_t.TryComposeWithPrecendence(sta, LowestPrecendenceAllowed, true)} : {_f.TryComposeWithPrecendence(sta, LowestPrecendenceAllowed, true)}"; 
        }
    }

    public class SNBinary : SNOperator
    {
        public readonly SNExpression E1, E2;
        public readonly string Pattern;

        public SNBinary(int p, Order o, string pat, SNExpression? e1, SNExpression? e2) : base(p, o,
            false) // seems okay
        {
            E1 = e1 ?? new SNLiteralUndefined();
            E2 = e2 ?? new SNLiteralUndefined();
            Pattern = pat;
            _mwe = e1!.MutableWhenEvaluated || e2!.MutableWhenEvaluated;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            var k = Pattern == "{0} === {1}";
            if (k)
            {
                var a = 1;
            }
            bool o1 = false, o2 = false;
            if (CalcOrder == Order.RightToLeft)
            {
                o1 = true;
            }
            else if (CalcOrder == Order.LeftToRight)
            {
                o2 = true;
            }
            var s1 = E1.TryComposeWithPrecendence(sta, _forceIgnorePrecendence ? MinPrecendence : LowestPrecendenceAllowed, o1);
            var s2 = E2.TryComposeWithPrecendence(sta, _forceIgnorePrecendence ? MinPrecendence : LowestPrecendenceAllowed, o2);
            return string.Format(Pattern, s1, s2);
        }

        public override bool Equals(object? obj)
        {
            if (obj is SNBinary o)
            {
                return Pattern.Equals(o.Pattern) && E1.Equals(o.E1) && E2.Equals(o.E2);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Pattern.GetHashCode() ^ E1.GetHashCode() ^ E2.GetHashCode();
        }
    }

    public class SNUnary : SNOperator
    {
        public SNExpression E { get; protected set; }
        public string Pattern { get; protected set; }


        public SNUnary(int p, Order o, string pat, SNExpression? e1) : base(p, o, e1 != null ? e1.MutableWhenEvaluated : false)
        {
            E = e1 ?? new SNLiteralUndefined();
            Pattern = pat;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            var s1 = E.TryComposeWithPrecendence(sta, _forceIgnorePrecendence ? MinPrecendence : LowestPrecendenceAllowed);
            return string.Format(Pattern, s1);
        }
    }

    public class SNCheckTarget : SNOperator
    {
        private SNExpression _e1;

        public SNCheckTarget(SNExpression e) : base(18, Order.LeftToRight, false)
        {
            _e1 = e;
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            var s = _e1.TryComposeWithPrecendence(sta, MinPrecendence);
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
            var flag = E2 is SNLiteral e2l && e2l.IsStringLiteral;
            var s1 = E1.TryComposeWithPrecendence(sta, LowestPrecendenceAllowed);
            var flag2 = string.IsNullOrEmpty(s1) || s1 == "undefined";
            var s2 = flag ?
                ((SNLiteral) E2).GetRawString() :
                E2.TryComposeWithPrecendence(sta, MinPrecendence);
            if (flag2) {
                return flag ? s2 : $"this[{s2}]";
            }
            var p = flag ? _pattern2 : Pattern;
            return string.Format(p, s1, s2);
        }
    }

    public class SNFunctionCall : SNBinary
    {
        public override bool MutableWhenEvaluated => true;
        public SNFunctionCall(SNExpression? e1, SNExpression? e2, bool isNew = false) : base(18, Order.LeftToRight, (isNew ? "new " : "") + "{0}({1})", e1, e2)
        {
            _mwe = true;
            _forceIgnorePrecendence = true;
        }
    }

    public static class OprUtils
    {
        // defined
        public static SNOperator Cast(SNExpression? e1, SNExpression? e2) { return new SNBinary(SNExpression.MaxPrecendence, SNOperator.Order.NotAcceptable, "{0} as {1}", e1, e2); }
        public static SNOperator KeyAssign(SNExpression? e1, SNExpression? e2) { return new SNBinary(SNExpression.MaxPrecendence, SNOperator.Order.NotAcceptable, "{0}: {1}", e1, e2); }


        // reference: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Operator_Precedence

        // public static SNOperator Grouping(SNExpression e) { return new SNUnary(19, SNOperator.Order.NotAcceptable, "( {0} )", e); }
        // public static SNOperator MemberAccess(SNExpression? e1, SNExpression? e2) { return new SNBinary(18, SNOperator.Order.LeftToRight, "{0} . {1}", e1, e2); }
        // public static SNOperator ComputedMemberAccess(SNExpression? e1, SNExpression? e2) { return new SNBinary(18, SNOperator.Order.LeftToRight, "{0} [ {1} ]", e1, e2); }
        public static SNOperator New(SNExpression? e1, SNExpression? e2) { return new SNFunctionCall(e1, e2, true); }
        public static SNOperator FunctionCall(SNExpression? e1, SNExpression? e2) { return new SNFunctionCall(e1, e2); }
        public static SNStatement FunctionCall2(SNExpression? e1, SNExpression? e2) { return new SNToStatement(new SNFunctionCall(e1, e2)); }
        public static SNOperator Optionalchaining(SNExpression e) { return new SNUnary(18, SNOperator.Order.LeftToRight, "?.", e); }
        public static SNOperator NewWithoutArgs(SNExpression e) { return new SNUnary(17, SNOperator.Order.RightToLeft, "new {0}", e); }
        public static SNOperator PostfixIncrement(SNExpression e) { return new SNUnary(16, SNOperator.Order.NotAcceptable, "{0}++", e); }
        public static SNOperator PostfixDecrement(SNExpression e) { return new SNUnary(16, SNOperator.Order.NotAcceptable, "{0}--", e); }
        public static SNExpression LogicalNot(SNExpression e) {
            if (e is SNUnary su && su.Pattern == "!{0}")
                return su.E;
            else if (e is SNBinary sb)
            {
                if (sb.Pattern == "{0} == {1}")
                    return Inequality(sb.E1, sb.E2);
                else if (sb.Pattern == "{0} != {1}")
                    return Equality(sb.E1, sb.E2);
                else if (sb.Pattern == "{0} === {1}")
                    return StrictInequality(sb.E1, sb.E2);
                else if (sb.Pattern == "{0} !== {1}")
                    return StrictEquality(sb.E1, sb.E2);
            }
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
        public static SNOperator BitwiseAnd(SNExpression? e1, SNExpression? e2) { return new SNBinary(6/*8*/, SNOperator.Order.LeftToRight, "{0} & {1}", e1, e2); }
        public static SNOperator BitwiseXor(SNExpression? e1, SNExpression? e2) { return new SNBinary(6/*7*/, SNOperator.Order.LeftToRight, "{0} ^ {1}", e1, e2); }
        public static SNOperator BitwiseOr(SNExpression? e1, SNExpression? e2) { return new SNBinary(6, SNOperator.Order.LeftToRight, "{0} | {1}", e1, e2); }
        public static SNOperator LogicalAnd(SNExpression? e1, SNExpression? e2) { return new SNBinary(4/*actually should be 5, but for readability*/, SNOperator.Order.LeftToRight, "{0} && {1}", e1, e2); }
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

    public class SNPlainCode : SNStatement
    {
        public string String { get; set; }
        public SNPlainCode(string str)
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
        public readonly SNExpression E1, E2;
        public bool NewVar;
        public SNValAssign(SNExpression? e1, SNExpression? e2, bool newVar = false)
        {
            E1 = e1 ?? new SNLiteralUndefined();
            E1 = SNNominator.Check(E1);
            E2 = e2 ?? new SNLiteralUndefined();
            NewVar = newVar; // TODO
        }

        public override string TryComposeRaw(StatementCollection sta)
        {
            return $"{(NewVar ? "var " : "")}{E1.TryComposeRaw(sta)} = {E2.TryComposeRaw(sta)}";
        }

        public override bool TryCompose(StatementCollection sta, StringBuilder sb)
        {
            if (NewVar)
                sb.Append("var ");
            E1.TryCompose(sta, sb);
            sb.Append(" = ");
            E2.TryCompose(sta, sb);
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
            _info = CompilationUtils.GetNameAndArguments(Instruction);
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
            _info = CompilationUtils.GetNameAndArguments(Instruction);
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

        public SNExpression Condition;
        public StatementCollection Unbranch;
        public StatementCollection Branch;

        public SNControlCase(
            SNExpression? condition,
            StatementCollection unbranch, 
            StatementCollection branch
            ) : base()
        {
            Condition = condition ?? new SNNominator("[[null condition]]");
            Unbranch = unbranch;
            Branch = branch;
            if (Unbranch.IsEmpty() && !Branch.IsEmpty())
            {
                Unbranch = branch;
                Branch = unbranch;
                Condition = OprUtils.LogicalNot(Condition);
            }
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
            var pattern = !elseIfBranch ? "if ({0})\n" : ("\n" + "else if ({0})\n".ToStringWithIndent(sta.CurrentIndent));
            var tmp = Condition;

            var b1 = Unbranch;
            var b2 = Branch;
            var ei1 = IsElseIfBranch(b1, out var nc1); 
            var ei2 = IsElseIfBranch(b2, out var nc2); // these are ensured to compile

            // do not need to reverse since it is already done in node pool
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
                    tmp = OprUtils.LogicalNot(tmp);
                }
                var flag = false;
                if (!b1.IsEmpty())
                {
                    flag = true;
                    sb.Append(string.Format(pattern, tmp.TryComposeRaw(sta)));
                    sta.CallSubCollection(b1, sb);
                }
                // sb.Append(string.Format(pattern, tmp.TryComposeRaw(sta)));
                // sta.CallSubCollection(b1, sb);
                nc2!.TryCompose(sta, sb, flag);
            }
            else
            {
                if (!b1.IsEmpty() || !b2.IsEmpty())
                {
                    sb.Append(string.Format(pattern, tmp.TryComposeRaw(sta)));
                    sta.CallSubCollection(b1, sb);
                }
                if (!b2.IsEmpty())
                {
                    sb.Append("\n" + "else\n".ToStringWithIndent(sta.CurrentIndent));
                    sta.CallSubCollection(b2, sb);
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
            if (!Maintain.IsEmpty())
            {
                sb.Append("// loop maintain condition\n");
                sta.CallSubCollection(Maintain, sb);
            }
            sb.Append("\n" + $"while ({tmp})\n".ToStringWithIndent(sta.CurrentIndent));
            sta.CallSubCollection(Branch, sb);
            return false;
        }
    }

}
