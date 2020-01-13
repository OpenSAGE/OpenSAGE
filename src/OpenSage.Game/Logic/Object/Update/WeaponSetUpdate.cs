using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponSetUpdateModuleData : UpdateModuleData
    {
        public List<WeaponSlotHardpoint> WeaponSlotHardpoints { get; } = new List<WeaponSlotHardpoint>();
        public List<WeaponSlotTurret> WeaponSlotTurrets { get; } = new List<WeaponSlotTurret>();
        public List<WeaponSlotHierarchicalTurret> WeaponSlotHierarchicalTurrets { get; } = new List<WeaponSlotHierarchicalTurret>();
    }

    public class WeaponSlotHardpoint
    {
        public uint ID { get; private set; }
        public bool AllowInterleavedFiring { get; private set; }
        public WeaponSlotInterleavedStyle InterleavedStyle { get; private set; }
        public WeaponChoiceCriteria WeaponChoiceCriteria { get; private set; }
        public List<WeaponSlotWeaponData> Weapons { get; } = new List<WeaponSlotWeaponData>();
    }

    public class WeaponSlotTurret : WeaponSlotHardpoint
    {
        public TurretAIData TurretSettings { get; private set; }
    }

    public sealed class WeaponSlotHierarchicalTurret : WeaponSlotTurret
    {
        public uint ParentID { get; private set; }
    }

    public enum WeaponSlotInterleavedStyle
    {
        InterleaveFirstAvailable,
        InterleaveRandom,
    }

    public sealed class WeaponSlotWeaponData
    {
        public WeaponSlot Ordering { get; private set; }
        public LazyAssetReference<WeaponTemplate> Template { get; private set; }
        public LazyAssetReference<UpgradeTemplate> Upgrade { get; private set; }
        public BitArray<ObjectStatus> ObjectStatus { get; private set; }
        public bool IsPlayerUpgradePermanent { get; private set; }
    }
}
