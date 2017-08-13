using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class Rank
    {
        internal static Rank Parse(IniParser parser)
        {
            parser.ParseIdentifier();

            var level = parser.ParseInteger();

            var result = parser.ParseBlock(FieldParseTable);

            parser.NextTokenIf(IniTokenType.EndOfLine);

            result.Level = level;

            return result;
        }

        private static readonly IniParseTable<Rank> FieldParseTable = new IniParseTable<Rank>
        {
            { "RankName", (parser, x) => x.RankName = parser.ParseAsciiString() },
            { "SkillPointsNeeded", (parser, x) => x.SkillPointsNeeded = parser.ParseInteger() },
            { "SciencesGranted", (parser, x) => x.SciencesGranted = parser.ParseAsciiString() },
            { "SciencePurchasePointsGranted", (parser, x) => x.SciencePurchasePointsGranted = parser.ParseInteger() }
        };

        public int Level { get; private set; }

        public string RankName { get; private set; }
        public int SkillPointsNeeded { get; private set; }
        public string SciencesGranted { get; private set; }
        public int SciencePurchasePointsGranted { get; private set; }
    }
}
