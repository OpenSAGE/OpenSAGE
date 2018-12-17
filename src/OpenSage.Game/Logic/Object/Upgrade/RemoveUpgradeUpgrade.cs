using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class RemoveUpgradeUpgradeModuleData : UpgradeModuleData
    {
        internal static RemoveUpgradeUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RemoveUpgradeUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<RemoveUpgradeUpgradeModuleData>
            {
                { "UpgradeGroupsToRemove", (parser, x) => x.UpgradeGroupsToRemove = parser.ParseAssetReference() },
                { "UpgradeToRemove", (parser, x) => x.UpgradeToRemove = parser.ParseAssetReferenceArray() }
            });

        public string UpgradeGroupsToRemove { get; private set; }
        public string[] UpgradeToRemove { get; private set; }
    }
}
