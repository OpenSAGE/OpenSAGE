using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class UnpauseSpecialPowerUpgrade : UpgradeModule
    {
        private readonly UnpauseSpecialPowerUpgradeModuleData _moduleData;

        internal UnpauseSpecialPowerUpgrade(GameObject gameObject, UnpauseSpecialPowerUpgradeModuleData moduleData)
            : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        protected override void OnUpgrade()
        {
            var powerToUnpause = _moduleData.SpecialPowerTemplate.Value;
            foreach (var specialPowerModule in _gameObject.FindBehaviors<SpecialPowerModule>())
            {
                if (specialPowerModule.Matches(powerToUnpause))
                {
                    specialPowerModule.Unpause();
                }
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

    public sealed class UnpauseSpecialPowerUpgradeModuleData : UpgradeModuleData
    {
        internal static UnpauseSpecialPowerUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<UnpauseSpecialPowerUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<UnpauseSpecialPowerUpgradeModuleData>
            {
                { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseSpecialPowerReference() },
                { "ObeyRechageOnTrigger", (parser, x) => x.ObeyRechageOnTrigger = parser.ParseBoolean() },
            });

        public LazyAssetReference<SpecialPower> SpecialPowerTemplate { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ObeyRechageOnTrigger { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new UnpauseSpecialPowerUpgrade(gameObject, this);
        }
    }
}
