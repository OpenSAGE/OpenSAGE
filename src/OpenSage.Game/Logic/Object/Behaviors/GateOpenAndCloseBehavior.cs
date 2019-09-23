using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class GateOpenAndCloseBehaviorModuleData : BehaviorModuleData
    {
        internal static GateOpenAndCloseBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GateOpenAndCloseBehaviorModuleData> FieldParseTable = new IniParseTable<GateOpenAndCloseBehaviorModuleData>
        {
            { "ResetTimeInMilliseconds", (parser, x) => x.ResetTimeInMilliseconds = parser.ParseInteger() },
            { "OpenByDefault", (parser, x) => x.OpenByDefault = parser.ParseBoolean() },
            { "PercentOpenForPathing", (parser, x) => x.PercentOpenForPathing = parser.ParsePercentage() },
            { "SoundOpeningGateLoop", (parser, x) => x.SoundOpeningGateLoop = parser.ParseAssetReference() },
            { "SoundClosingGateLoop", (parser, x) => x.SoundClosingGateLoop = parser.ParseAssetReference() },
            { "SoundFinishedOpeningGate", (parser, x) => x.SoundFinishedOpeningGate = parser.ParseAssetReference() },
            { "SoundFinishedClosingGate", (parser, x) => x.SoundFinishedClosingGate = parser.ParseAssetReference() },
            { "TimeBeforePlayingOpenSound", (parser, x) => x.TimeBeforePlayingOpenSound = parser.ParseInteger() },
            { "TimeBeforePlayingClosedSound", (parser, x) => x.TimeBeforePlayingClosedSound = parser.ParseInteger() },
            { "Proxy", (parser, x) => x.Proxy = parser.ParseAssetReference() },
            { "RepelCollidingUnits", (parser, x) => x.RepelCollidingUnits = parser.ParseBoolean() }
        };

        public int ResetTimeInMilliseconds { get; private set; }
        public bool OpenByDefault { get; private set; }
        public Percentage PercentOpenForPathing { get; private set; }
        public string SoundOpeningGateLoop { get; private set; }
        public string SoundClosingGateLoop { get; private set; }
        public string SoundFinishedOpeningGate { get; private set; }
        public string SoundFinishedClosingGate { get; private set; }
        public int TimeBeforePlayingOpenSound { get; private set; }
        public int TimeBeforePlayingClosedSound { get; private set; }
        public string Proxy { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool RepelCollidingUnits { get; private set; }
    }
}
