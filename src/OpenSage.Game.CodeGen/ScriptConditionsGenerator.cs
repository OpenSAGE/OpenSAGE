using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace OpenSage.Game.CodeGen
{
    [Generator]
    public sealed class ScriptConditionsGenerator : ScriptContentGeneratorBase
    {
        public override void Execute(GeneratorExecutionContext context)
        {
            var sb = new StringBuilder();

            sb.AppendLine("namespace OpenSage.Scripting");
            sb.AppendLine("{");
            sb.AppendLine("    partial class ScriptConditions");
            sb.AppendLine("    {");
            sb.AppendLine("        private static bool EvaluateImpl(ScriptExecutionContext context, ScriptCondition condition, ScriptConditionType conditionType)");
            sb.AppendLine("        {");
            sb.AppendLine("            var game = context.Game.SageGame;");
            sb.AppendLine();
            sb.AppendLine("            switch (conditionType)");
            sb.AppendLine("            {");

            WriteCases(context, sb);

            sb.AppendLine("                default:");
            sb.AppendLine("                    Logger.Warn($\"Script condition type '{conditionType}' not implemented\");");
            sb.AppendLine("                    return false;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource("ScriptConditions.Execution.g.cs", sb.ToString());
        }

        private static void WriteCases(GeneratorExecutionContext context, StringBuilder sb)
        {
            var scriptConditionsType = context.Compilation.GetTypeByMetadataName("OpenSage.Scripting.ScriptConditions");

            var scriptConditionNameLookup = GetScriptContentNameLookup(context, "OpenSage.Scripting.ScriptConditionType");
            var sageGameNameLookup = GetSageGameNameLookup(context);

            var methods = scriptConditionsType.GetMembers()
                .Where(x => x.Kind == SymbolKind.Method)
                .Cast<IMethodSymbol>();

            foreach (var method in methods)
            {
                var scriptConditionAttribute = method.GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass.Name == "ScriptConditionAttribute");

                if (scriptConditionAttribute == null)
                {
                    continue;
                }

                var parameters = method.Parameters;

                if (parameters[0].Type.Name != "ScriptExecutionContext")
                {
                    throw new InvalidOperationException();
                }

                var parameterTypes = new ITypeSymbol[parameters.Length - 1];
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    parameterTypes[i] = parameters[i + 1].Type;
                }

                var conditionType = (uint)scriptConditionAttribute.ConstructorArguments[0].Value;
                var conditionName = scriptConditionNameLookup[conditionType];

                sb.Append($"                case ScriptConditionType.{conditionName}");

                var games = scriptConditionAttribute.ConstructorArguments[3].Values;
                if (games.Length > 0)
                {
                    sb.Append(" when ");
                    for (var i = 0; i < games.Length; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(" || ");
                        }
                        sb.Append($"game == SageGame.{sageGameNameLookup[(int)games[i].Value]}");
                    }
                }

                sb.AppendLine(":");

                sb.Append($"                    if ({method.Name}(context");
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    sb.Append($", {GetArgument(i, parameterTypes, "condition")}");
                }
                sb.AppendLine($"))");
                sb.AppendLine("                    {");

                var displayTemplate = (string)scriptConditionAttribute.ConstructorArguments[2].Value;
                sb.Append($"                        Logger.Info(string.Format(\"Script condition evaluated to true: {displayTemplate}\"");
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    sb.Append($", {GetArgument(i, parameterTypes, "condition")}");
                }
                sb.AppendLine("));");

                sb.AppendLine("                        return true;");
                sb.AppendLine("                    }");
                sb.AppendLine("                    return false;");
                sb.AppendLine();
            }
        }
    }
}
