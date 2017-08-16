using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to call for the TreadDebrisRight and TreadDebrisLeft (unless overriden) particle 
    /// system definitions and allows use of TruckPowerslideSound and TruckLandingSound within the 
    /// UnitSpecificSounds section of the object.
    /// 
    /// This module also includes automatic logic for showing and hiding of HEADLIGHT bones in and 
    /// out of the NIGHT ConditionState.
    /// </summary>
    public sealed class W3dTruckDraw : W3dModelDraw
    {
        internal static W3dTruckDraw Parse(IniParser parser)
        {
            return parser.ParseBlock(SupplyFieldParseTable);
        }

        private static readonly IniParseTable<W3dTruckDraw> SupplyFieldParseTable = new IniParseTable<W3dTruckDraw>
        {
            
        }.Concat<W3dTruckDraw, W3dModelDraw>(ModelFieldParseTable);
    }
}
