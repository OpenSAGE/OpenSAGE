﻿using OpenSage.Data.Ini;

namespace OpenSage.Eva
{
    [AddedIn(SageGame.Bfme)]
    public sealed class MiscEvaData : BaseSingletonAsset
    {
        internal static void Parse(IniParser parser, MiscEvaData value) => parser.ParseBlockContent(value, FieldParseTable);

        private static readonly IniParseTable<MiscEvaData> FieldParseTable = new IniParseTable<MiscEvaData>
        {
            { "EnemySightedMaxVoicePositionScanRange", (parser, x) => x.EnemySightedMaxVoicePositionScanRange = parser.ParseInteger() },
            { "EnemyCampDestroyedDamageTimeoutMS", (parser, x) => x.EnemyCampDestroyedDamageTimeoutMS = parser.ParseInteger() },
            { "FriendlyCampDestroyedDamageTimeoutMS", (parser, x) => x.FriendlyCampDestroyedDamageTimeoutMS = parser.ParseInteger() },
            { "MaxMillisecondsToKeepJumpToEvents", (parser, x) => x.MaxMillisecondsToKeepJumpToEvents = parser.ParseInteger() },
            { "MaxMillisecondsBeforeResettingLastJumpTo", (parser, x) => x.MaxMillisecondsBeforeResettingLastJumpTo = parser.ParseInteger() },
            { "MinDistanceBetweenJumpToEvents", (parser, x) => x.MinDistanceBetweenJumpToEvents = parser.ParseInteger() }
        };

        public int EnemySightedMaxVoicePositionScanRange { get; private set; }
        public int EnemyCampDestroyedDamageTimeoutMS { get; private set; }
        public int FriendlyCampDestroyedDamageTimeoutMS { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MaxMillisecondsToKeepJumpToEvents { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MaxMillisecondsBeforeResettingLastJumpTo { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MinDistanceBetweenJumpToEvents { get; private set; }
    }
}
