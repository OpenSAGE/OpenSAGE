using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class DynamicShroudClearingRangeUpdateModuleData : UpdateModuleData
    {
        internal static DynamicShroudClearingRangeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DynamicShroudClearingRangeUpdateModuleData> FieldParseTable = new IniParseTable<DynamicShroudClearingRangeUpdateModuleData>
        {
            { "FinalVision", (parser, x) => x.FinalVision = parser.ParseFloat() },
            { "ChangeInterval", (parser, x) => x.ChangeInterval = parser.ParseInteger() },
            { "ShrinkDelay", (parser, x) => x.ShrinkDelay = parser.ParseInteger() },
            { "ShrinkTime", (parser, x) => x.ShrinkTime = parser.ParseInteger() },
            { "GrowDelay", (parser, x) => x.GrowDelay = parser.ParseInteger() },
            { "GrowTime", (parser, x) => x.GrowTime = parser.ParseInteger() },
            { "GrowInterval", (parser, x) => x.GrowInterval = parser.ParseInteger() },

            { "GridDecalTemplate", (parser, x) => x.GridDecalTemplate = RadiusDecalTemplate.Parse(parser) }
        };

        public float FinalVision { get; private set; }
        public int ChangeInterval { get; private set; }
        public int ShrinkDelay { get; private set; }
        public int ShrinkTime { get; private set; }
        public int GrowDelay { get; private set; }
        public int GrowTime { get; private set; }
        public int GrowInterval { get; private set; }

        public RadiusDecalTemplate GridDecalTemplate { get; private set; }
    }
}
