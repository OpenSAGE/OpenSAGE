using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Special-case draw module which is interdependent with the W3DDependencyModelDraw module.
    /// Allows other objects to be attached to this object through use of AttachToBoneInContainer 
    /// logic.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class W3dOverlordTruckDrawModuleData : W3dTruckDrawModuleData
    {
        internal static new W3dOverlordTruckDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dOverlordTruckDrawModuleData> FieldParseTable = W3dTruckDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dOverlordTruckDrawModuleData>());
    }
}
