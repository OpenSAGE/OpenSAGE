using System.Numerics;
using OpenSage.Mathematics.FixedMath;

namespace OpenSage.Logic.Object
{
    internal sealed class WeaponTarget
    {
        public readonly WeaponTargetType TargetType;
        public readonly Vector3? TargetGroundPosition;
        public readonly GameObject TargetObject;

        public Vector3 TargetPosition => TargetType == WeaponTargetType.Position
            ? TargetGroundPosition.Value
            : TargetObject.Transform.Translation;

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

        public void DoDamage(DamageType damageType, Fix64 amount)
        {
            if (TargetType == WeaponTargetType.Object)
            {
                TargetObject.Body.DoDamage(damageType, amount);
            }
        }
    }
}
