using OpenSage.Data.Ini;

namespace OpenSage.LivingWorld
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class LivingWorldAiTemplate
    {
        internal static LivingWorldAiTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldAiTemplate> FieldParseTable = new IniParseTable<LivingWorldAiTemplate>
        {
            { "DesiredSoldierRatio", (parser, x) => x.DesiredSoldierRatio = parser.ParseInteger() },
            { "DesiredArcherRatio", (parser, x) => x.DesiredArcherRatio = parser.ParseInteger() },
            { "DesiredPikemenRatio", (parser, x) => x.DesiredPikemenRatio = parser.ParseInteger() },
            { "DesiredCavalryRatio", (parser, x) => x.DesiredCavalryRatio = parser.ParseInteger() },
            { "DesiredMonsterRatio", (parser, x) => x.DesiredMonsterRatio = parser.ParseInteger() },
            { "DesiredHeroRatio", (parser, x) => x.DesiredHeroRatio = parser.ParseInteger() },
            { "DesiredFortressRatio", (parser, x) => x.DesiredFortressRatio = parser.ParseInteger() },
            { "DesiredSiegeRatio", (parser, x) => x.DesiredSiegeRatio = parser.ParseInteger() },
            { "DesiredSupportRatio", (parser, x) => x.DesiredSupportRatio = parser.ParseInteger() },

            { "BuildingScoreArmory", (parser, x) => x.BuildingScoreArmory = parser.ParseInteger() },
            { "BuildingScoreBarracks", (parser, x) => x.BuildingScoreBarracks = parser.ParseInteger() },
            { "BuildingScoreCastle", (parser, x) => x.BuildingScoreCastle = parser.ParseInteger() },
            { "BuildingScoreFarm", (parser, x) => x.BuildingScoreFarm = parser.ParseInteger() },

            { "BonusPreferenceResource", (parser, x) => x.BonusPreferenceResource = parser.ParseInteger() },
            { "BonusPreferenceArmy", (parser, x) => x.BonusPreferenceArmy = parser.ParseInteger() },
            { "BonusPreferenceLegendary", (parser, x) => x.BonusPreferenceLegendary = parser.ParseInteger() },
            { "BonusPreferenceAttack", (parser, x) => x.BonusPreferenceAttack = parser.ParseInteger() },
            { "BonusPreferenceDefense", (parser, x) => x.BonusPreferenceDefense = parser.ParseInteger() },
            { "BonusPreferenceExperience", (parser, x) => x.BonusPreferenceExperience = parser.ParseInteger() },
            { "BonusPreferenceTreasury", (parser, x) => x.BonusPreferenceTreasury = parser.ParseInteger() },
        };

        public string Name { get; private set; }

        public int DesiredSoldierRatio { get; private set; }
        public int DesiredArcherRatio { get; private set; }
        public int DesiredPikemenRatio { get; private set; }
        public int DesiredCavalryRatio { get; private set; }
        public int DesiredMonsterRatio { get; private set; }
        public int DesiredHeroRatio { get; private set; }
        public int DesiredFortressRatio { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int DesiredSiegeRatio { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int DesiredSupportRatio { get; private set; }

        public int BuildingScoreArmory { get; private set; }
        public int BuildingScoreBarracks { get; private set; }
        public int BuildingScoreCastle { get; private set; }
        public int BuildingScoreFarm { get; private set; }

        public int BonusPreferenceResource { get; private set; }
        public int BonusPreferenceArmy { get; private set; }
        public int BonusPreferenceLegendary { get; private set; }
        public int BonusPreferenceAttack { get; private set; }
        public int BonusPreferenceDefense { get; private set; }
        public int BonusPreferenceExperience { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int BonusPreferenceTreasury { get; private set; }
    }
}
