using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class SupplyCenterDockUpdate : DockUpdate
    {
        private GameObject _gameObject;
        private SupplyCenterDockUpdateModuleData _moduleData;

        internal SupplyCenterDockUpdate(GameObject gameObject, SupplyCenterDockUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        public void DumpBoxes(AssetStore assetStore, ref int numBoxes, int additionalAmountPerBox)
        {
            var gameData = assetStore.GameData.Current;
            var amountPerBox = (gameData.ValuePerSupplyBox + additionalAmountPerBox) * _moduleData.ValueMultiplier;

            if (_moduleData.BonusScience != null)
            {
                var bonusUpgradeDefinition = assetStore.Upgrades.GetByName(_moduleData.BonusScience);
                if (_gameObject.HasUpgrade(bonusUpgradeDefinition))
                {
                    amountPerBox *= _moduleData.BonusScienceMultiplier;
                }
            }

            _gameObject.Owner.BankAccount.Deposit((uint)(numBoxes * amountPerBox * _gameObject.ProductionModifier));
            numBoxes = 0;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);
        }

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    public sealed class SupplyCenterDockUpdateModuleData : DockUpdateModuleData
    {
        internal static SupplyCenterDockUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SupplyCenterDockUpdateModuleData> FieldParseTable = DockUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<SupplyCenterDockUpdateModuleData>
            {
                { "GrantTemporaryStealth", (parser, x) => x.GrantTemporaryStealth = parser.ParseInteger() },
                { "BonusScience", (parser, x) => x.BonusScience = parser.ParseString() },
                { "BonusScienceMultiplier", (parser, x) => x.BonusScienceMultiplier = parser.ParsePercentage() },
                { "ValueMultiplier", (parser, x) => x.ValueMultiplier = parser.ParseFloat() }
            });

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int GrantTemporaryStealth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string BonusScience { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage BonusScienceMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float ValueMultiplier { get; private set; } = 1.0f;

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new SupplyCenterDockUpdate(gameObject, this);
        }
    }
}
