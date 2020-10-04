using System.IO;
using System.Linq;
using OpenSage.Data.Map;

namespace OpenSage.Scripting
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

        public ScriptAction Copy(string appendix)
        {
            return new ScriptAction()
            {
                Arguments = Arguments.Select(a => a.Copy(appendix)).ToArray(),
                ContentType = ContentType,
                Enabled = Enabled,
                InternalName = InternalName
            };
        }
    }
}
