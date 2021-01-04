using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    internal sealed class ExperienceLevelCreateBehavior : CreateModule
    {
        GameObject _gameObject;
        ExperienceLevelCreateModuleData _moduleData;

        internal ExperienceLevelCreateBehavior(GameObject gameObject, ExperienceLevelCreateModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
        }

        internal override void Execute(BehaviorUpdateContext context)
        {
            _gameObject.Rank = _moduleData.LevelToGrant;
        }
    }

    public sealed class ExperienceLevelCreateModuleData : CreateModuleData
    {
        internal static ExperienceLevelCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ExperienceLevelCreateModuleData> FieldParseTable = new IniParseTable<ExperienceLevelCreateModuleData>
        {
            { "LevelToGrant", (parser, x) => x.LevelToGrant = parser.ParseInteger() },
            { "MPOnly", (parser, x) => x.MPOnly = parser.ParseBoolean() }
        };

        public int LevelToGrant { get; private set; }
        public bool MPOnly { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ExperienceLevelCreateBehavior(gameObject, this);
        }
    }
}
