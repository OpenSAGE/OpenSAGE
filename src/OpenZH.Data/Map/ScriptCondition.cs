using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class ScriptCondition : ScriptContent<ScriptCondition, ScriptConditionType>
    {
        public const string AssetName = "Condition";

        private const ushort MinimumVersionThatHasInternalName = 4;

        internal static ScriptCondition Parse(BinaryReader reader, MapParseContext context)
        {
            return Parse(reader, context, MinimumVersionThatHasInternalName);
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteTo(writer, assetNames, MinimumVersionThatHasInternalName);
        }
    }
}
