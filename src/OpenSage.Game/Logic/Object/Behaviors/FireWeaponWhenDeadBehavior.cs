#nullable enable

using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class FireWeaponWhenDeadBehavior : BehaviorModule, IUpgradeableModule
{
    private readonly GameObject _gameObject;
    private readonly GameContext _context;
    private readonly FireWeaponWhenDeadBehaviorModuleData _moduleData;

    internal UpgradeLogic UpgradeLogic { get; }

    internal FireWeaponWhenDeadBehavior(GameObject gameObject, GameContext context, FireWeaponWhenDeadBehaviorModuleData moduleData)
    {
        _gameObject = gameObject;
        _context = context;
        _moduleData = moduleData;
        UpgradeLogic = new UpgradeLogic(moduleData.UpgradeData, OnUpgrade);
    }

    public bool CanUpgrade(UpgradeSet existingUpgrades) => UpgradeLogic.CanUpgrade(existingUpgrades);

    public void TryUpgrade(UpgradeSet completedUpgrades) => UpgradeLogic.TryUpgrade(completedUpgrades);

    private void OnUpgrade() { }

    internal override void OnDie(BehaviorUpdateContext context, DeathType deathType, BitArray<ObjectStatus> status)
    {
        if (!_moduleData.DieData.IsApplicable(deathType, status))
        {
            return;
        }

        if (!UpgradeLogic.Triggered)
        {
            return;
        }

        // TODO: DelayTime
        // TODO: WeaponOffset

        var deathWeaponTemplate = _moduleData.DeathWeapon?.Value;
        if (deathWeaponTemplate != null)
        {
            var deathWeapon = new Weapon(
                _gameObject,
                deathWeaponTemplate,
                WeaponSlot.Primary,
                _context);

            deathWeapon.SetTarget(new WeaponTarget(_gameObject.Translation));
            deathWeapon.Fire();
        }
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistObject(UpgradeLogic);
    }
}

public sealed class FireWeaponWhenDeadBehaviorModuleData : UpgradeModuleData
{
    internal static FireWeaponWhenDeadBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<FireWeaponWhenDeadBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
        .Concat(new IniParseTableChild<FireWeaponWhenDeadBehaviorModuleData, DieLogicData>(x => x.DieData, DieLogicData.FieldParseTable))
        .Concat(new IniParseTable<FireWeaponWhenDeadBehaviorModuleData>
        {
            { "DeathWeapon", (parser, x) => x.DeathWeapon = parser.ParseWeaponTemplateReference() },
            { "DelayTime", (parser, x) => x.DelayTime = parser.ParseInteger() },
            { "WeaponOffset", (parser, x) => x.WeaponOffset = parser.ParseVector3() },
        });

    public DieLogicData DieData { get; } = new();

    public LazyAssetReference<WeaponTemplate>? DeathWeapon { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int DelayTime { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public Vector3 WeaponOffset { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
    {
        return new FireWeaponWhenDeadBehavior(gameObject, context, this);
    }
}
