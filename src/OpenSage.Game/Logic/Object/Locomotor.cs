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
    public sealed class Locomotor
    {
        private readonly GameObject _gameObject;
        private readonly LocomotorSet _locomotorSet;
        private readonly LocomotorTemplate _locomotorTemplate;

        public float LiftFactor;

        public Locomotor(GameObject gameObject, LocomotorSet locomotorSet)
        {
            _gameObject = gameObject;
            _locomotorSet = locomotorSet;
            _locomotorTemplate = locomotorSet.Locomotor.Value;
            LiftFactor = 1.0f;
        }

        //TODO: check if the damaged values exists
        public float GetAcceleration()
        {
            return _gameObject.IsDamaged
                ? GetScaledLocomotorValue(_ => _.AccelerationDamaged)
                : GetScaledLocomotorValue(_ => _.Acceleration);
        }

        private float GetFrontWheelTurnAngle()
        {
            return GetScaledLocomotorValue(_ => _.FrontWheelTurnAngle);
        }

        private float GetTurnRate()
        {
            return _gameObject.IsDamaged
                ? GetScaledLocomotorValue(_ => _.TurnRateDamaged)
                : GetScaledLocomotorValue(_ => _.TurnRate);
        }

        public float GetSpeed()
        {
            // TODO: this is probably not correct for BFME
            if (_locomotorTemplate.Speed.HasValue)
            {
                return _gameObject.IsDamaged
                    ? GetScaledLocomotorValue(_ => _.SpeedDamaged)
                    : GetScaledLocomotorValue(_ => _.Speed.Value);
            }
            return _locomotorSet.Speed;
        }

        public float GetLift()
        {
            var currentLift = _gameObject.IsDamaged
                ? GetScaledLocomotorValue(_ => _.LiftDamaged)
                : GetScaledLocomotorValue(_ => _.Lift);
            return currentLift * LiftFactor;
        }

        public bool RotateToTargetDirection(in TimeInterval gameTime, in Vector3 targetDirection)
        {
            var deltaTime = (float) gameTime.DeltaTime.TotalSeconds;

            var transform = _gameObject.Transform;
            var currentYaw = -transform.EulerAngles.Z;

            var targetYaw = MathUtility.GetYawFromDirection(new Vector2(targetDirection.X, targetDirection.Y));
            var angleDelta = MathUtility.CalculateAngleDelta(targetYaw, currentYaw);

            if (MathF.Abs(angleDelta) < 0.1f) return true;

            var d = MathUtility.ToRadians(GetTurnRate()) * deltaTime;
            var newDelta = -MathF.Sign(angleDelta) * MathF.Min(MathF.Abs(angleDelta), MathF.Abs(d));
            var yaw = currentYaw + newDelta;

            transform.Rotation = Quaternion.CreateFromYawPitchRoll(0.0f, 0.0f, yaw);
            return false;
        }

        public bool MoveTowardsPosition(in TimeInterval gameTime, in Vector3 targetPoint, HeightMap heightMap, in Vector3? nextPoint)
        {
            var deltaTime = (float) gameTime.DeltaTime.TotalSeconds;

            var transform = _gameObject.Transform;

            var x = transform.Translation.X;
            var y = transform.Translation.Y;
            var z = transform.Translation.Z;
            var trans = transform.Translation;

            var oldSpeed = _gameObject.Speed;

            var delta = targetPoint - transform.Translation;

            // Distance is 2D
            var distanceRemaining = delta.Vector2XY().Length();
            var braking = GetScaledLocomotorValue(_ => _.Braking);

            switch (_locomotorTemplate.Appearance)
            {
                case LocomotorAppearance.Treads:
                    break;
                case LocomotorAppearance.Wings:
                    braking = 0; // TODO: aircrafts should only brake while landing (do they?)
                    break;
                default:
                    if (nextPoint != null) braking = 0;

                    var circumference = 360.0f / GetTurnRate() * oldSpeed;
                    var radius = circumference / MathUtility.TwoPi;

                    if (distanceRemaining < (radius + 0.25f) && nextPoint != null)
                        // turn towards next point
                        return true;
                    break;
            }

            if (distanceRemaining < 0.25f) return true;

            var minimumBrakingDistance = braking > 0
                ? (oldSpeed * oldSpeed) / (braking * 2.0f) // s = v² / 2a
                : 0;

            // Are we braking or accelerating?
            var accelerating = distanceRemaining > minimumBrakingDistance;
            var currentAcceleration = accelerating
                ? GetAcceleration()
                : -braking;

            var deltaSpeed = currentAcceleration * deltaTime;

            var newSpeed = oldSpeed + deltaSpeed;
            var reachedTurnSpeed = newSpeed >= GetScaledLocomotorValue(_ => _.MinTurnSpeed);
            _gameObject.Speed = Math.Clamp(newSpeed, 0, GetSpeed());

            // This locomotor speed is distance/second
            var distance = MathF.Min(_gameObject.Speed * deltaTime, distanceRemaining);

            // The distance we're moving
            var direction = Vector2.Normalize(delta.Vector2XY());
            var moveDirection = direction;

            // Calculate rotation
            var currentYaw = -transform.EulerAngles.Z;

            var targetYaw = MathUtility.GetYawFromDirection(direction);
            var angleDelta = MathUtility.CalculateAngleDelta(targetYaw, currentYaw);

            var d = MathUtility.ToRadians(GetTurnRate()) * deltaTime;
            var turningDelta = MathF.Min(MathF.Abs(angleDelta), MathF.Abs(d));
            var newDelta = -MathF.Sign(angleDelta) * turningDelta;

            var yaw = reachedTurnSpeed ? currentYaw + newDelta : currentYaw;

            _gameObject.SteeringWheelsYaw = Math.Clamp(-angleDelta, MathUtility.ToRadians(-GetFrontWheelTurnAngle()), MathUtility.ToRadians(GetFrontWheelTurnAngle()));

            var thrust = 0.0f;
            var deltaZ = 0.0f;

            // height
            var height = heightMap.GetHeight(x, y);
            switch (_locomotorTemplate.Appearance)
            {
                case LocomotorAppearance.Thrust:
                    var targetZ = targetPoint.Z;
                    if (nextPoint != null) targetZ = height + _locomotorTemplate.PreferredHeight;
                    deltaZ = (distance / distanceRemaining) * (targetZ - trans.Z);
                    trans.Z += deltaZ;
                    break;
                case LocomotorAppearance.Wings:
                case LocomotorAppearance.Hover:
                    thrust = GetCurrentThrust(height, deltaTime, transform);
                    if (!reachedTurnSpeed) break;
                    trans.Z += thrust;
                    break;
                case LocomotorAppearance.Treads:
                    trans.Z = height;
                    if (MathF.Abs(targetYaw - yaw) > MathUtility.ToRadians(2.0f)) //first fully rotate towards target point
                    {
                        distance = 0.0f;
                    }
                    break;
                default:
                    trans.Z = height;
                    break;
            }

            // moving direction
            var lookingDirection = transform.LookDirection;
            switch (_locomotorTemplate.Appearance)
            {
                case LocomotorAppearance.Thrust:
                case LocomotorAppearance.Treads:
                    break;
                default:
                    moveDirection = Vector2.Normalize(new Vector2(lookingDirection.X, lookingDirection.Y));
                    break;
            }

            // model roll and pitch
            var modelPitch = 0.0f;
            var modelRoll = 0.0f;
            switch (_locomotorTemplate.Appearance)
            {
                case LocomotorAppearance.Thrust:
                    modelPitch = deltaZ;
                    break;
                case LocomotorAppearance.Wings:
                    if (!reachedTurnSpeed) break;
                    modelPitch = -thrust / distance + distance * _locomotorTemplate.ForwardVelocityPitchFactor;
                    var angle = Math.Clamp(angleDelta, -MathUtility.PiOver4, MathUtility.PiOver4);
                    modelRoll = angle * distance * _locomotorTemplate.LateralVelocityRollFactor;
                    break;
                case LocomotorAppearance.Hover:
                    modelPitch = -distance * _locomotorTemplate.ForwardVelocityPitchFactor;
                    modelRoll = angleDelta * distance * _locomotorTemplate.LateralVelocityRollFactor;
                    break;
            }

            // roll and pitch according to terrain
            var worldPitch = 0.0f;
            var worldRoll = 0.0f;
            switch (_locomotorTemplate.Appearance)
            {
                case LocomotorAppearance.Treads:
                case LocomotorAppearance.FourWheels:
                    var normal = heightMap.GetNormal(x, y);

                    var deltaX = normal.X;
                    var deltaY = normal.Y;

                    worldPitch = (float) Math.Asin(deltaX);
                    worldRoll = (float) Math.Asin(deltaY);
                    break;
            }

            trans += new Vector3(moveDirection * distance, 0);
            _gameObject.Transform.Translation = trans;

            _gameObject.ModelTransform.Rotation = Quaternion.CreateFromYawPitchRoll(modelPitch, modelRoll, 0);
            _gameObject.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(worldPitch, worldRoll, yaw);

            return false;
        }

        public void MaintainPosition(in TimeInterval gameTime, HeightMap heightMap)
        {
            switch (_locomotorTemplate.Appearance)
            {
                case LocomotorAppearance.Wings:
                    var deltaTime = (float) gameTime.DeltaTime.TotalSeconds;

                    var transform = _gameObject.Transform;
                    var trans = transform.Translation;

                    _gameObject.Speed = GetSpeed();
                    var circumference = MathUtility.TwoPi * GetScaledLocomotorValue(x => x.CirclingRadius);
                    var timePerRoundtrip = circumference / _gameObject.Speed;

                    var moveDirection = Vector2.Normalize(new Vector2(transform.LookDirection.X, transform.LookDirection.Y));

                    var deltaTransform = new Vector3(moveDirection * _gameObject.Speed * deltaTime, 0);

                    trans += deltaTransform;
                    transform.Translation = trans;

                    var height = heightMap.GetHeight(transform.Translation.X, transform.Translation.Y);
                    trans.Z += GetCurrentThrust(height, deltaTime, transform);
                    transform.Translation = trans;

                    var normal = heightMap.GetNormal(transform.Translation.X, transform.Translation.Y);

                    // TODO: in order to align to the terrain, but this messes up our lookingDirection -> we move randomly instead of in a clear circle
                    var worldPitch = 0; //-(float) Math.Asin(normal.X);
                    var worldRoll = 0; //-(float) Math.Asin(normal.Y);

                    var deltaYaw = (deltaTime / timePerRoundtrip) * MathUtility.TwoPi;
                    var worldYaw = -transform.EulerAngles.Z + deltaYaw;
                    var modelRoll = -deltaYaw * deltaTransform.Length();
                    _gameObject.ModelTransform.Rotation = Quaternion.CreateFromYawPitchRoll(0, modelRoll, 0);
                    _gameObject.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(worldPitch, worldRoll, worldYaw);
                    break;
            }
        }

        private float GetCurrentThrust(float terrainHeight, float deltaTime, Transform transform)
        {
            var heightRemaining = (terrainHeight + _locomotorTemplate.PreferredHeight) - transform.Translation.Z;
            var lift = GetLift();
            _gameObject.Lift += lift;
            _gameObject.Lift = Math.Clamp(_gameObject.Lift, 0, lift);
            return MathF.Min(_gameObject.Lift * deltaTime, heightRemaining);
        }

        public float GetScaledLocomotorValue(Func<LocomotorTemplate, float> getValue)
        {
            return (_locomotorSet.Speed / 100.0f) * getValue(_locomotorTemplate);
        }

        public float GetLocomotorValue(Func<LocomotorTemplate, float> getValue) => getValue(_locomotorTemplate);
    }
}
