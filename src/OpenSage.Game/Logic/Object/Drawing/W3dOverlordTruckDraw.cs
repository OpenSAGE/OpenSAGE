using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Special-case draw module which is interdependent with the W3DDependencyModelDraw module.
    /// Allows other objects to be attached to this object through use of AttachToBoneInContainer 
    /// logic.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class W3dOverlordTruckDraw : W3dTruckDraw
    {
        internal static new W3dOverlordTruckDraw Parse(IniParser parser) => parser.ParseBlock(OverlordTruckFieldParseTable);

        internal static readonly IniParseTable<W3dOverlordTruckDraw> OverlordTruckFieldParseTable = new IniParseTable<W3dOverlordTruckDraw>()
            .Concat<W3dOverlordTruckDraw, W3dTruckDraw>(TruckFieldParseTable);
    }
}
