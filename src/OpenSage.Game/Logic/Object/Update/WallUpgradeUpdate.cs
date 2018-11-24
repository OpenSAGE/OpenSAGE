using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class WallUpgradeUpdateModuleData : UpdateModuleData
    {
        internal static WallUpgradeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<WallUpgradeUpdateModuleData> FieldParseTable = new IniParseTable<WallUpgradeUpdateModuleData>();
    }
}
