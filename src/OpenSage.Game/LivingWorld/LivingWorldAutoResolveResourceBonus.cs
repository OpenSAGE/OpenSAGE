using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.LivingWorld
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class LivingWorldAutoResolveResourceBonus
    {
        internal static LivingWorldAutoResolveResourceBonus Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldAutoResolveResourceBonus> FieldParseTable = new IniParseTable<LivingWorldAutoResolveResourceBonus>
        {
            { "Sides", (parser, x) => x.Sides = parser.ParseAssetReferenceArray() },
            { "Bonus", (parser, x) => x.ResourceBonuses.Add(ResourceBonus.Parse(parser)) },
        };

        public string Name { get; private set; }

        public string[] Sides { get; private set; }
        public List<ResourceBonus> ResourceBonuses { get; } = new List<ResourceBonus>();
    }

    public class ResourceBonus
    {
        internal static ResourceBonus Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ResourceBonus> FieldParseTable = new IniParseTable<ResourceBonus>
        {
            { "MinResourceBonus", (parser, x) => x.MinResourceBonus = parser.ParseInteger() },
            { "WeaponMultiplier", (parser, x) => x.WeaponMultiplier = parser.ParseFloat() },
            { "ArmorMultiplier", (parser, x) => x.ArmorMultiplier = parser.ParseFloat() },
        };

        public int MinResourceBonus { get; private set; }
        public float WeaponMultiplier { get; private set; }
        public float ArmorMultiplier { get; private set; }
    }
}
