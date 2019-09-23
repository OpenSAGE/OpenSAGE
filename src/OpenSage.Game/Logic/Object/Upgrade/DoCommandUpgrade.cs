using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class DoCommandUpgradeModuleData : UpgradeModuleData
    {
        internal static DoCommandUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DoCommandUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<DoCommandUpgradeModuleData>
            {
                { "GetUpgradeCommandButtonName", (parser, x) => x.GetUpgradeCommandButtonName = parser.ParseAssetReference() },
                { "RemoveUpgradeCommandButtonName", (parser, x) => x.RemoveUpgradeCommandButtonName = parser.ParseAssetReference() },
            });

        public string GetUpgradeCommandButtonName { get; private set; }
        public string RemoveUpgradeCommandButtonName { get; private set; }
    }
}
