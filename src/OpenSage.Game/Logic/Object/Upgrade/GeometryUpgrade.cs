using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class GeometryUpgradeModuleData : UpgradeModuleData
    {
        internal static GeometryUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<GeometryUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<GeometryUpgradeModuleData>
            {
                { "ShowGeometry", (parser, x) => x.ShowGeometry = parser.ParseAssetReference() },
                { "HideGeometry", (parser, x) => x.HideGeometry = parser.ParseAssetReference() },
                { "WallBoundsMesh", (parser,x) => x.WallBoundsMesh = parser.ParseAssetReference() },
                { "RampMesh1", (parser, x) => x.RampMesh1 = parser.ParseAssetReference() },
                { "RampMesh2", (parser, x) => x.RampMesh2 = parser.ParseAssetReference() },
            });

        public string ShowGeometry { get; private set; }
        public string HideGeometry { get; private set; }
        public string WallBoundsMesh { get; private set; }
        public string RampMesh1 { get; private set; }
        public string RampMesh2 { get; private set; }
    }
}
