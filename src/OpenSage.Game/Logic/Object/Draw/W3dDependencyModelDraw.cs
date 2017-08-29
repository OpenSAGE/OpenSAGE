using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Special case draw module which is interdependent with the W3DOverlordTankDraw, 
    /// W3DOverlordAirCraftDraw and W3DOverlordTruckDraw modules.
    /// </summary>
    public sealed class W3dDependencyModelDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dDependencyModelDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dDependencyModelDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dDependencyModelDrawModuleData>
            {
                { "AttachToBoneInContainer", (parser, x) => x.AttachToBoneInContainer = parser.ParseBoneName() }
            });

        public string AttachToBoneInContainer { get; private set; }
    }
}
