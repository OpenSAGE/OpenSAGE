using System.Numerics;
using OpenSage.Mathematics.FixedMath;

namespace OpenSage.Logic.Object
{
    internal sealed class WeaponTarget
    {
        public readonly WeaponTargetType TargetType;
        public readonly Vector3? TargetGroundPosition;
        public readonly GameObject TargetObject;

        public bool IsDestroyed => TargetType == WeaponTargetType.Object && TargetObject.Destroyed;

        public Vector3 TargetPosition => TargetType == WeaponTargetType.Position
            ? TargetGroundPosition.Value
            : TargetObject.Translation;

        internal WeaponTarget(in Vector3 targetGroundPosition)
        {
            TargetType = WeaponTargetType.Position;
            TargetGroundPosition = targetGroundPosition;
        }

        internal WeaponTarget(GameObject targetObject)
        {
            TargetType = WeaponTargetType.Object;
            TargetObject = targetObject;
        }

        public void DoDamage(DamageType damageType, Fix64 amount, DeathType deathType, TimeInterval time)
        {
            if (TargetType == WeaponTargetType.Object)
            {
                TargetObject.Body.DoDamage(damageType, amount, deathType, time);
            }
        }
    }
}
