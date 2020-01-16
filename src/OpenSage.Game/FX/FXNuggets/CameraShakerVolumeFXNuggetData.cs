using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class CameraShakerVolumeFXNuggetData : FXNuggetData
    {
        internal static CameraShakerVolumeFXNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CameraShakerVolumeFXNuggetData> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<CameraShakerVolumeFXNuggetData>
        {
            { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
            { "Duration_Seconds", (parser, x) => x.Duration = parser.ParseFloat() },
            { "Amplitude_Degrees", (parser, x) => x.Amplitude = parser.ParseFloat() }
        });

        public int Radius { get; private set; }
        public float Duration { get; private set; }
        public float Amplitude { get; private set; }
    }
}
