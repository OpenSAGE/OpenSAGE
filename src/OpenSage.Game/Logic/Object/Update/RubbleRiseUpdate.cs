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

        public int MinRubbleRiseDelay { get; private set; }
        public int MaxRubbleRiseDelay { get; private set; }
        public float RubbleRiseDamping { get; private set; }
        public float RubbleHeight { get; private set; }
        public float MaxShudder { get; private set; }
        public int MinBurstDelay { get; private set; }
        public int MaxBurstDelay { get; private set; }
        public int BigBurstFrequency { get; private set; }
        public Dictionary<StructureCollapsePhase, string> FXLists { get; } = new Dictionary<StructureCollapsePhase, string>();
    }
}
