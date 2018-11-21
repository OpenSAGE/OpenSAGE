using System.Collections.Generic;
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
            { "AlwaysAfraidOf", (parser, x) => x.AlwaysAfraidOf = ObjectFilter.Parse(parser) },
            { "PointAt", (parser, x) => x.PointAt = ObjectFilter.Parse(parser) },
            { "FearScanDistance", (parser, x) => x.FearScanDistance = parser.ParseInteger() },
            { "AddEmotion", (parser, x) => x.Emotions.Add(Emotion.Parse(parser)) },
            { "HeroScanDistance", (parser, x) => x.HeroScanDistance = parser.ParseInteger() }
        };
        public int TauntAndPointDistance { get; internal set; }
		public int TauntAndPointUpdateDelay	{ get; internal set; }
		public ObjectFilter TauntAndPointExcluded { get; internal set; }
 		public ObjectFilter AfraidOf { get; internal set; }
        public ObjectFilter AlwaysAfraidOf { get; internal set; }
 		public ObjectFilter PointAt { get; internal set; }
		public int FearScanDistance { get; internal set; }
		public List<Emotion> Emotions { get; internal set; } = new List<Emotion>();
        public int HeroScanDistance { get; internal set; }
    }

    public sealed class Emotion
    {
        internal static Emotion Parse(IniParser parser)
        {
            var firstToken = parser.GetNextToken();
            var secondToken = parser.GetNextTokenOptional();

            Emotion result;
            if (secondToken.HasValue)
            {
                result = parser.ParseBlock(FieldParseTable);
                result.Type = IniParser.ParseEnum<EmotionType>(firstToken);
                result.EmotionName = parser.ScanAssetReference(secondToken.Value);
            }
            else
            {
                result = new Emotion();
                result.Type = EmotionType.None;
                result.EmotionName = parser.ScanAssetReference(firstToken);
            }
            return result;
        }

        internal static readonly IniParseTable<Emotion> FieldParseTable = new IniParseTable<Emotion>
        {
            //{ "AnimationName", (parser, x) => x.AnimationName = parser.ParseString() },
        };

        public EmotionType Type { get; internal set; }
        public string EmotionName { get; internal set; }
    }

    public enum EmotionType
    {
        None = 0,

        [IniEnum("OVERRIDE")]
        Override,
    }
}
