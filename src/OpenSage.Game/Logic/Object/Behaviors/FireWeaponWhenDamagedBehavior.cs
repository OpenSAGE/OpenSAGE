using OpenSage.Data.Ini;
using OpenSage.Logic.Object.Damage;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponWhenDamagedBehaviorModuleData : UpgradeModuleData
    {
        internal static FireWeaponWhenDamagedBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FireWeaponWhenDamagedBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<FireWeaponWhenDamagedBehaviorModuleData>
            {
                { "ContinuousWeaponDamaged", (parser, x) => x.ContinuousWeaponDamaged = parser.ParseAssetReference() },
                { "ContinuousWeaponReallyDamaged", (parser, x) => x.ContinuousWeaponReallyDamaged = parser.ParseAssetReference() },

                { "ReactionWeaponPristine", (parser, x) => x.ReactionWeaponPristine = parser.ParseAssetReference() },
                { "ReactionWeaponDamaged", (parser, x) => x.ReactionWeaponDamaged = parser.ParseAssetReference() },
                { "ReactionWeaponReallyDamaged", (parser, x) => x.ReactionWeaponReallyDamaged = parser.ParseAssetReference() },

                { "DamageTypes", (parser, x) => x.DamageTypes = parser.ParseEnumBitArray<DamageType>() },
                { "DamageAmount", (parser, x) => x.DamageAmount = parser.ParseInteger() }
            });

        public string ContinuousWeaponDamaged { get; private set; }
        public string ContinuousWeaponReallyDamaged { get; private set; }

        public string ReactionWeaponPristine { get; private set; }
        public string ReactionWeaponDamaged { get; private set; }
        public string ReactionWeaponReallyDamaged { get; private set; }

        public BitArray<DamageType> DamageTypes { get; private set; }

        /// <summary>
        /// If damage >= this value, the weapon will be fired.
        /// </summary>
        public int DamageAmount { get; private set; }
    }
}
