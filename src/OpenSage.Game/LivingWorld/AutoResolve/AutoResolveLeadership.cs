using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.LivingWorld.AutoResolve
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AutoResolveLeadership : BaseAsset
    {
        internal static AutoResolveLeadership Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("AutoResolveLeadership", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<AutoResolveLeadership> FieldParseTable = new IniParseTable<AutoResolveLeadership>
        {
            { "Affects", (parser, x) => x.Affects = parser.ParseAssetReferenceArray() },
            { "AffectsHigherLevelFirst", (parser, x) => x.AffectsHigherLevelFirst = parser.ParseBoolean() },
            { "BonusForLevel", (parser, x) => x.BonusForLevel = BonusForLevel.Parse(parser) }
        };

        public string[] Affects { get; private set; }
        public bool AffectsHigherLevelFirst { get; private set; }
        public BonusForLevel BonusForLevel { get; private set; }
    }

    public class BonusForLevel
    {
        internal static BonusForLevel Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BonusForLevel> FieldParseTable = new IniParseTable<BonusForLevel>
        {
            { "MinLevel", (parser, x) => x.MinLevel = parser.ParseInteger() },
            { "WeaponMultiplier", (parser, x) => x.WeaponMultiplier = parser.ParsePercentage() },
            { "ArmorMultiplier", (parser, x) => x.ArmorMultiplier = parser.ParsePercentage() },
            { "ExperienceMultiplier", (parser, x) => x.ExperienceMultiplier = parser.ParsePercentage() },
            { "MaximumUnitsAffected", (parser, x) => x.MaximumUnitsAffected = parser.ParseInteger() },
            { "Priority", (parser, x) => x.Priority = parser.ParseInteger() },
        };

        public int MinLevel { get; private set; }
        public Percentage WeaponMultiplier { get; private set; }
        public Percentage ArmorMultiplier { get; private set; }
        public Percentage ExperienceMultiplier { get; private set; }
        public int MaximumUnitsAffected { get; private set; }
        public int Priority { get; private set; }
    }
  
}
