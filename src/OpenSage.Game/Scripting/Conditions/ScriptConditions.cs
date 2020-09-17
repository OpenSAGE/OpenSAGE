using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenSage.Scripting
{
    internal static partial class ScriptConditions
    {
        private static readonly Dictionary<ScriptConditionType, ScriptingCondition> Conditions;

        static ScriptConditions()
        {
            // TODO: All of this should be done at compile-time with a SourceGenerator, once .NET 5.0 has been released.

            Conditions = new Dictionary<ScriptConditionType, ScriptingCondition>();

            var methods = typeof(ScriptConditions).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                var scriptActionAttribute = method.GetCustomAttribute<ScriptConditionAttribute>();
                if (scriptActionAttribute != null)
                {
                    var localMethod = method;

                    var parameters = method.GetParameters();

                    if (parameters[0].ParameterType != typeof(ScriptExecutionContext))
                    {
                        throw new InvalidOperationException();
                    }

                    var typeCodes = new TypeCode[parameters.Length - 1];
                    for (var i = 0; i < typeCodes.Length; i++)
                    {
                        typeCodes[i] = Type.GetTypeCode(parameters[i + 1].ParameterType);
                    }

                    Conditions.Add(
                        scriptActionAttribute.ConditionType,
                        (context, condition) =>
                        {
                            if (condition.Arguments.Length != parameters.Length - 1)
                            {
                                throw new InvalidOperationException();
                            }

                            var arguments = new object[typeCodes.Length + 1];
                            arguments[0] = context;

                            for (var i = 0; i < condition.Arguments.Length; i++)
                            {
                                var argument = condition.Arguments[i];

                                arguments[i + 1] = (typeCodes[i]) switch
                                {
                                    TypeCode.String => argument.StringValue,
                                    TypeCode.Single => argument.FloatValue.Value,
                                    TypeCode.Int32 => argument.IntValue.Value,
                                    TypeCode.Boolean => argument.IntValueAsBool,
                                    _ => throw new InvalidOperationException(),
                                };
                            }

                            return (bool) localMethod.Invoke(null, arguments);
                        });
                }
            }
        }

        public static bool Evaluate(ScriptExecutionContext context, ScriptCondition condition)
        {
            if (!Conditions.TryGetValue(condition.ContentType, out var conditionFunction))
            {
                // TODO: Implement this condition type.
                return false;
            }

            return conditionFunction(context, condition);
        }
    }
}
