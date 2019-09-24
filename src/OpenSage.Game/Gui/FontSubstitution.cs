using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Gui
{
    [AddedIn(SageGame.Bfme)]
    public sealed class FontSubstitution : BaseAsset
    {
        internal static FontSubstitution Parse(IniParser parser)
        {
            var fontName = parser.ParseQuotedString();

            var result = parser.ParseTopLevelBlock(FieldParseTable);

            result.SetNameAndInstanceId("FontSubstitution", fontName);

            return result;
        }

        private static readonly IniParseTable<FontSubstitution> FieldParseTable = new IniParseTable<FontSubstitution>
        {
            { "Size", (parser, x) => x.Substitutions.Add(Substitution.Parse(parser)) }
        };

        public List<Substitution> Substitutions { get; } = new List<Substitution>();
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class Substitution
    {
        public int Size { get; private set; }

        public int ReplacementFontSize { get; private set; }
        public string ReplacementFontName { get; private set; }

        internal static Substitution Parse(IniParser parser)
        {
            return new Substitution
            {
                Size = parser.ParseInteger(),
                ReplacementFontSize = parser.ParseInteger(),
                ReplacementFontName = parser.ParseString()
            };
        }
    }
}
