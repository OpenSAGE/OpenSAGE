using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class DemoTrapUpdateModuleData : UpdateModuleData
    {
        internal static DemoTrapUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DemoTrapUpdateModuleData> FieldParseTable = new IniParseTable<DemoTrapUpdateModuleData>
        {
            { "DefaultProximityMode", (parser, x) => x.DefaultProximityMode = parser.ParseBoolean() },
            { "DetonationWeaponSlot", (parser, x) => x.DetonationWeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "ProximityModeWeaponSlot", (parser, x) => x.ProximityModeWeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "ManualModeWeaponSlot", (parser, x) => x.ManualModeWeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "TriggerDetonationRange", (parser, x) => x.TriggerDetonationRange = parser.ParseFloat() },
            { "IgnoreTargetTypes", (parser, x) => x.IgnoreTargetTypes = parser.ParseEnumBitArray<ObjectKinds>() },
            { "AutoDetonationWithFriendsInvolved", (parser, x) => x.AutoDetonationWithFriendsInvolved = parser.ParseBoolean() },
            { "DetonateWhenKilled", (parser, x) => x.DetonateWhenKilled = parser.ParseBoolean() }
        };

        public bool DefaultProximityMode { get; private set; }
        public WeaponSlot DetonationWeaponSlot { get; private set; }
        public WeaponSlot ProximityModeWeaponSlot { get; private set; }
        public WeaponSlot ManualModeWeaponSlot { get; private set; }
        public float TriggerDetonationRange { get; private set; }
        public BitArray<ObjectKinds> IgnoreTargetTypes { get; private set; }
        public bool AutoDetonationWithFriendsInvolved { get; private set; }
        public bool DetonateWhenKilled { get; private set; }
    }
}
