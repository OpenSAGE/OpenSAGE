namespace OpenSage.Logic.Object;

/// <summary>
/// Similar to <see cref="WeaponStatus"/>, but with slightly different values. It's used
/// within <see cref="GameObject"/> to store the current set of <see cref="ModelConditionFlag"/>s
/// that have been applied for the current weapon status.
/// </summary>
public enum WeaponModelConditionFlagSet : byte
{
    None = 0,
    Firing = 1,
    BetweenShots = 2,
    Reloading = 3,
    PreAttack = 4,
}
