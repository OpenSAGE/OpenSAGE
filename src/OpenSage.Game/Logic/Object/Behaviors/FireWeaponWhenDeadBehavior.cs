#nullable enable

using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class FireWeaponWhenDeadBehavior : BehaviorModule, IUpgradeableModule, IDieModule
{
    private readonly FireWeaponWhenDeadBehaviorModuleData _moduleData;

    internal UpgradeLogic UpgradeLogic { get; }

    internal FireWeaponWhenDeadBehavior(GameObject gameObject, GameEngine gameEngine, FireWeaponWhenDeadBehaviorModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;
        UpgradeLogic = new UpgradeLogic(moduleData.UpgradeData, OnUpgrade);
    }

    public bool CanUpgrade(UpgradeSet existingUpgrades) => UpgradeLogic.CanUpgrade(existingUpgrades);

    public void TryUpgrade(UpgradeSet completedUpgrades) => UpgradeLogic.TryUpgrade(completedUpgrades);

    private void OnUpgrade() { }

    void IDieModule.OnDie(in DamageInfoInput damageInput)
    {
        if (!_moduleData.DieData.IsDieApplicable(damageInput, GameObject))
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
                GameObject,
                deathWeaponTemplate,
                WeaponSlot.Primary,
                GameEngine);

            deathWeapon.SetTarget(new WeaponTarget(GameObject.Translation));
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

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new FireWeaponWhenDeadBehavior(gameObject, gameEngine, this);
    }
}
