using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class MiscEvaData
    {
        internal static MiscEvaData Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<MiscEvaData> FieldParseTable = new IniParseTable<MiscEvaData>
        {
            { "EnemySightedMaxVoicePositionScanRange", (parser, x) => x.EnemySightedMaxVoicePositionScanRange = parser.ParseInteger() },
            { "EnemyCampDestroyedDamageTimeoutMS", (parser, x) => x.EnemyCampDestroyedDamageTimeoutMS = parser.ParseInteger() },
            { "FriendlyCampDestroyedDamageTimeoutMS", (parser, x) => x.FriendlyCampDestroyedDamageTimeoutMS = parser.ParseInteger() }
        };

        public int EnemySightedMaxVoicePositionScanRange { get; private set; }
        public int EnemyCampDestroyedDamageTimeoutMS { get; private set; }
        public int FriendlyCampDestroyedDamageTimeoutMS { get; private set; }
    }
}
