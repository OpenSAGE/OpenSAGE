using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LevelUpUpgrade : UpgradeModule
    {
        private readonly GameObject _gameObject;
        private readonly LevelUpUpgradeModuleData _moduleData;

        internal LevelUpUpgrade(GameObject gameObject, LevelUpUpgradeModuleData moduleData)
            : base(moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal override void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            if (triggered)
            {
                //_gameObject.Se
            }
        }
    }


    [AddedIn(SageGame.Bfme)]
    public sealed class LevelUpUpgradeModuleData : UpgradeModuleData
    {
        internal static LevelUpUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<LevelUpUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<LevelUpUpgradeModuleData>
            {
                { "LevelsToGain", (parser, x) => x.LevelsToGain = parser.ParseInteger() },
                { "LevelCap", (parser, x) => x.LevelCap = parser.ParseInteger() }
            });

        public int LevelsToGain { get; private set; }
        public int LevelCap { get; private set; }
    }
}
