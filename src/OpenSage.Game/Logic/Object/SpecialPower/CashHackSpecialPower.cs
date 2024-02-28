using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class CashHackSpecialPower : SpecialPowerModule, IUpgradableScienceModule
    {
        private readonly CashHackSpecialPowerModuleData _moduleData;

        private uint _currentAmount;

        internal CashHackSpecialPower(GameObject gameObject, GameContext context, CashHackSpecialPowerModuleData moduleData) : base(gameObject, context, moduleData)
        {
            _moduleData = moduleData;
            _currentAmount = (uint)moduleData.MoneyAmount;
        }

        public void Activate(GameObject target)
        {
            var targetBankAccount = target.Owner.BankAccount;
            var amountToTransfer = Math.Min(targetBankAccount.Money, _currentAmount);
            targetBankAccount.Withdraw(amountToTransfer);
            target.ActiveCashEvent = new CashEvent(-(int)amountToTransfer, new ColorRgb(255, 0, 0));
            GameObject.Owner.BankAccount.Deposit(amountToTransfer);

            base.Activate(target.Transform.Translation);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }

        public void TryUpgrade(Science purchasedScience)
        {
            foreach (var (science, amount) in _moduleData.UpgradeMoneyAmounts)
            {
                if (science.Value == purchasedScience)
                {
                    _currentAmount = (uint)amount;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Allows you to steal money from an enemy supply center. The special power specified in
    /// <see cref="SpecialPowerTemplate"/> must use the <see cref="SpecialPowerType.CashHack"/> type.
    /// </summary>
    public sealed class CashHackSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new CashHackSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CashHackSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<CashHackSpecialPowerModuleData>
            {
                { "UpgradeMoneyAmount", (parser, x) => x.UpgradeMoneyAmounts.Add(CashHackSpecialPowerUpgrade.Parse(parser)) },
                { "MoneyAmount", (parser, x) => x.MoneyAmount = parser.ParseInteger() }
            });

        public List<CashHackSpecialPowerUpgrade> UpgradeMoneyAmounts { get; } = new List<CashHackSpecialPowerUpgrade>();

        /// <summary>
        /// Amount of money to steal.
        /// </summary>
        public int MoneyAmount { get; private set; }

        internal override CashHackSpecialPower CreateModule(GameObject gameObject, GameContext context)
        {
            return new CashHackSpecialPower(gameObject, context, this);
        }
    }

    public readonly record struct CashHackSpecialPowerUpgrade(LazyAssetReference<Science> Science, int MoneyAmount)
    {
        internal static CashHackSpecialPowerUpgrade Parse(IniParser parser)
        {
            return new CashHackSpecialPowerUpgrade
            {
                Science = parser.ParseScienceReference(),
                MoneyAmount = parser.ParseInteger()
            };
        }
    }
}
