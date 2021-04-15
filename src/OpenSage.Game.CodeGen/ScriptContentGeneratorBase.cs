using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace OpenSage
{
    public abstract class ScriptContentGeneratorBase : ISourceGenerator
    {
        public abstract void Execute(GeneratorExecutionContext context);

        protected static Dictionary<int, string> GetSageGameNameLookup(GeneratorExecutionContext context)
        {
            var sageGameType = context.Compilation.GetTypeByMetadataName("OpenSage.SageGame");

#pragma warning disable RS1024 // Compare symbols correctly
            return sageGameType.GetMembers()
                .Where(x => x.Kind == SymbolKind.Field)
                .ToDictionary(x => (int) ((IFieldSymbol) x).ConstantValue, x => x.Name);
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

        public void Initialize(GeneratorInitializationContext context)
        {

        }
    }
}
