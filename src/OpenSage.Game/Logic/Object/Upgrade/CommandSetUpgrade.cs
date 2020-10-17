using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Gui.ControlBar;

namespace OpenSage.Logic.Object
{
    public class CommandSetUpgrade : UpgradeModule
    {
        private readonly CommandSetUpgradeModuleData _moduleData;
        private readonly LazyAssetReference<CommandSet> _defaultCommandSet;

        public CommandSetUpgrade(GameObject gameObject, CommandSetUpgradeModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
            _defaultCommandSet = gameObject.Definition.CommandSet;
        }

        internal override void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            if (triggered)
            {
                _gameObject.Definition.CommandSet = _moduleData.CommandSet;
            }
            else
            {
                _gameObject.Definition.CommandSet = _defaultCommandSet;
            }
        }
    }

    /// <summary>
    /// Gives the object the ability to change commandsets back and forth when this module exists multiple times.
    /// </summary>
    public sealed class CommandSetUpgradeModuleData : UpgradeModuleData
    {
        internal static CommandSetUpgradeModuleData Parse(IniParser parser)
        {
            var result = parser.ParseBlock(FieldParseTable);
            if (result.CommandSetAlt != null
                || result.TriggerAlt != null)
            {
                var k = 0;
            }
            return result;
        }

        private static new readonly IniParseTable<CommandSetUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<CommandSetUpgradeModuleData>
            {
                { "RemovesUpgrades", (parser, x) => x.RemovesUpgrades = parser.ParseAssetReferenceArray() },
                { "CommandSet", (parser, x) => x.CommandSet = parser.ParseCommandSetReference() },
                { "CommandSetAlt", (parser, x) => x.CommandSetAlt = parser.ParseAssetReference() },
                { "TriggerAlt", (parser, x) => x.TriggerAlt = parser.ParseAssetReference() }
            });

        public string[] RemovesUpgrades { get; private set; }
        public LazyAssetReference<CommandSet> CommandSet { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string CommandSetAlt { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string TriggerAlt { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new CommandSetUpgrade(gameObject, this);
        }
    }
}
