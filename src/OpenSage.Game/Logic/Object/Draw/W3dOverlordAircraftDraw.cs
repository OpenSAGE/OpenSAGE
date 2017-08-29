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
    public sealed class W3dOverlordAircraftDraw : W3dModelDrawModuleData
    {
        internal static W3dOverlordAircraftDraw Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dOverlordAircraftDraw> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dOverlordAircraftDraw>());
    }
}
