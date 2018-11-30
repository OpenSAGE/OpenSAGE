using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class GrabPassengerSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new GrabPassengerSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<GrabPassengerSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<GrabPassengerSpecialPowerModuleData>
            {
                { "AllowTree", (parser, x) => x.AllowTree = parser.ParseBoolean() },
            });

        public bool AllowTree { get; private set; }
    }
}
