using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class RubbleRiseUpdateModuleData : UpdateModuleData
    {
        internal static RubbleRiseUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RubbleRiseUpdateModuleData> FieldParseTable = new IniParseTable<RubbleRiseUpdateModuleData>
        {
            { "MinRubbleRiseDelay", (parser, x) => x.MinRubbleRiseDelay = parser.ParseInteger() },
            { "MaxRubbleRiseDelay", (parser, x) => x.MaxRubbleRiseDelay = parser.ParseInteger() },
            { "RubbleRiseDamping", (parser, x) => x.RubbleRiseDamping = parser.ParseFloat() },
            { "RubbleHeight", (parser, x) => x.RubbleHeight = parser.ParseFloat() },
            { "MaxShudder", (parser, x) => x.MaxShudder = parser.ParseFloat() },
            { "MinBurstDelay", (parser, x) => x.MinBurstDelay = parser.ParseInteger() },
            { "MaxBurstDelay", (parser, x) => x.MaxBurstDelay = parser.ParseInteger() },
            { "BigBurstFrequency", (parser, x) => x.BigBurstFrequency = parser.ParseInteger() },
            { "FXList", (parser, x) => x.FXLists[parser.ParseEnum<StructureCollapsePhase>()] = parser.ParseAssetReference() },
        };

        public int MinRubbleRiseDelay { get; internal set; }
        public int MaxRubbleRiseDelay { get; internal set; }
        public float RubbleRiseDamping { get; internal set; }
        public float RubbleHeight { get; internal set; }
        public float MaxShudder { get; internal set; }
        public int MinBurstDelay { get; internal set; }
        public int MaxBurstDelay { get; internal set; }
        public int BigBurstFrequency { get; internal set; }
        public Dictionary<StructureCollapsePhase, string> FXLists { get; } = new Dictionary<StructureCollapsePhase, string>();
    }
}
