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
            });

        public ModelConditionFlag ConditionFlag { get; private set; }
        public BitArray<ModelConditionFlag> AddConditionFlags { get; private set; }
    }
}
