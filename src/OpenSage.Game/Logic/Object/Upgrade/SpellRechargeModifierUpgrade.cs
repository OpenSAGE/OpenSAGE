using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class SpellRechargeModifierUpgradeModuleData : UpgradeModuleData
    {
        internal static SpellRechargeModifierUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SpellRechargeModifierUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<SpellRechargeModifierUpgradeModuleData>
            {
                { "LabelForPalantirString", (parser, x) => x.LabelForPalantirString = parser.ParseLocalizedStringKey() },
                { "Percentage", (parser, x) => x.Percentages.Add(parser.ParsePercentage()) }
            });

        public string LabelForPalantirString { get; private set; }
        public List<Percentage> Percentages { get; } = new List<Percentage>();
    }
}
