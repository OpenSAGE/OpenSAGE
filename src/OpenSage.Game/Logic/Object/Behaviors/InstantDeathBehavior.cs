using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class InstantDeathBehaviorModuleData : BehaviorModuleData
    {
        internal static InstantDeathBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InstantDeathBehaviorModuleData> FieldParseTable = new IniParseTable<InstantDeathBehaviorModuleData>
        {
            { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ObjectStatus>() },
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "FX", (parser, x) => x.FX = parser.ParseAssetReference() },
            { "OCL", (parser, x) => x.OCL = parser.ParseAssetReference() },
        };

        public ObjectStatus RequiredStatus { get; private set; }
        public BitArray<DeathType> DeathTypes { get; private set; }
        public string FX { get; private set; }
        public string OCL { get; private set; }
    }
}
