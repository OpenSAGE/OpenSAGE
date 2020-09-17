using System;
using System.Collections.Generic;
using System.Reflection;
using OpenSage.Data.Ini;

namespace OpenSage.Scripting
{
    internal static partial class ScriptActions
    {
        private static readonly Dictionary<ScriptActionType, ScriptingAction> Actions;
        private static readonly Dictionary<string, Enum> CachedEnumMap;

        static ScriptActions()
        {
            // TODO: All of this should be done at compile-time with a SourceGenerator, once .NET 5.0 has been released.

            Actions = new Dictionary<ScriptActionType, ScriptingAction>();

            var methods = typeof(ScriptActions).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                var scriptActionAttribute = method.GetCustomAttribute<ScriptActionAttribute>();
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

                    Actions.Add(
                        scriptActionAttribute.ActionType,
                        (context, action) =>
                        {
                            if (action.Arguments.Length > parameters.Length - 1)
                            {
                                throw new InvalidOperationException();
                            }

                            var arguments = new object[typeCodes.Length + 1];
                            arguments[0] = context;

                            for (var i = 0; i < action.Arguments.Length; i++)
                            {
                                var argument = action.Arguments[i];

                                arguments[i + 1] = (typeCodes[i]) switch
                                {
                                    TypeCode.String => argument.StringValue,
                                    TypeCode.Single => argument.FloatValue.Value,
                                    TypeCode.Int32 => argument.IntValue.Value,
                                    TypeCode.Boolean => argument.IntValueAsBool,
                                    _ => throw new InvalidOperationException(),
                                };
                            }

                            // Fill in remaining parameters with default values. For example,
                            // MOVE_CAMERA_TO got a couple of extra parameters between Generals and BFME.
                            for (var i = action.Arguments.Length; i < parameters.Length - 1; i++)
                            {
                                arguments[i + 1] = (typeCodes[i]) switch
                                {
                                    TypeCode.String => "",
                                    TypeCode.Single => 0.0f,
                                    TypeCode.Int32 => 0,
                                    TypeCode.Boolean => false,
                                    _ => throw new InvalidOperationException(),
                                };
                            }

                            localMethod.Invoke(null, arguments);
                        });
                }
            }

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

            if (!Actions.TryGetValue(actionType, out var actionFunction))
            {
                // TODO: Implement this action type.
                return;
            }

            actionFunction(context, action);
        }
    }
}
