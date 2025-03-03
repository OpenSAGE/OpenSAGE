using System;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class ArmorTemplateSet : IConditionState<ArmorSetCondition>
    {
        internal static ArmorTemplateSet Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ArmorTemplateSet> FieldParseTable = new IniParseTable<ArmorTemplateSet>
        {
            { "Conditions", (parser, x) => x.Conditions.CopyFrom(parser.ParseEnumBitArray<ArmorSetCondition>()) },
            { "Armor", (parser, x) => x.Armor = parser.ParseArmorTemplateReference() },
            { "DamageFX", (parser, x) => x.DamageFX = parser.ParseDamageFXReference() },
        };

        public readonly BitArray<ArmorSetCondition> Conditions = new BitArray<ArmorSetCondition>();
        public LazyAssetReference<ArmorTemplate> Armor { get; private set; }
        public LazyAssetReference<DamageFX> DamageFX { get; private set; }

        public ReadOnlySpan<BitArray<ArmorSetCondition>> ConditionFlags => new(in Conditions);
    }
}
