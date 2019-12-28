using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object
{
    internal sealed class Locomotor
    {
        private readonly GameObject _gameObject;
        private readonly LocomotorSet _locomotorSet;
        private readonly LocomotorTemplate _locomotorTemplate;

        public Locomotor(GameObject gameObject, LocomotorSet locomotorSet)
        {
            _gameObject = gameObject;
            _locomotorSet = locomotorSet;
            _locomotorTemplate = locomotorSet.Locomotor.Value;
        }

        public void LocalLogicTick(in TimeInterval gameTime, in List<Vector3> targetPoints, HeightMap heightMap)
        {
            var deltaTime = (float) gameTime.DeltaTime.TotalSeconds;

            var transform = _gameObject.Transform;

            var x = transform.Translation.X;
            var y = transform.Translation.Y;
            var trans = transform.Translation;

            var oldSpeed = _gameObject.Speed;

            // When we get to minimum braking distance, start braking.
            var deltaLast = targetPoints.Last() - transform.Translation;
            var distanceRemaining = deltaLast.Length();

            var minimumBrakingDistance = (oldSpeed * oldSpeed) / GetLocomotorValue(x => x.Braking);

            // Are we braking or accelerating?
            var accelerating = distanceRemaining > minimumBrakingDistance;
            var currentAcceleration = accelerating
                ? GetLocomotorValue(x => x.Acceleration)
                : -GetLocomotorValue(x => x.Braking);

            var deltaSpeed = currentAcceleration * deltaTime;

            var newSpeed = oldSpeed + deltaSpeed;
            newSpeed = MathUtility.Clamp(newSpeed, 0.0f, GetLocomotorValue(x => x.Speed));

            _gameObject.Speed = newSpeed;

            // This locomotor speed is distance/second
            var distance = newSpeed * deltaTime;
            if (distance > distanceRemaining)
            {
                distance = distanceRemaining;
            }

            // TODO: Do this properly. Needs to be negative for reverse?
            //_gameObject.Speed = GetLocomotorValue(l => l.Speed);

            //var currentAngle = -transform.EulerAngles.Z;
            //var angleDelta = TargetAngle - currentAngle;

            //var d = GetLocomotorValue(l => l.TurnRate) * deltaTime * 0.1f;
            //var newAngle = currentAngle + (angleDelta * d);
            ////var newAngle = currentAngle + d;

            //if (Math.Abs(angleDelta) > 0.1f)
            //{
            //    var pitch = 0.0f;
            //    if (_locomotorTemplate.Appearance == LocomotorAppearance.FourWheels) // TODO
            //    {
            //        var normal = heightMap?.GetNormal(x, y) ?? new Vector3();
            //        pitch = (float) Math.Atan2(Vector3.UnitZ.Y - normal.Y, Vector3.UnitZ.X - normal.X);
            //    }
            //    transform.Rotation = Quaternion.CreateFromYawPitchRoll(pitch, 0.0f, newAngle);
            //}

            var deltaFirst = targetPoints.First() - transform.Translation;
            var direction = Vector3.Normalize(deltaFirst);
            trans += direction * distance;
            trans.Z = heightMap.GetHeight(trans.X, trans.Y);
            transform.Translation = trans;
        }

        private float GetLocomotorValue(Func<LocomotorTemplate, float> getValue)
        {
            return (_locomotorSet.Speed / 100.0f) * getValue(_locomotorTemplate);
        }
    }
}
