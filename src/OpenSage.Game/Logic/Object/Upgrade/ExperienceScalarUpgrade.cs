using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class ExperienceScalarUpgrade : UpgradeModule
    {
        private readonly GameObject _gameObject;
        private readonly ExperienceScalarUpgradeModuleData _moduleData;

        internal ExperienceScalarUpgrade(GameObject gameObject, ExperienceScalarUpgradeModuleData moduleData)
            : base(moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal override void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            if (triggered)
            {
                _gameObject.ExperienceMultiplier += _moduleData.AddXPScalar;
            }
        }
    }

    public sealed class ExperienceScalarUpgradeModuleData : UpgradeModuleData
    {
        internal static ExperienceScalarUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ExperienceScalarUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<ExperienceScalarUpgradeModuleData>
            {
                { "AddXPScalar", (parser, x) => x.AddXPScalar = parser.ParseFloat() }
            });

        public float AddXPScalar { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ExperienceScalarUpgrade(gameObject, this);
        }
    }
}
