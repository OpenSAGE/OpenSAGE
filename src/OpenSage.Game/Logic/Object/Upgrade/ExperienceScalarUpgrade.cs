using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class ExperienceScalarUpgrade : UpgradeModule
    {
        private readonly ExperienceScalarUpgradeModuleData _moduleData;

        internal ExperienceScalarUpgrade(GameObject gameObject, ExperienceScalarUpgradeModuleData moduleData)
            : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        protected override void OnUpgrade()
        {
            _gameObject.ExperienceMultiplier += _moduleData.AddXPScalar;
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
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
