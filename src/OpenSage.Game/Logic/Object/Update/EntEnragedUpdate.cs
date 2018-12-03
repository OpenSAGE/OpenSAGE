using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class EntEnragedUpdateModuleData : UpdateModuleData
    {
        internal static EntEnragedUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<EntEnragedUpdateModuleData> FieldParseTable = new IniParseTable<EntEnragedUpdateModuleData>
        {
            { "EnragedLifeTimer", (parser, x) => x.EnragedLifeTimer = parser.ParseFloat() },
            { "HatedObjectFilter", (parser, x) => x.HatedObjectFilter = ObjectFilter.Parse(parser) },
            { "FriendlyDeadFilter", (parser, x) => x.FriendlyDeadFilter = ObjectFilter.Parse(parser) },
            { "EnragedTime", (parser, x) => x.EnragedTime = parser.ParseInteger() },
            { "TimeUntilCanRageAgain", (parser, x) => x.TimeUntilCanRageAgain = parser.ParseInteger() },
            { "EnragedTransitionTime", (parser, x) => x.EnragedTransitionTime = parser.ParseInteger() },
            { "EnragedTransitionFX", (parser, x) => x.EnragedTransitionFX = parser.ParseAssetReference() },
            { "EnragedOnBuffFX", (parser, x) => x.EnragedOnBuffFX = parser.ParseAssetReference() },
            { "EnragedOffBuffFX", (parser, x) => x.EnragedOffBuffFX = parser.ParseAssetReference() },
        };

        public float EnragedLifeTimer { get; private set; }
        public ObjectFilter HatedObjectFilter { get; private set; }
        public ObjectFilter FriendlyDeadFilter { get; private set; }
        public int EnragedTime { get; private set; }
        public int TimeUntilCanRageAgain { get; private set; }
        public int EnragedTransitionTime { get; private set; }
        public string EnragedTransitionFX { get; private set; }
        public string EnragedOnBuffFX { get; private set; }
        public string EnragedOffBuffFX { get; private set; }
    }
}
