using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage;

public abstract class Entity : DisposableBase
{
    private readonly Transform _transform = Transform.CreateIdentity();

    public virtual Transform Transform => _transform;

    public float Yaw => Transform.Yaw;
    public Vector3 EulerAngles => Transform.EulerAngles;
    public Vector3 LookDirection => Transform.LookDirection;
    public Vector3 Translation => Transform.Translation;
    public Quaternion Rotation => Transform.Rotation;
    public Matrix4x4 TransformMatrix => Transform.Matrix;

    public void SetTransformMatrix(Matrix4x4 matrix)
    {
        Transform.Matrix = matrix;
        OnEntityMoved();
    }

    public void SetTranslation(Vector3 translation)
    {
        if (Transform.Translation == translation)
        {
            return;
        }
        Transform.Translation = translation;
        OnEntityMoved();
    }

    public void SetRotation(Quaternion rotation)
    {
        if (Transform.Rotation == rotation)
        {
            return;
        }
        Transform.Rotation = rotation;
        OnEntityMoved();
    }

    public void SetOrientation(float angle)
    {
        // TODO(Port): Implement this.
        if (Transform.Yaw == angle)
        {
            return;
        }
        Transform.Rotation = QuaternionUtility.CreateFromYawPitchRoll_ZUp(angle, 0, 0);
        OnEntityMoved();
    }

    public void SetScale(float scale)
    {
        Transform.Scale = scale;
        OnEntityMoved();
    }

    public void UpdateTransform(in Vector3? translation = null, in Quaternion? rotation = null, float scale = 1.0f)
    {
        Transform.Translation = translation ?? Transform.Translation;
        Transform.Rotation = rotation ?? Transform.Rotation;
        Transform.Scale = scale;
        OnEntityMoved();
    }

    protected virtual void OnEntityMoved() { }
}
