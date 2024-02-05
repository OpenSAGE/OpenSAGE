using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class DieModule : BehaviorModule
    {
        protected DieModuleData ModuleData { get; }

        protected DieModule(DieModuleData moduleData)
        {
            ModuleData = moduleData;
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }

        internal sealed override void OnDie(BehaviorUpdateContext context, DeathType deathType, ObjectStatus? status)
        {
            if (!IsCorrectStatus(status) || !IsCorrectDeathType(deathType))
            {
                return;
            }

            Die(context, deathType);
        }

        private bool IsCorrectStatus(ObjectStatus? status)
        {
            var required = !ModuleData.RequiredStatus.HasValue || // if nothing is required, we pass
                                (status.HasValue && ModuleData.RequiredStatus == status); // or if we are the one of the required statuses, we pass
            var notExempt = !ModuleData.ExemptStatus.HasValue || // if nothing is exempt, we pass
                                !status.HasValue || // if we don't have a status, we can't be exempt, so we pass
                                ModuleData.ExemptStatus != status.Value; // or if we are not one of the exempt statuses, we pass
            return required && notExempt;
        }

        private bool IsCorrectDeathType(DeathType deathType)
        {
            return ModuleData.DeathTypes == null || ModuleData.DeathTypes.Get(deathType);
        }

        private protected virtual void Die(BehaviorUpdateContext context, DeathType deathType) { }
    }

    public abstract class DieModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Die;

        internal static readonly IniParseTable<DieModuleData> FieldParseTable = new IniParseTable<DieModuleData>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "ExemptStatus", (parser, x) => x.ExemptStatus = parser.ParseEnum<ObjectStatus>() },
            { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ObjectStatus>() }
        };

        public BitArray<DeathType>? DeathTypes { get; private set; }
        public ObjectStatus? ExemptStatus { get; private set; }
        public ObjectStatus? RequiredStatus { get; private set; }
    }
}
