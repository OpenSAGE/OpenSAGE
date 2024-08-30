using OpenSage.Logic.Object;
using OpenSage.Logic.Object.Damage;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Behaviors;

public class DamageTests : StatePersisterTest
{
    #region DamageData

    /// <summary>
    /// Generals default DamageData object.
    /// </summary>
    private static readonly byte[] GeneralsDefaultDamageData = [V1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, V1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void DamageData_Default_V1()
    {
        var stream = SaveData(GeneralsDefaultDamageData);
        var reader = new StateReader(stream, Generals);
        var data = new DamageData();
        data.Persist(reader);
    }

    #endregion

    #region DamageData Request

    /// <summary>
    /// Generals default DamageDataRequest object.
    /// </summary>
    private static readonly byte[] GeneralsDefaultDamageDataRequest = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void DamageDataRequest_Default_V1()
    {
        var stream = SaveData(GeneralsDefaultDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageDataRequest();
        data.Persist(reader);

        Assert.Equal(0u, data.DamageDealer);
        Assert.Equal(0, data.Unknown1);
        Assert.Equal(DamageType.Explosion, data.DamageType); // this is correct - a DamageType of 0 is Explosion (see Generals_DamageDataRequest_Tomahawk)
        Assert.Equal(DamageType.Explosion, data.DamageTypeUnknown); // v3
        Assert.Equal(DeathType.Normal, data.DeathType);
        Assert.Equal(0, data.DamageToDeal);
        Assert.Equal(string.Empty, data.AttackerName); // v3
    }

    /// <summary>
    /// When damage has been applied, a damage dealer is set, damage and death types are set, and damage to deal is set.
    /// </summary>
    private static readonly byte[] GeneralsMissileDefenderDamageDataRequest = [0x07, 0x00, 0x00, 0x00, 0x04, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x42];

    [Fact]
    public void DamageDataRequest_MissileDefender_V1()
    {
        var stream = SaveData(GeneralsMissileDefenderDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageDataRequest();
        data.Persist(reader);

        Assert.Equal(7u, data.DamageDealer);
        Assert.Equal(4, data.Unknown1);
        Assert.Equal(DamageType.InfantryMissile, data.DamageType);
        Assert.Equal(DamageType.Explosion, data.DamageTypeUnknown); // v3
        Assert.Equal(DeathType.Normal, data.DeathType);
        Assert.Equal(40, data.DamageToDeal, 0.01);
        Assert.Equal(string.Empty, data.AttackerName); // v3
    }

    /// <summary>
    /// When damage has been applied, a damage dealer is set, damage and death types are set, and damage to deal is set.
    /// </summary>
    private static readonly byte[] GeneralsTomahawkDamageDataRequest = [0x0f, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x16, 0x43];

    [Fact]
    public void DamageDataRequest_Tomahawk_V1()
    {
        var stream = SaveData(GeneralsTomahawkDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageDataRequest();
        data.Persist(reader);

        Assert.Equal(15u, data.DamageDealer);
        Assert.Equal(4, data.Unknown1);
        Assert.Equal(DamageType.Explosion, data.DamageType);
        Assert.Equal(DamageType.Explosion, data.DamageTypeUnknown); // v3
        Assert.Equal(DeathType.Exploded, data.DeathType);
        Assert.Equal(150, data.DamageToDeal, 0.01);
        Assert.Equal(string.Empty, data.AttackerName); // v3
    }

    /// <summary>
    /// When damage has been applied, a damage dealer is set, damage and death types are set, and damage to deal is set.
    /// </summary>
    private static readonly byte[] GeneralsHumveeGunDamageDataRequest = [0x11, 0x00, 0x00, 0x00, 0x04, 0x00, 0x1e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x41];

    [Fact]
    public void DamageDataRequest_HumveeGun_V1()
    {
        var stream = SaveData(GeneralsHumveeGunDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageDataRequest();
        data.Persist(reader);

        Assert.Equal(17u, data.DamageDealer);
        Assert.Equal(4, data.Unknown1);
        Assert.Equal(DamageType.ComancheVulcan, data.DamageType);
        Assert.Equal(DamageType.Explosion, data.DamageTypeUnknown); // v3
        Assert.Equal(DeathType.Normal, data.DeathType);
        Assert.Equal(8, data.DamageToDeal, 0.01);
        Assert.Equal(string.Empty, data.AttackerName); // v3
    }

    /// <summary>
    /// When damage has been applied, a damage dealer is still, damage and death types are set to be healing-related, and damage to deal is set.
    /// </summary>
    private static readonly byte[] GeneralsHealingDamageDataRequest = [0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x56, 0x55, 0x55, 0x40];

    [Fact]
    public void DamageDataRequest_Healing_V1()
    {
        var stream = SaveData(GeneralsHealingDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageDataRequest();
        data.Persist(reader);

        Assert.Equal(5u, data.DamageDealer);
        Assert.Equal(0, data.Unknown1);
        Assert.Equal(DamageType.Healing, data.DamageType);
        Assert.Equal(DamageType.Explosion, data.DamageTypeUnknown); // v3
        Assert.Equal(DeathType.None, data.DeathType);
        Assert.Equal(3.333, data.DamageToDeal, 0.01);
        Assert.Equal(string.Empty, data.AttackerName); // v3
    }

    /// <summary>
    /// When damaged by a burning ember, the damage dealer is the burning unit.
    /// </summary>
    private static readonly byte[] GeneralsBurningEmberDamageDataRequest = [0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x40];

    [Fact]
    public void DamageDataRequest_BurningEmber_V1()
    {
        var stream = SaveData(GeneralsBurningEmberDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageDataRequest();
        data.Persist(reader);

        Assert.Equal(5u, data.DamageDealer);
        Assert.Equal(0, data.Unknown1);
        Assert.Equal(DamageType.Flame, data.DamageType);
        Assert.Equal(DamageType.Explosion, data.DamageTypeUnknown); // v3
        Assert.Equal(DeathType.Burned, data.DeathType);
        Assert.Equal(3, data.DamageToDeal, 0.01);
        Assert.Equal(string.Empty, data.AttackerName); // v3
    }

    /// <summary>
    /// Tnt attacks have a death type of suicided
    /// </summary>
    private static readonly byte[] GeneralsTntDamageDataRequest = [0x07, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0xfa, 0x43];

    [Fact]
    public void DamageDataRequest_Tnt_V1()
    {
        var stream = SaveData(GeneralsTntDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageDataRequest();
        data.Persist(reader);

        Assert.Equal(7u, data.DamageDealer);
        Assert.Equal(4, data.Unknown1);
        Assert.Equal(DamageType.Explosion, data.DamageType);
        Assert.Equal(DamageType.Explosion, data.DamageTypeUnknown); // v3
        Assert.Equal(DeathType.Suicided, data.DeathType);
        Assert.Equal(500, data.DamageToDeal, 0.01);
        Assert.Equal(string.Empty, data.AttackerName); // v3
    }

    /// <summary>
    /// Tnt attacks have a death type of suicided
    /// </summary>
    private static readonly byte[] GeneralsToxinFieldDamageDataRequest = [0x1c, 0x00, 0x00, 0x00, 0x04, 0x00, 0x09, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40];

    [Fact]
    public void DamageDataRequest_ToxinField_V1()
    {
        var stream = SaveData(GeneralsToxinFieldDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageDataRequest();
        data.Persist(reader);

        Assert.Equal(28u, data.DamageDealer);
        Assert.Equal(4, data.Unknown1);
        Assert.Equal(DamageType.Poison, data.DamageType);
        Assert.Equal(DamageType.Explosion, data.DamageTypeUnknown); // v3
        Assert.Equal(DeathType.Poisoned, data.DeathType);
        Assert.Equal(2, data.DamageToDeal, 0.01);
        Assert.Equal(string.Empty, data.AttackerName); // v3
    }

    /// <summary>
    /// Zero Hour default DamageDataRequest object.
    /// </summary>
    private static readonly byte[] ZeroHourDefaultDamageDataRequest = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void DamageDataRequest_Default_V3()
    {
        var stream = SaveData(ZeroHourDefaultDamageDataRequest, V3);
        var reader = new StateReader(stream, ZeroHour);
        var data = new DamageDataRequest();
        data.Persist(reader);

        Assert.Equal(0u, data.DamageDealer);
        Assert.Equal(0, data.Unknown1);
        Assert.Equal(DamageType.Explosion, data.DamageType);
        Assert.Equal(DamageType.Unresistable, data.DamageTypeUnknown); // unclear what this is
        Assert.Equal(DeathType.Normal, data.DeathType);
        Assert.Equal(0, data.DamageToDeal);
        Assert.Equal(string.Empty, data.AttackerName);
    }


    /// <summary>
    /// When damage has been applied, in addition to the V1 fields and attacker name will also be set.
    /// </summary>
    private static readonly byte[] ZeroHourMissileDefenderDamageDataRequest = [0x05, 0x00, 0x00, 0x00, 0x04, 0x00, 0x18, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1e, 0x41, 0x6d, 0x65, 0x72, 0x69, 0x63, 0x61, 0x49, 0x6e, 0x66, 0x61, 0x6e, 0x74, 0x72, 0x79, 0x4d, 0x69, 0x73, 0x73, 0x69, 0x6c, 0x65, 0x44, 0x65, 0x66, 0x65, 0x6e, 0x64, 0x65, 0x72];

    [Fact]
    public void DamageDataRequest_MissileDefender_V3()
    {
        var stream = SaveData(ZeroHourMissileDefenderDamageDataRequest, V3);
        var reader = new StateReader(stream, ZeroHour);
        var data = new DamageDataRequest();
        data.Persist(reader);

        Assert.Equal(5u, data.DamageDealer);
        Assert.Equal(4, data.Unknown1);
        Assert.Equal(DamageType.InfantryMissile, data.DamageType);
        Assert.Equal(DamageType.Unresistable, data.DamageTypeUnknown); // unclear what this is
        Assert.Equal(DeathType.Normal, data.DeathType);
        Assert.Equal(40, data.DamageToDeal, 0.01);
        Assert.Equal("AmericaInfantryMissileDefender", data.AttackerName);
    }

    #endregion

    #region DamageData Result

    /// <summary>
    /// Generals default DamageDataResult object.
    /// </summary>
    private static readonly byte[] GeneralsDefaultDamageDataResult = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void DamageDataResult_Default_V1()
    {
        var stream = SaveData(GeneralsDefaultDamageDataResult);
        var reader = new StateReader(stream, Generals);
        var data = new DamageDataResult();
        data.Persist(reader);

        Assert.Equal(0, data.DamageAfterArmorCalculation);
        Assert.Equal(0, data.ActualDamageApplied);
    }

    /// <summary>
    ///
    /// </summary>
    private static readonly byte[] GeneralsMissileDamageDataResult = [0x00, 0x00, 0x20, 0x42, 0x00, 0x00, 0x20, 0x42, 0x00];

    [Fact]
    public void DamageDataResult_Missile_V1()
    {
        var stream = SaveData(GeneralsMissileDamageDataResult);
        var reader = new StateReader(stream, Generals);
        var data = new DamageDataResult();
        data.Persist(reader);

        Assert.Equal(40, data.DamageAfterArmorCalculation, 0.01);
        Assert.Equal(40, data.ActualDamageApplied, 0.01);
    }

    /// <summary>
    /// When an object is healing, the actual damage applied is negative.
    /// </summary>
    private static readonly byte[] GeneralsHealingDamageDataResult = [0x56, 0x55, 0x55, 0x40, 0x00, 0x48, 0x55, 0xc0, 0x00];

    [Fact]
    public void DamageDataResult_V1()
    {
        var stream = SaveData(GeneralsHealingDamageDataResult);
        var reader = new StateReader(stream, Generals);
        var data = new DamageDataResult();
        data.Persist(reader);

        Assert.Equal(3.333, data.DamageAfterArmorCalculation, 0.01);
        Assert.Equal(-3.333, data.ActualDamageApplied, 0.01);
    }

    #endregion
}
