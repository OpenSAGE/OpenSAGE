using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.LivingWorld
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class LivingWorldAutoResolveSciencePurchasePointBonus
    {
        internal static LivingWorldAutoResolveSciencePurchasePointBonus Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldAutoResolveSciencePurchasePointBonus> FieldParseTable = new IniParseTable<LivingWorldAutoResolveSciencePurchasePointBonus>
        {
            { "Sides", (parser, x) => x.Sides = parser.ParseAssetReferenceArray() },
            { "Bonus", (parser, x) => x.PurchasePointBonuses.Add(PurchasePointBonus.Parse(parser)) },
        };

        public string Name { get; private set; }

        public string[] Sides { get; private set; }
        public List<PurchasePointBonus> PurchasePointBonuses { get; } = new List<PurchasePointBonus>();
    }

    public class PurchasePointBonus
    {
        internal static PurchasePointBonus Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PurchasePointBonus> FieldParseTable = new IniParseTable<PurchasePointBonus>
        {
            { "MinSciencePurchasePointsForBonus", (parser, x) => x.MinSciencePurchasePointsForBonus = parser.ParseInteger() },
            { "WeaponMultiplier", (parser, x) => x.WeaponMultiplier = parser.ParseFloat() },
            { "ArmorMultiplier", (parser, x) => x.ArmorMultiplier = parser.ParseFloat() },
        };

        public int MinSciencePurchasePointsForBonus { get; private set; }
        public float WeaponMultiplier { get; private set; }
        public float ArmorMultiplier { get; private set; }
    }
}
