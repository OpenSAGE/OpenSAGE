using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAS2.Runtime.Library
{
    public enum ResultType
    {
        Executing = 1,
        Normal = 2,
        Return = 4,
        Throw = 8
    }
    public class ESCallable
    {
        public delegate Result Func(ExecutionContext context, ESObject thisVar, IList<Value>? args);

        public class Result
        {
            public static ExecutionContext NativeContext = new(null, null, null, null, null, null, null, 0, "<Native>" );

            private readonly ResultType _type;
            private readonly ExecutionContext? _context;
            public Func<Result, Result?>? Recall { get; private set; }
            private readonly Value? _value;

            public ResultType Type { get { return _context == null ? _type : _context.Result; } }
            public Value Value { get { return (_context == null ? _value : (_context.ReturnValue)) ?? Value.Undefined(); } }
            public ExecutionContext Context => _context ?? NativeContext;

            public Result(ExecutionContext ec, Func<Result, Result?>? recall = null) { _context = ec; Recall = recall; _type = ResultType.Executing; } // special type to definedfunction
            public Result() { _type = ResultType.Normal; } // normal, empty, empty

            public Result(ResultType t, Value? v) { _type = t; _value = v; }

            public void SetRecallCode(Func<Result, Result?>? rc) { Recall = rc; }
            public Result? ExecuteRecallCode() // if push nothing back, create a function to return null or a Result with Value == null; elsewhere something will be pushed
            {
                return Recall == null ? this : Recall(this);
            }
        }

        public static Result Normal(Value? v) { return new(ResultType.Normal, v); }
        public static Result Return(Value v) { return new(ResultType.Return, v); }
        public static Result Throw(Value v) { return new(ResultType.Throw, v); }
        public static Result Throw(ESError e) { return new(ResultType.Throw, Value.FromObject(e)); }
    }

}
