using System;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage
{
    public sealed class TransformComponent : EntityComponent
    {
        public event EventHandler TransformChanged;

        private bool _needToRecreateLocalMatrix;
        private bool _needToRecreateWorldMatrices;
        private Matrix4x4 _cachedLocalMatrix;
        private Matrix4x4 _cachedLocalToWorldMatrix;
        private Matrix4x4 _cachedWorldToLocalMatrix;
        private Vector3? _cachedLocalEulerAngles;

        internal TransformComponent ParentDirect;

        public TransformComponent Parent
        {
            get { return ParentDirect; }
            set
            {
                var oldParent = ParentDirect;
                if (oldParent == value)
                {
                    return;
                }

                // TODO: Also remove and add components from this hierarchy.

                oldParent?.Children.Remove(this);
                value?.Children.Add(this);
            }
        }

        public TransformChildrenCollection Children { get; }

        private Vector3 _localScale;

        /// <summary>
        /// Gets or sets the scale relative to the parent entity.
        /// </summary>
        public Vector3 LocalScale
        {
            get { return _localScale; }
            set
            {
                _localScale = value;
                ClearCachedMatrices();
            }
        }

        /// <summary>
        /// Gets the scale in world space.
        /// </summary>
        public Vector3 WorldScale
        {
            get
            {
                Matrix4x4.Decompose(LocalToWorldMatrix, out var scale, out var _, out var _);
                return scale;
            }
        }

        private Vector3 _localPosition;

        /// <summary>
        /// Gets or sets the position relative to the parent entity.
        /// </summary>
        public Vector3 LocalPosition
        {
            get { return _localPosition; }
            set
            {
                _localPosition = value;
                ClearCachedMatrices();
            }
        }

        /// <summary>
        /// Gets or sets the position in world space.
        /// </summary>
        public Vector3 WorldPosition
        {
            get
            {
                return (ParentDirect != null)
                    ? Vector3.Transform(LocalPosition, ParentDirect.LocalToWorldMatrix)
                    : LocalPosition;
            }
            set
            {
                LocalPosition = (ParentDirect != null)
                    ? Vector3.Transform(value, ParentDirect.WorldToLocalMatrix)
                    : value;
                ClearCachedMatrices();
            }
        }

        private Quaternion _localRotation;

        /// <summary>
        /// Gets or sets the rotation relative to the parent entity.
        /// </summary>
        public Quaternion LocalRotation
        {
            get { return _localRotation; }
            set
            {
                _localRotation = value;
                _cachedLocalEulerAngles = null;
                ClearCachedMatrices();
            }
        }

        /// <summary>
        /// Gets or sets the rotation in world space.
        /// </summary>
        public Quaternion WorldRotation
        {
            get
            {
                return (ParentDirect != null)
                    ? LocalRotation * ParentDirect.WorldRotation
                    : LocalRotation;
            }
            set
            {
                LocalRotation = (ParentDirect != null)
                    ? value * ParentDirect.WorldRotation
                    : value;
                ClearCachedMatrices();
            }
        }

        /// <summary>
        /// Gets or sets the rotation, relative to the parent entity, using Euler angles.
        /// </summary>
        /// <remarks>
        /// The specified vector should contain the rotation around the X, Y, and Z axes.
        /// </remarks>
        public Vector3 LocalEulerAngles
        {
            get
            {
                if (_cachedLocalEulerAngles != null)
                    return _cachedLocalEulerAngles.Value;
                return QuaternionUtility.ToEuler(LocalRotation);
            }
            set
            {
                LocalRotation = QuaternionUtility.FromEuler(value);
                _cachedLocalEulerAngles = value;
            }
        }

        /// <summary>
        /// Gets or sets the rotation in world space using Euler angles.
        /// </summary>
        /// <remarks>
        /// The specified vector should contain the rotation around the X, Y, and Z axes.
        /// </remarks>
        public Vector3 WorldEulerAngles
        {
            get { return QuaternionUtility.ToEuler(WorldRotation); }
            set { WorldRotation = QuaternionUtility.FromEuler(value); }
        }

        /// <summary>
        /// Gets the backward vector for the matrix represented by this transform.
        /// </summary>
        public Vector3 Backward => LocalToWorldMatrix.Backward();

        /// <summary>
        /// Gets the forward vector for the matrix represented by this transform.
        /// </summary>
        public Vector3 Forward => LocalToWorldMatrix.Forward();

        /// <summary>
        /// Gets the left vector for the matrix represented by this transform.
        /// </summary>
        public Vector3 Left => LocalToWorldMatrix.Left();

        /// <summary>
        /// Gets the right vector for the matrix represented by this transform.
        /// </summary>
        public Vector3 Right => LocalToWorldMatrix.Right();

        /// <summary>
        /// Gets the up vector for the matrix represented by this transform.
        /// </summary>
        public Vector3 Up => LocalToWorldMatrix.Up();

        /// <summary>
        /// Concatenated matrix representing local scale, rotation and translation.
        /// </summary>
        internal Matrix4x4 LocalMatrix
        {
            get
            {
                if (_needToRecreateLocalMatrix)
                {
                    CacheLocalMatrix();
                    RaiseTransformChanged();
                }

                return _cachedLocalMatrix;
            }
        }

        /// <summary>
        /// Gets a matrix that transforms from the space represented by this <see cref="TransformComponent"/> into world space.
        /// </summary>
        public Matrix4x4 LocalToWorldMatrix
        {
            get
            {
                if (_needToRecreateWorldMatrices)
                {
                    CacheWorldMatrices();
                    RaiseTransformChanged();
                }

                return _cachedLocalToWorldMatrix;
            }
        }

        internal void ClearCachedMatrices()
        {
            ClearCachedMatricesLocal();

            foreach (var childTransform in Children)
                childTransform.ClearCachedMatrices();
        }

        private void ClearCachedMatricesLocal()
        {
            _needToRecreateLocalMatrix = true;
            _needToRecreateWorldMatrices = true;
        }

        private void RaiseTransformChanged()
        {
            TransformChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CacheLocalMatrix()
        {
            _cachedLocalMatrix = Matrix4x4.CreateScale(LocalScale)
                * Matrix4x4.CreateFromQuaternion(LocalRotation)
                * Matrix4x4.CreateTranslation(LocalPosition);

            _needToRecreateLocalMatrix = false;
        }

        private void CacheWorldMatrices()
        {
            _cachedLocalToWorldMatrix = AppendMatrixRecursive(this);
            Matrix4x4.Invert(_cachedLocalToWorldMatrix, out _cachedWorldToLocalMatrix);

            _needToRecreateWorldMatrices = false;
        }

        private static Matrix4x4 AppendMatrixRecursive(TransformComponent transform)
        {
            if (transform == null)
                return Matrix4x4.Identity;

            var result = AppendMatrixRecursive(transform.ParentDirect);
            result = transform.LocalMatrix * result;

            return result;
        }

        /// <summary>
        /// Gets a matrix that transforms from world space to the space represented by this <see cref="TransformComponent"/>.
        /// </summary>
        public Matrix4x4 WorldToLocalMatrix
        {
            get
            {
                if (_needToRecreateWorldMatrices)
                    CacheWorldMatrices();

                return _cachedWorldToLocalMatrix;
            }
        }

        /// <summary>
        /// Creates a new <see cref="TransformComponent"/>.
        /// </summary>
        public TransformComponent()
        {
            Children = new TransformChildrenCollection(this);

            LocalScale = new Vector3(1, 1, 1);
            LocalPosition = Vector3.Zero;
            LocalRotation = Quaternion.Identity;

            ClearCachedMatricesLocal();
        }

        /// <summary>
        /// Updates <see cref="LocalRotation"/> to point towards the specified world space position.
        /// </summary>
        public void LookAt(Vector3 worldPosition)
        {
            var matrix = Matrix4x4.CreateLookAt(WorldPosition, worldPosition, Vector3.UnitZ);
            LocalRotation = Quaternion.CreateFromRotationMatrix(Matrix4x4Utility.Invert(matrix));
        }

        /// <summary>
        /// Transforms a position from local to world space.
        /// </summary>
        /// <param name="position">The position to transform.</param>
        /// <returns>The transformed position.</returns>
        public Vector3 TransformPosition(Vector3 position)
        {
            return Vector3.Transform(position, LocalToWorldMatrix);
        }

        /// <summary>
        /// Transforms a direction vector from local to world space.
        /// </summary>
        /// <param name="direction">The direction vector to transform.</param>
        /// <returns>The transformed direction vector.</returns>
        public Vector3 TransformDirection(Vector3 direction)
        {
            return Vector3.Transform(direction, WorldRotation);
        }

        /// <summary>
        /// Transforms a position from world to local space.
        /// </summary>
        /// <param name="position">The position to transform.</param>
        /// <returns>The transformed position.</returns>
        public Vector3 InverseTransformPosition(Vector3 position)
        {
            return Vector3.Transform(position, WorldToLocalMatrix);
        }

        /// <summary>
        /// Transforms a direction vector from world to local space.
        /// </summary>
        /// <param name="direction">The direction vector to transform.</param>
        /// <returns>The transformed direction vector.</returns>
        public Vector3 InverseTransformDirection(Vector3 direction)
        {
            return Vector3.TransformNormal(direction, WorldToLocalMatrix);
        }

        /// <summary>
        /// Updates <see cref="LocalRotation"/> to a rotation around the specified axis by the specified angle.
        /// </summary>
        public void RotateAround(Vector3 axis, float angle)
        {
            LocalRotation = Quaternion.CreateFromAxisAngle(axis, angle);
        }

        /// <summary>
        /// Rotates this transform by the specified angles, in local or world space.
        /// </summary>
        public void Rotate(float xAngle, float yAngle, float zAngle, CoordinateSpace relativeTo = CoordinateSpace.Local)
        {
            if (relativeTo == CoordinateSpace.Local)
            {
                LocalRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, zAngle);
                LocalRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, yAngle);
                LocalRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, xAngle);
            }
            else
            {
                WorldRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, zAngle);
                WorldRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, yAngle);
                WorldRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, xAngle);
            }
        }

        /// <summary>
        /// Translates by the specified amount, in local or world space.
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="relativeTo"></param>
        public void Translate(Vector3 translation, CoordinateSpace relativeTo = CoordinateSpace.Local)
        {
            WorldPosition += (relativeTo == CoordinateSpace.Local)
                ? TransformDirection(translation)
                : translation;
        }
    }
}
