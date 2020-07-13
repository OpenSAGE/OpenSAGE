using System;
using System.Numerics;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Locomotors control how an object moves towards a target position.
    /// (Locomotors are not the only way an object can move; some behaviors
    /// can do this too, without using a locomotor, for example DumbProjectileBehavior.)
    ///
    /// Locomotor "appearance" controls many aspects of their movement:
    ///
    /// - TREADS
    ///   This is supposed to move like a tank with treads. The object turns while stationary,
    ///   and moves only in a straight line.
    ///
    /// - WINGS
    ///
    /// - HOVER
    ///
    /// - THRUST
    ///
    /// - TWO_LEGS
    ///
    /// - FOUR_WHEELS
    ///
    /// Other Locomotor properties allow more fine-grained control:
    ///
    /// - ForwardAccelerationPitchFactor and LateralAccelerationRollFactor
    ///   These control how much acceleration and cornering, respectively, will cause
    ///   the chassis to pitch or roll.
    /// </summary>
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

        //TODO: check if the damaged values exists
        private float GetAcceleration()
        {
            return _gameObject.IsDamaged
                ? GetLocomotorValue(x => x.AccelerationDamaged)
                : GetLocomotorValue(x => x.Acceleration);
        }

        private float GetTurnRate()
        {
            return _gameObject.IsDamaged
                ? GetLocomotorValue(x => x.TurnRateDamaged)
                : GetLocomotorValue(x => x.TurnRate);
        }

        private float GetSpeed()
        {
            // TODO: this is probably not correct for BFME
            if (_locomotorTemplate.Speed.HasValue)
            {
                return _gameObject.IsDamaged
                    ? GetLocomotorValue(x => x.SpeedDamaged)
                    : GetLocomotorValue(x => x.Speed.Value);
            }
            else
            {
                return _locomotorSet.Speed;
            }
        }

        private float GetLift()
        {
            return _gameObject.IsDamaged
                ? GetLocomotorValue(x => x.LiftDamaged)
                : GetLocomotorValue(x => x.Lift);
        }

        public void MoveTowardsPosition(in TimeInterval gameTime, in Vector3 targetPoint, HeightMap heightMap)
        {
            var deltaTime = (float) gameTime.DeltaTime.TotalSeconds;

            var transform = _gameObject.Transform;

            var x = transform.Translation.X;
            var y = transform.Translation.Y;
            var z = transform.Translation.Z;
            var trans = transform.Translation;

            var oldSpeed = _gameObject.Speed;

            // When we get to minimum braking distance, start braking.
            var delta = targetPoint - transform.Translation;
            // Distance is 2D
            var distanceRemaining = delta.Vector2XY().Length();
            var damaged = _gameObject.IsDamaged;

            var braking = GetLocomotorValue(x => x.Braking);
            var minimumBrakingDistance = braking > 0
                ? (oldSpeed * oldSpeed) / braking
                : 0;

            // Are we braking or accelerating?
            var accelerating = distanceRemaining > minimumBrakingDistance;
            var currentAcceleration = accelerating
                ? GetAcceleration()
                : -GetLocomotorValue(x => x.Braking);

            var deltaSpeed = currentAcceleration * deltaTime;

            var newSpeed = oldSpeed + deltaSpeed;
            newSpeed = Math.Clamp(newSpeed, 0.0f, GetSpeed());

            _gameObject.Speed = newSpeed;

            // This locomotor speed is distance/second
            var distance = MathF.Min(newSpeed * deltaTime, distanceRemaining);

            // The distance we're moving
            var direction = Vector2.Normalize(delta.Vector2XY());
            trans += new Vector3(direction * distance, 0.0f);

            switch (_locomotorTemplate.Appearance)
            {
                case LocomotorAppearance.Thrust:
                    trans.Z += (distance / distanceRemaining) * (targetPoint.Z - trans.Z);
                    break;

                default:
                    var height = heightMap.GetHeight(x, y);
                    if (!_locomotorTemplate.StickToGround)
                    {
                        var heightRemaining = (height + _locomotorTemplate.PreferredHeight) - z;
                        var oldLift = _gameObject.Lift;
                        var lift = GetLift();
                        var newLift = oldLift + lift;
                        newLift = Math.Clamp(newLift, 0.0f, lift);
                        _gameObject.Lift = newLift;
                        trans.Z += MathF.Min(newLift * deltaTime, heightRemaining);
                    }
                    else
                    {
                        trans.Z = height;
                    }
                    break;
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
