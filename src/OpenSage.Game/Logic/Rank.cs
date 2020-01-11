using OpenSage.Data.Ini;

namespace OpenSage.Logic
{
    public sealed class Rank : BaseAsset
    {
        internal static Rank Parse(IniParser parser)
        {
            return parser.ParseIndexedBlock(
                (x, level) =>
                {
                    x.Level = level;
                    x.SetNameAndInstanceId("Rank", level.ToString());
                },
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

        [AddedIn(SageGame.Bfme)]
        public int SkillPointsNeededDefault { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int SkillPointsNeededCampaign { get; private set; }
    }
}
