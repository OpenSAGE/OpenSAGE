using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Special-case draw module which is interdependent with the W3DDependencyModelDraw module.
    /// Allows other objects to be attached to this object through use of AttachToBoneInContainer 
    /// logic.
    /// </summary>
    public sealed class W3dOverlordTankDrawModuleData : W3dTankDrawModuleData
    {
        internal static new W3dOverlordTankDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dOverlordTankDrawModuleData> FieldParseTable = W3dTankDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dOverlordTankDrawModuleData>());
    }
}
