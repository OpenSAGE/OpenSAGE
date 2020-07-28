using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponTemplateSet
    {
        internal static WeaponTemplateSet Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<WeaponTemplateSet> FieldParseTable = new IniParseTable<WeaponTemplateSet>
        {
            { "Conditions", (parser, x) => x.Conditions = parser.ParseEnumBitArray<WeaponSetConditions>() },
            { "Weapon", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.Weapon = parser.ParseWeaponTemplateReference()) },
            { "PreferredAgainst", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.PreferredAgainst = parser.ParseEnumBitArray<ObjectKinds>()) },
            { "AutoChooseSources", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.AutoChooseSources = parser.ParseEnumFlags<CommandSourceTypes>()) },
            { "ShareWeaponReloadTime", (parser, x) => x.ShareWeaponReloadTime = parser.ParseBoolean() },
            { "WeaponLockSharedAcrossSets", (parser, x) => x.WeaponLockSharedAcrossSets = parser.ParseBoolean() },
            { "OnlyAgainst", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.PreferredAgainst = parser.ParseEnumBitArray<ObjectKinds>()) },
            { "OnlyInCondition", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.OnlyInCondition = parser.ParseEnumBitArray<ModelConditionFlag>()) },
            { "ReadyStatusSharedWithinSet", (parser, x) => x.ReadyStatusSharedWithinSet = parser.ParseBoolean() },
            { "DefaultWeaponChoiceCritera", (parser, x) => x.DefaultWeaponChoiceCriteria = parser.ParseEnum<WeaponChoiceCriteria>() }
        };

        public BitArray<WeaponSetConditions> Conditions { get; private set; } = new BitArray<WeaponSetConditions>();
        public Dictionary<WeaponSlot, WeaponSetSlot> Slots { get; } = new Dictionary<WeaponSlot, WeaponSetSlot>();
        public bool ShareWeaponReloadTime { get; private set; }
        public bool WeaponLockSharedAcrossSets { get; private set; }
        public WeaponChoiceCriteria DefaultWeaponChoiceCriteria { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ReadyStatusSharedWithinSet { get; private set; }

        private void ParseWeaponSlotProperty(IniParser parser, Action<WeaponSetSlot> parseValue)
        {
            var weaponSlot = parser.ParseEnum<WeaponSlot>();

            if (!Slots.TryGetValue(weaponSlot, out var weaponSetSlot))
            {
                Slots.Add(weaponSlot, weaponSetSlot = new WeaponSetSlot());
            }

            parseValue(weaponSetSlot);
        }

        /// <summary>
        /// Converts a Generals-style WeaponSet to a C&C3-style WeaponSetUpdate.
        /// </summary>
        internal WeaponSetUpdateModuleData ToWeaponSetUpdate(AIUpdateModuleData aiUpdate)
        {
            // TODO: Need to move this up a level, to allow for multiple WeaponSets.
            var result = new WeaponSetUpdateModuleData();

            var slotsToProcess = new SortedSet<WeaponSlot>(Slots.Keys);

            var id = 1u;

            void SetWeaponSlotData(WeaponSlotHardpointData weaponSlot)
            {
                // TODO
                weaponSlot.WeaponChoiceCriteria = WeaponChoiceCriteria.PreferMostDamage;
            }

            void AddWeapon(WeaponSlotHardpointData weaponSlot, WeaponSlot slot)
            {
                if (!Slots.TryGetValue(slot, out var weaponSetSlot))
                {
                    return;
                }

                // TODO: PreferredAgainst
                // TODO: OnlyAgainst
                // TODO: AutoChooseSources

                weaponSlot.Weapons.Add(new WeaponSlotWeaponData
                {
                    Ordering = slot,
                    Template = weaponSetSlot.Weapon,
                });

                slotsToProcess.Remove(slot);
            }

            void AddWeaponSlotTurret(TurretAIData turretAIData)
            {
                var weaponSlot = new WeaponSlotTurretData
                {
                    ID = id++,
                    TurretSettings = turretAIData,
                };

                SetWeaponSlotData(weaponSlot);

                foreach (var controlledWeaponSlot in turretAIData.ControlledWeaponSlots.GetSetBits())
                {
                    AddWeapon(weaponSlot, controlledWeaponSlot);
                }

                if (weaponSlot.Weapons.Count == 0)
                {
                    return;
                }

                result.WeaponSlotTurrets.Add(weaponSlot);
            }

            if (aiUpdate?.Turret != null)
            {
                AddWeaponSlotTurret(aiUpdate.Turret);
            }

            if (aiUpdate?.AltTurret != null)
            {
                AddWeaponSlotTurret(aiUpdate.AltTurret);
            }

            // If there were any weapon slots that weren't controlled by a turret,
            // create a WeaponSlotHardpoint for them.
            if (slotsToProcess.Count > 0)
            {
                var weaponSlot = new WeaponSlotHardpointData
                {
                    ID = id++
                };

                SetWeaponSlotData(weaponSlot);

                while (slotsToProcess.Count > 0)
                {
                    var slot = slotsToProcess.Min;
                    AddWeapon(weaponSlot, slot);
                    slotsToProcess.Remove(slot);
                }

                result.WeaponSlotHardpoints.Add(weaponSlot);
            }

            return result;
        }
    }

    [AddedIn(SageGame.Bfme2)]
    public enum WeaponChoiceCriteria
    {
        PreferMostDamage,
        PreferLongestDamage,
        PreferGrabOverDamage,

        [IniEnum("PREFER_LEAST_MOVEMENT")]
        PreferLeastMovement,

        [IniEnum("SELECT_AT_RANDOM")]
        SelectAtRandom,

        UseWeaponSetDefaultCriteria,
    }
}
