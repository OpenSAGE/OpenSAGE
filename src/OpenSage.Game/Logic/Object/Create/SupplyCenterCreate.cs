using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class SupplyCenterCreate : CreateModule, IDestroyModule
    {
        private readonly GameObject _gameObject;

        public SupplyCenterCreate(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        protected override void OnBuildCompleteImpl()
        {
            foreach (var player in _gameObject.GameContext.Scene3D.Players)
            {
                player.SupplyManager.AddSupplyCenter(_gameObject);
            }
        }

        public void OnDestroy()
        {
            foreach (var player in _gameObject.GameContext.Scene3D.Players)
            {
                player.SupplyManager.RemoveSupplyCenter(_gameObject);
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    /// <summary>
    /// Requires the <see cref="ObjectKinds.SupplySource"/> KindOf defined in order to work properly.
    /// Ensures the object acts as a destination for collection of supplies.
    /// </summary>
    public sealed class SupplyCenterCreateModuleData : CreateModuleData
    {
        internal static SupplyCenterCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SupplyCenterCreateModuleData> FieldParseTable = new IniParseTable<SupplyCenterCreateModuleData>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new SupplyCenterCreate(gameObject);
        }
    }
}
