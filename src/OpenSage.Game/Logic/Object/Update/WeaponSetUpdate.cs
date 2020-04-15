using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponSetUpdate : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly WeaponSetUpdateModuleData _moduleData;

        internal WeaponSetUpdate(GameObject gameObject, WeaponSetUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            
        }
    }

    public sealed class WeaponSetUpdateModuleData : UpdateModuleData
    {
        public List<WeaponSlotHardpointData> WeaponSlotHardpoints { get; } = new List<WeaponSlotHardpointData>();
        public List<WeaponSlotTurretData> WeaponSlotTurrets { get; } = new List<WeaponSlotTurretData>();
        public List<WeaponSlotHierarchicalTurretData> WeaponSlotHierarchicalTurrets { get; } = new List<WeaponSlotHierarchicalTurretData>();

        internal override BehaviorModule CreateModule(GameObject gameObject)
        {
            return new WeaponSetUpdate(gameObject, this);
        }
    }

    public class WeaponSlotHardpointData
    {
        public uint ID { get; internal set; }
        public bool AllowInterleavedFiring { get; private set; }
        public WeaponSlotInterleavedStyle InterleavedStyle { get; private set; }
        public WeaponChoiceCriteria WeaponChoiceCriteria { get; internal set; }
        public List<WeaponSlotWeaponData> Weapons { get; } = new List<WeaponSlotWeaponData>();
    }

    public class WeaponSlotTurretData : WeaponSlotHardpointData
    {
        public TurretAIData TurretSettings { get; internal set; }
    }

    public sealed class WeaponSlotHierarchicalTurretData : WeaponSlotTurretData
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
        public WeaponSlot Ordering { get; internal set; }
        public LazyAssetReference<WeaponTemplate> Template { get; internal set; }
        public LazyAssetReference<UpgradeTemplate> Upgrade { get; private set; }
        public BitArray<ObjectStatus> ObjectStatus { get; private set; }
        public bool IsPlayerUpgradePermanent { get; private set; }
    }
}
