﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class PlayerHealSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new PlayerHealSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<PlayerHealSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<PlayerHealSpecialPowerModuleData>
            {
                { "HealAffects", (parser, x) => x.HealAffects = parser.ParseEnumFlags<ObjectKinds>() },
                { "HealAmount", (parser, x) => x.HealAmount = parser.ParseFloat() },
                { "HealRadius", (parser, x) => x.HealRadius = parser.ParseFloat() },
                { "HealFX", (parser, x) => x.HealFX = parser.ParseAssetReference() },
                { "HealOCL", (parser, x) => x.HealOCL = parser.ParseAssetReference() },
                { "HealAsPercent", (parser, x) => x.HealAsPercent = parser.ParseBoolean() }
            });

        public ObjectKinds HealAffects { get; private set; }
        public float HealAmount { get; private set; }
        public float HealRadius { get; private set; }
        public string HealFX { get; private set; }
        public string HealOCL { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool HealAsPercent { get; private set; }
    }
}
