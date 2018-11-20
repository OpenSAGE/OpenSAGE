using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class EmotionTrackerUpdateModuleData : UpdateModuleData
    {
        internal static EmotionTrackerUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<EmotionTrackerUpdateModuleData> FieldParseTable = new IniParseTable<EmotionTrackerUpdateModuleData>
        {
            { "TauntAndPointDistance", (parser, x) => x.TauntAndPointDistance = parser.ParseInteger() },
            { "TauntAndPointUpdateDelay", (parser, x) => x.TauntAndPointUpdateDelay = parser.ParseInteger() },
            { "TauntAndPointExcluded", (parser, x) => x.TauntAndPointExcluded = ObjectFilter.Parse(parser) },
            { "AfraidOf", (parser, x) => x.AfraidOf = ObjectFilter.Parse(parser) },
            { "PointAt", (parser, x) => x.PointAt = ObjectFilter.Parse(parser) },
            { "FearScanDistance", (parser, x) => x.FearScanDistance = parser.ParseInteger() },
            { "AddEmotion", (parser, x) => x.AddEmotion = parser.ParseAssetReference() },
        };
        public int TauntAndPointDistance { get; internal set; }
		public int TauntAndPointUpdateDelay	{ get; internal set; }
		public ObjectFilter TauntAndPointExcluded { get; internal set; }
 		public ObjectFilter AfraidOf { get; internal set; }
 		public ObjectFilter PointAt { get; internal set; }
		public int FearScanDistance { get; internal set; }
		public string AddEmotion { get; internal set; }
    }
}
