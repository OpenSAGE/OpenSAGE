using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class DieModule : BehaviorModule
    {
        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);
        }
    }

    public abstract class DieModuleData : BehaviorModuleData
    {
        public override ModuleKind ModuleKind => ModuleKind.Die;

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
