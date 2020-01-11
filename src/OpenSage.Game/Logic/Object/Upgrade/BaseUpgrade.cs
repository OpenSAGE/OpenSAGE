using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class BaseUpgradeModuleData : UpgradeModuleData
    {
        internal static BaseUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<BaseUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<BaseUpgradeModuleData>
            {
                { "BuildingTemplateName", (parser, x) => x.BuildingTemplateName = parser.ParseString() },
                { "PlacementPrefix", (parser, x) => x.PlacementPrefix = parser.ParseString() },
                { "PlacementIndex", (parser, x) => x.PlacementIndex = parser.ParseInteger() },
            });

        public string BuildingTemplateName { get; private set; }
        public string PlacementPrefix { get; private set; }
        public int PlacementIndex { get; private set; }
    }
}
