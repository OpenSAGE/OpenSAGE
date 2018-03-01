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
        };

        public string Name { get; private set; }

        public int Priority { get; private set; }
        public int TimeBetweenChecksMS { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int TimeBetweenEventsMS { get; private set; }

        public int ExpirationTimeMS { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int QuietTimeMS { get; private set; }

        public List<EvaSideSound> SideSounds { get; } = new List<EvaSideSound>();
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
