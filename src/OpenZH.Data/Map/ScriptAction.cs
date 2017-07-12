using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class ScriptAction : ScriptContent<ScriptAction, ScriptActionType>
    {
        public const string AssetNameTrue = "ScriptAction";
        public const string AssetNameFalse = "ScriptActionFalse";

        private const ushort MinimumVersionThatHasInternalName = 2;

        public static ScriptAction Parse(BinaryReader reader, MapParseContext context)
        {
            return Parse(reader, context, MinimumVersionThatHasInternalName);
        }

        public void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteTo(writer, assetNames, MinimumVersionThatHasInternalName);
        }
    }
}
