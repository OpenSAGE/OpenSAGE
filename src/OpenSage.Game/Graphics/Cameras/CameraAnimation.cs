using System;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras
{
    public sealed class CameraAnimation
    {
        private readonly Vector3 _startPosition;
        private readonly Vector3 _endPosition;

        private readonly Vector3 _startDirection;
        private Vector3? _endDirection;

        private readonly TimeSpan _startTime;
        private readonly TimeSpan _duration;
        private readonly TimeSpan _endTime;

        private readonly float _startPitch;
        private float? _endPitch;

        private readonly float _startZoom;
        private float? _endZoom;

        public bool Finished { get; private set; }

        public CameraAnimation(
            Vector3 startPosition,
            Vector3 endPosition,
            Vector3 startDirection,
            TimeSpan startTime,
            TimeSpan duration,
            float startPitch,
            float startZoom)
        {
            _startPosition = startPosition;
            _endPosition = endPosition;

            _startDirection = startDirection;

            _startTime = startTime;
            _duration = duration;
            _endTime = startTime + duration;

            _startPitch = startPitch;
            _startZoom = startZoom;
        }

        public void SetFinalLookToward(Vector3 lookToward)
        {
            _endDirection = Vector3.Normalize(lookToward - _endPosition);
        }

        public void SetFinalPitch(float endPitch)
        {
            _endPitch = endPitch;
        }

        public void SetFinalZoom(float endZoom)
        {
            _endZoom = endZoom;
        }

        internal void Update(RtsCameraController camera, GameTime gameTime)
        {
            var currentTimeFraction = (float) ((gameTime.TotalGameTime - _startTime).TotalSeconds / _duration.TotalSeconds);
            currentTimeFraction = Math.Min(currentTimeFraction, 1);

            var movementDirection = _endPosition - _startPosition;

            var currentPosition = _startPosition + movementDirection * currentTimeFraction;

            camera.TerrainPosition = currentPosition;

            if (_endDirection != null)
            {
                var lookDirection = Vector3.Normalize(Vector3Utility.Slerp(_startDirection, _endDirection.Value, currentTimeFraction));

                camera.LookDirection = lookDirection;

                camera.TerrainPosition = Vector3Utility.Slerp(_startPosition + _startDirection, _endPosition + _endDirection.Value, currentTimeFraction);
            }

            if (_endPitch != null)
            {
                var pitch = MathUtility.Lerp(_startPitch, _endPitch.Value, currentTimeFraction);

                camera.Pitch = pitch;
            }

            if (_endZoom != null)
            {
                var zoom = MathUtility.Lerp(_startZoom, _endZoom.Value, currentTimeFraction);

                camera.Zoom = zoom;
            }

            if (gameTime.TotalGameTime > _endTime)
            {
                Finished = true;
            }
        }
    }
}
