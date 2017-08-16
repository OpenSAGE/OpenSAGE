using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponWhenDamagedBehavior : ObjectBehavior
    {
        internal static FireWeaponWhenDamagedBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireWeaponWhenDamagedBehavior> FieldParseTable = new IniParseTable<FireWeaponWhenDamagedBehavior>
        {
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "ContinuousWeaponDamaged", (parser, x) => x.ContinuousWeaponDamaged = parser.ParseAssetReference() },
            { "ContinuousWeaponReallyDamaged", (parser, x) => x.ContinuousWeaponReallyDamaged = parser.ParseAssetReference() },
            { "DamageTypes", (parser, x) => x.DamageTypes = parser.ParseEnumBitArray<DamageType>() },
            { "DamageAmount", (parser, x) => x.DamageAmount = parser.ParseInteger() }
        };

        public bool StartsActive { get; private set; }
        public string ContinuousWeaponDamaged { get; private set; }
        public string ContinuousWeaponReallyDamaged { get; private set; }
        public BitArray<DamageType> DamageTypes { get; private set; }

        /// <summary>
        /// If damage >= this value, the weapon will be fired.
        /// </summary>
        public int DamageAmount { get; private set; }
    }
}
