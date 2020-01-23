using OpenSage.Content;
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
            { "DamageFX", (parser, x) => x.DamageFX = parser.ParseAssetReference() },
        };

        public BitArray<ArmorSetCondition> Conditions { get; private set; }
        public LazyAssetReference<ArmorTemplate> Armor { get; private set; }
        public string DamageFX { get; private set; }
    }
}
