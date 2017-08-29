using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Default Draw used by tanks. Hardcoded to call for the TrackDebrisDirtRight and 
    /// TrackDebrisDirtLeft particle system definitions.
    /// </summary>
    public class W3dTankDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dTankDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dTankDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dTankDrawModuleData>
            {
                { "TreadDebrisLeft", (parser, x) => x.TreadDebrisLeft = parser.ParseAssetReference() },
                { "TreadDebrisRight", (parser, x) => x.TreadDebrisRight = parser.ParseAssetReference() },

                { "TreadAnimationRate", (parser, x) => x.TreadAnimationRate = parser.ParseFloat() },
                { "TreadDriveSpeedFraction", (parser, x) => x.TreadDriveSpeedFraction = parser.ParseFloat() },
                { "TreadPivotSpeedFraction", (parser, x) => x.TreadPivotSpeedFraction = parser.ParseFloat() },
            });

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string TreadDebrisLeft { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string TreadDebrisRight { get; private set; }

        public float TreadAnimationRate { get; private set; }
        public float TreadDriveSpeedFraction { get; private set; }
        public float TreadPivotSpeedFraction { get; private set; }
    }
}
