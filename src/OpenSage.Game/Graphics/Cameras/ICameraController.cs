using System;
using System.Collections.Generic;
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
        void ModLookToward(in Vector3 position);

        CameraAnimation StartAnimation(
            IReadOnlyList<Vector3> points,
            TimeSpan startTime,
            TimeSpan duration);

        void EndAnimation();

        void UpdateCamera(Camera camera, in CameraInputState inputState, in TimeInterval gameTime);
    }
}
