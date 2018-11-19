using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponWhenDeadBehaviorModuleData : UpgradeModuleData
    {
        internal static FireWeaponWhenDeadBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FireWeaponWhenDeadBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<FireWeaponWhenDeadBehaviorModuleData>
            {
                { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ObjectStatus>() },
                { "ExemptStatus", (parser, x) => x.ExemptStatus = parser.ParseEnum<ObjectStatus>() },
                { "DeathWeapon", (parser, x) => x.DeathWeapon = parser.ParseAssetReference() },
                { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
                { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
                { "DelayTime", (parser, x) => x.DelayTime = parser.ParseInteger() },
                { "WeaponOffset", (parser, x) => x.WeaponOffset = parser.ParsePoint3D() },
            });

        public ObjectStatus RequiredStatus { get; private set; }
        public ObjectStatus ExemptStatus { get; private set; }
        public string DeathWeapon { get; private set; }
        public bool StartsActive { get; private set; }
        public BitArray<DeathType> DeathTypes { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DelayTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Point3D WeaponOffset { get; private set; }
    }
}
