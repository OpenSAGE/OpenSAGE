using OpenSage.Data.Ini;
using OpenSage.Mathematics.FixedMath;

namespace OpenSage.Logic.Object
{
    public sealed class MaxHealthUpgrade : UpgradeModule
    {
        private readonly GameObject _gameObject;
        private readonly MaxHealthUpgradeModuleData _moduleData;

        internal MaxHealthUpgrade(GameObject gameObject, MaxHealthUpgradeModuleData moduleData)
            : base(moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal override void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            if (triggered)
            {
                switch (_moduleData.ChangeType)
                {
                    case MaxHealthChangeType.PreserveRatio:
                        Fix64 ratio = _gameObject.Body.Health / _gameObject.Body.MaxHealth;
                        _gameObject.Body.Health += ratio * (Fix64) _moduleData.AddMaxHealth;
                        break;
                    case MaxHealthChangeType.AddCurrentHealthToo:
                        _gameObject.Body.Health += (Fix64) _moduleData.AddMaxHealth;
                        break;
                    case MaxHealthChangeType.SameCurrentHealth:
                        // Don't add any new health
                        break;
                }

                _gameObject.Body.MaxHealth += (Fix64) _moduleData.AddMaxHealth;
            }
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
