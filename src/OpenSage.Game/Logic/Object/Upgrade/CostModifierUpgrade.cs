using System.Collections.Generic;
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
                { "Percentage", (parser, x) => x.Percentages.Add(parser.ParsePercentage()) },
                { "LabelForPalantirString", (parser, x) => x.LabelForPalantirString = parser.ParseLocalizedStringKey() },
            });

        public ObjectKinds EffectKindOf { get; private set; }
        public List<float> Percentages { get; private set; } = new List<float>();

        [AddedIn(SageGame.Bfme)]
        public string LabelForPalantirString { get; private set; }
    }
}
