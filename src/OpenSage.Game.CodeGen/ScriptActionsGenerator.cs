using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace OpenSage.Game.CodeGen
{
    [Generator]
    public sealed class ScriptActionsGenerator : ScriptContentGeneratorBase
    {
        public override string ScriptContentClassName => "ScriptActions";

        public override string ScriptContentTypeEnumName => "ScriptActionType";

        protected override void Execute(
            SourceProductionContext context,
            INamedTypeSymbol scriptContentClass,
            INamedTypeSymbol scriptContentTypeEnum,
            INamedTypeSymbol sageGameEnum)
        {
            var sb = new StringBuilder();

            sb.AppendLine("namespace OpenSage.Scripting");
            sb.AppendLine("{");
            sb.AppendLine("    partial class ScriptActions");
            sb.AppendLine("    {");
            sb.AppendLine("        private static void ExecuteImpl(ScriptExecutionContext context, ScriptAction action, ScriptActionType actionType)");
            sb.AppendLine("        {");
            sb.AppendLine("            var game = context.Game.SageGame;");
            sb.AppendLine();
            sb.AppendLine("            switch (actionType)");
            sb.AppendLine("            {");

            WriteCases(scriptContentClass, scriptContentTypeEnum, sageGameEnum, sb);

            sb.AppendLine("                default:");
            sb.AppendLine("                    Logger.Warn($\"Script action type '{actionType}' not implemented\");");
            sb.AppendLine("                    break;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource("ScriptActions.Execution.g.cs", sb.ToString());
        }

        private static void WriteCases(
            INamedTypeSymbol scriptContentClass,
            INamedTypeSymbol scriptContentTypeEnum,
            INamedTypeSymbol sageGameEnum,
            StringBuilder sb)
        {
            var scriptActionNameLookup = GetScriptContentNameLookup(scriptContentTypeEnum);
            var sageGameNameLookup = GetSageGameNameLookup(sageGameEnum);

            var methods = scriptContentClass.GetMembers()
                .Where(x => x.Kind == SymbolKind.Method)
                .Cast<IMethodSymbol>();

            foreach (var method in methods)
            {
                var scriptActionAttribute = method.GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass.Name == "ScriptActionAttribute");

                if (scriptActionAttribute == null)
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

                var actionType = (uint)scriptActionAttribute.ConstructorArguments[0].Value;
                var actionName = scriptActionNameLookup[actionType];

                sb.Append($"                case ScriptActionType.{actionName}");

                var games = scriptActionAttribute.ConstructorArguments[3].Values;
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

                sb.Append($"                    {method.Name}(context");
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    sb.Append($", {GetArgument(i, parameterTypes, "action")}");
                }
                sb.AppendLine($");");

                var displayTemplate = (string)scriptActionAttribute.ConstructorArguments[2].Value;
                sb.Append($"                    Logger.Info(string.Format(\"Executed script action: {displayTemplate}\"");
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    sb.Append($", {GetArgument(i, parameterTypes, "action")}");
                }
                sb.AppendLine("));");

                sb.AppendLine($"                    break;");
                sb.AppendLine();
            }
        }
    }
}
