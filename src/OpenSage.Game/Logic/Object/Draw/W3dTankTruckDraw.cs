using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Special case module that allows parameters from W3DTankDraw and W3DTruckDraw to be used. 
    /// Effectively this is a module that combines the other two.
    /// This can be useful for half track type units.
    /// </summary>
    public sealed class W3dTankTruckDrawModuleData : W3dTruckDrawModuleData
    {
        internal static new W3dTankTruckDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dTankTruckDrawModuleData> FieldParseTable = W3dTruckDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dTankTruckDrawModuleData>
            {
                { "TreadAnimationRate", (parser, x) => x.TreadAnimationRate = parser.ParseFloat() },
                { "TreadDriveSpeedFraction", (parser, x) => x.TreadDriveSpeedFraction = parser.ParseFloat() },
                { "TreadPivotSpeedFraction", (parser, x) => x.TreadPivotSpeedFraction = parser.ParseFloat() },
            });

        // Duplicated from W3dTankDraw
        public float TreadAnimationRate { get; private set; }
        public float TreadDriveSpeedFraction { get; private set; }
        public float TreadPivotSpeedFraction { get; private set; }
    }
}
