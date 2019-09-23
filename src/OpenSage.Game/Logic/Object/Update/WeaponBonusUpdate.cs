using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Temporarily triggers use of a specific WeaponBonus from GameData.ini.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class WeaponBonusUpdateModuleData : UpdateModuleData
    {
        internal static WeaponBonusUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<WeaponBonusUpdateModuleData> FieldParseTable = new IniParseTable<WeaponBonusUpdateModuleData>
        {
            { "RequiredAffectKindOf", (parser, x) => x.RequiredAffectKindOf = parser.ParseEnum<ObjectKinds>() },
            { "ForbiddenAffectKindOf", (parser, x) => x.ForbiddenAffectKindOf = parser.ParseEnum<ObjectKinds>() },
            { "BonusDuration", (parser, x) => x.BonusDuration = parser.ParseInteger() },
            { "BonusDelay", (parser, x) => x.BonusDelay = parser.ParseInteger() },
            { "BonusRange", (parser, x) => x.BonusRange = parser.ParseInteger() },
            { "BonusConditionType", (parser, x) => x.BonusConditionType = parser.ParseEnum<WeaponBonusType>() },
        };

        public ObjectKinds RequiredAffectKindOf { get; private set; }
        public ObjectKinds ForbiddenAffectKindOf { get; private set; }
        public int BonusDuration { get; private set; }
        public int BonusDelay { get; private set; }
        public int BonusRange { get; private set; }
        public WeaponBonusType BonusConditionType { get; private set; }
    }
}
