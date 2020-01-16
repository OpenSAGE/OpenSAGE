using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class TintDrawableFXNuggetData : FXNuggetData
    {
        internal static TintDrawableFXNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TintDrawableFXNuggetData> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<TintDrawableFXNuggetData>
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
