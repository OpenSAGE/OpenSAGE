using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Special case draw module which is interdependent with the W3DOverlordTankDraw, 
    /// W3DOverlordAirCraftDraw and W3DOverlordTruckDraw modules.
    /// </summary>
    public sealed class W3dDependencyModelDraw : W3dModelDraw
    {
        internal static W3dDependencyModelDraw Parse(IniParser parser) => parser.ParseBlock(DependencyModelFieldParseTable);

        private static readonly IniParseTable<W3dDependencyModelDraw> DependencyModelFieldParseTable = new IniParseTable<W3dDependencyModelDraw>
        {
            { "AttachToBoneInContainer", (parser, x) => x.AttachToBoneInContainer = parser.ParseBoneName() }
        }.Concat<W3dDependencyModelDraw, W3dModelDraw>(ModelFieldParseTable);

        public string AttachToBoneInContainer { get; private set; }
    }
}
