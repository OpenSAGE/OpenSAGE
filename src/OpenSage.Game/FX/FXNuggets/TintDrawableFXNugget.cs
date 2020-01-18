using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class TintDrawableFXNugget : FXNugget
    {
        internal static TintDrawableFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TintDrawableFXNugget> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<TintDrawableFXNugget>
        {
            { "Color", (parser, x) => x.Color = parser.ParseColorRgb() },
            { "PreColorTime", (parser, x) => x.PreColorTime = parser.ParseInteger() },
            { "PostColorTime", (parser, x) => x.PostColorTime = parser.ParseInteger() },
            { "SustainedColorTime", (parser, x) => x.SustainedColorTime = parser.ParseInteger() },
            { "Frequency", (parser, x) => x.Frequency = parser.ParseFloat() },
            { "Amplitude", (parser, x) => x.Amplitude = parser.ParseFloat() },
        });

        public ColorRgb Color { get; private set; }
        public int PreColorTime { get; private set; }
        public int PostColorTime { get; private set; }
        public int SustainedColorTime { get; private set; }
        public float Frequency { get; private set; }
        public float Amplitude { get; private set; }
    }
}
