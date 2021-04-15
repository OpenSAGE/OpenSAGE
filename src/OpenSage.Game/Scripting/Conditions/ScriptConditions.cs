using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Scripting
{
    internal static partial class ScriptConditions
    {
        private static NLog.Logger Logger => NLog.LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, Enum> CachedEnumMap;

        static ScriptConditions()
        {
            CachedEnumMap = IniParser.GetEnumMap<ScriptConditionType>();
        }

        public static bool Evaluate(ScriptExecutionContext context, ScriptCondition condition)
        {
            var conditionType = condition.ContentType;

            if (condition.InternalName != null)
            {
                if (CachedEnumMap.TryGetValue(condition.InternalName.Name, out var untypedConditionType))
                {
                    conditionType = (ScriptConditionType) untypedConditionType;
                }
            }

            return EvaluateImpl(context, condition, conditionType);
        }
    }
}
