using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Graphics.Cameras;

public interface IEditorCameraController
{
    float Zoom { get; set; }

    Vector3 Position { get; set; }

    void SetPitch(float pitch);

    void SetLookDirection(Vector3 lookDirection);

    void UpdateCamera(Camera camera, in EditorCameraInputState inputState, in TimeInterval gameTime);

    void GoToObject(GameObject gameObject);

}
