namespace OpenSage.Logic.Object
{
    internal sealed class WeaponEffectExecutionContext
    {
        public readonly Weapon Weapon;
        public readonly GameContext GameContext;
        public readonly TimeInterval Time;

        public WeaponEffectExecutionContext(Weapon weapon, GameContext gameContext, TimeInterval time)
        {
            Weapon = weapon;
            GameContext = gameContext;
            Time = time;
        }
    }
}
