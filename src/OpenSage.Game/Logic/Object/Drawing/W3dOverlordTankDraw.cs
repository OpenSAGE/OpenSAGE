using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Special-case draw module which is interdependent with the W3DDependencyModelDraw module.
    /// Allows other objects to be attached to this object through use of AttachToBoneInContainer 
    /// logic.
    /// </summary>
    public sealed class W3dOverlordTankDraw : W3dTankDraw
    {
        internal static new W3dOverlordTankDraw Parse(IniParser parser) => parser.ParseBlock(OverlordTankFieldParseTable);

        internal static readonly IniParseTable<W3dOverlordTankDraw> OverlordTankFieldParseTable = new IniParseTable<W3dOverlordTankDraw>()
            .Concat<W3dOverlordTankDraw, W3dTankDraw>(TankFieldParseTable);
    }
}
