using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class StrafeAreaUpdateModuleData : UpdateModuleData
    {
        internal static StrafeAreaUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StrafeAreaUpdateModuleData> FieldParseTable = new IniParseTable<StrafeAreaUpdateModuleData>
        {
            { "WeaponName", (parser, x) => x.WeaponName = parser.ParseAssetReference() },
            { "StrafeAreaRadius", (parser, x) => x.StrafeAreaRadius = parser.ParseInteger() },
            { "Sweepfrequency", (parser, x) => x.Sweepfrequency = parser.ParseFloat() },
            { "SweepAmplitude", (parser, x) => x.SweepAmplitude = parser.ParseInteger() },
            { "Slope", (parser, x) => x.Slope = parser.ParseInteger() },
            { "InitialSweepPhase", (parser, x) => x.InitialSweepPhase = parser.ParseFloat() },
        };

        public string WeaponName { get; private set; }
        public int StrafeAreaRadius { get; private set; }
        public float Sweepfrequency { get; private set; }
        public int SweepAmplitude { get; private set; }
        public int Slope { get; private set; }
        public float InitialSweepPhase { get; private set; }
    }
}
