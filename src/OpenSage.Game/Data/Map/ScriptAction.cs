using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class ScriptAction : ScriptContent<ScriptAction, ScriptActionType>
    {
        public const string AssetNameTrue = "ScriptAction";
        public const string AssetNameFalse = "ScriptActionFalse";

        private const ushort MinimumVersionThatHasInternalName = 2;
        private const ushort MinimumVersionThatHasEnabledFlag = 3;

        internal static ScriptAction Parse(BinaryReader reader, MapParseContext context)
        {
            return Parse(reader, context, MinimumVersionThatHasInternalName, MinimumVersionThatHasEnabledFlag);
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteTo(writer, assetNames, MinimumVersionThatHasInternalName, MinimumVersionThatHasEnabledFlag);
        }
    }
}
