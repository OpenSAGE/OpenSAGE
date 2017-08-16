using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponSet
    {
        internal static WeaponSet Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<WeaponSet> FieldParseTable = new IniParseTable<WeaponSet>
        {
            { "Conditions", (parser, x) => x.Conditions = parser.ParseEnumFlags<WeaponSetConditions>() },
            { "Weapon", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.Weapon = parser.ParseAssetReference()) },
            { "PreferredAgainst", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.PreferredAgainst = parser.ParseEnumBitArray<ObjectKinds>()) },
            { "AutoChooseSources", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.AutoChooseSources = parser.ParseEnumFlags<CommandSourceTypes>()) },
            { "ShareWeaponReloadTime", (parser, x) => x.ShareWeaponReloadTime = parser.ParseBoolean() },
        };

        public WeaponSetConditions Conditions { get; private set; }
        public Dictionary<WeaponSlot, WeaponSetSlot> Slots { get; } = new Dictionary<WeaponSlot, WeaponSetSlot>();
        public bool ShareWeaponReloadTime { get; private set; }

        private void ParseWeaponSlotProperty(IniParser parser, Action<WeaponSetSlot> parseValue)
        {
            var weaponSlot = parser.ParseEnum<WeaponSlot>();

            if (!Slots.TryGetValue(weaponSlot, out var weaponSetSlot))
            {
                Slots.Add(weaponSlot, weaponSetSlot = new WeaponSetSlot());
            }

            parseValue(weaponSetSlot);
        }
    }
}
