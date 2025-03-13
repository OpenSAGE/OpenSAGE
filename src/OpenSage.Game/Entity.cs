using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage;

public abstract class Entity : DisposableBase
{
    private readonly Transform _transform = Transform.CreateIdentity();

    public virtual Transform Transform => _transform;

    public float Yaw => _transform.Yaw;
    public Vector3 EulerAngles => _transform.EulerAngles;
    public Vector3 LookDirection => _transform.LookDirection;
    public Vector3 Translation => _transform.Translation;
    public Quaternion Rotation => _transform.Rotation;
    public Matrix4x4 TransformMatrix => _transform.Matrix;

    public void SetTransformMatrix(Matrix4x4 matrix)
    {
        _transform.Matrix = matrix;
        OnEntityMoved();
    }

    public void SetTranslation(Vector3 translation)
    {
        if (_transform.Translation == translation)
        {
            return;
        }
        _transform.Translation = translation;
        OnEntityMoved();
    }

    public void SetRotation(Quaternion rotation)
    {
        if (_transform.Rotation == rotation)
        {
            return;
        }
        _transform.Rotation = rotation;
        OnEntityMoved();
    }

    public void SetOrientation(float angle)
    {
        // TODO(Port): Implement this.
        if (_transform.Yaw == angle)
        {
            return;
        }
        _transform.Rotation = QuaternionUtility.CreateFromYawPitchRoll_ZUp(angle, 0, 0);
        OnEntityMoved();
    }

    public void SetScale(float scale)
    {
        _transform.Scale = scale;
        OnEntityMoved();
    }

    public void UpdateTransform(in Vector3? translation = null, in Quaternion? rotation = null, float scale = 1.0f)
    {
        _transform.Translation = translation ?? _transform.Translation;
        _transform.Rotation = rotation ?? _transform.Rotation;
        _transform.Scale = scale;
        OnEntityMoved();
    }

    protected virtual void OnEntityMoved() { }
}
