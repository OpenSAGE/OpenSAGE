﻿using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class ArmorTemplateSet
    {
        internal static ArmorTemplateSet Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ArmorTemplateSet> FieldParseTable = new IniParseTable<ArmorTemplateSet>
        {
            { "Conditions", (parser, x) => x.Conditions = parser.ParseEnumBitArray<ArmorSetCondition>() },
            { "Armor", (parser, x) => x.Armor = parser.ParseArmorTemplateReference() },
            { "DamageFX", (parser, x) => x.DamageFX = parser.ParseDamageFXReference() },
        };

        public BitArray<ArmorSetCondition> Conditions { get; private set; } = new BitArray<ArmorSetCondition>();
        public LazyAssetReference<ArmorTemplate> Armor { get; private set; }
        public LazyAssetReference<DamageFX> DamageFX { get; private set; }
    }
}
