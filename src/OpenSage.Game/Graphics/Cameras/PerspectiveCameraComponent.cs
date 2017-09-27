using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras
{
    /// <summary>
    /// Represents a perspective projection camera. 
    /// </summary>
    /// <remarks>
    /// PerspectiveCamera specifies a projection of a 3-D model to a 2-D visual surface. This projection includes
    /// perspective foreshortening. In other words, the PerspectiveCamera describes a frustrum whose sides converge 
    /// toward a point on the horizon. Objects closer to the camera appear larger, and objects farther from the camera
    /// appear smaller.
    /// </remarks>
    public sealed class PerspectiveCameraComponent : CameraComponent
    {
        private float _fieldOfView;

        /// <summary>
        /// Gets or sets a value that represents the camera's horizontal field of view in degrees. 
        /// </summary>
        public float FieldOfView
        {
            get { return _fieldOfView; }
            set
            {
                _fieldOfView = value;
                ClearCachedProjectionMatrix();
            }
        }

        /// <summary>
        /// Creates a new <see cref="PerspectiveCameraComponent" />.
        /// </summary>
        public PerspectiveCameraComponent()
        {
            FieldOfView = 45;
        }

        /// <inheritdoc />
        protected override Matrix4x4 GetProjectionMatrix(float aspectRatio)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(
                MathUtility.ToRadians(FieldOfView), aspectRatio,
                NearPlaneDistance, FarPlaneDistance);
        }
    }
}
