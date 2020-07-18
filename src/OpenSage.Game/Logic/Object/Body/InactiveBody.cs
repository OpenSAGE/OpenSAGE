using OpenSage.Data.Ini;
using OpenSage.Mathematics.FixedMath;

namespace OpenSage.Logic.Object
{
    public sealed class InactiveBody : BodyModule
    {
        private readonly GameObject _gameObject;

        internal InactiveBody(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public override Fix64 MaxHealth => Fix64.Zero;

        public override void DoDamage(DamageType damageType, Fix64 amount, DeathType deathType, TimeInterval time)
        {
            // TODO

            _gameObject.Die(deathType, time);
        }
    }

    /// <summary>
    /// Prevents normal interaction with other objects.
    /// </summary>
    public sealed class InactiveBodyModuleData : BodyModuleData
    {
        internal static InactiveBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InactiveBodyModuleData> FieldParseTable = new IniParseTable<InactiveBodyModuleData>();

        internal override BodyModule CreateBodyModule(GameObject gameObject)
        {
            return new InactiveBody(gameObject);
        }
    }
}
