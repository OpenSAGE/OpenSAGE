using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ExperienceLevelCreateModuleData : CreateModuleData
    {
        internal static ExperienceLevelCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ExperienceLevelCreateModuleData> FieldParseTable = new IniParseTable<ExperienceLevelCreateModuleData>
        {
            { "LevelToGrant", (parser, x) => x.LevelToGrant = parser.ParseInteger() },
            { "MPOnly", (parser, x) => x.MPOnly = parser.ParseBoolean() }
        };

        public int LevelToGrant { get; private set; }
        public bool MPOnly { get; private set; }
    }
}
