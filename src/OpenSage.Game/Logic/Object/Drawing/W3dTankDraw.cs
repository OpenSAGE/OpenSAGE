using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Default Draw used by tanks. Hardcoded to call for the TrackDebrisDirtRight and 
    /// TrackDebrisDirtLeft particle system definitions.
    /// </summary>
    public class W3dTankDraw : W3dModelDraw
    {
        internal static W3dTankDraw Parse(IniParser parser) => parser.ParseBlock(TankFieldParseTable);

        internal static readonly IniParseTable<W3dTankDraw> TankFieldParseTable = new IniParseTable<W3dTankDraw>
        {
            { "TrackMarks", (parser, x) => x.TrackMarks = parser.ParseFileName() },

            { "TreadDebrisLeft", (parser, x) => x.TreadDebrisLeft = parser.ParseAssetReference() },
            { "TreadDebrisRight", (parser, x) => x.TreadDebrisRight = parser.ParseAssetReference() },

            { "TreadAnimationRate", (parser, x) => x.TreadAnimationRate = parser.ParseFloat() },
            { "TreadDriveSpeedFraction", (parser, x) => x.TreadDriveSpeedFraction = parser.ParseFloat() },
            { "TreadPivotSpeedFraction", (parser, x) => x.TreadPivotSpeedFraction = parser.ParseFloat() },

            { "InitialRecoilSpeed", (parser, x) => x.InitialRecoilSpeed = parser.ParseInteger() },
            { "MaxRecoilDistance", (parser, x) => x.MaxRecoilDistance = parser.ParseFloat() },
            { "RecoilSettleSpeed", (parser, x) => x.RecoilSettleSpeed = parser.ParseInteger() },
        }.Concat<W3dTankDraw, W3dModelDraw>(ModelFieldParseTable);

        public string TrackMarks { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string TreadDebrisLeft { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string TreadDebrisRight { get; private set; }

        public float TreadAnimationRate { get; private set; }
        public float TreadDriveSpeedFraction { get; private set; }
        public float TreadPivotSpeedFraction { get; private set; }

        public int InitialRecoilSpeed { get; private set; }
        public float MaxRecoilDistance { get; private set; }
        public int RecoilSettleSpeed { get; private set; }
    }
}
