using System;
using System.Numerics;

namespace OpenSage.Graphics.Cameras
{
    public sealed class CameraAnimation
    {
        private readonly Vector3 _startPosition;
        private readonly Vector3 _endPosition;

        private readonly Vector3 _startDirection;

        private readonly TimeSpan _startTime;
        private readonly TimeSpan _duration;
        private readonly TimeSpan _endTime;

        public bool Finished { get; private set; }

        public CameraAnimation(
            Vector3 startPosition,
            Vector3 endPosition,
            Vector3 startDirection,
            TimeSpan startTime,
            TimeSpan duration)
        {
            _startPosition = startPosition;
            _endPosition = endPosition;

            _startDirection = startDirection;

            _startTime = startTime;
            _duration = duration;
            _endTime = startTime + duration;
        }

        public void SetFinalDirection(Vector3 endDirection)
        {

        }

        public void SetFinalPitch(float endPitch)
        {

        }

        public void SetFinalZoom(float endZoom)
        {

        }

        internal void Update(RtsCameraController camera, GameTime gameTime)
        {
            // TODO: Do update.
            var currentTimeFraction = (float) ((gameTime.TotalGameTime - _startTime).TotalSeconds / _duration.TotalSeconds);
            currentTimeFraction = Math.Min(currentTimeFraction, 1);

            var direction = _endPosition - _startPosition;

            var currentPosition = _startPosition + direction * currentTimeFraction;

            camera.TerrainPosition = currentPosition;

            if (gameTime.TotalGameTime > _endTime)
            {
                Finished = true;
            }
        }
    }
}
