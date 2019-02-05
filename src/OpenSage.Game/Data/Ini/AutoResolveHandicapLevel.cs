using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AutoResolveHandicapLevel
    {
        internal static AutoResolveHandicapLevel Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoResolveHandicapLevel> FieldParseTable = new IniParseTable<AutoResolveHandicapLevel>
        {
            { "GUIDisplayedLevel", (parser, x) => x.GUIDisplayedLevel = parser.ParseInteger() },
            { "WeaponMultiplier", (parser, x) => x.WeaponMultiplier = parser.ParseFloat() },
            { "ArmorMultiplier", (parser, x) => x.ArmorMultiplier = parser.ParseFloat() },
            { "ExperienceMultiplier", (parser, x) => x.ExperienceMultiplier = parser.ParseFloat() },
        };

        public int GUIDisplayedLevel { get; private set; }
        public float WeaponMultiplier { get; private set; }
        public float ArmorMultiplier { get; private set; }
        public float ExperienceMultiplier { get; private set; }
    }
}
