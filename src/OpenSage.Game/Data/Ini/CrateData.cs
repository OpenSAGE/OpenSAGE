using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class CrateData
    {
        internal static CrateData Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<CrateData> FieldParseTable = new IniParseTable<CrateData>
        {
            { "CreationChance", (parser, x) => x.CreationChance = parser.ParseFloat() },
            { "KilledByType", (parser, x) => x.KilledByType = parser.ParseEnum<ObjectKinds>() },
            { "KillerScience", (parser, x) => x.KillerScience = parser.ParseAssetReference() },
            { "VeterancyLevel", (parser, x) => x.VeterancyLevel = parser.ParseEnum<VeterancyLevel>() },
            { "OwnedByMaker", (parser, x) => x.OwnedByMaker = parser.ParseBoolean() },
            { "CrateObject", (parser, x) => x.CrateObjects.Add(CrateObject.Parse(parser)) },
        };

        public string Name { get; private set; }

        public float CreationChance { get; private set; }
        public ObjectKinds? KilledByType { get; private set; }
        public string KillerScience { get; private set; }
        public VeterancyLevel? VeterancyLevel { get; private set; }
        public bool OwnedByMaker { get; private set; }

        public List<CrateObject> CrateObjects { get; } = new List<CrateObject>();
    }

    public enum VeterancyLevel
    {
        [IniEnum("REGULAR")]
        Regular,

        [IniEnum("VETERAN")]
        Veteran,

        [IniEnum("ELITE")]
        Elite,

        [IniEnum("HEROIC")]
        Heroic
    }

    public sealed class CrateObject
    {
        internal static CrateObject Parse(IniParser parser)
        {
            return new CrateObject
            {
                ObjectName = parser.ParseAssetReference(),
                Probability = parser.ParseFloat()
            };
        }

        public string ObjectName { get; private set; }
        public float Probability { get; private set; }
    }
}
