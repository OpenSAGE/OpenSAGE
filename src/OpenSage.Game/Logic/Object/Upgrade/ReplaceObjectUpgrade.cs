using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class ReplaceObjectUpgradeModuleData : UpgradeModuleData
    {
        internal static ReplaceObjectUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ReplaceObjectUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<ReplaceObjectUpgradeModuleData>
            {
                { "ReplaceObject", (parser, x) => x.ReplaceObject = parser.ParseAssetReference() },
            });

        public string ReplaceObject { get; private set; }
    }
}
