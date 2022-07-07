namespace OpenSage.Logic.Object
{
    internal sealed class WeaponEffectExecutionContext
    {
        public readonly Weapon Weapon;
        public readonly GameContext GameContext;

        public WeaponEffectExecutionContext(Weapon weapon, GameContext gameContext)
        {
            Weapon = weapon;
            GameContext = gameContext;
        }
    }
}
