using System.Numerics;

namespace OpenSage.Graphics.Cameras
{
    public interface ICamera
    {
        public float FieldOfView { get; set; }
        public float FarPlaneDistance { get; set; }
        public Vector3 Position { get; }
        public void SetLookAt(in Vector3 cameraPosition, in Vector3 cameraTarget, in Vector3 up);
    }
}