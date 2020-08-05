using OpenSage.Data.Ini;
using OpenSage.Mathematics.FixedMath;

namespace OpenSage.Logic.Object
{
    public sealed class HighlanderBody : ActiveBody
    {
        internal HighlanderBody(GameObject gameObject, HighlanderBodyModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

        public override void DoDamage(DamageType damageType, Fix64 amount, DeathType deathType, TimeInterval time)
        {
            // TODO: Don't think this is right.
            if (damageType == DamageType.Unresistable)
            {
                Health -= amount;

                if (Health < (Fix64)0)
                {
                    Health = (Fix64) 0;
                }

                // TODO: DamageFX

                if (Health <= Fix64.Zero)
                {
                    GameObject.Die(deathType, time);
                }
            }
        }
    }

    /// <summary>
    /// Allows the object to take damage but not die. The object will only die from irresistable damage.
    /// </summary>
    public sealed class HighlanderBodyModuleData : ActiveBodyModuleData
    {
        internal static new HighlanderBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<HighlanderBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<HighlanderBodyModuleData>());

        internal override BodyModule CreateBodyModule(GameObject gameObject)
        {
            return new HighlanderBody(gameObject, this);
        }
    }
}
