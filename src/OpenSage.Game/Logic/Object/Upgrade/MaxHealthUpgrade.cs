using OpenSage.Data.Ini;
using FixedMath.NET;

namespace OpenSage.Logic.Object
{
    internal sealed class MaxHealthUpgrade : UpgradeModule
    {
        private readonly MaxHealthUpgradeModuleData _moduleData;

        internal MaxHealthUpgrade(GameObject gameObject, MaxHealthUpgradeModuleData moduleData)
            : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        internal override void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            if (triggered)
            {
                switch (_moduleData.ChangeType)
                {
                    case MaxHealthChangeType.PreserveRatio:
                        _gameObject.Health += _gameObject.HealthPercentage * (Fix64) _moduleData.AddMaxHealth;
                        break;
                    case MaxHealthChangeType.AddCurrentHealthToo:
                        _gameObject.Health += (Fix64) _moduleData.AddMaxHealth;
                        break;
                    case MaxHealthChangeType.SameCurrentHealth:
                        // Don't add any new health
                        break;
                }

                _gameObject.MaxHealth += (Fix64) _moduleData.AddMaxHealth;
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    public sealed class MaxHealthUpgradeModuleData : UpgradeModuleData
    {
        internal static MaxHealthUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<MaxHealthUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<MaxHealthUpgradeModuleData>
            {
                { "AddMaxHealth", (parser, x) => x.AddMaxHealth = parser.ParseFloat() },
                { "ChangeType", (parser, x) => x.ChangeType = parser.ParseEnum<MaxHealthChangeType>() },
            });

        public float AddMaxHealth { get; private set; }
        public MaxHealthChangeType ChangeType { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new MaxHealthUpgrade(gameObject, this);
        }
    }

    public enum MaxHealthChangeType
    {
        [IniEnum("PRESERVE_RATIO")]
        PreserveRatio,

        [IniEnum("ADD_CURRENT_HEALTH_TOO")]
        AddCurrentHealthToo,

        [IniEnum("SAME_CURRENTHEALTH")]
        SameCurrentHealth
    }
}
