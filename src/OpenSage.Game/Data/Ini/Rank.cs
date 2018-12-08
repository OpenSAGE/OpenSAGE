using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class Rank
    {
        internal static Rank Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Level = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Rank> FieldParseTable = new IniParseTable<Rank>
        {
            { "RankName", (parser, x) => x.RankName = parser.ParseLocalizedStringKey() },
            { "SkillPointsNeeded", (parser, x) => x.SkillPointsNeeded = parser.ParseInteger() },
            { "SciencesGranted", (parser, x) => x.SciencesGranted = parser.ParseAssetReference() },
            { "SciencePurchasePointsGranted", (parser, x) => x.SciencePurchasePointsGranted = parser.ParseInteger() },
            { "SkillPointsNeededDefault", (parser, x) => x.SkillPointsNeededDefault = parser.ParseInteger() },
             { "SkillPointsNeededCampaign", (parser, x) => x.SkillPointsNeededCampaign = parser.ParseInteger() },
        };

        public int Level { get; private set; }

        public string RankName { get; private set; }
        public int SkillPointsNeeded { get; private set; }
        public string SciencesGranted { get; private set; }
        public int SciencePurchasePointsGranted { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int SkillPointsNeededDefault { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int SkillPointsNeededCampaign { get; private set; }
    }
}
