using System.Numerics;
using OpenSage.Data.Ini;

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
            { "HeroModeTrigger", (parser, x) => x.HeroModeTrigger = parser.ParseBoolean() },
            { "FireWeaponNugget", (parser, x) => x.FireWeaponNugget = WeaponNugget.Parse(parser) }
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

        [AddedIn(SageGame.Bfme2)]
        public WeaponNugget FireWeaponNugget { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class WeaponNugget
    {
        internal static WeaponNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);
        private static readonly IniParseTable<WeaponNugget> FieldParseTable = new IniParseTable<WeaponNugget>
        {
            { "WeaponName", (parser, x) => x.WeaponName = parser.ParseAssetReference() },
            { "FireDelay", (parser, x) => x.FireDelay = parser.ParseInteger() },
            { "OneShot", (parser, x) => x.OneShot = parser.ParseBoolean() },
            { "Offset", (parser, x) => x.Offset = parser.ParseVector3() }
        };
        public string WeaponName { get; private set; }
        public int FireDelay { get; private set; }
        public bool OneShot { get; private set; }
        public Vector3 Offset { get; private set; }
    }
}
