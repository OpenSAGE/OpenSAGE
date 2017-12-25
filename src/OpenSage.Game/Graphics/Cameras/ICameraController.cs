using System;
using System.Numerics;

namespace OpenSage.Graphics.Cameras
{
    public interface ICameraController
    {
        float Pitch { get; set; }
        float Zoom { get; set; }

        Vector3 TerrainPosition { get; set; }

        void SetLookDirection(Vector3 lookDirection);

        void ModSetFinalPitch(float finalPitch);
        void ModSetFinalZoom(float finalZoom);
        void ModFinalLookToward(in Vector3 position);

        CameraAnimation StartAnimation(
            Vector3 startPosition,
            Vector3 endPosition,
            TimeSpan startTime,
            TimeSpan duration);

        void EndAnimation();

        void UpdateCamera(CameraComponent camera, in CameraInputState inputState, GameTime gameTime);
    }
}
