using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public class UpgradeDieModule : DieModule
    {
        private readonly GameObject _gameObject;
        private readonly GameContext _context;
        private readonly UpgradeDieModuleData _moduleData;

        internal UpgradeDieModule(GameObject gameObject, GameContext context, UpgradeDieModuleData moduleData) : base(moduleData)
        {
            _gameObject = gameObject;
            _context = context;
            _moduleData = moduleData;
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }

        private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
        {
            var parent = _context.GameLogic.GetObjectById(_gameObject.CreatedByObjectID);

            parent?.RemoveUpgrade(_moduleData.UpgradeToRemove.UpgradeName.Value);

            base.Die(context, deathType);
        }
    }

    /// <summary>
    /// Frees the object-based upgrade for the producer object.
    /// </summary>
    public sealed class UpgradeDieModuleData : DieModuleData
    {
        internal static UpgradeDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<UpgradeDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<UpgradeDieModuleData>
            {
                { "UpgradeToRemove", (parser, x) => x.UpgradeToRemove = UpgradeToRemove.Parse(parser) }
            });

        public UpgradeToRemove UpgradeToRemove { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new UpgradeDieModule(gameObject, context, this);
        }
    }

    public struct UpgradeToRemove
    {
        internal static UpgradeToRemove Parse(IniParser parser)
        {
            return new UpgradeToRemove
            {
                UpgradeName = parser.ParseUpgradeReference(),
                ModuleTag = parser.ParseIdentifier(),
            };
        }

        public LazyAssetReference<UpgradeTemplate> UpgradeName { get; private set; }
        public string ModuleTag { get; private set; }
    }
}
