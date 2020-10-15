using System;
using System.Collections.Generic;
using System.Reflection;
using OpenSage.Data.Ini;

namespace OpenSage.Scripting
{
    internal static partial class ScriptActions
    {
        private static NLog.Logger Logger => NLog.LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<ScriptActionKey, ScriptingAction> Actions;
        private static readonly Dictionary<string, Enum> CachedEnumMap;

        static ScriptActions()
        {
            // TODO: All of this should be done at compile-time with a SourceGenerator, once .NET 5.0 has been released.

            Actions = new Dictionary<ScriptActionKey, ScriptingAction>();

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

                    void AddAction(ScriptActionKey key)
                    {
                        Actions.Add(
                            key,
                            (context, action) =>
                            {
                                if (action.Arguments.Length > parameters.Length - 1)
                                {
                                    throw new InvalidOperationException();
                                }

                                var arguments = new object[typeCodes.Length + 1];
                                arguments[0] = context;

                                var logMessageArgs = new object[typeCodes.Length];

                                void SetLogMessageArgument(int index)
                                {
                                    var arg = typeCodes[index] == TypeCode.Int32 && parameters[index + 1].ParameterType.IsEnum
                                        ? Enum.ToObject(parameters[index + 1].ParameterType, (int) arguments[index + 1])
                                        : arguments[index + 1];
                                    logMessageArgs[index] = arg;
                                }

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
                                    SetLogMessageArgument(i);
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
                                    SetLogMessageArgument(i);
                                }

                                localMethod.Invoke(null, arguments);

                                var logMessage = string.Format(scriptActionAttribute.DisplayTemplate, logMessageArgs);
                                Logger.Info($"Executed script action: {logMessage}");
                            });
                    }

                    if (scriptActionAttribute.Games.Length > 0)
                    {
                        foreach (var sageGame in scriptActionAttribute.Games)
                        {
                            AddAction(new ScriptActionKey(scriptActionAttribute.ActionType, sageGame));
                        }
                    }
                    else
                    {
                        AddAction(new ScriptActionKey(scriptActionAttribute.ActionType, null));
                    }
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

            var key = new ScriptActionKey(actionType, context.Scene.Game.SageGame);
            if (!Actions.TryGetValue(key, out var actionFunction))
            {
                key = new ScriptActionKey(actionType, null);
                if (!Actions.TryGetValue(key, out actionFunction))
                {
                    Logger.Warn($"Script action type '{actionType}' not implemented");
                    return;
                }
            }

            actionFunction(context, action);
        }

        private readonly struct ScriptActionKey : IEquatable<ScriptActionKey>
        {
            public readonly ScriptActionType Action;
            public readonly SageGame? Game;

            public ScriptActionKey(ScriptActionType action, SageGame? game)
            {
                Action = action;
                Game = game;
            }

            public override bool Equals(object obj) => obj is ScriptActionKey key && Equals(key);

            public bool Equals(ScriptActionKey other) => Action == other.Action && Game == other.Game;

            public override int GetHashCode() => HashCode.Combine(Action, Game);
        }
    }
}
