using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AimWeaponBehaviorModuleData : UpgradeModuleData
    {
        internal static AimWeaponBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AimWeaponBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<AimWeaponBehaviorModuleData>
            {
                { "AimHighThreshold", (parser, x) => x.AimHighThreshold = parser.ParseFloat() },
                { "AimLowThreshold", (parser, x) => x.AimLowThreshold = parser.ParseFloat() },
            });

        public float AimHighThreshold { get; private set; }
        public float AimLowThreshold { get; private set; }
    }
}
