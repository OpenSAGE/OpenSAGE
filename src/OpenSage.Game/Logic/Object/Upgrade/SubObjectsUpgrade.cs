using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Shows/hides sub-objects on this object's model via upgrading.
    /// </summary>
    public sealed class SubObjectsUpgradeModuleData : UpgradeModuleData
    {
        internal static SubObjectsUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SubObjectsUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<SubObjectsUpgradeModuleData>
            {
                { "ShowSubObjects", (parser, x) => x.ShowSubObjects = parser.ParseAssetReferenceArray() },
                { "HideSubObjects", (parser, x) => x.HideSubObjects = parser.ParseAssetReferenceArray() },
            });

        public string[] ShowSubObjects { get; private set; }
        public string[] HideSubObjects { get; private set; }
    }
}
