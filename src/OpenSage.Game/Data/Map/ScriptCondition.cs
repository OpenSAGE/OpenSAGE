using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class ScriptCondition : ScriptContent<ScriptCondition, ScriptConditionType>
    {
        public const string AssetName = "Condition";

        private const ushort MinimumVersionThatHasInternalName = 4;
        private const ushort MinimumVersionThatHasEnabledFlag = 5;

        public bool IsInverted { get; private set; }

        internal static ScriptCondition Parse(BinaryReader reader, MapParseContext context)
        {
            return Parse(
                reader,
                context,
                MinimumVersionThatHasInternalName,
                MinimumVersionThatHasEnabledFlag,
                (version, x) =>
                {
                    if (version >= MinimumVersionThatHasEnabledFlag)
                    {
                        x.IsInverted = reader.ReadBooleanUInt32Checked();
                    }
                });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteTo(
                writer,
                assetNames,
                MinimumVersionThatHasInternalName,
                MinimumVersionThatHasEnabledFlag,
                () => writer.WriteBooleanUInt32(IsInverted));
        }
    }
}
