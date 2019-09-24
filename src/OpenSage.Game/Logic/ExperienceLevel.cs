using OpenSage.Data.Ini;
using OpenSage.Gui.InGame;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ExperienceLevel : BaseAsset
    {
        internal static ExperienceLevel Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("ExperienceLevel", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<ExperienceLevel> FieldParseTable = new IniParseTable<ExperienceLevel>
        {
            { "TargetNames", (parser, x) => x.TargetNames = parser.ParseAssetReferenceArray() },
            { "RequiredExperience", (parser, x) => x.RequiredExperience = parser.ParseInteger() },
            { "ExperienceAward", (parser, x) => x.ExperienceAward = parser.ParseInteger() },
            { "ExperienceAwardOwnGuysDie", (parser, x) => x.ExperienceAwardOwnGuysDie = parser.ParseInteger() },
            { "InformUpdateModule", (parser, x) => x.InformUpdateModule = parser.ParseBoolean() },
            { "Upgrades", (parser, x) => x.Upgrades = parser.ParseAssetReferenceArray() },
            { "ShowLevelUpTint", (parser, x) => x.ShowLevelUpTint = parser.ParseBoolean() },
            { "AttributeModifiers", (parser, x) => x.AttributeModifiers = parser.ParseAssetReferenceArray() },
            { "Rank", (parser, x) => x.Rank = parser.ParseInteger() },
            { "LevelUpFx", (parser, x) => x.LevelUpFX = parser.ParseAssetReference() },
            { "LevelUpTintColor", (parser, x) => x.LevelUpTintColor = parser.ParseColorRgb() },
            { "LevelUpTintPreColorTime", (parser, x) => x.LevelUpTintPreColorTime = parser.ParseInteger() },
            { "LevelUpTintPostColorTime", (parser, x) => x.LevelUpTintPostColorTime = parser.ParseInteger() },
            { "LevelUpTintSustainColorTime", (parser, x) => x.LevelUpTintSustainColorTime = parser.ParseInteger() },
            { "EmotionType", (parser, x) => x.EmotionType = parser.ParseEnum<EmotionType>() },
            { "SelectionDecal", (parser, x) => x.SelectionDecal = RadiusDecalTemplate.Parse(parser) }
        };

        public string[] TargetNames { get; private set; }
        public int RequiredExperience { get; private set; }
        public int ExperienceAward { get; private set; }
        public int ExperienceAwardOwnGuysDie { get; private set; }
        public bool InformUpdateModule { get; private set; }
        public string[] Upgrades { get; private set; }
        public bool ShowLevelUpTint { get; private set; }
        public string[] AttributeModifiers { get; private set; }
        public int Rank { get; private set; }
        public string LevelUpFX { get; private set; }
        public ColorRgb LevelUpTintColor { get; private set; }
        public int LevelUpTintPreColorTime { get; private set; }
        public int LevelUpTintPostColorTime { get; private set; }
        public int LevelUpTintSustainColorTime { get; private set; }
        public EmotionType EmotionType { get; private set; }
        public RadiusDecalTemplate SelectionDecal { get; private set; }
    }
}
