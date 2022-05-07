using System.Numerics;
using FixedMath.NET;

namespace OpenSage.Logic.Object
{
    internal sealed class WeaponTarget
    {
        private readonly GameObjectCollection _gameObjects;

        public readonly WeaponTargetType TargetType;
        public readonly Vector3? TargetGroundPosition;
        public readonly uint TargetObjectId;

        public bool IsDestroyed => TargetType == WeaponTargetType.Object && GetTargetObject() == null;

        public Vector3 TargetPosition => TargetType == WeaponTargetType.Position
            ? TargetGroundPosition.Value
            : GetTargetObject().Translation;

        internal WeaponTarget(in Vector3 targetGroundPosition)
        {
            TargetType = WeaponTargetType.Position;
            TargetGroundPosition = targetGroundPosition;
        }

        internal WeaponTarget(GameObjectCollection gameObjects, uint targetObjectId)
        {
            _gameObjects = gameObjects;

            TargetType = WeaponTargetType.Object;
            TargetObjectId = targetObjectId;
        }

        public GameObject GetTargetObject() => _gameObjects.GetObjectById(TargetObjectId);

        public void DoDamage(DamageType damageType, Fix64 amount, DeathType deathType, TimeInterval time)
        {
            if (TargetType == WeaponTargetType.Object)
            {
                GetTargetObject().DoDamage(damageType, amount, deathType, time);
            }
        }
    }
}
