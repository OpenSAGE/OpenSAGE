using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class WeaponModeSpecialPowerUpdateModuleData : BehaviorModuleData
    {
        internal static WeaponModeSpecialPowerUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<WeaponModeSpecialPowerUpdateModuleData> FieldParseTable = new IniParseTable<WeaponModeSpecialPowerUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "AttributeModifier", (parser, x) => x.AttributeModifier = parser.ParseAssetReference() },
            { "Duration", (parser, x) => x.Duration = parser.ParseInteger() },
            { "LockWeaponSlot", (parser, x) => x.LockWeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "WeaponSetFlags", (parser, x) => x.WeaponSetFlags = parser.ParseEnumFlags<WeaponSetConditions>() },
            { "StartsPaused", (parser, x) => x.StartsPaused = parser.ParseBoolean() },
            { "InitiateSound", (parser, x) => x.InitiateSound = parser.ParseAssetReference() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public int Duration { get; private set; }
        public string AttributeModifier { get; private set; }
        public WeaponSlot LockWeaponSlot { get; private set; }
        public WeaponSetConditions WeaponSetFlags { get; private set; }
        public bool StartsPaused { get; private set; }
        public string InitiateSound { get; private set; }
    }
}
