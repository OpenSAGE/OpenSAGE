using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class ReplaceSelfUpgradeModuleData : UpgradeModuleData
    {
        internal static ReplaceSelfUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ReplaceSelfUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<ReplaceSelfUpgradeModuleData>
            {
                { "ReplaceWith", (parser, x) => x.ReplaceWith = parser.ParseAssetReference() },
            });

        public string ReplaceWith { get; private set; }
    }
}
