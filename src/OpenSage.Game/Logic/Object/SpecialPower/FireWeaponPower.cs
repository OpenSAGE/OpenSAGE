using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class FireWeaponPowerModuleData : SpecialPowerModuleData
    {
        internal static FireWeaponPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FireWeaponPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<FireWeaponPowerModuleData>
            {
                { "MaxShotsToFire", (parser, x) => x.MaxShotsToFire = parser.ParseInteger() },
            });

        public int MaxShotsToFire { get; private set; }
    }
}
