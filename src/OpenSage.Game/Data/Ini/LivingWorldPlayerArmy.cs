using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldPlayerArmy
    {
        internal static LivingWorldPlayerArmy Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldPlayerArmy> FieldParseTable = new IniParseTable<LivingWorldPlayerArmy>
        {
            { "Name", (parser, x) => x.Name = parser.ParseAssetReference() },
            { "DisplayNameTag", (parser, x) => x.DisplayNameTag = parser.ParseLocalizedStringKey() },
            { "Color", (parser, x) => x.Color = parser.ParseColorRgb() },
            { "NightColor", (parser, x) => x.NightColor = parser.ParseColorRgb() },
            { "MinCommandPoints", (parser, x) => x.MinCommandPoints = parser.ParseInteger() },
            { "ReplenishArmyName", (parser, x) => x.ReplenishArmyName = parser.ParseAssetReference() },

            { "ArmyEntry", (parser, x) => x.ArmyEntries.Add(LivingWorldPlayerArmyEntry.Parse(parser)) },
        };

        public string Name { get; private set; }
        public string DisplayNameTag { get; private set; }
        public ColorRgb Color { get; private set; }
        public ColorRgb NightColor { get; private set; }
        public int MinCommandPoints { get; private set; }
        public string ReplenishArmyName { get; private set; }

        public List<LivingWorldPlayerArmyEntry> ArmyEntries { get; } = new List<LivingWorldPlayerArmyEntry>();
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldPlayerArmyEntry
    {
        internal static LivingWorldPlayerArmyEntry Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldPlayerArmyEntry> FieldParseTable = new IniParseTable<LivingWorldPlayerArmyEntry>
        {
            { "ThingTemplate", (parser, x) => x.ThingTemplate = parser.ParseAssetReference() },
            { "Quantity", (parser, x) => x.Quantity = parser.ParseInteger() }
        };

        public string ThingTemplate { get; private set; }
        public int Quantity { get; private set; }
    }
}
