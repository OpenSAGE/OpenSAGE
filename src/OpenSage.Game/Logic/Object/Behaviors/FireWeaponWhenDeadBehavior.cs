using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponWhenDeadBehavior : BehaviorModule, IUpgradeableModule
    {
        private readonly UpgradeLogic _upgradeLogic;

        internal FireWeaponWhenDeadBehavior(FireWeaponWhenDeadBehaviorModuleData moduleData)
        {
            _upgradeLogic = new UpgradeLogic(moduleData.UpgradeData, this);
        }

        void IUpgradeableModule.OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            // TODO
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            _upgradeLogic.Load(reader);
        }
    }

    public sealed class FireWeaponWhenDeadBehaviorModuleData : UpgradeModuleData
    {
        internal static FireWeaponWhenDeadBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FireWeaponWhenDeadBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<FireWeaponWhenDeadBehaviorModuleData>
            {
                { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ObjectStatus>() },
                { "ExemptStatus", (parser, x) => x.ExemptStatus = parser.ParseEnum<ObjectStatus>() },
                { "DeathWeapon", (parser, x) => x.DeathWeapon = parser.ParseAssetReference() },
                { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
                { "DelayTime", (parser, x) => x.DelayTime = parser.ParseInteger() },
                { "WeaponOffset", (parser, x) => x.WeaponOffset = parser.ParseVector3() },
            });

        public ObjectStatus RequiredStatus { get; private set; }
        public ObjectStatus ExemptStatus { get; private set; }
        public string DeathWeapon { get; private set; }
        public BitArray<DeathType> DeathTypes { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DelayTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Vector3 WeaponOffset { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new FireWeaponWhenDeadBehavior(this);
        }
    }
}
