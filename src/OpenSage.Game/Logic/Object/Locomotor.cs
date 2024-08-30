﻿using System;
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
    public sealed class Locomotor : IPersistableObject
    {
        private readonly GameObject _gameObject;
        private readonly float _baseSpeed;

        private uint _frameSomething;
        private Vector3 _positionSomething;
        private float _unknownFloat1;
        private float _unknownFloat2 = 99999.0f;
        private float _unknownFloat3 = 99999.0f;
        private float _unknownFloat4 = 99999.0f;
        private float _unknownFloat5 = 99999.0f;
        private float _unknownFloat6 = 99999.0f;
        private float _unknownFloat7 = 1.0f;
        private LocomotorFlags _flags;
        private float _unknownFloat8;
        private float _unknownFloat9;
        private float _unknownFloat10;
        private float _unknownFloat11;

        public readonly LocomotorTemplate LocomotorTemplate;

        public float LiftFactor;

        public Locomotor(GameObject gameObject, LocomotorTemplate locomotorTemplate, float baseSpeed)
        {
            _gameObject = gameObject;
            LocomotorTemplate = locomotorTemplate;
            _baseSpeed = baseSpeed;
            LiftFactor = 1.0f;
        }

        //TODO: check if the damaged values exists
        public float GetAcceleration()
        {
            return _gameObject.IsDamaged
                ? GetScaledLocomotorValue(x => x.AccelerationDamaged)
                : GetScaledLocomotorValue(x => x.Acceleration);
        }

        private float GetFrontWheelTurnAngle()
        {
            return GetScaledLocomotorValue(x => x.FrontWheelTurnAngle);
        }

        private float GetTurnRate()
        {
            return _gameObject.IsDamaged
                ? GetScaledLocomotorValue(x => x.TurnRateDamaged)
                : GetScaledLocomotorValue(x => x.TurnRate);
        }

        public float GetSpeed()
        {
            if (LocomotorTemplate.Speed.HasValue)
            {
                return _gameObject.IsDamaged
                    ? GetScaledLocomotorValue(x => x.SpeedDamaged)
                    : GetScaledLocomotorValue(x => x.Speed.Value);
            }
            // TODO: How do we get the damaged speed for BFME?
            return _baseSpeed;
        }

        public float GetLift()
        {
            var currentLift = _gameObject.IsDamaged
                ? GetScaledLocomotorValue(x => x.LiftDamaged)
                : GetScaledLocomotorValue(x => x.Lift);
            return currentLift * LiftFactor;
        }

        public bool RotateToTargetDirection(in Vector3 targetDirection)
        {
            var currentYaw = _gameObject.Yaw;

            var targetYaw = MathUtility.GetYawFromDirection(new Vector2(targetDirection.X, targetDirection.Y));
            var angleDelta = MathUtility.CalculateAngleDelta(targetYaw, currentYaw);

            if (MathF.Abs(angleDelta) < 0.1f)
            {
                return true;
            }

            var d = GetTurnRate();
            var newDelta = -MathF.Sign(angleDelta) * MathF.Min(MathF.Abs(angleDelta), MathF.Abs(d));
            var yaw = currentYaw + newDelta;

            _gameObject.SetRotation(Quaternion.CreateFromYawPitchRoll(0.0f, 0.0f, yaw));
            return false;
        }

        public bool MoveTowardsPosition(in Vector3 targetPoint, HeightMap heightMap, in Vector3? nextPoint)
        {
            var translation = _gameObject.Translation;
            var x = translation.X;
            var y = translation.Y;
            var z = translation.Z;

            var oldSpeed = _gameObject.Speed;

            var delta = targetPoint - translation;

            // Distance is 2D
            var distanceRemaining = delta.Vector2XY().Length();
            var braking = GetScaledLocomotorValue(_ => _.Braking);

            switch (LocomotorTemplate.Appearance)
            {
                case LocomotorAppearance.Treads:
                    break;
                case LocomotorAppearance.Wings:
                    braking = 0; // TODO: aircrafts should only brake while landing (do they?)
                    break;
                default:
                    if (nextPoint != null)
                    {
                        braking = 0;
                    }

                    var circumference = MathUtility.TwoPi / GetTurnRate() * oldSpeed;
                    var radius = circumference / MathUtility.TwoPi;

                    if (distanceRemaining < (radius + 0.25f) && nextPoint != null)
                    {
                        // turn towards next point
                        return true;
                    }

                    break;
            }

            if (distanceRemaining < 0.25f)
            {
                return true;
            }

            var minimumBrakingDistance = braking > 0
                ? (oldSpeed * oldSpeed) / (braking * 2.0f) // s = v² / 2a
                : 0;

            // Are we braking or accelerating?
            var accelerating = distanceRemaining > minimumBrakingDistance;
            var currentAcceleration = accelerating
                ? GetAcceleration()
                : -braking;

            var deltaSpeed = currentAcceleration;

            var newSpeed = oldSpeed + deltaSpeed;
            var reachedTurnSpeed = newSpeed >= GetScaledLocomotorValue(_ => _.MinTurnSpeed);
            _gameObject.Speed = Math.Clamp(newSpeed, 0, GetSpeed());

            // This locomotor speed is distance/second
            var distance = MathF.Min(_gameObject.Speed, distanceRemaining);

            // The distance we're moving
            var direction = Vector2.Normalize(delta.Vector2XY());
            var moveDirection = direction;

            // Calculate rotation
            var currentYaw = _gameObject.Yaw;

            var targetYaw = MathUtility.GetYawFromDirection(direction);
            var angleDelta = MathUtility.CalculateAngleDelta(targetYaw, currentYaw);

            var d = GetTurnRate();
            var turningDelta = MathF.Min(MathF.Abs(angleDelta), MathF.Abs(d));
            var newDelta = -MathF.Sign(angleDelta) * turningDelta;

            var yaw = reachedTurnSpeed ? currentYaw + newDelta : currentYaw;

            _gameObject.SteeringWheelsYaw = Math.Clamp(-angleDelta, -GetFrontWheelTurnAngle(), GetFrontWheelTurnAngle());

            var thrust = 0.0f;
            var deltaZ = 0.0f;

            // height
            var height = heightMap.GetHeight(x, y);
            switch (LocomotorTemplate.Appearance)
            {
                case LocomotorAppearance.Thrust:
                    var targetZ = targetPoint.Z;
                    if (nextPoint != null)
                    {
                        targetZ = height + LocomotorTemplate.PreferredHeight;
                    }

                    deltaZ = (distance / distanceRemaining) * (targetZ - translation.Z);
                    translation.Z += deltaZ;
                    break;
                case LocomotorAppearance.GiantBird:
                case LocomotorAppearance.Wings:
                case LocomotorAppearance.Hover:
                    thrust = GetCurrentThrust(height, z);
                    if (!reachedTurnSpeed)
                    {
                        break;
                    }

                    translation.Z += thrust;
                    break;
                case LocomotorAppearance.Treads:
                    translation.Z = height;
                    if (MathF.Abs(targetYaw - yaw) > MathUtility.ToRadians(2.0f)) //first fully rotate towards target point
                    {
                        distance = 0.0f;
                    }
                    break;
                default:
                    translation.Z = height;
                    break;
            }

            // moving direction
            var lookingDirection = _gameObject.LookDirection;
            switch (LocomotorTemplate.Appearance)
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
            switch (LocomotorTemplate.Appearance)
            {
                case LocomotorAppearance.Thrust:
                    modelPitch = deltaZ;
                    break;
                case LocomotorAppearance.Wings:
                    if (!reachedTurnSpeed)
                    {
                        break;
                    }

                    modelPitch = -thrust / distance + distance * LocomotorTemplate.ForwardVelocityPitchFactor;
                    var angle = Math.Clamp(angleDelta, -MathUtility.PiOver4, MathUtility.PiOver4);
                    modelRoll = angle * distance * LocomotorTemplate.LateralVelocityRollFactor;
                    break;
                case LocomotorAppearance.Hover:
                    modelPitch = -distance * LocomotorTemplate.ForwardVelocityPitchFactor;
                    modelRoll = angleDelta * distance * LocomotorTemplate.LateralVelocityRollFactor;
                    break;
            }

            // roll and pitch according to terrain
            var worldPitch = 0.0f;
            var worldRoll = 0.0f;
            switch (LocomotorTemplate.Appearance)
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

            translation += new Vector3(moveDirection * distance, 0);
            _gameObject.SetTranslation(translation);

            _gameObject.ModelTransform.Rotation = Quaternion.CreateFromYawPitchRoll(modelPitch, modelRoll, 0);
            _gameObject.SetRotation(Quaternion.CreateFromYawPitchRoll(worldPitch, worldRoll, yaw));

            return false;
        }

        public void MaintainPosition(HeightMap heightMap)
        {
            var translation = _gameObject.Translation;
            var height = heightMap.GetHeight(translation.X, translation.Y);
            switch (LocomotorTemplate.Appearance)
            {
                case LocomotorAppearance.Wings:
                    _gameObject.Speed = GetSpeed();
                    var circumference = MathUtility.TwoPi * GetScaledLocomotorValue(x => x.CirclingRadius);
                    var timePerRoundtrip = circumference / _gameObject.Speed;

                    var moveDirection = Vector2.Normalize(new Vector2(_gameObject.LookDirection.X, _gameObject.LookDirection.Y));

                    var deltaTransform = new Vector3(moveDirection * _gameObject.Speed, 0);

                    translation += deltaTransform;

                    translation.Z += GetCurrentThrust(height, translation.Z);
                    _gameObject.SetTranslation(translation);

                    var normal = heightMap.GetNormal(translation.X, translation.Y);

                    // TODO: in order to align to the terrain, but this messes up our lookingDirection -> we move randomly instead of in a clear circle
                    var worldPitch = 0; //-(float) Math.Asin(normal.X);
                    var worldRoll = 0; //-(float) Math.Asin(normal.Y);

                    var deltaYaw = timePerRoundtrip * MathUtility.TwoPi;
                    var worldYaw = _gameObject.Yaw + deltaYaw;
                    var modelRoll = -deltaYaw * deltaTransform.Length();
                    _gameObject.ModelTransform.Rotation = Quaternion.CreateFromYawPitchRoll(0, modelRoll, 0);
                    _gameObject.SetRotation(Quaternion.CreateFromYawPitchRoll(worldPitch, worldRoll, worldYaw));
                    break;
                case LocomotorAppearance.Hover:
                    _gameObject.SetTranslation(translation with { Z = translation.Z + GetCurrentThrust(height, translation.Z) });
                    break;
            }
        }

        private float GetCurrentThrust(float terrainHeight, float height)
        {
            var heightRemaining = (terrainHeight + LocomotorTemplate.PreferredHeight) - height;
            var lift = GetLift();
            if (heightRemaining < 0)
            {
                lift *= -1;
            }
            _gameObject.Lift += lift;

            var maxLiftVelocity = lift * Game.LogicFramesPerSecond; // lift is given in dist/sec^s
            _gameObject.Lift = heightRemaining >= 0 ? Math.Clamp(_gameObject.Lift, 0, maxLiftVelocity) : Math.Clamp(_gameObject.Lift, maxLiftVelocity, 0);
            return heightRemaining >= 0 ? MathF.Min(_gameObject.Lift, heightRemaining) : MathF.Max(_gameObject.Lift, heightRemaining);
        }

        public float GetScaledLocomotorValue(Func<LocomotorTemplate, float> getValue)
        {
            return (_baseSpeed / 100.0f) * getValue(LocomotorTemplate);
        }

        public float GetLocomotorValue(Func<LocomotorTemplate, float> getValue) => getValue(LocomotorTemplate);

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.PersistFrame(ref _frameSomething); // Equal to CurrentFrame + 75
            reader.PersistVector3(ref _positionSomething);
            reader.PersistSingle(ref _unknownFloat1);

            reader.PersistSingle(ref _unknownFloat2); // 99999
            if (_unknownFloat2 != 99999.0f)
            {
                throw new InvalidStateException();
            }

            reader.PersistSingle(ref _unknownFloat3); // 99999
            if (_unknownFloat3 != 99999.0f)
            {
                throw new InvalidStateException();
            }

            reader.PersistSingle(ref _unknownFloat4); // 99999
            if (_unknownFloat4 != 99999.0f)
            {
                throw new InvalidStateException();
            }

            reader.PersistSingle(ref _unknownFloat5); // 99999
            if (_unknownFloat5 != 99999.0f)
            {
                throw new InvalidStateException();
            }

            reader.PersistSingle(ref _unknownFloat6); // 99999, 0

            reader.PersistSingle(ref _unknownFloat7); // 1
            if (_unknownFloat7 != 1.0f)
            {
                throw new InvalidStateException();
            }

            reader.PersistEnumFlags(ref _flags); // 0, 4
            reader.PersistSingle(ref _unknownFloat8); // 0, 100

            reader.PersistSingle(ref _unknownFloat9); // 1, 0 mid humvee evac (unsure if related)
            if (_unknownFloat9 != 0f && _unknownFloat9 != 1.0f)
            {
                throw new InvalidStateException();
            }

            reader.PersistSingle(ref _unknownFloat10); // 0.4849...
            reader.PersistSingle(ref _unknownFloat11); // 0.0892...
        }
    }

    [Flags]
    public enum LocomotorFlags
    {
        None = 0,
        Unknown1 = 1 << 0,
        Unknown2 = 1 << 1,
        Unknown3 = 1 << 2,
        Unknown4 = 1 << 3,
        Unknown5 = 1 << 5,
        Unknown6 = 1 << 6,
        Unknown7 = 1 << 7,
        Unknown8 = 1 << 8,
        Unknown9 = 1 << 9,
        Unknown10 = 1 << 10,
        Unknown11 = 1 << 11,
        Unknown12 = 1 << 12,
    }
}
