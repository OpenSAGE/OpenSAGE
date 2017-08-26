using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class FireWeaponPower : ObjectBehavior
    {
        internal static FireWeaponPower Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireWeaponPower> FieldParseTable = new IniParseTable<FireWeaponPower>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "MaxShotsToFire", (parser, x) => x.MaxShotsToFire = parser.ParseInteger() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public int MaxShotsToFire { get; private set; }
    }
}
