using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class DualWeaponBehaviorModuleData : UpgradeModuleData
    {
        internal static DualWeaponBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DualWeaponBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<DualWeaponBehaviorModuleData>
            {
                { "SwitchWeaponOnCloseRangeDistance", (parser, x) => x.SwitchWeaponOnCloseRangeDistance = parser.ParseInteger() },
               
            });

        public int SwitchWeaponOnCloseRangeDistance { get; internal set; }
    }
}
