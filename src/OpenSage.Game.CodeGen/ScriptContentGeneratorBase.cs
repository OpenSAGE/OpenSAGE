using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenSage
{
    public abstract class ScriptContentGeneratorBase : IIncrementalGenerator
    {
        public abstract string ScriptContentClassName { get; }

        public abstract string ScriptContentTypeEnumName { get; }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var scriptActionsClass = context.SyntaxProvider.CreateSyntaxProvider(
                (s, token) => s is ClassDeclarationSyntax cds && cds.Identifier.Text == ScriptContentClassName,
                static (ctx, token) =>
                {
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, token) as INamedTypeSymbol;

                    if (symbol?.ContainingNamespace?.ToString() == "OpenSage.Scripting")
                    {
                        return symbol;
                    }

                    return null;
                });

            var scriptActionsTypeEnum = context.SyntaxProvider.CreateSyntaxProvider(
                (s, token) => s is EnumDeclarationSyntax eds && eds.Identifier.Text == ScriptContentTypeEnumName,
                static (ctx, token) =>
                {
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, token) as INamedTypeSymbol;

                    if (symbol?.ContainingNamespace?.ToString() == "OpenSage.Scripting")
                    {
                        return symbol;
                    }

                    return null;
                });

            var sageGameEnum = context.SyntaxProvider.CreateSyntaxProvider(
                static (s, token) => s is EnumDeclarationSyntax eds && eds.Identifier.Text == "SageGame",
                static (ctx, token) =>
                {
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, token) as INamedTypeSymbol;

                    if (symbol?.ContainingNamespace?.Name == "OpenSage")
                    {
                        return symbol;
                    }

                    return null;
                });

            var interestingTypes = scriptActionsClass.Collect()
                .Combine(scriptActionsTypeEnum.Collect())
                .Combine(sageGameEnum.Collect());

            context.RegisterSourceOutput(
                interestingTypes,
                (spc, source) =>
                {
                    Execute(spc, source.Left.Left.First(), source.Left.Right.First(), source.Right.First());
                });
        }

        protected abstract void Execute(
            SourceProductionContext context,
            INamedTypeSymbol scriptContentClass,
            INamedTypeSymbol scriptContentTypeEnum,
            INamedTypeSymbol sageGameEnum);

        protected static Dictionary<int, string> GetSageGameNameLookup(GeneratorExecutionContext context)
        {
            var sageGameType = context.Compilation.GetTypeByMetadataName("OpenSage.SageGame");

#pragma warning disable RS1024 // Compare symbols correctly
            return sageGameType.GetMembers()
                .Where(x => x.Kind == SymbolKind.Field)
                .ToDictionary(x => (int) ((IFieldSymbol) x).ConstantValue, x => x.Name);
#pragma warning restore RS1024 // Compare symbols correctly
        }

        protected static Dictionary<int, string> GetSageGameNameLookup(INamedTypeSymbol sageGameType)
        {
#pragma warning disable RS1024 // Compare symbols correctly
            return sageGameType.GetMembers()
                .Where(x => x.Kind == SymbolKind.Field)
                .ToDictionary(x => (int)((IFieldSymbol)x).ConstantValue, x => x.Name);
#pragma warning restore RS1024 // Compare symbols correctly
        }

        protected static Dictionary<uint, string> GetScriptContentNameLookup(GeneratorExecutionContext context, string enumTypeName)
        {
            var contentTypeType = context.Compilation.GetTypeByMetadataName(enumTypeName);

#pragma warning disable RS1024 // Compare symbols correctly
            return contentTypeType.GetMembers()
                .Where(x => x.Kind == SymbolKind.Field)
                .ToDictionary(x => (uint) ((IFieldSymbol) x).ConstantValue, x => x.Name);
#pragma warning restore RS1024 // Compare symbols correctly
        }

        protected static Dictionary<uint, string> GetScriptContentNameLookup(INamedTypeSymbol contentTypeType)
        {
#pragma warning disable RS1024 // Compare symbols correctly
            return contentTypeType.GetMembers()
                .Where(x => x.Kind == SymbolKind.Field)
                .ToDictionary(x => (uint)((IFieldSymbol)x).ConstantValue, x => x.Name);
#pragma warning restore RS1024 // Compare symbols correctly
        }

        protected static string GetArgument(int index, ITypeSymbol[] parameterTypes, string variableName)
        {
            var parameterType = parameterTypes[index];

            var result = $"{variableName}.Arguments[{index}].{GetArgumentFieldName(parameterType)}";

            if (parameterType.TypeKind == TypeKind.Enum)
            {
                result = $"({parameterType.Name}){result}";
            }

            return result;
        }

        protected static string GetArgumentFieldName(ITypeSymbol type)
        {
            if (type.TypeKind == TypeKind.Enum)
            {
                return "IntValue.Value";
            }

            return type.SpecialType switch
            {
                SpecialType.System_String => "StringValue",
                SpecialType.System_Single => "FloatValue.Value",
                SpecialType.System_Int32 => "IntValue.Value",
                SpecialType.System_Boolean => "IntValueAsBool",
                _ => throw new InvalidOperationException($"Type {type.SpecialType} not handled")
            };
        }
    }
}
