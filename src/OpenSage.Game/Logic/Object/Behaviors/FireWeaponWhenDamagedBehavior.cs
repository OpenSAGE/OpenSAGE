#nullable enable

using System;
using System.Diagnostics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class FireWeaponWhenDamagedBehavior : UpdateModule, IUpgradeableModule, IDamageModule
{
    static FireWeaponWhenDamagedBehavior()
    {
        Debug.Assert(Enum.GetValues<BodyDamageType>().Length == 4, "Expected 4 values in BodyDamageType enum.");
    }

    private readonly FireWeaponWhenDamagedBehaviorModuleData _moduleData;

    private readonly Weapon?[] _reactionWeapons = new Weapon?[4];
    private readonly Weapon?[] _continuousWeapons = new Weapon?[4];

    internal Weapon?[] ReactionWeapons => _reactionWeapons;
    internal Weapon?[] ContinuousWeapons => _continuousWeapons;

    internal UpgradeLogic UpgradeLogic { get; }

    public FireWeaponWhenDamagedBehavior(GameObject gameObject, GameEngine gameEngine, FireWeaponWhenDamagedBehaviorModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;

        UpgradeLogic = new UpgradeLogic(moduleData.UpgradeData, OnUpgrade);

        _reactionWeapons[0] = CreateWeapon(moduleData.ReactionWeaponPristine);
        _reactionWeapons[1] = CreateWeapon(moduleData.ReactionWeaponDamaged);
        _reactionWeapons[2] = CreateWeapon(moduleData.ReactionWeaponReallyDamaged);
        _reactionWeapons[3] = CreateWeapon(moduleData.ReactionWeaponRubble);

        _continuousWeapons[0] = CreateWeapon(moduleData.ContinuousWeaponPristine);
        _continuousWeapons[1] = CreateWeapon(moduleData.ContinuousWeaponDamaged);
        _continuousWeapons[2] = CreateWeapon(moduleData.ContinuousWeaponReallyDamaged);
        _continuousWeapons[3] = CreateWeapon(moduleData.ContinuousWeaponRubble);
    }

    private Weapon? CreateWeapon(LazyAssetReference<WeaponTemplate>? weaponTemplateReference)
    {
        var weaponTemplate = weaponTemplateReference?.Value;

        if (weaponTemplate == null)
        {
            return null;
        }

        return new Weapon(
            GameObject,
            weaponTemplate,
            WeaponSlot.Primary,
            GameEngine);
    }

    public bool CanUpgrade(UpgradeSet existingUpgrades) => UpgradeLogic.CanUpgrade(existingUpgrades);

    public void TryUpgrade(UpgradeSet completedUpgrades) => UpgradeLogic.TryUpgrade(completedUpgrades);

    private void OnUpgrade() { }

    public void OnDamage(in DamageInfo damageData)
    {
        if (!UpgradeLogic.Triggered)
        {
            return;
        }

        if (!_moduleData.DamageTypes.Get(damageData.Request.DamageType))
        {
            return;
        }

        if (damageData.Result.ActualDamageDealt < _moduleData.DamageAmount)
        {
            return;
        }

        FireWeaponIfPresentAndReady(_reactionWeapons);
    }

    private protected override void RunUpdate(BehaviorUpdateContext context)
    {
        if (!UpgradeLogic.Triggered)
        {
            return;
        }

        FireWeaponIfPresentAndReady(_continuousWeapons);
    }

    private void FireWeaponIfPresentAndReady(Weapon?[] weapons)
    {
        var weapon = weapons[(int)GameObject.BodyModule.DamageState];

        if (weapon == null || !weapon.IsInactive)
        {
            return;
        }

        weapon.SetTarget(new WeaponTarget(GameObject.Translation));
        weapon.Fire();
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        base.Load(reader);

        reader.PersistObject(UpgradeLogic);

        for (var i = 0; i < _reactionWeapons.Length; i++)
        {
            PersistWeapon(reader, _reactionWeapons[i]);
        }

        for (var i = 0; i < _continuousWeapons.Length; i++)
        {
            PersistWeapon(reader, _continuousWeapons[i]);
        }
    }

    private static void PersistWeapon(StatePersister persister, Weapon? weapon)
    {
        var isWeaponPresent = weapon != null;
        persister.PersistBoolean(ref isWeaponPresent);

        if (isWeaponPresent)
        {
            persister.PersistObject(weapon);
        }
    }
}

public sealed class FireWeaponWhenDamagedBehaviorModuleData : UpgradeModuleData
{
    internal static FireWeaponWhenDamagedBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<FireWeaponWhenDamagedBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
        .Concat(new IniParseTable<FireWeaponWhenDamagedBehaviorModuleData>
        {
            { "ContinuousWeaponPristine", (parser, x) => x.ContinuousWeaponPristine = parser.ParseWeaponTemplateReference() },
            { "ContinuousWeaponDamaged", (parser, x) => x.ContinuousWeaponDamaged = parser.ParseWeaponTemplateReference() },
            { "ContinuousWeaponReallyDamaged", (parser, x) => x.ContinuousWeaponReallyDamaged = parser.ParseWeaponTemplateReference() },
            { "ContinuousWeaponRubble", (parser, x) => x.ContinuousWeaponRubble = parser.ParseWeaponTemplateReference() },

            { "ReactionWeaponPristine", (parser, x) => x.ReactionWeaponPristine = parser.ParseWeaponTemplateReference() },
            { "ReactionWeaponDamaged", (parser, x) => x.ReactionWeaponDamaged = parser.ParseWeaponTemplateReference() },
            { "ReactionWeaponReallyDamaged", (parser, x) => x.ReactionWeaponReallyDamaged = parser.ParseWeaponTemplateReference() },
            { "ReactionWeaponRubble", (parser, x) => x.ReactionWeaponRubble = parser.ParseWeaponTemplateReference() },

            { "DamageTypes", (parser, x) => x.DamageTypes = parser.ParseEnumBitArray<DamageType>() },
            { "DamageAmount", (parser, x) => x.DamageAmount = parser.ParseFloat() }
        });

    public LazyAssetReference<WeaponTemplate>? ContinuousWeaponPristine { get; internal set; }
    public LazyAssetReference<WeaponTemplate>? ContinuousWeaponDamaged { get; internal set; }
    public LazyAssetReference<WeaponTemplate>? ContinuousWeaponReallyDamaged { get; internal set; }
    public LazyAssetReference<WeaponTemplate>? ContinuousWeaponRubble { get; internal set; }

    public LazyAssetReference<WeaponTemplate>? ReactionWeaponPristine { get; internal set; }
    public LazyAssetReference<WeaponTemplate>? ReactionWeaponDamaged { get; internal set; }
    public LazyAssetReference<WeaponTemplate>? ReactionWeaponReallyDamaged { get; internal set; }
    public LazyAssetReference<WeaponTemplate>? ReactionWeaponRubble { get; internal set; }

    public BitArray<DamageType> DamageTypes { get; private set; } = BitArray<DamageType>.CreateAllSet();

    /// <summary>
    /// If damage >= this value, the weapon will be fired.
    /// </summary>
    public float DamageAmount { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new FireWeaponWhenDamagedBehavior(gameObject, gameEngine, this);
    }
}
