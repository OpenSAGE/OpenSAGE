using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Switches to a model condition state via upgrades.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class ModelConditionUpgradeModuleData : UpgradeModuleData
    {
        internal static ModelConditionUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ModelConditionUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<ModelConditionUpgradeModuleData>
            {
                { "ConditionFlag", (parser, x) => x.ConditionFlag = parser.ParseEnum<ModelConditionFlag>() },
                { "AddConditionFlags", (parser, x) => x.AddConditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>() },
                { "Permanent", (parser, x) => x.Permanent = parser.ParseBoolean() },
                { "RemoveConditionFlags", (parser, x) => x.RemoveConditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>() },
                { "RemoveConditionFlagsInRange", (parser, x) => x.RemoveConditionFlagsInRange = parser.ParseEnumBitArray<ModelConditionFlag>() },
                { "AddTempConditionFlag", (parser, x) => x.AddTempConditionFlag = parser.ParseAttributeEnum<ModelConditionFlag>("ModelConditionState") },
                { "TempConditionTime", (parser, x) => x.TempConditionTime = parser.ParseFloat() },
            });

        public ModelConditionFlag ConditionFlag { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModelConditionFlag> AddConditionFlags { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool Permanent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModelConditionFlag> RemoveConditionFlags { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModelConditionFlag> RemoveConditionFlagsInRange { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ModelConditionFlag AddTempConditionFlag { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float TempConditionTime { get; private set; }
    }
}
