using OpenSage.Data.Ini.Parser;

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

        public string BuildingTemplateName { get; internal set; }
        public string PlacementPrefix {get; internal set; }
        public int PlacementIndex { get; internal set; }
    }
}
