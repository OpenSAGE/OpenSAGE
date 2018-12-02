using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponUpdateModuleData : UpdateModuleData
    {
        internal static FireWeaponUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireWeaponUpdateModuleData> FieldParseTable = new IniParseTable<FireWeaponUpdateModuleData>
        {
            { "Weapon", (parser, x) => x.Weapon = parser.ParseAssetReference() },
            { "ExclusiveWeaponDelay", (parser, x) => x.ExclusiveWeaponDelay = parser.ParseInteger() },
            { "InitialDelay", (parser, x) => x.InitialDelay = parser.ParseInteger() },
            { "ChargingModeTrigger", (parser, x) => x.ChargingModeTrigger = parser.ParseBoolean() },
            { "AliveOnly", (parser, x) => x.AliveOnly = parser.ParseBoolean() },
            { "HeroModeTrigger", (parser, x) => x.HeroModeTrigger = parser.ParseBoolean() }
        };

        public string Weapon { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int ExclusiveWeaponDelay { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int InitialDelay { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ChargingModeTrigger { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AliveOnly { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HeroModeTrigger { get; private set; }
    }
}
