using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyGainCreate : CreateModule
    {
        private readonly GameObject _gameObject;
        private readonly VeterancyGainCreateModuleData _moduleData;

        public VeterancyGainCreate(GameObject gameObject, VeterancyGainCreateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }

        public override void OnCreate()
        {
            if (_moduleData.ScienceRequired != null && !_gameObject.Owner.HasScience(_moduleData.ScienceRequired.Value))
            {
                return;
            }

            // units like the minigunner or tank battlemaster can start at vet 1 and be upgraded to vet 2, and so have two VeterancyGainCreate modules
            var level = (int)_moduleData.StartingLevel;
            if (level > _gameObject.Rank)
            {
                _gameObject.Rank = level;
            }
        }
    }

    public sealed class VeterancyGainCreateModuleData : CreateModuleData
    {
        internal static VeterancyGainCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<VeterancyGainCreateModuleData> FieldParseTable = new IniParseTable<VeterancyGainCreateModuleData>
        {
            { "StartingLevel", (parser, x) => x.StartingLevel = parser.ParseEnum<VeterancyLevel>() },
            { "ScienceRequired", (parser, x) => x.ScienceRequired = parser.ParseScienceReference() },
        };

        public VeterancyLevel StartingLevel { get; private set; }
        public LazyAssetReference<Science> ScienceRequired { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new VeterancyGainCreate(gameObject, this);
        }
    }
}
