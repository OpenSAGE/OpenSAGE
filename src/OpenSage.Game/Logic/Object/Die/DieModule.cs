using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class DieModule : BehaviorModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);
        }
    }

    public abstract class DieModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Die;

        internal static readonly IniParseTable<DieModuleData> FieldParseTable = new IniParseTable<DieModuleData>
        {
            { "VeterancyLevels", (parser, x) => x.VeterancyLevels = parser.ParseEnumBitArray<VeterancyLevel>() },
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "ExemptStatus", (parser, x) => x.ExemptStatus = parser.ParseEnum<ObjectStatus>() },
            { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ObjectStatus>() }
        };

        public BitArray<VeterancyLevel> VeterancyLevels { get; private set; }
        public BitArray<DeathType> DeathTypes { get; private set; }
        public ObjectStatus ExemptStatus { get; private set; }
        public ObjectStatus RequiredStatus { get; private set; }
    }
}
