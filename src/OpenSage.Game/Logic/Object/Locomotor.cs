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

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Locomotor(GameObject gameObject, LocomotorSet locomotorSet)
        {
            _gameObject = gameObject;
            _locomotorSet = locomotorSet;
            _locomotorTemplate = locomotorSet.Locomotor.Value;
        }

        //TODO: check if the damaged values exists
        private float GetAcceleration()
        {
            var damaged = _gameObject.Damaged;
            return damaged ? GetLocomotorValue(x => x.AccelerationDamaged)
                           : GetLocomotorValue(x => x.Acceleration);
        }

        private float GetTurnRate()
        {
            var damaged = _gameObject.Damaged;
            return damaged ? GetLocomotorValue(x => x.TurnRateDamaged)
                           : GetLocomotorValue(x => x.TurnRate);
        }

        private float GetSpeed()
        {
            //TODO: this is probably not correct for BFME
            var damaged = _gameObject.Damaged;
            if (_locomotorTemplate.Speed.HasValue)
            {
                return damaged ? GetLocomotorValue(x => x.SpeedDamaged)
               : GetLocomotorValue(x => x.Speed.Value);
            }
            else
            {
                return _locomotorSet.Speed;
            }
        }

        private float GetLift()
        {
            var damaged = _gameObject.Damaged;
            return damaged ? GetLocomotorValue(x => x.LiftDamaged)
                           : GetLocomotorValue(x => x.Lift);
        }

        public void LocalLogicTick(in TimeInterval gameTime, in List<Vector3> targetPoints, HeightMap heightMap)
        {
            var deltaTime = (float) gameTime.DeltaTime.TotalSeconds;

            var transform = _gameObject.Transform;

            var x = transform.Translation.X;
            var y = transform.Translation.Y;
            var z = transform.Translation.Z;
            var trans = transform.Translation;

            var oldSpeed = _gameObject.Speed;

            // When we get to minimum braking distance, start braking.
            var deltaLast = targetPoints.Last() - transform.Translation;
            // Distance is 2D
            var distanceRemaining = deltaLast.Vector2XY().Length();
            var damaged = _gameObject.Damaged;

            var minimumBrakingDistance = (oldSpeed * oldSpeed) / GetLocomotorValue(x => x.Braking);

            // Are we braking or accelerating?
            var accelerating = distanceRemaining > minimumBrakingDistance;
            var currentAcceleration = accelerating
                ? GetAcceleration()
                : -GetLocomotorValue(x => x.Braking);

            var deltaSpeed = currentAcceleration * deltaTime;

            var newSpeed = oldSpeed + deltaSpeed;
            newSpeed = MathUtility.Clamp(newSpeed, 0.0f, GetSpeed());

            _gameObject.Speed = newSpeed;

            // This locomotor speed is distance/second
            var distance = MathF.Min(newSpeed * deltaTime, distanceRemaining);

            // Calculate translation
            var deltaFirst = targetPoints.First() - transform.Translation;
            // The distance we're moving
            var direction = Vector2.Normalize(deltaFirst.Vector2XY());
            trans += new Vector3(direction * distance, 0.0f);

            var height = heightMap.GetHeight(x, y);
            if (!_locomotorTemplate.StickToGround)
            {
                var heightRemaining = (height + _locomotorTemplate.PreferredHeight) - z;
                var oldLift = _gameObject.Lift;
                var lift = GetLift();
                var newLift = oldLift + lift;
                newLift = MathUtility.Clamp(newLift, 0.0f, lift);
                _gameObject.Lift = newLift;
                trans.Z += MathF.Min(newLift * deltaTime, heightRemaining);
            }
            else
            {
                trans.Z = height;
            }
            transform.Translation = trans;

            // Calculate rotation
            var currentYaw = -transform.EulerAngles.Z;

            var targetYaw = MathUtility.GetYawFromDirection(direction);
            var angleDelta = MathUtility.CalculateAngleDelta(targetYaw, currentYaw);

            var d = MathUtility.ToRadians(GetTurnRate()) * deltaTime;
            var newDelta = -MathF.Sign(angleDelta) * MathF.Min(MathF.Abs(angleDelta), MathF.Abs(d));
            var yaw = currentYaw + newDelta;
            var pitch = 0.0f;

            if (_locomotorTemplate.Appearance == LocomotorAppearance.FourWheels)
            {
                //TODO: fix this
                var normal = heightMap?.GetNormal(x, y) ?? Vector3.UnitZ;
                pitch = MathUtility.GetPitchFromDirection(normal);
            }

            transform.Rotation = Quaternion.CreateFromYawPitchRoll(pitch, 0.0f, yaw);
        }

        private float GetLocomotorValue(Func<LocomotorTemplate, float> getValue)
        {
            return (_locomotorSet.Speed / 100.0f) * getValue(_locomotorTemplate);
        }
    }
}
