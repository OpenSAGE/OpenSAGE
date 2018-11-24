using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class StructureCollapseUpdateModuleData : UpdateModuleData
    {
        internal static StructureCollapseUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StructureCollapseUpdateModuleData> FieldParseTable = new IniParseTable<StructureCollapseUpdateModuleData>
        {
            { "MinCollapseDelay", (parser, x) => x.MinCollapseDelay = parser.ParseInteger() },
            { "MaxCollapseDelay", (parser, x) => x.MaxCollapseDelay = parser.ParseInteger() },
            { "CollapseDamping", (parser, x) => x.CollapseDamping = parser.ParseFloat() },
            { "MaxShudder", (parser, x) => x.MaxShudder = parser.ParseFloat() },
            { "MinBurstDelay", (parser, x) => x.MinBurstDelay = parser.ParseInteger() },
            { "MaxBurstDelay", (parser, x) => x.MaxBurstDelay = parser.ParseInteger() },
            { "BigBurstFrequency", (parser, x) => x.BigBurstFrequency = parser.ParseInteger() },

            { "OCL", (parser, x) => x.OCLs[parser.ParseEnum<StructureCollapsePhase>()] = parser.ParseAssetReference() },
            { "FXList", (parser, x) => x.FXLists[parser.ParseEnum<StructureCollapsePhase>()] = parser.ParseAssetReference() },
            { "DestroyObjectWhenDone", (parser, x) => x.DestroyObjectWhenDone = parser.ParseBoolean() },
            { "CollapseHeight", (parser, x) => x.CollapseHeight = parser.ParseInteger() },
        };

        public int MinCollapseDelay { get; private set; }
        public int MaxCollapseDelay { get; private set; }
        public float CollapseDamping { get; private set; }
        public float MaxShudder { get; private set; }
        public int MinBurstDelay { get; private set; }
        public int MaxBurstDelay { get; private set; }
        public int BigBurstFrequency { get; private set; }

        public Dictionary<StructureCollapsePhase, string> OCLs { get; } = new Dictionary<StructureCollapsePhase, string>();
        public Dictionary<StructureCollapsePhase, string> FXLists { get; } = new Dictionary<StructureCollapsePhase, string>();

        [AddedIn(SageGame.Bfme)]
        public bool DestroyObjectWhenDone { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int CollapseHeight { get; private set; }
    }

    public enum StructureCollapsePhase
    {
        [IniEnum("INITIAL")]
        Initial,

        [IniEnum("DELAY")]
        Delay,

        [IniEnum("BURST")]
        Burst,

        [IniEnum("FINAL")]
        Final,

        [IniEnum("ALMOST_FINAL"), AddedIn(SageGame.Bfme)]
        AlmostFinal
    }
}
