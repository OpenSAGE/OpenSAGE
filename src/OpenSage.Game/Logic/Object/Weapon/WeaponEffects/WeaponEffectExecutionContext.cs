namespace OpenSage.Logic.Object;

internal sealed class WeaponEffectExecutionContext
{
    public readonly Weapon Weapon;
    public readonly IGameEngine GameEngine;

    public WeaponEffectExecutionContext(Weapon weapon, IGameEngine gameEngine)
    {
        Weapon = weapon;
        GameEngine = gameEngine;
    }
}
