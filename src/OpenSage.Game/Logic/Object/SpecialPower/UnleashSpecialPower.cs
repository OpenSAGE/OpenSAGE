using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class UnleashSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new UnleashSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<UnleashSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<UnleashSpecialPowerModuleData>
            {
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
                { "AwardXPForTriggering", (parser, x) => x.AwardXPForTriggering = parser.ParseInteger() },
                { "Instant", (parser, x) => x.Instant = parser.ParseBoolean() },
            });

        public int UnpackTime { get; private set; }
        public int AwardXPForTriggering { get; private set; }
        public bool Instant { get; private set; }
    }
}
