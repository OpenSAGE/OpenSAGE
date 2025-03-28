using System.Numerics;
using FixedMath.NET;

namespace OpenSage.Logic.Object;

internal sealed class WeaponTarget
{
    private readonly IGameObjectCollection _gameObjects;

    public readonly WeaponTargetType TargetType;
    public readonly Vector3? TargetGroundPosition;
    public readonly ObjectId TargetObjectId;

    public bool IsDestroyed => TargetType == WeaponTargetType.Object && GetTargetObject() == null;

    public Vector3 TargetPosition => TargetType == WeaponTargetType.Position
        ? TargetGroundPosition.Value
        : GetTargetObject().Translation;

    internal WeaponTarget(in Vector3 targetGroundPosition)
    {
        TargetType = WeaponTargetType.Position;
        TargetGroundPosition = targetGroundPosition;
    }

    internal WeaponTarget(IGameObjectCollection gameObjects, ObjectId targetObjectId)
    {
        _gameObjects = gameObjects;

        TargetType = WeaponTargetType.Object;
        TargetObjectId = targetObjectId;
    }

    public GameObject GetTargetObject() => _gameObjects.GetObjectById(TargetObjectId);

    public void DoDamage(DamageType damageType, Fix64 amount, DeathType deathType, GameObject damageDealer)
    {
        if (TargetType == WeaponTargetType.Object)
        {
            GetTargetObject().AttemptDamage(new DamageInfoInput(damageDealer)
            {
                DamageType = damageType,
                DeathType = deathType,
                Amount = (float)amount,
            });
        }
    }
}
