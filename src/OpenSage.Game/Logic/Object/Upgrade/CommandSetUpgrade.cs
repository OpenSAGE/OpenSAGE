using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Gives the object the ability to change commandsets back and forth when this module two times.
    /// </summary>
    public sealed class CommandSetUpgradeModuleData : UpgradeModuleData
    {
        internal static CommandSetUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CommandSetUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<CommandSetUpgradeModuleData>
            {
                { "RemovesUpgrades", (parser, x) => x.RemovesUpgrades = parser.ParseAssetReferenceArray() },
                { "CommandSet", (parser, x) => x.CommandSet = parser.ParseAssetReference() },
                { "CommandSetAlt", (parser, x) => x.CommandSetAlt = parser.ParseAssetReference() },
                { "TriggerAlt", (parser, x) => x.TriggerAlt = parser.ParseAssetReference() }
            });

        public string[] RemovesUpgrades { get; private set; }
        public string CommandSet { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string CommandSetAlt { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string TriggerAlt { get; private set; }
    }
}
