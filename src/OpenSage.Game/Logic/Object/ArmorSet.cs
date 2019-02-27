﻿using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class ArmorSet
    {
        internal static ArmorSet Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ArmorSet> FieldParseTable = new IniParseTable<ArmorSet>
        {
            { "Conditions", (parser, x) => x.Conditions = parser.ParseEnumBitArray<ArmorSetCondition>() },
            { "Armor", (parser, x) => x.Armor = parser.ParseArmorReference() },
            { "DamageFX", (parser, x) => x.DamageFX = parser.ParseAssetReference() },
        };

        public BitArray<ArmorSetCondition> Conditions { get; private set; }
        public Armor Armor { get; private set; }
        public string DamageFX { get; private set; }
    }
}
