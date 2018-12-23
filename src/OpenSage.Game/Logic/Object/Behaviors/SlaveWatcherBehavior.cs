using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class SlaveWatcherBehaviorModuleData : UpdateModuleData
    {
        internal static SlaveWatcherBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<SlaveWatcherBehaviorModuleData> FieldParseTable = new IniParseTable<SlaveWatcherBehaviorModuleData>
        {
            { "RemoveUpgrade", (parser, x) => x.RemoveUpgrade = parser.ParseAssetReference() },
            { "GrantUpgrade", (parser, x) => x.GrantUpgrade = parser.ParseAssetReference() },
            { "ShareUpgrades", (parser, x) => x.ShareUpgrades = parser.ParseBoolean() },
            { "LetSlaveLive", (parser, x) => x.LetSlaveLive = parser.ParseBoolean() },
        };

        public string RemoveUpgrade { get; private set; }
        public string GrantUpgrade { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ShareUpgrades { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool LetSlaveLive { get; private set; }
    }
}
