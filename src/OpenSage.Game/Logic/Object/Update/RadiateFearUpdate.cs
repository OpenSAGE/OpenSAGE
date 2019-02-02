using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class RadiateFearUpdateModuleData : UpdateModuleData
    {
        internal static RadiateFearUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RadiateFearUpdateModuleData> FieldParseTable = new IniParseTable<RadiateFearUpdateModuleData>
        {
            { "InitiallyActive", (parser, x) => x.InitiallyActive = parser.ParseBoolean() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseIdentifier() },
            { "WhichSpecialPower", (parser, x) => x.WhichSpecialPower = parser.ParseInteger() },
            { "GenerateTerror", (parser, x) => x.GenerateTerror = parser.ParseBoolean() },
            { "GenerateFear", (parser, x) => x.GenerateFear = parser.ParseBoolean() },
            { "EmotionPulseRadius", (parser, x) => x.EmotionPulseRadius = parser.ParseFloat() },
            { "EmotionPulseInterval", (parser, x) => x.EmotionPulseInterval = parser.ParseInteger() },
            { "VictimFilter", (parser, x) => x.VictimFilter = ObjectFilter.Parse(parser) },
            { "GenerateUncontrollableFear", (parser, x) => x.GenerateUncontrollableFear = parser.ParseBoolean() },
            { "RequiresAllTriggers", (parser, x) => x.RequiresAllTriggers = parser.ParseBoolean() }
        };
        
        public bool InitiallyActive { get; private set; }
        public string TriggeredBy { get; private set; }
        public int WhichSpecialPower { get; private set; }
        public bool GenerateTerror { get; private set; }
        public bool GenerateFear { get; private set; }
        public float EmotionPulseRadius { get; private set; }
        public int EmotionPulseInterval { get; private set; }
        public ObjectFilter VictimFilter { get; private set; }
        public bool GenerateUncontrollableFear { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool RequiresAllTriggers { get; private set; }
    }
}
