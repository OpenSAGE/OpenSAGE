using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class StructureCollapseUpdateBehavior : ObjectBehavior
    {
        internal static StructureCollapseUpdateBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<StructureCollapseUpdateBehavior> FieldParseTable = new IniParseTable<StructureCollapseUpdateBehavior>
        {
            { "MinCollapseDelay", (parser, x) => x.MinCollapseDelay = parser.ParseInteger() },
            { "MaxCollapseDelay", (parser, x) => x.MaxCollapseDelay = parser.ParseInteger() },
            { "CollapseDamping", (parser, x) => x.CollapseDamping = parser.ParseFloat() },
            { "MaxShudder", (parser, x) => x.MaxShudder = parser.ParseFloat() },
            { "MinBurstDelay", (parser, x) => x.MinBurstDelay = parser.ParseInteger() },
            { "MaxBurstDelay", (parser, x) => x.MaxBurstDelay = parser.ParseInteger() },
            { "BigBurstFrequency", (parser, x) => x.BigBurstFrequency = parser.ParseInteger() },

            { "OCL", (parser, x) => x.OCLs[parser.ParseEnum<StructureCollapseStage>()] = parser.ParseAssetReference() },
            { "FXList", (parser, x) => x.FXLists[parser.ParseEnum<StructureCollapseStage>()] = parser.ParseAssetReference() },
        };

        public int MinCollapseDelay { get; private set; }
        public int MaxCollapseDelay { get; private set; }
        public float CollapseDamping { get; private set; }
        public float MaxShudder { get; private set; }
        public int MinBurstDelay { get; private set; }
        public int MaxBurstDelay { get; private set; }
        public int BigBurstFrequency { get; private set; }

        public Dictionary<StructureCollapseStage, string> OCLs { get; } = new Dictionary<StructureCollapseStage, string>();
        public Dictionary<StructureCollapseStage, string> FXLists { get; } = new Dictionary<StructureCollapseStage, string>();
    }

    public enum StructureCollapseStage
    {
        [IniEnum("INITIAL")]
        Initial,

        [IniEnum("DELAY")]
        Delay,

        [IniEnum("BURST")]
        Burst,

        [IniEnum("FINAL")]
        Final
    }
}
