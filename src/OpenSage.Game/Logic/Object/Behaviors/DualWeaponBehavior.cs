using OpenSage.Data.Ini;

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
                { "UseRealVictimRange", (parser, x) => x.UseRealVictimRange = parser.ParseBoolean() }
            });

        public int SwitchWeaponOnCloseRangeDistance { get; private set; }
        public bool UseRealVictimRange { get; private set; }
    }
}
