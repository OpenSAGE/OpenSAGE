using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class FellBeastSwoopPowerModuleData : SpecialPowerModuleData
    {
        internal static new FellBeastSwoopPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FellBeastSwoopPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<FellBeastSwoopPowerModuleData>
            {
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
                { "AwardXPForTriggering", (parser, x) => x.AwardXPForTriggering = parser.ParseInteger() },
                { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() }
            });

        public int UnpackTime { get; private set; }
        public int AwardXPForTriggering { get; private set; }
        public float StartAbilityRange { get; private set; }
    }
}
