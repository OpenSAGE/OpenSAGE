using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CostModifierUpgradeModuleData : UpgradeModuleData
    {
        internal static CostModifierUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CostModifierUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<CostModifierUpgradeModuleData>
            {
                { "EffectKindOf", (parser, x) => x.EffectKindOf = parser.ParseEnum<ObjectKinds>() },
                { "Percentage", (parser, x) => x.Percentage = parser.ParsePercentage() }
            });

        public ObjectKinds EffectKindOf { get; private set; }
        public float Percentage { get; private set; }
    }
}
