using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using OpenSage.Logic.AI;
using OpenSage.Mathematics;
using OpenSage.Utilities;

namespace OpenSage.Logic.Object;

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
    private const float DonutTimeDelaySeconds = 2.5f;
    private const float DonutDistance = 4.0f * AIPathfind.PathfindCellSizeF;
    private const float MaxBrakingFactor = 5.0f;

    private readonly GameEngine _gameEngine;
    private readonly float _baseSpeedFactor;

    private LogicFrame _donutTimer;
    private Vector3 _maintainPosition;
    private float _brakingFactor;
    private float _maxLift = LocomotorTemplate.BigNumber;
    private float _maxSpeed = LocomotorTemplate.BigNumber;
    private float _maxAcceleration = LocomotorTemplate.BigNumber;
    private float _maxBraking = LocomotorTemplate.BigNumber;
    private float _maxTurnRate = LocomotorTemplate.BigNumber;
    private float _closeEnoughDistance = 1.0f;
    private LocomotorFlags _flags;
    private float _preferredHeight;
    private float _preferredHeightDamping;
    private float _angleOffset;
    private float _offsetIncrement;

    public readonly LocomotorTemplate LocomotorTemplate;

    // TODO(Port): Remove this.
    public float LiftFactor;

    public float MinSpeed => LocomotorTemplate.MinSpeed;

    public Locomotor(GameEngine gameEngine, LocomotorTemplate template, float baseSpeed)
    {
        _gameEngine = gameEngine;

        LocomotorTemplate = template;

        _baseSpeedFactor = baseSpeed / 100.0f;
        LiftFactor = 1.0f;

        _closeEnoughDistance = LocomotorTemplate.CloseEnoughDist;
        SetFlag(LocomotorFlags.IsCloseEnoughDistance3D, template.CloseEnoughDist3D);

        _preferredHeight = template.PreferredHeight;
        _preferredHeightDamping = template.PreferredHeightDamping;

        _angleOffset = gameEngine.Random.NextSingle(-MathF.PI / 6.0f, MathF.PI / 6.0f);
        _offsetIncrement = (MathF.PI / 40.0f) * (gameEngine.Random.NextSingle(0.8f, 1.2f) / template.WanderLengthFactor);
        SetFlag(LocomotorFlags.OffsetIncreasing, gameEngine.Random.NextBoolean());

        ResetDonutTimer();
    }

    public void StartMove()
    {
        ResetDonutTimer();
    }

    private void ResetDonutTimer()
    {
        _donutTimer = _gameEngine.GameLogic.CurrentFrame + new LogicFrameSpan((uint)(DonutTimeDelaySeconds * _gameEngine.LogicFramesPerSecond));
    }

    private bool HasMovementPenalty(BodyDamageType condition)
    {
        return !condition.IsBetterThan(_gameEngine.AssetStore.GameData.Current.MovementPenaltyDamageState);
    }

    /// <summary>
    /// Returns the maximum speed for the specified condition.
    /// </summary>
    public float GetMaxSpeedForCondition(BodyDamageType condition)
    {
        var speed = HasMovementPenalty(condition)
            ? LocomotorTemplate.MaxSpeedDamaged * _baseSpeedFactor
            : LocomotorTemplate.MaxSpeed * _baseSpeedFactor;

        if (speed > _maxSpeed)
        {
            speed = _maxSpeed;
        }

        return speed;
    }

    /// <summary>
    /// Returns the maximum turn rate for the specified condition.
    /// </summary>
    public float GetMaxTurnRate(BodyDamageType condition)
    {
        var turn = HasMovementPenalty(condition)
            ? LocomotorTemplate.MaxTurnRateDamaged
            : LocomotorTemplate.MaxTurnRate;

        if (turn > _maxTurnRate)
        {
            turn = _maxTurnRate;
        }

        const float turnFactor = 2.0f;
        if (GetFlag(LocomotorFlags.UltraAccurate))
        {
            turn *= turnFactor; // Monster turning ability
        }

        return turn;
    }

    /// <summary>
    /// Returns the maximum acceleration for the specified condition.
    /// </summary>
    public float GetMaxAcceleration(BodyDamageType condition)
    {
        var acceleration = HasMovementPenalty(condition)
            ? LocomotorTemplate.AccelerationDamaged
            : LocomotorTemplate.Acceleration;

        if (acceleration > _maxAcceleration)
        {
            acceleration = _maxAcceleration;
        }

        return acceleration;
    }

    /// <summary>
    /// Returns the maximum braking.
    /// </summary>
    public float GetBraking()
    {
        var braking = LocomotorTemplate.Braking;

        if (braking > _maxBraking)
        {
            braking = _maxBraking;
        }

        return braking;
    }

    /// <summary>
    /// Returns the maximum lift for the specified condition.
    /// </summary>
    public float GetMaxLift(BodyDamageType condition)
    {
        var lift = HasMovementPenalty(condition)
            ? LocomotorTemplate.LiftDamaged
            : LocomotorTemplate.Lift;

        if (lift > _maxLift)
        {
            lift = _maxLift;
        }

        return lift;
    }

    public void LocoUpdateMoveTowardsAngle(GameObject obj, float goalAngle)
    {
        SetFlag(LocomotorFlags.MaintainPositionIsValid, false);

        if (obj == null)
        {
            return;
        }

        var physics = obj.Physics;
        if (physics == null)
        {
            Debug.Fail("You can only apply Locomotors to objects with Physics");
            return;
        }

        // Skip moveTowardsAngle if physics says you're stunned.
        if (physics.IsStunned)
        {
            return;
        }

        var minSpeed = MinSpeed;
        if (minSpeed > 0)
        {
            // Can't stay in one place; move in the desired direction at min speed.
            var desiredPos = obj.Translation;
            desiredPos.X += MathF.Cos(goalAngle) * minSpeed * 2;
            desiredPos.Y += MathF.Sin(goalAngle) * minSpeed * 2;
            // Pass a huge number for "dist to goal", so that we don't think
            // we're nearing our destination and thus slow down.
            var onPathDistToGoal = 99999.0f; ;
            var blocked = false;
            LocoUpdateMoveTowardsPosition(obj, desiredPos, onPathDistToGoal, minSpeed, ref blocked);

            // Don't need to call HandleBehaviorZ here, since
            // LocoUpdateMoveTowardsPosition will do so.
            return;
        }
        else
        {
            Debug.Assert(LocomotorTemplate.Appearance != LocomotorAppearance.Thrust, "THRUST should always have minspeeds!");
            var desiredPos = obj.Translation;
            desiredPos.X += MathF.Cos(goalAngle) * 1000.0f;
            desiredPos.Y += MathF.Sin(goalAngle) * 1000.0f;
            var rotating = RotateTowardsPosition(obj, desiredPos, out _);
            physics.Turning = rotating;
            HandleBehaviorZ(obj, physics, obj.Translation);
        }
    }

    private PhysicsTurningType RotateTowardsPosition(GameObject obj, in Vector3 goalPos, out float relAngle)
    {
        var bdt = obj.BodyModule.DamageState;
        var turnRate = GetMaxTurnRate(bdt);

        return RotateObjAroundLocoPivot(obj, goalPos, turnRate, out relAngle);
    }

    public void SetPhysicsOptions(GameObject obj)
    {
        var physics = obj.Physics;
        if (physics == null)
        {
            Debug.Fail("You can only apply Locomotors to objects with Physics");
            return;
        }

        // Crank up the friction in ultra-accurate mode to increase movement precision.
        const float extraFriction = 0.5f;
        var extraExtraFriction = GetFlag(LocomotorFlags.UltraAccurate)
            ? extraFriction
            : 0.0f;

        physics.ExtraFriction = LocomotorTemplate.Extra2DFriction + extraExtraFriction;

        // You'd think we wouldn't want friction in the air, but it's needed
        // for realistic behavior.
        physics.AllowAirborneFriction = LocomotorTemplate.Apply2DFrictionWhenAirborne;

        // Walking guys aren't allowed to catch huge (or even small) air.
        physics.StickToGround = LocomotorTemplate.StickToGround;
    }

    public void LocoUpdateMoveTowardsPosition(GameObject obj, in Vector3 goalPos, float onPathDistToGoal, float desiredSpeed, ref bool blocked)
    {
        SetFlag(LocomotorFlags.MaintainPositionIsValid, false);

        var bdt = obj.BodyModule.DamageState;
        var maxSpeed = GetMaxSpeedForCondition(bdt);

        // Sanity, we cannot use desired speed that is greater than our max
        // speed we are capable of moving at.
        if (desiredSpeed > maxSpeed)
        {
            desiredSpeed = maxSpeed;
        }

        var distToStopAtMaxSpeed = (maxSpeed / GetBraking()) * (maxSpeed / 2.0f);
        if (onPathDistToGoal > AIPathfind.PathfindCellSizeF && onPathDistToGoal > distToStopAtMaxSpeed)
        {
            SetFlag(LocomotorFlags.IsBraking, false);
            _brakingFactor = 1.0f;
        }

        var physics = obj.Physics;
        if (physics == null)
        {
            Debug.Fail("You can only apply Locomotors to objects with Physics");
            return;
        }

        // Skip moveTowardsPosition if physics says you're stunned.
        if (physics.IsStunned)
        {
            return;
        }

        // Do not allow for invalid positions that the pathfinder cannot handle.
        // For airborne objects we don't need the pathfinder so we'll ignore this.
        if ((LocomotorTemplate.Surfaces & Surfaces.Air) == 0
            && !_gameEngine.AI.Pathfinder.ValidMovementTerrain(obj.Layer, this, obj.Translation)
            && !GetFlag(LocomotorFlags.AllowInvalidPosition))
        {
            // Somehow, we have gotten to an invalid location.
            if (FixInvalidPosition(obj, physics))
            {
                // We adjusted us toward a legal position, so just return.
                return;
            }
        }

        // If the actual distance is farther, then use the actual distance so
        // we get there.
        var dx = goalPos.X - obj.Translation.X;
        var dy = goalPos.Y - obj.Translation.Y;
        var dz = goalPos.Z - obj.Translation.Z;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist > onPathDistToGoal)
        {
            if (!obj.IsKindOf(ObjectKinds.Projectile) && dist > 2 * onPathDistToGoal)
            {
                SetFlag(LocomotorFlags.IsBraking, true);
            }
            onPathDistToGoal = dist;
        }

        var treatAsAirborne = false;
        var pos = obj.Translation;
        var heightAboveSurface = pos.Z - _gameEngine.Game.TerrainLogic.GetLayerHeight(pos.X, pos.Y, obj.Layer);

        if (obj.TestStatus(ObjectStatus.DeckHeightOffset))
        {
            heightAboveSurface -= obj.CarrierDeckHeight;
        }

        if (heightAboveSurface > -(3 * 3) * _gameEngine.AssetStore.GameData.Current.Gravity)
        {
            // If we get high enough to stay up for 3 frames, then we left the ground.
            treatAsAirborne = true;
        }
        // We apply a zero acceleration to all units, as the call to
        // ApplyMotiveForce flags an object as being "driven" by a locomotor, rather
        // than being pushed around by objects bumping it.
        physics.ApplyMotiveForce(Vector3.Zero);

        if (blocked)
        {
            if (desiredSpeed > physics.VelocityMagnitude)
            {
                blocked = false;
            }
            if (treatAsAirborne && (LocomotorTemplate.Surfaces & Surfaces.Air) != 0)
            {
                // Airborne flying objects don't collide for now.
                blocked = false;
            }
        }

        if (blocked)
        {
            // Stop if we are about to run into the blocking object.
            physics.ScrubVelocity2D(desiredSpeed);
            var turnRate = GetMaxTurnRate(obj.BodyModule.DamageState);
            if (LocomotorTemplate.WanderWidthFactor == 0.0f)
            {
                blocked = RotateObjAroundLocoPivot(obj, goalPos, turnRate, out _) != PhysicsTurningType.None;
            }

            // It is very important to be sure to call this in all situations, even if not moving in 2D space.
            HandleBehaviorZ(obj, physics, goalPos);
            return;
        }

        if (LocomotorTemplate.Appearance == LocomotorAppearance.Wings)
        {
            SetFlag(LocomotorFlags.IsBraking, false);
        }

        var wasBraking = obj.TestStatus(ObjectStatus.IsBraking);

        physics.Turning = PhysicsTurningType.None;
        if (LocomotorTemplate.AllowAirborneMotiveForce || !treatAsAirborne)
        {
            switch (LocomotorTemplate.Appearance)
            {
                case LocomotorAppearance.TwoLegs:
                    MoveTowardsPositionLegs(obj, physics, goalPos, onPathDistToGoal, desiredSpeed);
                    break;
                case LocomotorAppearance.Climber:
                    MoveTowardsPositionClimb(obj, physics, goalPos, onPathDistToGoal, desiredSpeed);
                    break;
                case LocomotorAppearance.FourWheels:
                case LocomotorAppearance.Motorcycle:
                    MoveTowardsPositionWheels(obj, physics, goalPos, onPathDistToGoal, desiredSpeed);
                    break;
                case LocomotorAppearance.Treads:
                    MoveTowardsPositionTreads(obj, physics, goalPos, onPathDistToGoal, desiredSpeed);
                    break;
                case LocomotorAppearance.Hover:
                    MoveTowardsPositionHover(obj, physics, goalPos, onPathDistToGoal, desiredSpeed);
                    break;
                case LocomotorAppearance.Wings:
                    MoveTowardsPositionWings(obj, physics, goalPos, onPathDistToGoal, desiredSpeed);
                    break;
                case LocomotorAppearance.Thrust:
                    MoveTowardsPositionThrust(obj, physics, goalPos, onPathDistToGoal, desiredSpeed);
                    break;
                case LocomotorAppearance.Other:
                default:
                    MoveTowardsPositionOther(obj, physics, goalPos, onPathDistToGoal, desiredSpeed);
                    break;
            }
        }

        HandleBehaviorZ(obj, physics, goalPos);
        // Objects that are braking don't follow the normal physics, so they end up at their destination exactly.
        obj.SetObjectStatus(ObjectStatus.IsBraking, GetFlag(LocomotorFlags.IsBraking));

        if (wasBraking)
        {
            var minVel = AIPathfind.PathfindCellSizeF / _gameEngine.LogicFramesPerSecond;

            if (obj.IsKindOf(ObjectKinds.Projectile))
            {
                // Projectiles never stop braking once they start.  jba.
                obj.SetObjectStatus(ObjectStatus.IsBraking, true);
                // Projectiles cheat in 3 dimensions.
                dist = MathF.Sqrt(dx * dx + dy * dy + dz * dz);
                var vel = physics.VelocityMagnitude;
                if (vel < minVel)
                {
                    vel = minVel;
                }
                if (vel > dist)
                {
                    vel = dist; // do not overcompensate!
                }
                // Normalize.
                if (dist > 0.001f)
                {
                    dist = 1.0f / dist;
                    dx *= dist;
                    dy *= dist;
                    dz *= dist;
                    pos.X += dx * vel;
                    pos.Y += dy * vel;
                    pos.Z += dz * vel;
                }
            }
            else
            {
                // Not-projectiles only cheat in x & y.
                // Normalize.
                if (dist > 0.001f)
                {
                    var vel = MathF.Abs(physics.GetForwardSpeed2D());
                    if (vel < minVel)
                    {
                        vel = minVel;
                    }
                    if (vel > dist)
                    {
                        vel = dist; // do not overcompensate!
                    }
                    dist = 1.0f / dist;
                    dx *= dist;
                    dy *= dist;
                    pos.X += dx * vel;
                    pos.Y += dy * vel;
                }
            }
            obj.SetTranslation(pos);
        }
    }

    private bool FixInvalidPosition(GameObject obj, PhysicsBehavior physics)
    {
        if (obj.IsKindOf(ObjectKinds.Dozer))
        {
            // Don't fix dozers.
            return false;
        }

        var dx = 0;
        var dy = 0;
        for (var j = -1; j < 2; j++)
        {
            for (var i = -1; i < 2; i++)
            {
                var thePos = obj.Translation;
                thePos.X += i * AIPathfind.PathfindCellSizeF;
                thePos.Y += j * AIPathfind.PathfindCellSizeF;
                if (!_gameEngine.AI.Pathfinder.ValidMovementTerrain(obj.Layer, this, thePos))
                {
                    if (i < 0)
                    {
                        dx += 1;
                    }

                    if (i > 0)
                    {
                        dx -= 1;
                    }

                    if (j < 0)
                    {
                        dy += 1;
                    }

                    if (j > 0)
                    {
                        dy -= 1;
                    }
                }
            }
        }

        if (dx != 0 || dy != 0)
        {
            var correction = new Vector3(
                dx * physics.Mass / 5,
                dy * physics.Mass / 5,
                0);

            var correctionNormalized = Vector3.Normalize(correction);

            // Kill current velocity in the direction of the correction.
            var velocity = physics.Velocity;
            var dot = (velocity.X * correctionNormalized.X) + (velocity.Y * correctionNormalized.Y);
            if (dot > 0.25f)
            {
                // It was already leaving.
                return false;
            }

            // The original code has a commented-out line to clear the current acceleration:
            // // Kill current accel
            // physics.ClearAcceleration();

            if (dot < 0)
            {
                dot = MathF.Sqrt(-dot);
                correctionNormalized.X *= dot * physics.Mass;
                correctionNormalized.Y *= dot * physics.Mass;
                physics.ApplyMotiveForce(correctionNormalized);
            }

            // Apply correction.
            physics.ApplyMotiveForce(correction);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if we can maintain the position without being called every frame
    /// (e.g. we are not resting in the ground), false if not (e.g. we are hovering or circling).
    /// </summary>
    private bool HandleBehaviorZ(GameObject obj, PhysicsBehavior physics, in Vector3 goalPos)
    {
        bool requiresConstantCalling;

        // Keep the agent aligned on the terrain.
        switch (LocomotorTemplate.BehaviorZ)
        {
            case LocomotorBehaviorZ.NoZMotiveForce:
                // Nothing to do.
                requiresConstantCalling = false;
                break;

            case LocomotorBehaviorZ.SeaLevel:
                requiresConstantCalling = true;
                if (!obj.IsDisabledByType(DisabledType.Held))
                {
                    var pos = obj.Translation;
                    if (_gameEngine.Game.TerrainLogic.IsUnderwater(pos.X, pos.Y, out var waterZ))
                    {
                        pos.Z = waterZ;
                    }
                    else
                    {
                        pos.Z = _gameEngine.Game.TerrainLogic.GetLayerHeight(pos.X, pos.Y, obj.Layer);
                    }
                    obj.SetTranslation(pos);
                }
                break;

            case LocomotorBehaviorZ.FixedSurfaceRelativeHeight:
            case LocomotorBehaviorZ.FixedAbsoluteHeight:
                requiresConstantCalling = true;
                {
                    var pos = obj.Translation;
                    var surfaceRel = LocomotorTemplate.BehaviorZ == LocomotorBehaviorZ.FixedSurfaceRelativeHeight;
                    var surfaceHt = surfaceRel ? GetSurfaceHeightAtPoint(pos.X, pos.Y) : 0.0f;
                    pos.Z = _preferredHeight + (surfaceRel ? surfaceHt : 0);
                    obj.SetTranslation(pos);
                }
                break;

            case LocomotorBehaviorZ.RelativeToGroundAndBuildings:
                requiresConstantCalling = true;
                {
                    // Original comment:
                    // srj sez: use getGroundOrStructureHeight(), because someday it will cache building heights...
                    var pos = obj.Translation;
                    var surfaceHt = _gameEngine.Game.PartitionCellManager.GetGroundOrStructureHeight(pos.X, pos.Y);

                    pos.Z = _preferredHeight + surfaceHt;

                    obj.SetTranslation(pos);

                }
                break;
            case LocomotorBehaviorZ.RelativeToHighestLayer:
                requiresConstantCalling = true;
                {
                    if (_preferredHeight != 0.0f || GetFlag(LocomotorFlags.PreciseZPosition))
                    {
                        var pos = obj.Translation;

                        // Original comment:
                        // srj sez: if we aren't on the ground, never find the ground layer
                        var layerAtDest = obj.Layer;
                        if (layerAtDest == PathfindLayerType.Ground)
                        {
                            layerAtDest = _gameEngine.Game.TerrainLogic.GetHighestLayerForDestination(pos);
                        }

                        const bool clip = false; // Return the height, even if off the edge of the bridge proper.
                        var surfaceHt = _gameEngine.Game.TerrainLogic.GetLayerHeight(pos.X, pos.Y, layerAtDest, out _, clip);

                        var preferredHeight = _preferredHeight + surfaceHt;
                        if (GetFlag(LocomotorFlags.PreciseZPosition))
                        {
                            preferredHeight = goalPos.Z;
                        }

                        var delta = preferredHeight - pos.Z;
                        delta *= _preferredHeightDamping;
                        preferredHeight = pos.Z + delta;

                        var liftToUse = CalcLiftToUseAtPoint(obj, physics, pos.Z, surfaceHt, preferredHeight);

                        if (liftToUse != 0.0f)
                        {
                            var force = new Vector3(0, 0, liftToUse * physics.Mass);
                            physics.ApplyMotiveForce(force);
                        }
                    }
                }
                break;

            case LocomotorBehaviorZ.SurfaceRelativeHeight:
            case LocomotorBehaviorZ.AbsoluteHeight:
                requiresConstantCalling = true;
                {
                    if (_preferredHeight != 0.0f || GetFlag(LocomotorFlags.PreciseZPosition))
                    {
                        var pos = obj.Translation;

                        var surfaceRel = LocomotorTemplate.BehaviorZ == LocomotorBehaviorZ.SurfaceRelativeHeight;
                        var surfaceHt = surfaceRel ? GetSurfaceHeightAtPoint(pos.X, pos.Y) : 0.0f;
                        var preferredHeight = _preferredHeight + (surfaceRel ? surfaceHt : 0);
                        if (GetFlag(LocomotorFlags.PreciseZPosition))
                        {
                            preferredHeight = goalPos.Z;
                        }

                        var delta = preferredHeight - pos.Z;
                        delta *= _preferredHeightDamping;
                        preferredHeight = pos.Z + delta;

                        var liftToUse = CalcLiftToUseAtPoint(obj, physics, pos.Z, surfaceHt, preferredHeight);

                        if (liftToUse != 0.0f)
                        {
                            var force = new Vector3(0, 0, liftToUse * physics.Mass);
                            physics.ApplyMotiveForce(force);
                        }
                    }
                }
                break;

            default:
                throw new InvalidOperationException();
        }

        return requiresConstantCalling;
    }

    private void MoveTowardsPositionLegs(GameObject obj, PhysicsBehavior physics, in Vector3 goalPos, float onPathDistToGoal, float desiredSpeed)
    {
        if (LocomotorTemplate.DownhillOnly && obj.Translation.Z < goalPos.Z)
        {
            return;
        }

        var maxAcceleration = GetMaxAcceleration(obj.BodyModule.DamageState);

        // Sanity, we cannot use desired speed that is greater than our max
        // speed we are capable of moving at.
        var maxSpeed = GetMaxSpeedForCondition(obj.BodyModule.DamageState);
        if (desiredSpeed > maxSpeed)
        {
            desiredSpeed = maxSpeed;
        }

        // Locomotion for infantry.
        //
        // Orient toward goal position
        //
        var actualSpeed = physics.GetForwardSpeed2D();
        var angle = obj.Yaw;
        //	Real relAngle = ThePartitionManager->getRelativeAngle2D( obj, &goalPos );
        //	Real desiredAngle = angle + relAngle;
        var desiredAngle = MathF.Atan2(goalPos.Y - obj.Translation.Y, goalPos.X - obj.Translation.X);

        if (LocomotorTemplate.WanderWidthFactor != 0.0f)
        {
            var angleLimit = MathF.PI / 8 * LocomotorTemplate.WanderWidthFactor;
            // This is the wander offline code - it forces the desired angle
            // away from the goal, so we wander back & forth.
            if (GetFlag(LocomotorFlags.OffsetIncreasing))
            {
                _angleOffset += _offsetIncrement * actualSpeed;
                if (_angleOffset > angleLimit)
                {
                    SetFlag(LocomotorFlags.OffsetIncreasing, false);
                }
            }
            else
            {
                _angleOffset -= _offsetIncrement * actualSpeed;
                if (_angleOffset < -angleLimit)
                {
                    SetFlag(LocomotorFlags.OffsetIncreasing, true);
                }
            }
            desiredAngle = MathUtility.NormalizeAngle(desiredAngle + _angleOffset);
        }

        var relAngle = MathUtility.CalculateAngleDelta(desiredAngle, angle);
        LocoUpdateMoveTowardsAngle(obj, desiredAngle);

        // Modulate speed according to turning. The more we have to turn, the slower we go
        const float quarterPi = MathF.PI / 4.0f;
        var angleCoeff = MathF.Abs(relAngle) / (quarterPi);
        if (angleCoeff > 1.0f)
        {
            angleCoeff = 1.0f;
        }

        var goalSpeed = (1.0f - angleCoeff) * desiredSpeed;

        //Real slowDownDist = (actualSpeed - m_template->m_minSpeed) / getBraking();
        var slowDownDist = CalcSlowDownDist(actualSpeed, LocomotorTemplate.MinSpeed, GetBraking());
        if (onPathDistToGoal < slowDownDist && !GetFlag(LocomotorFlags.NoSlowDownAsApproachingDestination))
        {
            goalSpeed = LocomotorTemplate.MinSpeed;
        }

        // Maintain goal speed
        var speedDelta = goalSpeed - actualSpeed;
        if (speedDelta != 0.0f)
        {
            var mass = physics.Mass;
            var acceleration = (speedDelta > 0.0f)
                ? maxAcceleration :
                -GetBraking();
            var accelForce = mass * acceleration;

            // Don't accelerate/brake more than necessary. do a quick calc to
            // see how much force we really need to achieve our goal speed...
            var maxForceNeeded = mass * speedDelta;
            if (MathF.Abs(accelForce) > MathF.Abs(maxForceNeeded))
            {
                accelForce = maxForceNeeded;
            }

            var dir = obj.UnitDirectionVector2D;

            var force = new Vector3(
                accelForce * dir.X,
                accelForce * dir.Y,
                0.0f);

            // apply forces to object
            physics.ApplyMotiveForce(force);
        }
    }

    private void MoveTowardsPositionClimb(GameObject obj, PhysicsBehavior physics, in Vector3 goalPos, float onPathDistToGoal, float desiredSpeed)
    {
        var maxAcceleration = GetMaxAcceleration(obj.BodyModule.DamageState);

        // Sanity, we cannot use desired speed that is greater than our max
        // speed we are capable of moving at.
        var maxSpeed = GetMaxSpeedForCondition(obj.BodyModule.DamageState);
        if (desiredSpeed > maxSpeed)
        {
            desiredSpeed = maxSpeed;
        }

        // Locomotion for climbing infantry.

        var moveBackwards = false;

        var pos = obj.Translation;

        var dx = pos.X - goalPos.X;
        var dy = pos.Y - goalPos.Y;
        var dz = pos.Z - goalPos.Z;
        if (dz * dz > (AIPathfind.PathfindCellSizeF * AIPathfind.PathfindCellSizeF))
        {
            SetFlag(LocomotorFlags.Climbing, true);
        }
        if (MathF.Abs(dz) < 1)
        {
            SetFlag(LocomotorFlags.Climbing, false);
        }

        if (GetFlag(LocomotorFlags.Climbing))
        {
            var delta = goalPos;
            delta.X -= pos.X;
            delta.Y -= pos.Y;
            delta.Z = 0;
            delta = Vector3.Normalize(delta);
            delta.X += pos.X;
            delta.Y += pos.Y;
            delta.Z = _gameEngine.Game.TerrainLogic.GetGroundHeight(delta.X, delta.Y);
            if (delta.Z < pos.Z - 0.1f)
            {
                moveBackwards = true;
            }

            var groundSlope = MathF.Abs(delta.Z - pos.Z);
            if (groundSlope < 1.0f)
            {
                groundSlope = 1.0f;
            }
            if (groundSlope > 1.0f)
            {
                desiredSpeed /= groundSlope * 4;
            }
        }
        SetFlag(LocomotorFlags.MovingBackwards, moveBackwards);

        // Orient toward goal position.
        var angle = obj.Yaw;
        //	Real relAngle = ThePartitionManager->getRelativeAngle2D( obj, &goalPos );
        //	Real desiredAngle = angle + relAngle;
        var desiredAngle = MathF.Atan2(goalPos.Y - obj.Translation.Y, goalPos.X - obj.Translation.X);
        var relAngle = MathUtility.CalculateAngleDelta(desiredAngle, angle);

        if (moveBackwards)
        {
            desiredAngle = MathUtility.CalculateAngleDelta(desiredAngle, MathF.PI);
            relAngle = MathUtility.CalculateAngleDelta(desiredAngle, angle);
        }

        LocoUpdateMoveTowardsAngle(obj, desiredAngle);

        // Modulate speed according to turning. The more we have to turn, the slower we go.
        const float QUARTERPI = MathF.PI / 4.0f;
        var angleCoeff = MathF.Abs(relAngle) / QUARTERPI;
        if (angleCoeff > 1.0f)
        {
            angleCoeff = 1.0f;
        }

        var goalSpeed = (1.0f - angleCoeff) * desiredSpeed;

        var actualSpeed = physics.GetForwardSpeed2D();

        if (moveBackwards)
        {
            actualSpeed = -actualSpeed;
        }

        //Real slowDownDist = (actualSpeed - m_template->m_minSpeed) / getBraking();
        var slowDownDist = CalcSlowDownDist(actualSpeed, LocomotorTemplate.MinSpeed, GetBraking());
        if (onPathDistToGoal < slowDownDist && !GetFlag(LocomotorFlags.NoSlowDownAsApproachingDestination))
        {
            goalSpeed = LocomotorTemplate.MinSpeed;
        }

        // Maintain goal speed.
        var speedDelta = goalSpeed - actualSpeed;
        if (moveBackwards)
        {
            speedDelta = -goalSpeed + actualSpeed;
        }
        if (speedDelta != 0.0f)
        {
            var mass = physics.Mass;
            var acceleration = moveBackwards
                ? (speedDelta < 0.0f) ? -maxAcceleration : GetBraking()
                : (speedDelta > 0.0f) ? maxAcceleration : -GetBraking();
            var accelForce = mass * acceleration;

            // Don't accelerate/brake more than necessary. do a quick calc to
            // see how much force we really need to achieve our goal speed...
            var maxForceNeeded = mass * speedDelta;
            if (MathF.Abs(accelForce) > MathF.Abs(maxForceNeeded))
            {
                accelForce = maxForceNeeded;
            }

            var dir = obj.UnitDirectionVector2D;

            var force = new Vector3(
                accelForce * dir.X,
                accelForce * dir.Y,
                0.0f);

            // Apply forces to object.
            physics.ApplyMotiveForce(force);
        }
    }

    private void MoveTowardsPositionWheels(GameObject obj, PhysicsBehavior physics, in Vector3 goalPos, float onPathDistToGoal, float desiredSpeed)
    {
        var bdt = obj.BodyModule.DamageState;
        var maxSpeed = GetMaxSpeedForCondition(bdt);
        var maxTurnRate = GetMaxTurnRate(bdt);
        var maxAcceleration = GetMaxAcceleration(bdt);

        // Sanity, we cannot use desired speed that is greater than our max
        // speed we are capable of moving at.
        if (desiredSpeed > maxSpeed)
        {
            desiredSpeed = maxSpeed;
        }

        // Locomotion for wheeled vehicles, ie trucks.
        //
        // See if we are turning.  If so, use the min turn speed.
        //
        var turnSpeed = LocomotorTemplate.MinTurnSpeed;
        var angle = obj.Yaw;
        //	Real relAngle = ThePartitionManager->getRelativeAngle2D( obj, &goalPos );
        //	Real desiredAngle = angle + relAngle;
        var desiredAngle = MathF.Atan2(goalPos.Y - obj.Translation.Y, goalPos.X - obj.Translation.X);
        var relAngle = MathUtility.CalculateAngleDelta(desiredAngle, angle);

        var moveBackwards = false;

        // Wheeled vehicles can only turn while moving, so make sure the turn
        // speed is reasonable.
        if (turnSpeed < maxSpeed / 4.0f)
        {
            turnSpeed = maxSpeed / 4.0f;
        }

        var actualSpeed = physics.GetForwardSpeed2D();
        var do3pointTurn = false;
        if (actualSpeed == 0.0f)
        {
            SetFlag(LocomotorFlags.MovingBackwards, false);
            if (LocomotorTemplate.CanMoveBackwards && MathF.Abs(relAngle) > MathF.PI / 2.0f)
            {
                SetFlag(LocomotorFlags.MovingBackwards, true);
                SetFlag(LocomotorFlags.DoingThreePointTurn, onPathDistToGoal > 5 * obj.Geometry.MajorRadius);
            }

        }
        if (GetFlag(LocomotorFlags.MovingBackwards))
        {
            if (MathF.Abs(relAngle) < MathF.PI / 2)
            {
                moveBackwards = false;
                SetFlag(LocomotorFlags.MovingBackwards, false);
            }
            else
            {
                moveBackwards = true;
                SetFlag(LocomotorFlags.DoingThreePointTurn, onPathDistToGoal > 5 * obj.Geometry.MajorRadius);
                do3pointTurn = GetFlag(LocomotorFlags.DoingThreePointTurn);
                if (!do3pointTurn)
                {
                    desiredAngle = MathUtility.CalculateAngleDelta(desiredAngle, MathF.PI);
                    relAngle = MathUtility.CalculateAngleDelta(desiredAngle, angle);
                }
            }
        }

        const float SMALL_TURN = MathF.PI / 20.0f;
        if (MathF.Abs(relAngle) > SMALL_TURN)
        {
            if (desiredSpeed > turnSpeed)
            {
                desiredSpeed = turnSpeed;
            }
        }

        var goalSpeed = desiredSpeed;
        if (moveBackwards)
        {
            actualSpeed = -actualSpeed;
        }

        var slowDownTime = actualSpeed / GetBraking() + 1.0f;
        var slowDownDist = (actualSpeed / 1.5f) * slowDownTime + actualSpeed;
        var effectiveSlowDownDist = slowDownDist;
        if (effectiveSlowDownDist < 1 * AIPathfind.PathfindCellSizeF)
        {
            effectiveSlowDownDist = 1 * AIPathfind.PathfindCellSizeF;
        }

        const float FIFTEEN_DEGREES = MathF.PI / 12.0f;
        var projectFrames = _gameEngine.LogicFramesPerSecond / 2; // Project out 1/2 second.
        if (MathF.Abs(relAngle) > FIFTEEN_DEGREES)
        {
            // If we're turning more than 10 degrees, check & see if we're moving into "impassable territory"
            var distance = projectFrames * (goalSpeed + actualSpeed) / 2.0f;
            var targetAngle = obj.Yaw;
            var turnFactor = ((goalSpeed + actualSpeed) / 2.0f) / turnSpeed;
            if (turnFactor > 1.0f)
            {
                turnFactor = 1.0f;
            }
            var turnAmount = projectFrames * turnFactor * maxTurnRate / 4.0f;
            if (relAngle < 0)
            {
                targetAngle -= turnAmount;
            }
            else
            {
                targetAngle += turnAmount;
            }
            var offset = new Vector3(
                MathF.Cos(targetAngle) * distance,
                MathF.Sin(targetAngle) * distance,
                0);

            var pos = obj.Translation;

            var nextPos = new Vector3(
                pos.X + offset.X,
                pos.Y + offset.Y,
                pos.Z);

            pos = obj.Translation;

            var halfPos = new Vector3(
                pos.X + offset.X / 2,
                pos.Y + offset.Y / 2,
                pos.Z);

            if (!_gameEngine.AI.Pathfinder.ValidMovementTerrain(obj.Layer, this, halfPos) ||
                !_gameEngine.AI.Pathfinder.ValidMovementTerrain(obj.Layer, this, nextPos))
            {
                var rotating = RotateTowardsPosition(obj, goalPos, out _);
                physics.Turning = rotating;

                // Apply a zero force to object so that it acts "driven".
                physics.ApplyMotiveForce(Vector3.Zero);
                return;
            }
        }

        if (onPathDistToGoal < effectiveSlowDownDist && !GetFlag(LocomotorFlags.IsBraking) && !GetFlag(LocomotorFlags.NoSlowDownAsApproachingDestination))
        {
            SetFlag(LocomotorFlags.IsBraking, true);
            _brakingFactor = 1.1f;
        }


        if (onPathDistToGoal > AIPathfind.PathfindCellSizeF && onPathDistToGoal > 2.0 * slowDownDist)
        {
            SetFlag(LocomotorFlags.IsBraking, false);
        }

        if (onPathDistToGoal > DonutDistance)
        {
            ResetDonutTimer();
        }
        else
        {
            if (_donutTimer < _gameEngine.GameLogic.CurrentFrame)
            {
                SetFlag(LocomotorFlags.IsBraking, true);
            }
        }

        if (GetFlag(LocomotorFlags.IsBraking))
        {
            _brakingFactor = slowDownDist / onPathDistToGoal;
            _brakingFactor *= _brakingFactor;
            if (_brakingFactor > MaxBrakingFactor)
            {
                _brakingFactor = MaxBrakingFactor;
            }
            _brakingFactor = 1.0f;
            if (slowDownDist > onPathDistToGoal)
            {
                goalSpeed = actualSpeed - GetBraking();
                if (goalSpeed < 0.0f)
                {
                    goalSpeed = 0.0f;
                }
            }
            else if (slowDownDist > onPathDistToGoal * 0.75f)
            {
                goalSpeed = actualSpeed - GetBraking() / 2.0f;
                if (goalSpeed < 0.0f)
                {
                    goalSpeed = 0.0f;
                }
            }
            else
            {
                goalSpeed = actualSpeed;
            }
        }


        //DEBUG_LOG(("Actual speed %f, Braking factor %f, slowDownDist %f, Pathdist %f, goalSpeed %f\n",
        //	actualSpeed, m_brakingFactor, slowDownDist, onPathDistToGoal, goalSpeed));

        {
            // Wheeled can only turn while moving.
            var turnFactor = actualSpeed / turnSpeed;
            if (turnFactor < 0)
            {
                turnFactor = -turnFactor; // In case we're sliding backwards in a 3 pt turn.
            }
            if (turnFactor > 1.0f)
            {
                turnFactor = 1.0f;
            }
            var turnAmount = turnFactor * maxTurnRate;

            PhysicsTurningType rotating;
            if (moveBackwards && !do3pointTurn)
            {
                var backwardPos = obj.Translation;
                backwardPos.X += -(goalPos.X - obj.Translation.X);
                backwardPos.Y += -(goalPos.Y - obj.Translation.Y);
                rotating = RotateObjAroundLocoPivot(obj, backwardPos, turnAmount, out _);
            }
            else
            {
                rotating = RotateObjAroundLocoPivot(obj, goalPos, turnAmount, out _);
            }

            physics.Turning = rotating;
        }

        // Maintain goal speed
        var speedDelta = goalSpeed - actualSpeed;
        if (moveBackwards)
        {
            speedDelta = -goalSpeed + actualSpeed;
        }
        if (speedDelta != 0.0f)
        {
            var mass = physics.Mass;
            var acceleration = moveBackwards
                ? (speedDelta < 0.0f) ? -maxAcceleration : _brakingFactor * GetBraking()
                : (speedDelta > 0.0f) ? maxAcceleration : -_brakingFactor * GetBraking();
            var accelForce = mass * acceleration;

            // Don't accelerate/brake more than necessary. do a quick calc to
            // see how much force we really need to achieve our goal speed...
            var maxForceNeeded = mass * speedDelta;
            if (MathF.Abs(accelForce) > MathF.Abs(maxForceNeeded))
            {
                accelForce = maxForceNeeded;
            }

            //DEBUG_LOG(("Braking %d, actualSpeed %f, goalSpeed %f, delta %f, accel %f\n", getFlag(IS_BRAKING),
            //actualSpeed, goalSpeed, speedDelta, accelForce));

            var dir = obj.UnitDirectionVector2D;

            var force = new Vector3(
                accelForce * dir.X,
                accelForce * dir.Y,
                0.0f);

            // apply forces to object
            physics.ApplyMotiveForce(force);
        }
    }

    private void MoveTowardsPositionTreads(GameObject obj, PhysicsBehavior physics, in Vector3 goalPos, float onPathDistToGoal, float desiredSpeed)
    {
        // Sanity, we cannot use desired speed that is greater than our max
        // speed we are capable of moving at.
        var bdt = obj.BodyModule.DamageState;
        var maxSpeed = GetMaxSpeedForCondition(bdt);
        if (desiredSpeed > maxSpeed)
        {
            desiredSpeed = maxSpeed;
        }

        var maxAcceleration = GetMaxAcceleration(bdt);

        // Locomotion for treaded vehicles, ie tanks.

        //
        // Orient toward goal position
        //
        //	Real angle = obj->getOrientation();
        //	Real relAngle = ThePartitionManager->getRelativeAngle2D( obj, &goalPos );
        //	Real desiredAngle = angle + relAngle;
        var rotating = RotateTowardsPosition(obj, goalPos, out var relAngle);
        physics.Turning = rotating;

        //
        // Modulate speed according to turning. The more we have to turn, the slower we go
        //
        const float QUAETERPI = MathF.PI / 4.0f;
        var angleCoeff = MathF.Abs(relAngle) / QUAETERPI;
        if (angleCoeff > 1.0f)
        {
            angleCoeff = 1.0f;
        }

        var dx = obj.Translation.X - goalPos.X;
        var dy = obj.Translation.Y - goalPos.Y;


        var goalSpeed = (1.0f - angleCoeff) * desiredSpeed;


        //	if (speed < m_minTurnSpeed)
        //		speed = m_minTurnSpeed;

        var actualSpeed = physics.GetForwardSpeed2D();
        var slowDownTime = actualSpeed / GetBraking();
        var slowDownDist = (actualSpeed / 1.50f) * slowDownTime;

        if ((dx * dx) + (dy * dy) < MathUtility.Square(2 * AIPathfind.PathfindCellSizeF) && angleCoeff > 0.05)
        {
            goalSpeed = actualSpeed * 0.6f;
        }

        if (onPathDistToGoal < slowDownDist && !GetFlag(LocomotorFlags.IsBraking) && !GetFlag(LocomotorFlags.NoSlowDownAsApproachingDestination))
        {
            SetFlag(LocomotorFlags.IsBraking, true);
            _brakingFactor = 1.1f;
        }

        if (onPathDistToGoal > AIPathfind.PathfindCellSizeF && onPathDistToGoal > 2.0 * slowDownDist)
        {
            SetFlag(LocomotorFlags.IsBraking, false);
        }

        if (GetFlag(LocomotorFlags.IsBraking))
        {
            _brakingFactor = slowDownDist / onPathDistToGoal;
            _brakingFactor *= _brakingFactor;
            if (_brakingFactor > MaxBrakingFactor)
            {
                _brakingFactor = MaxBrakingFactor;
            }
            if (slowDownDist > onPathDistToGoal)
            {
                goalSpeed = actualSpeed - GetBraking();
                if (goalSpeed < 0.0f)
                {
                    goalSpeed = 0.0f;
                }
            }
            else if (slowDownDist > onPathDistToGoal * 0.75f)
            {
                goalSpeed = actualSpeed - GetBraking() / 2.0f;
                if (goalSpeed < 0.0f)
                {
                    goalSpeed = 0.0f;
                }
            }
            else
            {
                goalSpeed = actualSpeed;
            }
        }

        //DEBUG_LOG(("Actual speed %f, Braking factor %f, slowDownDist %f, Pathdist %f, goalSpeed %f\n",
        //	actualSpeed, m_brakingFactor, slowDownDist, onPathDistToGoal, goalSpeed));

        // Maintain goal speed.
        var speedDelta = goalSpeed - actualSpeed;
        if (speedDelta != 0.0f)
        {
            var mass = physics.Mass;
            var acceleration = (speedDelta > 0.0f) ? maxAcceleration : -_brakingFactor * GetBraking();
            var accelForce = mass * acceleration;

            // Don't accelerate/brake more than necessary. do a quick calc to
            // see how much force we really need to achieve our goal speed...
            var maxForceNeeded = mass * speedDelta;
            if (MathF.Abs(accelForce) > MathF.Abs(maxForceNeeded))
            {
                accelForce = maxForceNeeded;
            }

            var dir = obj.UnitDirectionVector2D;

            var force = new Vector3(
                accelForce * dir.X,
                accelForce * dir.Y,
                0.0f);

            // Apply forces to object.
            physics.ApplyMotiveForce(force);
        }
    }

    private void MoveTowardsPositionHover(GameObject obj, PhysicsBehavior physics, in Vector3 goalPos, float onPathDistToGoal, float desiredSpeed)
    {
        // Handle the 2D component.
        MoveTowardsPositionOther(obj, physics, goalPos, onPathDistToGoal, desiredSpeed);

        // Only hover locomotors care about their OverWater special effects.
        // (OverWater also affects speed, so this is not a client thing)
        var newPosition = obj.Translation;
        if (_gameEngine.Game.TerrainLogic.IsUnderwater(newPosition.X, newPosition.Y))
        {
            if (!GetFlag(LocomotorFlags.OverWater))
            {
                // Change my model condition because I used to not be over water, but now I am.
                SetFlag(LocomotorFlags.OverWater, true);
                obj.SetModelConditionState(ModelConditionFlag.OverWater);
            }
        }
        else
        {
            if (GetFlag(LocomotorFlags.OverWater))
            {
                // Here, I was, but now I'm not.
                SetFlag(LocomotorFlags.OverWater, false);
                obj.ClearModelConditionState(ModelConditionFlag.OverWater);
            }
        }
    }

    private void MoveTowardsPositionWings(GameObject obj, PhysicsBehavior physics, in Vector3 goalPos, float onPathDistToGoal, float desiredSpeed)
    {
        // Note: there's some commented code in the original C++ relating to circling for landing.

        // Handle the 2D component.
        MoveTowardsPositionOther(obj, physics, goalPos, onPathDistToGoal, desiredSpeed);
    }

    private void MoveTowardsPositionThrust(GameObject obj, PhysicsBehavior physics, in Vector3 goalPos, float onPathDistToGoal, float desiredSpeed)
    {
        var bdt = obj.BodyModule.DamageState;

        var maxForwardSpeed = GetMaxSpeedForCondition(bdt);
        desiredSpeed = Math.Clamp(desiredSpeed, LocomotorTemplate.MinSpeed, maxForwardSpeed);
        var actualForwardSpeed = physics.GetForwardSpeed3D();

        if (GetBraking() > 0)
        {
            //Real slowDownDist = (actualForwardSpeed - m_template->m_minSpeed) / getBraking();
            var slowDownDist = CalcSlowDownDist(actualForwardSpeed, LocomotorTemplate.MinSpeed, GetBraking());
            if (onPathDistToGoal < slowDownDist && !GetFlag(LocomotorFlags.NoSlowDownAsApproachingDestination))
            {
                desiredSpeed = LocomotorTemplate.MinSpeed;
            }
        }

        var localGoalPos = goalPos;

        // Out of the handleBehaviorZ() function.
        var pos = obj.Translation;
        if (_preferredHeight != 0.0f && !GetFlag(LocomotorFlags.PreciseZPosition))
        {
            // If we have a preferred flight height, and we haven't been told explicitly to ignore it...
            var surfaceHt = GetSurfaceHeightAtPoint(pos.X, pos.Y);
            localGoalPos.Z = _preferredHeight + surfaceHt;
            //		localGoalPos.z = goalPos.z;
            var delta = localGoalPos.Z - pos.Z;
            delta *= _preferredHeightDamping;
            localGoalPos.Z = pos.Z + delta;
        }

        var forwardDir = obj.TransformMatrix.GetXVector();

        // Maintain goal speed
        var forwardSpeedDelta = desiredSpeed - actualForwardSpeed;
        var maxAccel = (forwardSpeedDelta > 0.0f || GetBraking() == 0) ? GetMaxAcceleration(bdt) : -GetBraking();
        var maxTurnRate = GetMaxTurnRate(bdt);

        // What direction do we need to thrust in, in order to reach the goalpos?
        var desiredThrustDir = CalcDirectionToApplyThrust(obj, physics, localGoalPos, maxAccel);

        // We might not be able to thrust in that dir, so thrust as closely as we can.
        var maxThrustAngle = (maxTurnRate > 0) ? LocomotorTemplate.MaxThrustAngle : 0;
        var thrustAngle = TryToRotateVector3D(maxThrustAngle, forwardDir, desiredThrustDir, out var thrustDir);

        // Note that we are trying to orient in the direction of our vel, not the dir of our thrust.
        if (!IsNearlyZero(physics.VelocityMagnitude))
        {
            var veltmp = physics.Velocity;
            var vel = veltmp;
            var adjust = true;
            if (obj.TestStatus(ObjectStatus.IsBraking))
            {
                // Align to target, cause that's where we're going anyway.

                vel = goalPos - pos;
                if (IsNearlyZero(MathUtility.Square(vel.X) + MathUtility.Square(vel.Y) + MathUtility.Square(vel.Z)))
                {
                    // We are at target.
                    adjust = false;
                }
                maxTurnRate = 3 * maxTurnRate;
            }
            if (adjust)
            {
                /*Real orient =*/
                TryToOrientInThisDirection3D(obj, maxTurnRate, vel);
            }
        }

        if (forwardSpeedDelta != 0.0f || thrustAngle != 0.0f)
        {
            if (maxForwardSpeed <= 0.0f)
            {
                maxForwardSpeed = 0.01f; // In some cases, this is 0, hack for now.  jba.
            }
            var damping = Math.Clamp(maxAccel / maxForwardSpeed, 0.0f, 1.0f);
            var curVel = physics.Velocity;

            var accelVec = thrustDir * maxAccel - curVel * damping;
            //DEBUG_LOG(("accel %f (max %f) vel %f (max %f) damping %f\n",accelVec.Length(),maxAccel,curVel.Length(),maxForwardSpeed,damping));

            var mass = physics.Mass;

            var force = new Vector3(
                mass * accelVec.X,
                mass * accelVec.Y,
                mass * accelVec.Z);

            // Apply forces to object.
            physics.ApplyMotiveForce(force);
        }
    }

    /// <summary>
    /// Our meta-goal here is to calculate the direction we should apply our motive force
    /// in order to minimize the angle between(our velocity) and(direction towards goalpos).
    ///
    /// This is complicated by the fact that we generally have an intrinsic velocity already,
    /// that must be accounted for, and by the fact that we can only apply force in our
    /// forward-x-direction(with a thrust-angle-range), and(due to limited range) might not
    /// be able to apply the force in the optimal direction!
    /// </summary>
    private Vector3 CalcDirectionToApplyThrust(GameObject obj, PhysicsBehavior physics, in Vector3 ingoalPos, float maxAccel)
    {
        // Convert to Vector3, to use all its handy stuff.
        var objPos = obj.Translation;
        var goalPos = ingoalPos;

        var vecToGoal = goalPos - objPos;
        if (IsNearlyZero(vecToGoal.LengthSquared()))
        {
            // Goal pos is essentially same as current pos, so just stay the same & return.
            return obj.TransformMatrix.GetXVector();
        }

        // Get our cur vel into a useful Vector3 form.
        var curVel = physics.Velocity;

        // Add gravity to our vel so that we account for it in our calcs.
        curVel.Z += _gameEngine.AssetStore.GameData.Current.Gravity;

        var distToGoalSqr = vecToGoal.LengthSquared();
        var distToGoal = MathF.Sqrt(distToGoalSqr);
        var curVelMagSqr = curVel.LengthSquared();
        var curVelMag = MathF.Sqrt(curVelMagSqr);
        var maxAccelSqr = MathUtility.Square(maxAccel);

        var denom = curVelMagSqr - maxAccelSqr;
        if (!IsNearlyZero(denom))
        {
            // Solve the (greatly simplified) quadratic...
            var t = (distToGoal * (curVelMag + maxAccel)) / denom;
            var t2 = (distToGoal * (curVelMag - maxAccel)) / denom;
            if (t >= 0 || t2 >= 0)
            {
                // choose the smallest positive t.
                if (t < 0 || (t2 >= 0 && t2 < t))
                {
                    t = t2;
                }

                // plug it in.
                if (!IsNearlyZero(t))
                {
                    return Vector3.Normalize(
                        new Vector3(
                            (vecToGoal.X / t) - curVel.X,
                            (vecToGoal.Y / t) - curVel.Y,
                            (vecToGoal.Z / t) - curVel.Z));
                }
            }
        }

        // Doh... no (useful) solution. revert to dumb.
        return Vector3.Normalize(vecToGoal);
    }

    /// <summary>
    /// Returns the angle delta (in 3-space) we turned.
    /// </summary>
    /// <param name="maxAngle">If negative, this is a percent (0...1) of the distance to rotate 'em.</param>
    /// <param name="inCurDir"></param>
    /// <param name="inGoalDir"></param>
    /// <param name="actualDir"></param>
    /// <returns></returns>
    private static float TryToRotateVector3D(float maxAngle, in Vector3 inCurDir, in Vector3 inGoalDir, out Vector3 actualDir)
    {
        if (IsNearlyZero(maxAngle))
        {
            actualDir = inCurDir;
            return 0.0f;
        }

        var curDir = Vector3.Normalize(inCurDir);
        var goalDir = Vector3.Normalize(inGoalDir);

        // Dot of two unit vectors is cos of angle between them.
        var cosine = Vector3.Dot(curDir, goalDir);
        // Bound it in case of numerical error.
        var angleBetween = MathF.Acos(Math.Clamp(cosine, -1.0f, 1.0f));

        if (maxAngle < 0)
        {
            maxAngle = -maxAngle * angleBetween;
            if (IsNearlyZero(maxAngle))
            {
                actualDir = inCurDir;
                return 0.0f;
            }
        }

        if (MathF.Abs(angleBetween) <= maxAngle)
        {
            // Close enough
            actualDir = goalDir;
        }
        else
        {
            // Nah, try as much as we can in the right dir.
            // We need to rotate around the axis perpendicular to these two vecs.
            // but: cross of two vectors is the perpendicular axis!
            var objCrossGoal = Vector3.Normalize(Vector3.Cross(curDir, goalDir));

            angleBetween = maxAngle;
            var rotMtx = Matrix4x4.CreateFromAxisAngle(objCrossGoal, angleBetween);
            actualDir = Vector3.TransformNormal(curDir, rotMtx);
        }

        return angleBetween;
    }

    private static float TryToOrientInThisDirection3D(GameObject obj, float maxTurnRate, in Vector3 desiredDir)
    {
        var relAngle = TryToRotateVector3D(maxTurnRate, obj.TransformMatrix.GetXVector(), desiredDir, out var actualDir);
        if (relAngle != 0.0f)
        {
            var objPos = obj.Translation;

            var newXform = Matrix4x4Utility.CreateTransformMatrix(objPos, actualDir);
            obj.SetTransformMatrix(newXform);
        }
        return relAngle;
    }

    private void MoveTowardsPositionOther(GameObject obj, PhysicsBehavior physics, in Vector3 goalPos, float onPathDistToGoal, float desiredSpeed)
    {
        BodyDamageType bdt = obj.BodyModule.DamageState;
        var maxAcceleration = GetMaxAcceleration(bdt);

        // Sanity, we cannot use desired speed that is greater than our max
        // speed we are capable of moving at.
        var maxSpeed = GetMaxSpeedForCondition(bdt);
        if (desiredSpeed > maxSpeed)
        {
            desiredSpeed = maxSpeed;
        }

        var goalSpeed = desiredSpeed;
        var actualSpeed = physics.GetForwardSpeed2D();

        // Locomotion for other things, ie don't know what it is.
        //
        // Orient toward goal position.
        // Exception: if very close (ie, we could get there in 2 frames or less),
        // and ULTRA_ACCURATE, just slide into place.
        var pos = obj.Translation;
        var dirToApplyForce = obj.UnitDirectionVector2D;

        //DEBUG_ASSERTLOG(!getFlag(ULTRA_ACCURATE),("thresh %f %f (%f %f)\n",
        //fabs(goalPos.y - pos->y),fabs(goalPos.x - pos->x),
        //fabs(goalPos.y - pos->y)/goalSpeed,fabs(goalPos.x - pos->x)/goalSpeed));
        if (GetFlag(LocomotorFlags.UltraAccurate)
            && MathF.Abs(goalPos.Y - pos.Y) <= goalSpeed * LocomotorTemplate.SlideIntoPlaceTime
            && MathF.Abs(goalPos.X - pos.X) <= goalSpeed * LocomotorTemplate.SlideIntoPlaceTime)
        {
            // Don't turn, just slide in the right direction.
            physics.Turning = PhysicsTurningType.None;
            dirToApplyForce = new Vector3(
                goalPos.X - pos.X,
                goalPos.Y - pos.Y,
                0.0f);
            dirToApplyForce = Vector3.Normalize(dirToApplyForce);
        }
        else
        {
            var rotating = RotateTowardsPosition(obj, goalPos, out _);
            physics.Turning = rotating;
        }

        if (!GetFlag(LocomotorFlags.NoSlowDownAsApproachingDestination))
        {
            var slowDownDist = CalcSlowDownDist(actualSpeed, LocomotorTemplate.MinSpeed, GetBraking());
            if (onPathDistToGoal < slowDownDist)
            {
                goalSpeed = LocomotorTemplate.MinSpeed;
            }
        }

        // Maintain goal speed.
        var speedDelta = goalSpeed - actualSpeed;
        if (speedDelta != 0.0f)
        {
            var mass = physics.Mass;
            var acceleration = (speedDelta > 0.0f) ? maxAcceleration : -GetBraking();
            var accelForce = mass * acceleration;

            // Don't accelerate/brake more than necessary. do a quick calc to
            // see how much force we really need to achieve our goal speed...
            var maxForceNeeded = mass * speedDelta;
            if (MathF.Abs(accelForce) > MathF.Abs(maxForceNeeded))
            {
                accelForce = maxForceNeeded;
            }

            var force = new Vector3(
                accelForce * dirToApplyForce.X,
                accelForce * dirToApplyForce.Y,
                0.0f);

            // apply forces to object
            physics.ApplyMotiveForce(force);
        }
    }

    private float GetSurfaceHeightAtPoint(float x, float y)
    {
        var height = 0.0f;

        if (_gameEngine.Game.TerrainLogic.IsUnderwater(x, y, out var waterZ, out var terrainZ))
        {
            height += waterZ;
        }
        else
        {
            height += terrainZ;
        }

        return height;
    }

    private float CalcLiftToUseAtPoint(GameObject obj, PhysicsBehavior physics, float curZ, float surfaceAtPt, float preferredHeight)
    {
        // Take the classic equation:
        //   x = x0 + v*t + 0.5*a*t^2
        // and solve for acceleration.
        var bdt = obj.BodyModule.DamageState;
        var maxGrossLift = GetMaxLift(bdt);
        var maxNetLift = maxGrossLift + _gameEngine.AssetStore.GameData.Current.Gravity; // note that gravity is always negative.
        if (maxNetLift < 0)
        {
            maxNetLift = 0;
        }
        var curVelZ = physics.Velocity.Z;
        // Going down, braking is limited by net lift; going up, braking is limited by gravity.
        var maxAccel = GetFlag(LocomotorFlags.UltraAccurate)
            ? (curVelZ < 0) ? 2 * maxNetLift : -2 * maxNetLift
            : (curVelZ < 0) ? maxNetLift : _gameEngine.AssetStore.GameData.Current.Gravity;
        // See how far we need to slow to dead stop, given max braking.
        float desiredAccel;
        const float tinyAccel = 0.001f;
        if (MathF.Abs(maxAccel) > tinyAccel)
        {
            var deltaZ = preferredHeight - curZ;
            // Calc how far it will take for us to go from cur speed to zero speed, at max accel.
            // float brakeDist = CalcSlowDownDist(curVelZ, 0, maxAccel);
            // in theory, the above is the correct calculation, but in practice,
            // doesn't work in some situations (eg, opening of USA01 map). Why, I dunno.
            // But for now I have gone back to the old, looks-incorrect-to-me-but-works calc. (srj)
            var brakeDist = ((curVelZ * curVelZ) / MathF.Abs(maxAccel));
            if (MathF.Abs(brakeDist) > MathF.Abs(deltaZ))
            {
                // If the dist-to-accel (or dist-to-brake) is further than the dist-to-go,
                // use the max accel.
                desiredAccel = maxAccel;
            }
            else if (MathF.Abs(curVelZ) > LocomotorTemplate.SpeedLimitZ)
            {
                // or, if we're going too fast, limit it here.
                desiredAccel = LocomotorTemplate.SpeedLimitZ - curVelZ;
            }
            else
            {
                // Ok, figure out the correct accel to use to get us there at zero.
                //
                // dz = v t + 0.5 a t^2
                //	thus
                // a = 2(dz - v t)/t^2
                //	and
                // t = (-v +- sqrt(v*v + 2*a*dz))/a
                //
                // but if we assume t=1, then
                //	a=2(dz-v)
                // then, plug it back in and see if t is really 1...
                desiredAccel = 2.0f * (deltaZ - curVelZ);
            }
        }
        else
        {
            desiredAccel = 0.0f;
        }
        var liftToUse = desiredAccel - _gameEngine.AssetStore.GameData.Current.Gravity;
        if (GetFlag(LocomotorFlags.UltraAccurate))
        {
            // in ultra-accurate mode, we allow cheating.
            const float upFactor = 3.0f;
            if (liftToUse > upFactor * maxGrossLift)
            {
                liftToUse = upFactor * maxGrossLift;
            }
            // srj sez: we used to clip lift to zero here (not allowing neg lift).
            // however, I now think that allowing neg lift in ultra-accurate mode is
            // a good and desirable thing; in particular, it enables jets to complete
            // "short" landings more accurately (previously they sometimes would "float"
            // down, which sucked.) if you need to bump this back to zero, check it carefully...
            else if (liftToUse < -maxGrossLift)
            {
                liftToUse = -maxGrossLift;
            }
        }
        else
        {
            if (liftToUse > maxGrossLift)
            {
                liftToUse = maxGrossLift;
            }
            else if (liftToUse < 0.0f)
            {
                liftToUse = 0.0f;
            }
        }

        return liftToUse;
    }

    private PhysicsTurningType RotateObjAroundLocoPivot(GameObject obj, in Vector3 goalPos, float maxTurnRate, out float relAngle)
    {
        var angle = obj.Yaw;
        var offset = LocomotorTemplate.TurnPivotOffset;

        var turn = PhysicsTurningType.None;

        // When braking we do exact movement towards goal, instead of physics.
        // Rotating about pivot moves the object, and can make us miss our goal, so it is disabled.
        if (GetFlag(LocomotorFlags.IsBraking))
        {
            offset = 0.0f;
        }

        if (offset != 0.0f)
        {
            var radius = obj.Geometry.BoundingCircleRadius;
            var turnPointOffset = offset * radius;

            var turnPos = obj.Translation;
            var dir = obj.UnitDirectionVector2D;
            turnPos.X += dir.X * turnPointOffset;
            turnPos.Y += dir.Y * turnPointOffset;
            var dx = goalPos.X - turnPos.X;
            var dy = goalPos.Y - turnPos.Y;

            // If we are very close to the goal, we twitch due to rounding error. So just return.
            if (MathF.Abs(dx) < 0.1f && MathF.Abs(dy) < 0.1f)
            {
                relAngle = 0;
                return PhysicsTurningType.None;
            }

            var desiredAngle = MathF.Atan2(dy, dx);
            var amount = MathUtility.CalculateAngleDelta(desiredAngle, angle);
            relAngle = amount;
            if (amount > maxTurnRate)
            {
                amount = maxTurnRate;
                turn = PhysicsTurningType.Positive;
            }
            else if (amount < -maxTurnRate)
            {
                amount = -maxTurnRate;
                turn = PhysicsTurningType.Negative;
            }
            else
            {
                turn = PhysicsTurningType.None;
            }

            // Original comment:
            // @todo srj -- there's probably a more efficient & more direct way to do this. find it.
            var translationToTurnPos = Matrix4x4.CreateTranslation(turnPos.X, turnPos.Y, 0);
            var rotation = Matrix4x4.CreateRotationZ(amount);
            var translationBack = Matrix4x4.CreateTranslation(-turnPos.X, -turnPos.Y, 0);
            //var tmp = translationToTurnPos * rotation * translationBack;
            var tmp = translationBack * rotation * translationToTurnPos;
            var mtx = obj.TransformMatrix * tmp;
            obj.SetTransformMatrix(mtx);
        }
        else
        {
            var desiredAngle = MathF.Atan2(goalPos.Y - obj.Translation.Y, goalPos.X - obj.Translation.X);
            var amount = MathUtility.CalculateAngleDelta(desiredAngle, angle);
            relAngle = amount;
            if (amount > maxTurnRate)
            {
                amount = maxTurnRate;
                turn = PhysicsTurningType.Positive;
            }
            else if (amount < -maxTurnRate)
            {
                amount = -maxTurnRate;
                turn = PhysicsTurningType.Negative;
            }
            else
            {
                turn = PhysicsTurningType.None;
            }
            obj.SetOrientation(MathUtility.NormalizeAngle(angle + amount));
        }
        return turn;
    }

    /// <summary>
    /// Kills any current (2D) velocity (but stay at current position, or as close as possible).
    /// </summary>
    /// <returns>
    /// Returns <see langword="true"/> if we can maintain the position without
    /// being called every frame (e.g. we are resting on the ground),
    /// <see langword="false"/> if not (e.g. we are hovering or circling).
    /// </returns>
    public bool LocoUpdateMaintainCurrentPosition(GameObject obj)
    {
        if (!GetFlag(LocomotorFlags.MaintainPositionIsValid))
        {
            _maintainPosition = obj.Translation;
            SetFlag(LocomotorFlags.MaintainPositionIsValid, true);
        }

        ResetDonutTimer();
        SetFlag(LocomotorFlags.IsBraking, false);
        var physics = obj.Physics;
        if (physics == null)
        {
            Debug.Fail("You can only apply Locomotors to objects with Physics");
            return true;
        }

        var requiresConstantCalling = true; // assume the worst.
        switch (LocomotorTemplate.Appearance)
        {
            case LocomotorAppearance.Thrust:
                MaintainCurrentPositionThrust(obj, physics);
                requiresConstantCalling = true;
                break;
            case LocomotorAppearance.TwoLegs:
                MaintainCurrentPositionLegs(obj, physics);
                requiresConstantCalling = false;
                break;
            case LocomotorAppearance.Climber:
                MaintainCurrentPositionLegs(obj, physics);
                requiresConstantCalling = false;
                break;
            case LocomotorAppearance.FourWheels:
            case LocomotorAppearance.Motorcycle:
                MaintainCurrentPositionWheels(obj, physics);
                requiresConstantCalling = false;
                break;
            case LocomotorAppearance.Treads:
                MaintainCurrentPositionTreads(obj, physics);
                requiresConstantCalling = false;
                break;
            case LocomotorAppearance.Hover:
                MaintainCurrentPositionHover(obj, physics);
                requiresConstantCalling = true;
                break;
            case LocomotorAppearance.Wings:
                MaintainCurrentPositionWings(obj, physics);
                requiresConstantCalling = true;
                break;
            case LocomotorAppearance.Other:
            default:
                MaintainCurrentPositionOther(obj, physics);
                requiresConstantCalling = true;
                break;
        }

        // ... but we do need to do this even if not moving, for Hovering/Thrusting things.
        if (HandleBehaviorZ(obj, physics, _maintainPosition))
        {
            requiresConstantCalling = true;
        }

        return requiresConstantCalling;
    }

    private void MaintainCurrentPositionThrust(GameObject obj, PhysicsBehavior physics)
    {
        Debug.Assert(GetFlag(LocomotorFlags.MaintainPositionIsValid), "invalid maintain pos");
        // Original comment:
        // @todo srj -- should these also use the "circling radius" stuff, like wings?
        MoveTowardsPositionThrust(obj, physics, _maintainPosition, 0, LocomotorTemplate.MinSpeed);
    }

    private void MaintainCurrentPositionLegs(GameObject obj, PhysicsBehavior physics)
    {
        MaintainCurrentPositionOther(obj, physics);
    }

    private void MaintainCurrentPositionWheels(GameObject obj, PhysicsBehavior physics)
    {
        MaintainCurrentPositionOther(obj, physics);
    }

    private void MaintainCurrentPositionTreads(GameObject obj, PhysicsBehavior physics)
    {
        MaintainCurrentPositionOther(obj, physics);
    }

    private void MaintainCurrentPositionHover(GameObject obj, PhysicsBehavior physics)
    {
        physics.Turning = PhysicsTurningType.None;
        if (physics.IsMotive) // no need to stop something that isn't moving.
        {
            Debug.Assert(LocomotorTemplate.MinSpeed == 0.0f, "HOVER should always have zero minSpeeds (otherwise, they WING)");

            var bdt = obj.BodyModule.DamageState;
            var maxAcceleration = GetMaxAcceleration(bdt);
            var actualSpeed = physics.GetForwardSpeed2D();
            // Stop
            var minSpeed = MathF.Max(1.0E-10f, LocomotorTemplate.MinSpeed);
            var speedDelta = minSpeed - actualSpeed;
            if (MathF.Abs(speedDelta) > minSpeed)
            {
                var mass = physics.Mass;
                var acceleration = (speedDelta > 0.0f) ? maxAcceleration : -GetBraking();
                var accelForce = mass * acceleration;

                // don't accelerate/brake more than necessary. do a quick calc to
                // see how much force we really need to achieve our goal speed...
                var maxForceNeeded = mass * speedDelta;
                if (MathF.Abs(accelForce) > MathF.Abs(maxForceNeeded))
                {
                    accelForce = maxForceNeeded;
                }

                var dir = obj.UnitDirectionVector2D;

                var force = new Vector3(
                    accelForce * dir.X,
                    accelForce * dir.Y,
                    0.0f);

                // This comment is from the original code, but it looks like the comment
                // was added and then no code was actually added to do this.
                // -----
                // Apply a random kick (if applicable) to dirty-up visually.
                // The idea is that chopper pilots have to do course corrections all the time
                // Because of changes in wind, pressure, etc.
                // Those changes are added here, then the

                // Apply forces to object.
                physics.ApplyMotiveForce(force);
            }
        }
    }

    private void MaintainCurrentPositionWings(GameObject obj, PhysicsBehavior physics)
    {
        Debug.Assert(GetFlag(LocomotorFlags.MaintainPositionIsValid), "invalid maintain pos");
        physics.Turning = PhysicsTurningType.None;
        if (physics.IsMotive && obj.IsAboveTerrain) // no need to stop something that isn't moving (or is just sitting on the ground)
        {
            // aim for the spot on the opposite side of the circle.
            var bdt = obj.BodyModule.DamageState;
            var turnRadius = LocomotorTemplate.CirclingRadius;
            if (turnRadius == 0.0f)
            {
                turnRadius = CalcMinTurnRadius(bdt, out _);
            }

            // find the direction towards our "maintain pos"
            var pos = obj.Translation;
            var dx = _maintainPosition.X - pos.X;
            var dy = _maintainPosition.Y - pos.Y;
            var angleTowardMaintainPos = (IsNearlyZero(dx) && IsNearlyZero(dy))
                ? obj.Yaw
                : MathF.Atan2(dy, dx);

            var aimDir = (MathF.PI - MathF.PI / 8);
            if (turnRadius < 0)
            {
                turnRadius = -turnRadius;
                aimDir = -aimDir;
            }
            angleTowardMaintainPos += aimDir;

            // project a spot "radius" dist away from it, in that dir
            var desiredPos = _maintainPosition;
            desiredPos.X += MathF.Cos(angleTowardMaintainPos) * turnRadius;
            desiredPos.Y += MathF.Sin(angleTowardMaintainPos) * turnRadius;
            MoveTowardsPositionWings(obj, physics, desiredPos, 0, LocomotorTemplate.MinSpeed);
        }
    }

    private float CalcMinTurnRadius(BodyDamageType condition, out float timeToTravelThatDist)
    {
        var minSpeed = LocomotorTemplate.MinSpeed;   // in dist/frame
        var maxTurnRate = GetMaxTurnRate(condition); // in rads/frame

        // our minimum circumference will be like so:
        //
        // var minTurnCircum = maxSpeed * (2*PI / maxTurnRate);
        //
        // so therefore our minimum turn radius is:
        //
        // var minTurnRadius = minTurnCircum / 2*PI;
        //
        // so we just eliminate the middleman:
        // if we can't turn, return a huge-but-finite radius rather than NaN...
        var minTurnRadius = (maxTurnRate > 0.0f)
            ? minSpeed / maxTurnRate
            : LocomotorTemplate.BigNumber;

        timeToTravelThatDist = (minSpeed > 0.0f)
            ? (minTurnRadius / minSpeed)
            : 0.0f;

        return minTurnRadius;
    }

    private void MaintainCurrentPositionOther(GameObject obj, PhysicsBehavior physics)
    {
        physics.Turning = PhysicsTurningType.None;
        if (physics.IsMotive) // no need to stop something that isn't moving.
        {
            physics.ScrubVelocity2D(0); // stop.
        }
    }

    private static float CalcSlowDownDist(float curSpeed, float desiredSpeed, float maxBraking)
    {
        var delta = curSpeed - desiredSpeed;
        if (delta <= 0)
        {
            return 0.0f;
        }

        var dist = ((delta * delta) / MathF.Abs(maxBraking)) * 0.5f;

        // use a little fudge so that things can stop "on a dime" more easily...
        const float fudge = 1.05f;
        return dist * fudge;
    }

    private static bool IsNearlyZero(float value)
    {
        const float tinyEpsilon = 0.001f;
        return MathF.Abs(value) < tinyEpsilon;
    }

    private void SetFlag(LocomotorFlags flag, bool value)
    {
        if (value)
        {
            _flags |= flag;
        }
        else
        {
            _flags &= ~flag;
        }
    }

    private bool GetFlag(LocomotorFlags flag) => (_flags & flag) != 0;

    internal void DrawInspector()
    {
        ImGui.InputFloat3("Maintain position", ref _maintainPosition);
        ImGui.InputFloat("Braking factor", ref _brakingFactor);
        ImGui.InputFloat("Angle offset", ref _angleOffset);

        ImGui.Separator();

        void DrawFlag(string text, LocomotorFlags flag)
        {
            var value = GetFlag(flag);
            if (ImGui.Checkbox(text, ref value))
            {
                SetFlag(flag, value);
            }
        }

        DrawFlag("IsBraking", LocomotorFlags.IsBraking);
        DrawFlag("AllowInvalidPosition", LocomotorFlags.AllowInvalidPosition);
        DrawFlag("MaintainPositionIsValid", LocomotorFlags.MaintainPositionIsValid);
        DrawFlag("PreciseZPosition", LocomotorFlags.PreciseZPosition);
        DrawFlag("NoSlowDownAsApproachingDestination", LocomotorFlags.NoSlowDownAsApproachingDestination);
        DrawFlag("OverWater", LocomotorFlags.OverWater);
        DrawFlag("UltraAccurate", LocomotorFlags.UltraAccurate);
        DrawFlag("MovingBackwards", LocomotorFlags.MovingBackwards);
        DrawFlag("DoingThreePointTurn", LocomotorFlags.DoingThreePointTurn);
        DrawFlag("Climbing", LocomotorFlags.Climbing);
        DrawFlag("IsCloseEnoughDistance3D", LocomotorFlags.IsCloseEnoughDistance3D);
        DrawFlag("OffsetIncreasing", LocomotorFlags.OffsetIncreasing);
    }

    public void Persist(StatePersister reader)
    {
        var version = reader.PersistVersion(2);

        // We've never seen version 1 in the wild.
        if (version >= 2)
        {
            reader.PersistLogicFrame(ref _donutTimer);
        }

        reader.PersistVector3(ref _maintainPosition);
        reader.PersistSingle(ref _brakingFactor);
        reader.PersistSingle(ref _maxLift);
        reader.PersistSingle(ref _maxSpeed);
        reader.PersistSingle(ref _maxAcceleration);
        reader.PersistSingle(ref _maxBraking);
        reader.PersistSingle(ref _maxTurnRate);
        reader.PersistSingle(ref _closeEnoughDistance);
        reader.PersistEnumFlags(ref _flags);
        reader.PersistSingle(ref _preferredHeight);
        reader.PersistSingle(ref _preferredHeightDamping);
        reader.PersistSingle(ref _angleOffset);
        reader.PersistSingle(ref _offsetIncrement);
    }

    // These values are saved in save files, so they can't be changed.
    [Flags]
    private enum LocomotorFlags
    {
        None = 0,
        IsBraking = 1 << 0,
        AllowInvalidPosition = 1 << 1,
        MaintainPositionIsValid = 1 << 2,
        PreciseZPosition = 1 << 3,
        NoSlowDownAsApproachingDestination = 1 << 5,

        /// <summary>
        /// To allow things to move slower/faster over water and do special effects.
        /// </summary>
        OverWater = 1 << 6,

        UltraAccurate = 1 << 7,
        MovingBackwards = 1 << 8,
        DoingThreePointTurn = 1 << 9,
        Climbing = 1 << 10,
        IsCloseEnoughDistance3D = 1 << 11,
        OffsetIncreasing = 1 << 12,
    }
}
