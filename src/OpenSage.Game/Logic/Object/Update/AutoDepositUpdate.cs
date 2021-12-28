using System;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    internal sealed class AutoDepositUpdate : UpdateModule
    {
        GameObject _gameObject;
        AutoDepositUpdateModuleData _moduleData;

        private TimeSpan _waitUntil;

        private uint _unknownFrame;
        private bool _unknownBool1;
        private bool _unknownBool2;

        internal AutoDepositUpdate(GameObject gameObject, GameContext context, AutoDepositUpdateModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (_gameObject.IsBeingConstructed()
                 || (context.Time.TotalTime < _waitUntil))
            {
                return;
            }

            _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.DepositTiming);
            var amount = (uint) (_moduleData.DepositAmount * _gameObject.ProductionModifier);
            _gameObject.Owner.BankAccount.Deposit(amount);
            if (!_moduleData.GiveNoXP)
            {
                _gameObject.GainExperience((int)amount);
            }
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);

            _unknownFrame = reader.ReadUInt32();

            _unknownBool1 = reader.ReadBoolean();

            _unknownBool2 = reader.ReadBoolean();
        }
    }

    public sealed class AutoDepositUpdateModuleData : UpdateModuleData
    {
        internal static AutoDepositUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoDepositUpdateModuleData> FieldParseTable = new IniParseTable<AutoDepositUpdateModuleData>
        {
            { "DepositTiming", (parser, x) => x.DepositTiming = parser.ParseInteger() },
            { "DepositAmount", (parser, x) => x.DepositAmount = parser.ParseInteger() },
            { "InitialCaptureBonus", (parser, x) => x.InitialCaptureBonus = parser.ParseInteger() },
            { "ActualMoney", (parser, x) => x.ActualMoney = parser.ParseBoolean() },
            { "UpgradedBoost", (parser, x) => x.UpgradedBoost = BoostUpgrade.Parse(parser) },
            { "Upgrade", (parser, x) => x.Upgrade = parser.ParseAssetReference() },
            { "UpgradeBonusPercent", (parser, x) => x.UpgradeBonusPercent = parser.ParsePercentage() },
            { "UpgradeMustBePresent", (parser, x) => x.UpgradeMustBePresent = ObjectFilter.Parse(parser) },
            { "GiveNoXP", (parser, x) => x.GiveNoXP = parser.ParseBoolean() },
            { "OnlyWhenGarrisoned", (parser, x) => x.OnlyWhenGarrisoned = parser.ParseBoolean() },
        };

        /// <summary>
        /// How often, in milliseconds, to give money to the owning player.
        /// </summary>
        public int DepositTiming { get; private set; }

        /// <summary>
        /// Amount of cash to deposit after every <see cref="DepositTiming"/>.
        /// </summary>
        public int DepositAmount { get; private set; }

        /// <summary>
        /// One-time capture bonus.
        /// </summary>
        public int InitialCaptureBonus { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ActualMoney { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public BoostUpgrade UpgradedBoost { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string Upgrade { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage UpgradeBonusPercent { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectFilter UpgradeMustBePresent { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool GiveNoXP { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool OnlyWhenGarrisoned { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new AutoDepositUpdate(gameObject, context, this);
        }
    }
}
