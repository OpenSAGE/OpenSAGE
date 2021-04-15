using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Scripting
{
    internal static partial class ScriptActions
    {
        private static NLog.Logger Logger => NLog.LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, Enum> CachedEnumMap;

        static ScriptActions()
        {
            CachedEnumMap = IniParser.GetEnumMap<ScriptActionType>();
        }

        public static void Execute(ScriptExecutionContext context, ScriptAction action)
        {
            var actionType = action.ContentType;

            if (action.InternalName != null)
            {
                if (CachedEnumMap.TryGetValue(action.InternalName.Name, out var untypedActionType))
                {
                    actionType = (ScriptActionType) untypedActionType;
                }
            }

            ExecuteImpl(context, action, actionType);
        }
    }
}
