namespace OpenSage.Logic.Object;

internal sealed class WeaponEffectExecutionContext
{
    public readonly Weapon Weapon;
    public readonly GameEngine GameEngine;

    public WeaponEffectExecutionContext(Weapon weapon, GameEngine gameEngine)
    {
        Weapon = weapon;
        GameEngine = gameEngine;
    }
}
