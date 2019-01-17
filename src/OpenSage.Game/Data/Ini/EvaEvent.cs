using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class EvaEvent
    {
        internal static EvaEvent Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<EvaEvent> FieldParseTable = new IniParseTable<EvaEvent>
        {
            { "Priority", (parser, x) => x.Priority = parser.ParseInteger() },
            { "TimeBetweenChecksMS", (parser, x) => x.TimeBetweenChecksMS = parser.ParseInteger() },
            { "TimeBetweenEventsMS", (parser, x) => x.TimeBetweenEventsMS = parser.ParseInteger() },
            { "ExpirationTimeMS", (parser, x) => x.ExpirationTimeMS = parser.ParseInteger() },
            { "QuietTimeMS", (parser, x) => x.QuietTimeMS = parser.ParseInteger() },

            { "SideSounds", (parser, x) => x.SideSounds.Add(EvaSideSound.Parse(parser)) },
            // BFME
            { "SideSound", (parser, x) => x.SideSounds.Add(EvaSideSound.Parse(parser)) },

            { "AlwaysPlayFromHomeBase", (parser, x) => x.AlwaysPlayFromHomeBase = parser.ParseBoolean() },
            { "CountAsJumpToLocation", (parser, x) => x.CountAsJumpToLocation = parser.ParseBoolean() },
            { "MillisecondsToWaitBeforePlaying", (parser, x) => x.MillisecondsToWaitBeforePlaying = parser.ParseInteger() },
            { "OtherEvaEventsToBlock", (parser, x) => x.OtherEvaEventsToBlock = parser.ParseAssetReference() }
        };

        public string Name { get; internal set; }

        public int Priority { get; private set; }
        public int TimeBetweenChecksMS { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int TimeBetweenEventsMS { get; private set; }

        public int ExpirationTimeMS { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int QuietTimeMS { get; private set; }

        public List<EvaSideSound> SideSounds { get; } = new List<EvaSideSound>();

        [AddedIn(SageGame.Bfme2)]
        public bool AlwaysPlayFromHomeBase { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool CountAsJumpToLocation { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MillisecondsToWaitBeforePlaying { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string OtherEvaEventsToBlock { get; private set; }
    }

    public sealed class EvaSideSound
    {
        internal static EvaSideSound Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<EvaSideSound> FieldParseTable = new IniParseTable<EvaSideSound>
        {
            { "Side", (parser, x) => x.Side = parser.ParseAssetReference() },

            { "Sounds", (parser, x) => x.Sound = parser.ParseAssetReference() },

            // BFME
            { "Sound", (parser, x) => x.Sound = parser.ParseAssetReference() }
        };

        public string Side { get; private set; }
        public string Sound { get; private set; }
    }
}
