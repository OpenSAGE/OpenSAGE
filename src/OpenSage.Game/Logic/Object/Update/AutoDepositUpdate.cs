using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

internal sealed class AutoDepositUpdate : UpdateModule
{
    private readonly AutoDepositUpdateModuleData _moduleData;

    private LogicFrame _nextAwardFrame;
    private bool _shouldGrantInitialCaptureBonus = true; // this always starts as true, even if there is no capture bonus
    private bool _unknownBool2 = true;

    internal AutoDepositUpdate(GameObject gameObject, IGameEngine gameEngine, AutoDepositUpdateModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;
    }

    internal override void Update(BehaviorUpdateContext context)
    {
        if (GameObject.IsBeingConstructed())
        {
            return;
        }

        if (context.LogicFrame < _nextAwardFrame)
        {
            return;
        }

        _nextAwardFrame = context.LogicFrame + _moduleData.DepositTiming;
        var amount = (uint)(_moduleData.DepositAmount * GameObject.ProductionModifier);

        if (_moduleData.UpgradedBoost.HasValue && GameObject.HasUpgrade(_moduleData.UpgradedBoost.Value.UpgradeType.Value))
        {
            amount += (uint)(amount * (_moduleData.UpgradedBoost.Value.Boost / 100f));
        }

        GenerateAutoDepositCashEvent((int)amount);
        if (_moduleData.ActualMoney)
        {
            GameObject.Owner.BankAccount.Deposit(amount);
            if (!_moduleData.GiveNoXP)
            {
                GameObject.ExperienceTracker.AddExperiencePoints((int)amount);
            }
        }
    }

    public void GrantCaptureBonus()
    {
        if (_shouldGrantInitialCaptureBonus && _moduleData.InitialCaptureBonus != 0)
        {
            _shouldGrantInitialCaptureBonus = false;
            GenerateAutoDepositCashEvent(_moduleData.InitialCaptureBonus);

            // It doesn't appear capture bonus and actual money ever intersect, but just in case...
            if (_moduleData.ActualMoney)
            {
                GameObject.Owner.BankAccount.Deposit((uint)_moduleData.InitialCaptureBonus);
            }
        }
    }

    private void GenerateAutoDepositCashEvent(int amount)
    {
        GameObject.ActiveCashEvent = new CashEvent(amount, GameObject.Owner.Color, new Vector3(0, 0, 10));
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(2);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistLogicFrame(ref _nextAwardFrame);
        reader.PersistBoolean(ref _shouldGrantInitialCaptureBonus);
        reader.PersistBoolean(ref _unknownBool2);
    }
}

public sealed class AutoDepositUpdateModuleData : UpdateModuleData
{
    internal static AutoDepositUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<AutoDepositUpdateModuleData> FieldParseTable = new IniParseTable<AutoDepositUpdateModuleData>
    {
        { "DepositTiming", (parser, x) => x.DepositTiming = parser.ParseTimeMillisecondsToLogicFrames() },
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
    /// How often, in logic frames, to give money to the owning player.
    /// </summary>
    public LogicFrameSpan DepositTiming { get; private set; }

    /// <summary>
    /// Amount of cash to deposit after every <see cref="DepositTiming"/>.
    /// </summary>
    public int DepositAmount { get; private set; }

    /// <summary>
    /// One-time capture bonus.
    /// </summary>
    public int InitialCaptureBonus { get; private set; }

    /// <summary>
    /// Whether to actually award money or just make it appear to others that money has been awarded.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public bool ActualMoney { get; private set; } = true;

    /// <summary>
    /// Bonus cash percentage to add to the <see cref="DepositAmount"/> contingent upon the upgrade being researched.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public BoostUpgrade? UpgradedBoost { get; private set; }

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

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new AutoDepositUpdate(gameObject, gameEngine, this);
    }
}
