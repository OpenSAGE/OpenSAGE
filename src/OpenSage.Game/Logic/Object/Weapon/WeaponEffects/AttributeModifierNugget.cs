using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class AttributeModifierNugget : WeaponEffectNugget
    {
        internal static AttributeModifierNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AttributeModifierNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<AttributeModifierNugget>
            {
                { "AttributeModifier", (parser, x) => x.AttributeModifier = parser.ParseAssetReference() },
                { "DamageFXType", (parser, x) => x.DamageFXType = parser.ParseEnum<DamageFXType>() },
                { "AntiCategories", (parser, x) => x.AntiCategories = parser.ParseEnumBitArray<ModifierCategory>() },
                { "Radius", (parser, x) => x.Radius = parser.ParseLong() },
                { "AffectHordeMembers", (parser, x) => x.AffectHordeMembers = parser.ParseBoolean() },
            });

        public string AttributeModifier { get; private set; }
        public DamageFXType DamageFXType { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModifierCategory> AntiCategories { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public long Radius { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool AffectHordeMembers { get; private set; }
    }
}
