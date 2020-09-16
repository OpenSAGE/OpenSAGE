using System.IO;
using OpenSage.Data.Map;
using OpenSage.FileFormats;

namespace OpenSage.Scripting
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
                () =>
                {
                    if (Version >= MinimumVersionThatHasEnabledFlag)
                    {
                        writer.WriteBooleanUInt32(IsInverted);
                    }
                });
        }
    }
}
