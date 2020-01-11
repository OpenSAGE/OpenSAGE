using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class BezierProjectileBehaviorData : BehaviorModuleData
    {
        internal static BezierProjectileBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<BezierProjectileBehaviorData> FieldParseTable = new IniParseTable<BezierProjectileBehaviorData>
        {
            { "FirstHeight", (parser, x) => x.FirstHeight = parser.ParseInteger() },
            { "SecondHeight", (parser, x) => x.SecondHeight = parser.ParseInteger() },
            { "FirstPercentIndent", (parser, x) => x.FirstPercentIndent = parser.ParsePercentage() },
            { "SecondPercentIndent", (parser, x) => x.SecondPercentIndent = parser.ParsePercentage() },
            { "TumbleRandomly", (parser, x) => x.TumbleRandomly = parser.ParseBoolean() },
            { "CrushStyle", (parser, x) => x.CrushStyle = parser.ParseBoolean() },
            { "DieOnImpact", (parser, x) => x.DieOnImpact = parser.ParseBoolean() },
            { "BounceCount", (parser, x) => x.BounceCount = parser.ParseInteger() },
            { "BounceDistance", (parser, x) => x.BounceDistance = parser.ParseInteger() },
            { "BounceFirstHeight", (parser, x) => x.BounceFirstHeight = parser.ParseInteger() },
            { "BounceSecondHeight", (parser, x) => x.BounceSecondHeight = parser.ParseInteger() },
            { "BounceFirstPercentIndent", (parser, x) => x.BounceFirstPercentIndent = parser.ParsePercentage() },
            { "BounceSecondPercentIndent", (parser, x) => x.BounceSecondPercentIndet = parser.ParsePercentage() },
            { "GroundHitFX", (parser, x) => x.GroundHitFX = parser.ParseAssetReference() },
            { "GroundHitWeapon", (parser, x) => x.GroundHitWeapon = parser.ParseAssetReference() },
            { "GroundBounceFX", (parser, x) => x.GroundBounceFX = parser.ParseAssetReference() },
            { "GroundBounceWeapon", (parser, x) => x.GroundBounceWeapon = parser.ParseAssetReference() },
            { "DetonateCallsKill", (parser, x) => x.DetonateCallsKill = parser.ParseBoolean() },
            { "FlightPathAdjustDistPerSecond", (parser, x) => x.FlightPathAdjustDistPerSecond = parser.ParseInteger() },
            { "CurveFlattenMinDist", (parser, x) => x.CurveFlattenMinDist = parser.ParseFloat() },
            { "InvisibleFrames", (parser, x) => x.InvisibleFrames = parser.ParseInteger() },
            { "PreLandingStateTime", (parser, x) => x.PreLandingStateTime = parser.ParseInteger() },
            { "PreLandingEmotion", (parser, x) => x.PreLandingEmotion = parser.ParseEnum<EmotionType>() },
            { "PreLandingEmotionRadius", (parser, x) => x.PreLandingEmotionRadius = parser.ParseFloat() },
            { "FadeInTime", (parser, x) => x.FadeInTime = parser.ParseInteger() },
            { "IgnoreTerrainHeight", (parser, x) => x.IgnoreTerrainHeight = parser.ParseBoolean() },
            { "FirstPercentHeight", (parser, x) => x.FirstPercentHeight = parser.ParsePercentage() },
            { "SecondPercentHeight", (parser, x) => x.SecondPercentHeight = parser.ParsePercentage() },
            { "FinalStuckTime", (parser, x) => x.FinalStuckTime = parser.ParseInteger() },
            { "OrientToFlightPath", (parser, x) => x.OrientToFlightPath = parser.ParseBoolean() },
            { "PreLandingEmotionAffectsAllies", (parser, x) => x.PreLandingEmotionAffectsAllies = parser.ParseBoolean() },
        };

        public int FirstHeight { get; private set; }
        public int SecondHeight { get; private set; }
        public Percentage FirstPercentIndent { get; private set; }
        public Percentage SecondPercentIndent { get; private set; }
        public bool TumbleRandomly { get; private set; }

        public bool CrushStyle { get; private set; }
        public bool DieOnImpact { get; private set; }
        public int BounceCount { get; private set; }
        public int BounceDistance { get; private set; }
        public int BounceFirstHeight { get; private set; }
        public int BounceSecondHeight { get; private set; }
        public Percentage BounceFirstPercentIndent { get; private set; }
        public Percentage BounceSecondPercentIndet { get; private set; }
        public string GroundHitFX { get; private set; }
        public string GroundHitWeapon { get; private set; }
        public string GroundBounceFX { get; private set; }
        public string GroundBounceWeapon { get; private set; }
        public bool DetonateCallsKill { get; private set; }
        public int FlightPathAdjustDistPerSecond { get; private set; }
        public float CurveFlattenMinDist { get; private set; }
        public int InvisibleFrames { get; private set; }
        public int PreLandingStateTime { get; private set; }
        public EmotionType PreLandingEmotion { get; private set; }
        public float PreLandingEmotionRadius { get; private set; }
        public int FadeInTime { get; private set; }
        public bool IgnoreTerrainHeight { get; private set; }
        public Percentage FirstPercentHeight { get; private set; }
        public Percentage SecondPercentHeight { get; private set; }
        public int FinalStuckTime { get; private set; }
        public bool OrientToFlightPath { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool PreLandingEmotionAffectsAllies { get; private set; }
    }
}
