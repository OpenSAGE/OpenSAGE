using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponUpdate : ObjectBehavior
    {
        internal static FireWeaponUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireWeaponUpdate> FieldParseTable = new IniParseTable<FireWeaponUpdate>
        {
            { "Weapon", (parser, x) => x.Weapon = parser.ParseAssetReference() },
            { "ExclusiveWeaponDelay", (parser, x) => x.ExclusiveWeaponDelay = parser.ParseInteger() }
        };

        public string Weapon { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int ExclusiveWeaponDelay { get; private set; }
    }
}
