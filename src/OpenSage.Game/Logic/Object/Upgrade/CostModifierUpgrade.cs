﻿using System.Collections.Generic;
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
                { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) },
                { "UpgradeDiscount", (parser, x) => x.UpgradeDiscount = parser.ParseBoolean() },
                { "ApplyToTheseUpgrades", (parser, x) => x.ApplyToTheseUpgrades = parser.ParseAssetReferenceArray() }
            });

        public ObjectKinds EffectKindOf { get; private set; }
        public List<float> Percentages { get; private set; } = new List<float>();

        [AddedIn(SageGame.Bfme)]
        public string LabelForPalantirString { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectFilter ObjectFilter { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UpgradeDiscount { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] ApplyToTheseUpgrades { get; private set; }
    }
}
