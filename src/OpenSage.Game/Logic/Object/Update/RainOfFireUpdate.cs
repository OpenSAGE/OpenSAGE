using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class RainOfFireUpdateModuleData : UpdateModuleData
    {
        internal static RainOfFireUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RainOfFireUpdateModuleData> FieldParseTable = new IniParseTable<RainOfFireUpdateModuleData>
            {
                { "StartRainTime", (parser, x) => x.StartRainTime = parser.ParseInteger() },
                { "DarknessFadeTime", (parser, x) => x.DarknessFadeTime = parser.ParseInteger() },
                { "RainEmitterHeight", (parser, x) => x.RainEmitterHeight = parser.ParseFloat() },
                { "DarknessLevel", (parser, x) => x.DarknessLevel = parser.ParseFloat() },
                { "JitterRadius", (parser, x) => x.JitterRadius = parser.ParseFloat() },
                { "DPSMin", (parser, x) => x.DpsMin = parser.ParseFloat() },
                { "DPSMax", (parser, x) => x.DpsMax = parser.ParseFloat() },
                { "DPSRampupTime", (parser, x) => x.DpsRampupTime = parser.ParseInteger() },
            };

        public int StartRainTime { get; private set; }
        public int DarknessFadeTime { get; private set; }
        public float RainEmitterHeight { get; private set; }
        public float DarknessLevel { get; private set; }
        public float JitterRadius { get; private set; }
        public float DpsMin { get; private set; }
        public float DpsMax { get; private set; }
        public int DpsRampupTime { get; private set; }
    }
}
