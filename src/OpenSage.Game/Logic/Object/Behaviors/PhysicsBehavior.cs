using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using OpenSage.Audio;
using OpenSage.Data.Ini;
using OpenSage.Diagnostics.Util;
using OpenSage.Mathematics;
using OpenSage.Utilities;

namespace OpenSage.Logic.Object;

public class PhysicsBehavior : UpdateModule, ICollideModule
{
    private const float MinAeroFriction = 0.00f;
    private const float MinNonAeroFriction = 0.01f;
    private const float MaxFriction = 0.99f;

    private const float StunReliefEpsilon = 0.5f;

    private const float InvalidVelocityMagnitude = -1.0f;

    private uint MotiveFrames => (uint)GameEngine.LogicFramesPerSecond / 3;

    private readonly PhysicsBehaviorModuleData _moduleData;

    private readonly float _gravity;

    private readonly float _aerodynamicFriction;
    private readonly float _forwardFriction;
    private readonly float _lateralFriction;
    private readonly float _zFriction;

    private readonly AudioEvent _bounceSound;

    private IProjectileUpdate _projectileUpdate;

    private float _yawRate;
    private float _rollRate;
    private float _pitchRate;
    private Vector3 _acceleration;
    private Vector3 _previousAcceleration;
    private Vector3 _velocity;
    private PhysicsTurningType _turning;
    private ObjectId _ignoreCollisionsWith;
    private PhysicsFlagType _flags;
    private float _mass;
    private ObjectId _currentOverlap;
    private ObjectId _previousOverlap;
    private LogicFrame _motiveForceExpires;
    private float _extraBounciness;
    private float _extraFriction;

    private float _velocityMagnitude;

    protected override UpdateOrder UpdateOrder => UpdateOrder.Order1;

    public float CenterOfMassOffset => _moduleData.CenterOfMassOffset;

    public bool IsMotive => _motiveForceExpires > GameEngine.GameLogic.CurrentFrame;

    public bool IsStunned => GetFlag(PhysicsFlagType.IsStunned);

    public float Mass
    {
        get
        {
            var result = _mass;

            if (GameObject.Contain != null)
            {
                result += GameObject.Contain.GetContainedItemsMass();
            }

            return result;
        }
        set => _mass = value;
    }

    public float YawRate
    {
        get => _yawRate;
        set
        {
            _yawRate = value;
            SetHasPitchRollYaw();
        }
    }

    public float RollRate
    {
        get => _rollRate;
        set
        {
            _rollRate = value;
            SetHasPitchRollYaw();
        }
    }

    public float PitchRate
    {
        get => _pitchRate;
        set
        {
            _pitchRate = value;
            SetHasPitchRollYaw();
        }
    }

    internal Vector3 Acceleration => _acceleration;
    internal Vector3 LastAcceleration => _previousAcceleration;
    internal Vector3 Velocity => _velocity;

    public PhysicsTurningType Turning
    {
        get => _turning;
        set => _turning = value;
    }

    internal ObjectId IgnoreCollisionsWith => _ignoreCollisionsWith;
    internal PhysicsFlagType Flags => _flags;
    internal ObjectId CurrentOverlap => _currentOverlap;
    internal ObjectId PreviousOverlap => _previousOverlap;
    internal LogicFrame MotiveForceExpires => _motiveForceExpires;
    internal float ExtraBounciness => _extraBounciness;

    /// <summary>
    /// Modifier to friction(s).
    /// </summary>
    public float ExtraFriction
    {
        get => _extraFriction;
        set => _extraFriction = value;
    }

    internal float VelocityMagnitude
    {
        get
        {
            if (_velocityMagnitude == InvalidVelocityMagnitude)
            {
                _velocityMagnitude = _velocity.Length();
            }
            return _velocityMagnitude;
        }
    }

    public bool StickToGround
    {
        set => SetFlag(PhysicsFlagType.StickToGround, value);
    }

    public bool AllowAirborneFriction
    {
        set => SetFlag(PhysicsFlagType.ApplyFriction2DWhenAirborne, value);
    }

    internal PhysicsBehavior(GameObject gameObject, GameEngine gameEngine, PhysicsBehaviorModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;

        Mass = moduleData.Mass;

        SetAllowBouncing(moduleData.AllowBouncing);
        SetAllowCollideForce(moduleData.AllowCollideForce);

        SetWakeFrame(UpdateSleepTime.None);

        _gravity = gameEngine.AssetStore.GameData.Current.Gravity * moduleData.GravityMult;

        // todo: do these clamps still apply at different logic frame rates?
        _aerodynamicFriction = Math.Clamp(moduleData.AerodynamicFriction, MinAeroFriction, MaxFriction);
        _forwardFriction = Math.Clamp(moduleData.ForwardFriction, MinNonAeroFriction, MaxFriction);
        _lateralFriction = Math.Clamp(moduleData.LateralFriction, MinNonAeroFriction, MaxFriction);
        _zFriction = Math.Clamp(moduleData.ZFriction, MinNonAeroFriction, MaxFriction);
    }

    protected internal override void OnObjectCreated()
    {
        _projectileUpdate = GameObject.FindBehavior<IProjectileUpdate>();
    }

    public void SetAllowBouncing(bool allow) => SetFlag(PhysicsFlagType.AllowBounce, allow);

    public void SetAllowCollideForce(bool allow) => SetFlag(PhysicsFlagType.AllowCollideForce, allow);

    private void SetFlag(PhysicsFlagType flag, bool value)
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

    private bool GetFlag(PhysicsFlagType flag) => (_flags & flag) != 0;

    public void SetIgnoreCollisionsWith(GameObject gameObject)
    {
        _ignoreCollisionsWith = gameObject?.Id ?? ObjectId.Invalid;
    }

    private bool IsIgnoringCollisionsWith(ObjectId objectId)
    {
        return objectId.IsValid && objectId == _ignoreCollisionsWith;
    }

    public void SetStunned(bool allow)
    {
        SetFlag(PhysicsFlagType.IsStunned, allow);
    }

    /// <summary>
    /// Basic rigid body physics using an Euler integrator.
    ///
    /// Original code also has this comment:
    /// @todo Currently, only translations are integrated. Rotations should also be integrated. (MSB)
    /// </summary>
    public override UpdateSleepTime Update()
    {
        var obj = GameObject;
        var d = _moduleData;
        var airborneAtStart = GameObject.IsAboveTerrain;
        var activeVelocityZ = 0.0f;
        var bounceForce = Vector3.Zero;
        var gotBounceForce = false;

        Debug.Assert(!GetFlag(PhysicsFlagType.IsInUpdate));

        SetFlag(PhysicsFlagType.IsInUpdate, true);

        if (!GetFlag(PhysicsFlagType.UpdateEverRun))
        {
            // Set the flag so that we don't get bogus "collisions" on the first frame.
            SetFlag(PhysicsFlagType.WasAirborneLastFrame, airborneAtStart);
        }

        var prevPos = GameObject.Translation;
        _previousAcceleration = _acceleration;

        if (!GameObject.IsDisabledByType(DisabledType.Held))
        {
            var mtx = GameObject.TransformMatrix;

            ApplyGravitationalForces();
            ApplyFrictionalForces();

            // Integrate acceleration into velocity.
            _velocity += _acceleration;

            // When velocity gets tiny, just clamp to zero.
            const float threshold = 0.001f;
            if (MathF.Abs(_velocity.X) < threshold)
            {
                _velocity.X = 0.0f;
            }
            if (MathF.Abs(_velocity.Y) < threshold)
            {
                _velocity.Y = 0.0f;
            }
            if (MathF.Abs(_velocity.Z) < threshold)
            {
                _velocity.Z = 0.0f;
            }

            _velocityMagnitude = InvalidVelocityMagnitude;

            var oldPosZ = mtx.Translation.Z;

            // Integrate velocity into position.
            if (GameObject.TestStatus(ObjectStatus.IsBraking))
            {
                // Don't update position if the locomotor is braking.
                if (!GameObject.IsKindOf(ObjectKinds.Projectile))
                {
                    // Things other than projectiles don't cheat in z.
                    var translation = mtx.Translation;
                    translation.Z += _velocity.Z;
                    mtx.Translation = translation;
                }
            }
            else
            {
                mtx.Translation += _velocity;
            }

            if (Vector3Utility.IsNaN(mtx.Translation))
            {
                Debug.Fail("Object position is NaN");
                GameEngine.GameLogic.DestroyObject(obj);
            }

            // Check when to clear the stunned status.
            // This is new in Zero Hour.
            if (GetFlag(PhysicsFlagType.IsStunned))
            {
                if (IsWithinThreshold(_velocity, StunReliefEpsilon)
                    || !obj.IsSignificantlyAboveTerrain)
                {
                    SetStunned(false);
                    obj.ClearModelConditionState(ModelConditionFlag.Stunned);
                }
            }

            if (GetFlag(PhysicsFlagType.HasPitchRollYaw))
            {
                // You may be tempted to do something like this:
                //
                // 	Real rollAngle = -mtx.Get_X_Rotation();
                // 	Real pitchAngle = mtx.Get_Y_Rotation();
                // 	Real yawAngle = mtx.Get_Z_Rotation();
                // 	// do stuff to angles, then rebuild the mtx with 'em
                //
                // You must resist this temptation, because your code will be wrong!
                //
                // The problem is that you can't use these calls to later reconstruct
                // the matrix... because doing such a thing is highly order-dependent,
                // and furthermore, you'd have to use Euler angles (Not the Get_?_Rotation
                // calls) to be able to reconstruct 'em, and that's too slow to do for
                // every object every frame.
                //
                // The one exception is that it is OK to use Get_Z_Rotation() to get
                // the yaw angle.

                // Only update the position if we are not held.
                // (Otherwise, slowdeath sinking into ground won't work.)
                var yawRateToUse = _yawRate * d.PitchRollYawFactor;
                var pitchRateToUse = _pitchRate * d.PitchRollYawFactor;
                var rollRateToUse = _rollRate * d.PitchRollYawFactor;

                // With a center of mass listing, pitchRate needs to dampen towards
                // straight down / straight up.
                var offset = CenterOfMassOffset;

                // Magnitude sets initial rate. Here we care about sign.
                if (offset != 0.0f)
                {
                    var xvec = mtx.GetXVector();
                    var xy = MathF.Sqrt(xvec.X * xvec.X + xvec.Y * xvec.Y);
                    var pitchAngle = MathF.Atan2(xvec.Z, xy);
                    var remainingAngle = (offset > 0)
                        ? (MathF.PI / 2) - pitchAngle
                        : -(MathF.PI / 2) + pitchAngle;
                    var s = MathF.Sin(remainingAngle);
                    pitchRateToUse *= s;
                }

                // Update rotation.
                // Original comment:
                // @todo Rotation should use torques, and integrate just like forces (MSB)
                // note, we DON'T want to Pre-rotate (either inplace or not),
                // since we want to add our mods to the existing matrix.
                mtx.RotateX(rollRateToUse);
                mtx.RotateY(pitchRateToUse);
                mtx.RotateZ(yawRateToUse);
            }

            // Do not allow object to pass through the ground.
            var groundZ = GameEngine.Game.TerrainLogic.GetLayerHeight(
                mtx.GetXTranslation(),
                mtx.GetYTranslation(),
                obj.Layer);

            if (obj.TestStatus(ObjectStatus.DeckHeightOffset))
            {
                groundZ += obj.CarrierDeckHeight;
            }

            gotBounceForce = HandleBounce(oldPosZ, mtx.GetZTranslation(), groundZ, ref bounceForce);

            // Remember our z velocity prior to doing ground-slam adjustment.
            activeVelocityZ = _velocity.Z;
            if (mtx.GetZTranslation() <= groundZ)
            {
                // Note - when vehicles are going down a slope, they will maintain a small negative
                // z velocity as they go down. So don't slam it to 0 if they aren't slamming into
                // the ground.
                var dz = groundZ - mtx.GetZTranslation(); // Our excess z velocity
                _velocity.Z += dz; // Remove the excess z velocity
                if (_velocity.Z > 0.0f)
                {
                    _velocity.Z = 0.0f;
                }

                _velocityMagnitude = InvalidVelocityMagnitude;

                mtx.SetZTranslation(groundZ);

                // This flag is ALWAYS cleared once we hit the ground.
                SetFlag(PhysicsFlagType.AllowToFall, false);

                // When a stunned object hits the ground the first time, change its model state
                // from stunned flailing to just stunned.
                if (GetFlag(PhysicsFlagType.IsStunned))
                {
                    obj.ClearModelConditionState(ModelConditionFlag.StunnedFlailing);
                    obj.SetModelConditionState(ModelConditionFlag.Stunned);
                }
            }
            else if (mtx.GetZTranslation() > groundZ)
            {
                if (GetFlag(PhysicsFlagType.IsInFreefall))
                {
                    obj.SetDisabled(DisabledType.Freefall);
                    obj.SetModelConditionState(ModelConditionFlag.FreeFall);
                }
                else if (GetFlag(PhysicsFlagType.StickToGround) && !GetFlag(PhysicsFlagType.AllowToFall))
                {
                    mtx.SetZTranslation(groundZ);
                }
            }

            if (gotBounceForce)
            {
                // Right the object after the bounce since the pitch and roll may have been affected.
                var yawAngle = GameObject.TransformMatrix.GetZRotation();
                SetAngles(yawAngle, 0.0f, 0.0f);

                // Set the translation of the after bounce matrix to the one calculated above.
                var afterBounceMatrix = GameObject.TransformMatrix;
                afterBounceMatrix.Translation = mtx.Translation;

                // Set the result of the after bounce matrix as the object's final matrix.
                obj.SetTransformMatrix(afterBounceMatrix);
            }
            else
            {
                obj.SetTransformMatrix(mtx);
            }
        } // if not held

        // Reset the acceleration for accumulation next frame.
        _acceleration = Vector3.Zero;

        // Clear overlap object, which will be set by PhysicsCollide later.
        _previousOverlap = _currentOverlap;
        _currentOverlap = ObjectId.Invalid;

        if (gotBounceForce && GetFlag(PhysicsFlagType.AllowBounce))
        {
            ApplyForce(bounceForce);
        }

        var airborneAtEnd = obj.IsAboveTerrain;

        // It's not good enough to check for airborne being different between
        // the start and end of this method. We have to compare since last frame,
        // since (if we're held by a parachute, for instance) we might have been
        // moved by other bits of code!
        if (GetFlag(PhysicsFlagType.WasAirborneLastFrame) && !airborneAtEnd && !GetFlag(PhysicsFlagType.ImmuneToFallingDamage))
        {
            DoBounceSound(prevPos);

            // Original comment:
            // The normal always points straight down, through we could
            // get the alignWithTerrain() normal if it proves interesting.
            var normal = -Vector3.UnitZ;
            obj.OnCollide(null, obj.Translation, normal);

            // Don't bother trying to remember how far we've fallen. Instead,
            // we back-calculate it from our speed and gravity: v = sqrt(2*g*h).
            // Note that MinFallSpeedForDamage is always positive.
            //
            // Also note: since projectiles are immune to falling damage,
            // don't even bother doing this check here.
            var netSpeed = -activeVelocityZ - d.MinFallSpeedForDamage;
            if (netSpeed > 0.0f && _projectileUpdate == null)
            {
                // Only apply force if it's a pretty steep fall, so that things
                // going down hills don't injure themselves
                // (unless the hill is really steep).
                const float minAngleTan = 3.0f; // Roughly 71 degrees
                const float tinyDelta = 0.01f;
                if ((MathF.Abs(_velocity.X) <= tinyDelta || MathF.Abs(activeVelocityZ / _velocity.X) >= minAngleTan) &&
                    (MathF.Abs(_velocity.Y) <= tinyDelta || MathF.Abs(activeVelocityZ / _velocity.Y) >= minAngleTan))
                {
                    var damageAmount = netSpeed * Mass * d.FallHeightDamageFactor;

                    var damageInfo = new DamageData();
                    damageInfo.Request.DamageType = DamageType.Falling;
                    damageInfo.Request.DeathType = DeathType.Splatted;
                    damageInfo.Request.DamageDealer = obj.Id;
                    damageInfo.Request.DamageToDeal = damageAmount;
                    damageInfo.Request.ShockWaveAmount = 0.0f;

                    obj.AttemptDamage(ref damageInfo);

                    // If this killed us, add SPLATTED to get a cool death.
                    if (obj.IsEffectivelyDead)
                    {
                        obj.SetModelConditionState(ModelConditionFlag.Splatted);
                    }
                }
            }
        }

        if (!airborneAtEnd)
        {
            // Just in case.
            SetFlag(PhysicsFlagType.IsInFreefall, false);
            if (obj.IsDisabledByType(DisabledType.Freefall))
            {
                obj.ClearDisabled(DisabledType.Freefall);
            }
            obj.ClearModelConditionState(ModelConditionFlag.FreeFall);
        }

        // If we are effectively dead, we shouldn't recall kill.
        if (d.KillWhenRestingOnGround && !airborneAtEnd && IsVerySmall3D(_velocity))
        {
            if (!obj.IsKindOf(ObjectKinds.Drone) || obj.IsEffectivelyDead || obj.IsDisabledByType(DisabledType.Unmanned))
            {
                // Must be one of the following cases in order to splat:
                // 1. not a drone
                // 2. dead drone
                // 3. unmanned drone
                obj.Kill();
            }
        }

        SetFlag(PhysicsFlagType.UpdateEverRun, true);
        SetFlag(PhysicsFlagType.WasAirborneLastFrame, airborneAtEnd);

        SetFlag(PhysicsFlagType.IsInUpdate, false);

        return CalculateSleepTime();
    }

    /// <summary>
    /// Applies a force at the object's center of gravity.
    /// </summary>
    /// <param name="force">The force to apply.</param>
    public void ApplyForce(in Vector3 force)
    {
        Debug.Assert(!Vector3Utility.IsNaN(force));
        if (Vector3Utility.IsNaN(force))
        {
            return;
        }

        // F = ma  -->  a = F/M (divide force by mass)
        var mass = Mass;
        var modForce = force;
        if (IsMotive)
        {
            var dir = GameObject.UnitDirectionVector2D;
            // Only accept the lateral acceleration.
            var lateralDot = (force.X * -dir.Y) + (force.Y * dir.X);
            modForce.X = lateralDot * -dir.Y;
            modForce.Y = lateralDot * dir.X;
        }

        var massInverse = 1.0f / mass;
        _acceleration += modForce * massInverse;

        MaybeWakeUp();
    }

    private void MaybeWakeUp()
    {
        if (GetFlag(PhysicsFlagType.IsInUpdate))
        {
            // We're applying a force from inside our own update (probably for a bounce or friction).
            // Just do nothing, since update will calculate the correct sleep behavior at the end.
        }
        else
        {
            // When a force is applied by an external module, we must wake up, even if the force is zero.
            SetWakeFrame(UpdateSleepTime.None);
        }
    }

    /// <summary>
    /// Applies a shockwave force at the object's center of gravity.
    /// </summary>
    /// <param name="force">The shockwave force to apply.</param>
    public void ApplyShock(in Vector3 force)
    {
        var resistedForce = force * (1.0f - Math.Clamp(_moduleData.ShockResistance, 0.0f, 1.0f));

        // Apply the processed shock force to the object.
        ApplyForce(resistedForce);
    }

    /// <summary>
    /// Applies a random rotation at the object's center of gravity.
    /// </summary>
    public void ApplyRandomRotation()
    {
        // Ignore any pitch, roll and yaw rotation if behavior is stick to ground.
        if (GetFlag(PhysicsFlagType.StickToGround))
        {
            return;
        }

        // Set bounce to true for a while until the unit is complete bouncing.
        SetAllowBouncing(true);

        var randomModifier = GameEngine.Random.NextSingle(-1.0f, 1.0f);
        _yawRate += _moduleData.ShockMaxYaw * randomModifier;

        randomModifier = GameEngine.Random.NextSingle(-1.0f, 1.0f);
        _pitchRate += _moduleData.ShockMaxPitch * randomModifier;

        randomModifier = GameEngine.Random.NextSingle(-1.0f, 1.0f);
        _rollRate += _moduleData.ShockMaxRoll * randomModifier;

        MaybeWakeUp();
    }

    public void ApplyMotiveForce(in Vector3 force)
    {
        // Make it accept this force unquestioningly.
        _motiveForceExpires = new LogicFrame(0);
        ApplyForce(force);
        _motiveForceExpires = GameEngine.GameLogic.CurrentFrame + new LogicFrameSpan(MotiveFrames);
    }

    public void ResetDynamicPhysics()
    {
        _acceleration = Vector3.Zero;
        _previousAcceleration = Vector3.Zero;
        _velocity = Vector3.Zero;
        _velocityMagnitude = 0.0f;
        _turning = PhysicsTurningType.None;
        _yawRate = 0.0f;
        _rollRate = 0.0f;
        _pitchRate = 0.0f;

        SetFlag(PhysicsFlagType.HasPitchRollYaw, false);

        Debug.Assert(!GetFlag(PhysicsFlagType.IsInUpdate), "hmm, should not happen, may not work");

        SetWakeFrame(CalculateSleepTime());
    }

    private void ApplyGravitationalForces()
    {
        _acceleration.Z += _gravity;
    }

    private void ApplyFrictionalForces()
    {
        // Are we a plane that is taxiing on a deck with a height offset?
        // This deckTaxiing thing is new in Zero Hour, but it's backwards-compatible with Generals.
        var deckTaxiing = GameObject.TestStatus(ObjectStatus.DeckHeightOffset)
            && GameObject.AIUpdate?.CurrentLocomotorSetType == LocomotorSetType.Taxiing;

        if (GetFlag(PhysicsFlagType.ApplyFriction2DWhenAirborne)
            || !GameObject.IsSignificantlyAboveTerrain
            || deckTaxiing)
        {
            ApplyYawPitchRollDamping(1.0f - PhysicsBehaviorModuleData.DefaultLateralFriction);

            if (_velocity.X != 0.0f || _velocity.Y != 0.0f)
            {
                var dir = GameObject.UnitDirectionVector2D;
                var mass = Mass;

                var lateralDot = (_velocity.X * -dir.Y) + (_velocity.Y * dir.X);
                var lateralVelocityX = lateralDot * -dir.Y;
                var lateralVelocityY = lateralDot * dir.X;

                var lateralFriction = mass * _lateralFriction;

                var acceleration = new Vector3(
                    -(lateralFriction * lateralVelocityX),
                    -(lateralFriction * lateralVelocityY),
                    0.0f);

                if (!IsMotive)
                {
                    var forwardDot = (_velocity.X * dir.X) + (_velocity.Y * dir.Y);
                    var forwardVelocityX = forwardDot * dir.X;
                    var forwardVelocityY = forwardDot * dir.Y;
                    var forwardFriction = mass * _forwardFriction;
                    acceleration.X -= forwardFriction * forwardVelocityX;
                    acceleration.Y -= forwardFriction * forwardVelocityY;
                }

                ApplyForce(acceleration);
            }
        }
        else
        {
            var aerodynamicFriction = -_aerodynamicFriction; // negated!

            // Air resistance is proportional to velocity in the opposite direction.
            _acceleration += _velocity * aerodynamicFriction;

            // Since aerodynamic friction is negated, this results in 1.0 - aerodynamicFriction.
            ApplyYawPitchRollDamping(1.0f + aerodynamicFriction);
        }
    }

    private bool HandleBounce(float oldZ, float newZ, float groundZ, ref Vector3 bounceForce)
    {
        if (GetFlag(PhysicsFlagType.AllowBounce) && newZ <= groundZ)
        {
            const float minStiffness = 0.01f;
            const float maxStiffness = 0.99f;

            var stiffness = Math.Clamp(
                base.GameEngine.AssetStore.GameData.Current.GroundStiffness,
                minStiffness,
                maxStiffness);

            var desiredAccelerationZ = 0.0f;
            var velocityZ = _velocity.Z;
            if (oldZ > groundZ && velocityZ < 0.0f)
            {
                desiredAccelerationZ = MathF.Abs(velocityZ) * stiffness;
            }

            bounceForce = new Vector3(0.0f, 0.0f, Mass * desiredAccelerationZ);

            const float damping = 0.7f;
            ApplyYawPitchRollDamping(damping);

            if (velocityZ < 0.0f)
            {
                // TODO: Check that "Up" is correct here.
                var zVec = GameObject.TransformMatrix.GetZVector();
                var rollAngle = zVec.Z > 0 ? 0 : MathF.PI;
                // Don't flip both pitch and roll... we'll "flip" twice.
                var pitchAngle = 0.0f;
                var yawAngle = GameObject.Transform.Yaw;
                SetAngles(yawAngle, pitchAngle, rollAngle);
            }

            if (GameEngine.Game.SageGame == SageGame.CncGenerals)
            {
                return true;
            }

            if (bounceForce.Z > 0.0f)
            {
                TestStunnedUnitForDestruction();
                return true;
            }
            else
            {
                // TODO(Port): The original game called SetAllowBouncing(_originalAllowBounce),
                // but _originalAllowBounce is never actually set to true.
                // I think the intention was to call SetAllowBouncing(_moduleData.AllowBouncing),
                // but I won't change that yet because it might affect gameplay.
                SetAllowBouncing(false);
                return false;
            }
        }
        else
        {
            bounceForce = Vector3.Zero;
            return false;
        }
    }

    /// <summary>
    /// Tests whether unit needs to die because of being on illegal cell,
    /// upside down, or outside legal bounds.
    /// </summary>
    private void TestStunnedUnitForDestruction()
    {
        // Only do test if unit is stunned.
        if (!GetFlag(PhysicsFlagType.IsStunned))
        {
            return;
        }

        // Grab the object.
        var obj = GameObject;

        // If a stunned object is upside down when it hits the ground, kill it.
        if (GameObject.TransformMatrix.GetZVector().Z < 0.0f)
        {
            obj.Kill();
            return;
        }

        // Check if unit has exited playable area. If so, kill it.
        if (obj.IsOffMap)
        {
            obj.Kill();
            return;
        }

        // Check for being in cells that the unit has no locomotor for.
        var ai = obj.AIUpdate;
        if (ai == null)
        {
            return;
        }

        var pos = GameObject.Translation;

        // Check for object being stuck on cliffs. If so, kill it.
        if (GameEngine.Game.TerrainLogic.IsCliffCell(pos.X, pos.Y)
            && !ai.HasLocomotorForSurface(Surfaces.Cliff))
        {
            obj.Kill();
            return;
        }

        // Check for object being stuck on water. If so, kill it.
        if (GameEngine.Game.TerrainLogic.IsUnderwater(pos.X, pos.Y, out var _)
            && !ai.HasLocomotorForSurface(Surfaces.Water))
        {
            obj.Kill();
            return;
        }
    }

    private void ApplyYawPitchRollDamping(float factor)
    {
        _pitchRate *= factor;
        _rollRate *= factor;
        _yawRate *= factor;
        SetHasPitchRollYaw();
    }

    private void SetHasPitchRollYaw()
    {
        SetFlag(PhysicsFlagType.HasPitchRollYaw, _pitchRate != 0.0f || _rollRate != 0.0f || _yawRate != 0.0f);
    }

    private static bool IsVerySmall3D(in Vector3 v)
    {
        const float threshold = 0.01f;
        return IsWithinThreshold(v, threshold);
    }

    private static bool IsWithinThreshold(in Vector3 v, float threshold)
    {
        return MathF.Abs(v.X) < threshold && MathF.Abs(v.Y) < threshold && MathF.Abs(v.Z) < threshold;
    }

    public void SetBounceSound(AudioEvent bounceSound)
    {
        // TODO(Port): Implement this.
        throw new NotImplementedException();
    }

    public void SetAngles(float yaw, float pitch, float roll)
    {
        var pos = GameObject.Translation;

        // TODO(Port): Check if this is correct.
        var transform = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);

        transform.Translation = pos;

        GameObject.SetTransformMatrix(transform);
    }

    private void DoBounceSound(in Vector3 prevPos)
    {
        if (_bounceSound == null)
        {
            return;
        }

        // TODO(Port): Implement this.
        throw new NotImplementedException();
    }

    private UpdateSleepTime CalculateSleepTime()
    {
        if (_velocity == Vector3.Zero
            && _acceleration == Vector3.Zero
            && !GetFlag(PhysicsFlagType.HasPitchRollYaw)
            && !IsMotive
            && GameObject.Layer == PathfindLayerType.Ground
            && !GameObject.IsAboveTerrain
            && _currentOverlap.IsInvalid
            && _previousOverlap.IsInvalid
            && GetFlag(PhysicsFlagType.UpdateEverRun))
        {
            return UpdateSleepTime.Forever;
        }

        return UpdateSleepTime.None;
    }

    /// <summary>
    /// Resolves the collision between this object and <paramref name="other"/>
    /// by computing the amount the objects have overlapped, and apply proportional
    /// forces to push them apart.
    ///
    /// Note that this call only applies force to our object, not to <paramref name="other"/>
    /// Since the forces should be equal and opposite, this could be optimized.
    ///
    /// TODOs from original code:
    /// @todo Make this work properly for non-cylindrical objects (MSB)
    /// @todo Physics collision resolution is 2D - should it be 3D? (MSB)
    /// </summary>
    void ICollideModule.OnCollide(GameObject other, in Vector3 location, in Vector3 normal)
    {
        if (_projectileUpdate != null)
        {
            // Projectiles always get a chance to handle their own collisions,
            // and not go through here.
            if (_projectileUpdate.ProjectileHandleCollision(other))
            {
                return;
            }
        }

        var obj = GameObject;
        var objContainedBy = obj.ContainedBy;

        // Note that other == null means "collide with ground".
        if (other == null)
        {
            // If we are in a container, tell the container we collided with the ground.
            // (handy for parachutes)
            if (objContainedBy != null)
            {
                objContainedBy.OnCollide(other, location, normal);
            }
            return;
        }

        // If we are containing this object, ignore collisions with it.
        var otherContainedBy = other.ContainedBy;
        if (otherContainedBy == obj || objContainedBy == other)
        {
            return;
        }

        // If we're both on parachutes, we never collide with each other.
        // Original todo: @todo srj -- ugh, an awful hack for paradrop problems. fix someday.
        if (obj.TestStatus(ObjectStatus.Parachuting) && other.TestStatus(ObjectStatus.Parachuting))
        {
            return;
        }

        // Ignore collisions with our "ignore" thingie, if any (and vice versa).
        var ai = obj.AIUpdate;
        if (ai != null && ai.IgnoredObstacleID == other.Id)
        {
            // Original todo: @todo srj -- what the hell is this code doing here? ack!
            // Before we return, check for a very special case of an infantry colliding with
            // an unmanned vehicle. If this is the case, it'll become its new pilot!
            if (obj.IsKindOf(ObjectKinds.Infantry) && other.IsDisabledByType(DisabledType.Unmanned))
            {
                // This is in fact the case, and we are doing it here because it applies
                // to all infantry in any unmanned vehicle. This could be done via a special/new
                // module, but doing it here doesn't require a new module update to every infantry.
                other.ClearDisabled(DisabledType.Unmanned);

                // We need to be able to test whether an object on a team has been captured,
                // so set here that this object was captured.
                other.SetCaptured(true);

                other.Defect(obj.Team, 0);

                // In order to make things easier for the designers, we are going
                // to transfer the name of the infantry to the vehicle... so the
                // designer can control the vehicle with their scripts.
                GameEngine.Game.Scripting.TransferObjectName(obj.Name, other);

                GameEngine.GameLogic.DestroyObject(obj);
            }
            return;
        }

        var aiOther = other.AIUpdate;
        if (aiOther != null && aiOther.IgnoredObstacleID == obj.Id)
        {
            return;
        }

        if (IsIgnoringCollisionsWith(other.Id))
        {
            return;
        }

        var immobile = obj.IsKindOf(ObjectKinds.Immobile);
        var otherImmobile = other.IsKindOf(ObjectKinds.Immobile);

        var otherPhysics = other.Physics;
        if (otherPhysics != null)
        {
            if (otherPhysics.IsIgnoringCollisionsWith(obj.Id))
            {
                return;
            }
        }
        else
        {
            // If the object we have collided with has no physics, it is
            // insubstantial (exception: if it's immobile).
            if (!otherImmobile)
            {
                return;
            }
        }

        if (CheckForOverlapCollision(other))
        {
            // We should overlap them, rather than bounce them, so punt here.
            // (Note that we do this prior to checking for Physics update,
            // because Overlap doesn't require "other" to have Physics...)
            return;
        }
    }

    /// <summary>
    /// Do the collision.
    /// Cases:
    /// - The crusheeOther is !OVERLAPPABLE or !CRUSHABLE.               Result: return false
    /// - The crusherMe is !CRUSHER.                                     Result: return false
    /// - crusher is CRUSHER, crushee is CRUSHABLE but allied.           Result: return false
    /// - crusher is CRUSHER, crushee is OVERLAPPABLE but not CRUSHABLE. Result: return false
    /// - crusher has no PhysicsBehavior.                                Result: return false
    /// - crusher is CRUSHER, crushee is CRUSHABLE, but it is too hard.  Result: return false
    /// - crusher is CRUSHER, crushee is CRUSHABLE, hardness ok.         Result: return true
    /// </summary>
    /// <returns>Returns <code>true</code> if we want to skip having physics push us apart.</returns>
    private bool CheckForOverlapCollision(GameObject other)
    {
        // This is the most Supreme Truth... that unless I am moving right now,
        // I may not crush anything!
        if (IsVerySmall3D(_velocity))
        {
            return false;
        }

        var crusherMe = GameObject;
        var crusheeOther = other;

        // Determine if we can crush the other object.
        var selfCrushingOther = crusherMe.CanCrushOrSquish(crusheeOther, CrushSquishTestType.TestCrushOnly);
        var selfBeingCrushed = crusheeOther.CanCrushOrSquish(crusherMe, CrushSquishTestType.TestCrushOnly);

        if (selfCrushingOther && selfBeingCrushed)
        {
            // Is it possible to crush and be crushed at the same time?
            Debug.Fail($"{crusherMe.Definition.Name} (Crusher:{crusherMe.CrusherLevel}, Crushable:{crusherMe.CrushableLevel}) is attempting to crush {crusheeOther.Definition.Name} (Crusher:{crusheeOther.CrusherLevel}, Crushable:{crusheeOther.CrushableLevel}) but it is reciprocating - shouldn't be possible!");
            return false;
        }

        // If we are being crushed, then skip all this and return true.
        // This allows us to NOT react in the normal way and just be passive
        // to the overlap...
        if (selfBeingCrushed)
        {
            return true;
        }

        // Original code had this, but I don't think it's necessary.
        // // Grab physics module if there.
        // var crusherPhysics = this;
        // if (crusherPhysics == null)
        // {
        //     return null;
        // }

        if (!selfCrushingOther)
        {
            return false;
        }

        // Ok, add this to our list-of-overlapped-things.
        AddOverlap(crusheeOther);
        if (!WasPreviouslyOverlapped(crusheeOther))
        {
            var damageInfo = new DamageData();
            damageInfo.Request.DamageType = DamageType.Crush;
            damageInfo.Request.DeathType = DeathType.Crushed;
            damageInfo.Request.DamageDealer = crusherMe.Id;
            damageInfo.Request.DamageToDeal = 0.0f; // Yes, that's right - we don't want to damage, just to trigger the minor DamageFX, if any
            crusheeOther.AttemptDamage(ref damageInfo);
        }

        var crusheePos = crusheeOther.Translation;
        var crusherPos = crusherMe.Translation;

        var crusheeBody = crusheeOther.BodyModule;
        var frontCrushed = crusheeBody.FrontCrushed;
        var backCrushed = crusheeBody.BackCrushed;
        if (!(frontCrushed && backCrushed))
        {
            var crushIt = false;

            var dir = crusherMe.UnitDirectionVector2D;
            var crusheeDir = crusheeOther.UnitDirectionVector2D;
            var crushPointOffsetDistance = crusheeOther.Geometry.MajorRadius / 2;

            var crushPointOffset = new Vector3(
                crusheeDir.X * crushPointOffsetDistance,
                crusheeDir.Y * crushPointOffsetDistance,
                0.0f);

            Vector3 comparisonCoord;
            float dx, dy;

            // Original todo: @todo GS To account for different sized crushers, this should be redone as a box or circle test, not a point
            // First decide which crush point has the shortest perp to our direction ray.
            // float bestPerp = 9999;
            var crushTarget = CrushType.NoCrush;
            if (frontCrushed || backCrushed)
            {
                // Degenerate case; there is only one point to consider.
                crushTarget = frontCrushed
                    ? CrushType.BackEndCrush
                    : CrushType.FrontEndCrush;
            }
            else
            {
                // Else, there are three points.
                float frontPerpLength, backPerpLength, centerPerpLength;
                Vector3 frontVector, backVector, centerVector;
                {
                    comparisonCoord = crusheePos;
                    comparisonCoord.X += crushPointOffset.X;
                    comparisonCoord.Y += crushPointOffset.Y;
                    frontVector = comparisonCoord;
                    frontVector.X -= crusherPos.X;
                    frontVector.Y -= crusherPos.Y; // vector from me to the front crush point
                    frontVector.Z = 0;

                    var rayLength = frontVector.X * dir.X + frontVector.Y * dir.Y;
                    var dirVector = new Vector3(
                        rayLength * dir.X,
                        rayLength * dir.Y, // vector from me to point of perp along direction ray
                        0);

                    var perpVector = new Vector3(
                        dirVector.X - frontVector.X,
                        dirVector.Y - frontVector.Y, // vector from the front point perp to my direction
                        0);

                    frontPerpLength = perpVector.Length();
                }
                {
                    comparisonCoord = crusheePos;
                    comparisonCoord.X -= crushPointOffset.X;
                    comparisonCoord.Y -= crushPointOffset.Y;
                    backVector = comparisonCoord;
                    backVector.X -= crusherPos.X;
                    backVector.Y -= crusherPos.Y; // vector from me to the front crush point
                    backVector.Z = 0;

                    var rayLength = backVector.X * dir.X + backVector.Y * dir.Y;
                    var dirVector = new Vector3(
                        rayLength * dir.X,
                        rayLength * dir.Y, // vector from me to point of perp along direction ray
                        0);

                    var perpVector = new Vector3(
                        dirVector.X - backVector.X,
                        dirVector.Y - backVector.Y, // vector from the front point perp to my direction
                        0);

                    backPerpLength = perpVector.Length();
                }
                {
                    comparisonCoord = crusheePos;
                    centerVector = comparisonCoord;
                    centerVector.X -= crusherPos.X;
                    centerVector.Y -= crusherPos.Y; // vector from me to the front crush point
                    centerVector.Z = 0;

                    var rayLength = centerVector.X * dir.X + centerVector.Y * dir.Y;
                    var dirVector = new Vector3(
                        rayLength * dir.X,
                        rayLength * dir.Y, //vector from me to point of perp along direction ray
                        0);

                    var perpVector = new Vector3(
                        dirVector.X - centerVector.X,
                        dirVector.Y - centerVector.Y, //vector from the front point perp to my direction
                        0);

                    centerPerpLength = perpVector.Length();
                }

                // Now find the shortest. Use the straightline distance to crush point as tie breaker.
                if ((frontPerpLength <= centerPerpLength) && (frontPerpLength <= backPerpLength))
                {
                    if (PerpsLogicallyEqual(frontPerpLength, centerPerpLength)
                        || PerpsLogicallyEqual(frontPerpLength, backPerpLength))
                    {
                        var frontVectorLength = frontVector.Length();
                        if (PerpsLogicallyEqual(frontPerpLength, centerPerpLength))
                        {
                            var centerVectorLength = centerVector.Length();
                            crushTarget = frontVectorLength < centerVectorLength
                                ? CrushType.FrontEndCrush
                                : CrushType.TotalCrush;
                        }
                        else if (PerpsLogicallyEqual(frontPerpLength, backPerpLength))
                        {
                            var backVectorLength = backVector.Length();
                            crushTarget = frontVectorLength < backVectorLength
                                ? CrushType.FrontEndCrush
                                : CrushType.BackEndCrush;
                        }
                    }
                    else
                    {
                        crushTarget = CrushType.FrontEndCrush;
                    }
                }
                else if ((backPerpLength <= centerPerpLength) && (backPerpLength <= frontPerpLength))
                {
                    if (PerpsLogicallyEqual(backPerpLength, centerPerpLength)
                        || PerpsLogicallyEqual(backPerpLength, frontPerpLength))
                    {
                        var backVectorLength = backVector.Length();
                        if (PerpsLogicallyEqual(backPerpLength, centerPerpLength))
                        {
                            var centerVectorLength = centerVector.Length();
                            crushTarget = backVectorLength < centerVectorLength
                                ? CrushType.BackEndCrush
                                : CrushType.TotalCrush;
                        }
                        else if (PerpsLogicallyEqual(backPerpLength, frontPerpLength))
                        {
                            var frontVectorLength = frontVector.Length();
                            crushTarget = backVectorLength < frontVectorLength
                                ? CrushType.BackEndCrush
                                : CrushType.FrontEndCrush;
                        }
                    }
                    else
                    {
                        crushTarget = CrushType.BackEndCrush;
                    }
                }
                else // centerperp is shortest
                {
                    if (PerpsLogicallyEqual(centerPerpLength, backPerpLength)
                        || PerpsLogicallyEqual(centerPerpLength, frontPerpLength))
                    {
                        var centerVectorLength = centerVector.Length();
                        if (PerpsLogicallyEqual(centerPerpLength, frontPerpLength))
                        {
                            var frontVectorLength = frontVector.Length();
                            crushTarget = centerVectorLength < frontVectorLength
                                ? CrushType.TotalCrush
                                : CrushType.FrontEndCrush;
                        }
                        else if (PerpsLogicallyEqual(centerPerpLength, backPerpLength))
                        {
                            var backVectorLength = backVector.Length();
                            crushTarget = centerVectorLength < backVectorLength
                                ? CrushType.TotalCrush
                                : CrushType.BackEndCrush;
                        }
                    }
                    else
                    {
                        crushTarget = CrushType.TotalCrush;
                    }
                }
            }

            var distanceTooFarSquared = 2.25f * crushPointOffsetDistance * crushPointOffsetDistance;

            // And just because there is only one crush point left doesn't mean
            // we automatically get it. (1.5x)^2 means if we are outside the
            // crushed front point, we will not auto get the back point (only
            // 3/8 car spread)

            // Then ask ourselves if we have passed the correct crush point (dot < 0).
            if (crushTarget == CrushType.TotalCrush)
            {
                // Check the middle crush point
                comparisonCoord = crusheePos; // copy so can move to each crush point

                dx = comparisonCoord.X - crusherPos.X;
                dy = comparisonCoord.Y - crusherPos.Y;

                var dot = dir.X * dx + dir.Y * dy;
                var distanceSquared = (dx * dx) + (dy * dy);

                if ((dot < 0) && (distanceSquared < distanceTooFarSquared))
                {
                    // Past target point, but not too far in distance or angle.
                    crushIt = true;
                }
            }
            else if (crushTarget == CrushType.FrontEndCrush)
            {
                // Check the front point.
                comparisonCoord = crusheePos;
                comparisonCoord.X += crushPointOffset.X;
                comparisonCoord.Y += crushPointOffset.Y;

                dx = comparisonCoord.X - crusherPos.X;
                dy = comparisonCoord.Y - crusherPos.Y;

                var dot = dir.X * dx + dir.Y * dy;
                var distanceSquared = (dx * dx) + (dy * dy);

                if ((dot < 0) && (distanceSquared < distanceTooFarSquared))
                {
                    // Past target point, but not too far in distance or angle.
                    crushIt = true;
                }
            }
            else if (crushTarget == CrushType.BackEndCrush)
            {
                // Check back point
                comparisonCoord = crusheePos;
                comparisonCoord.X -= crushPointOffset.X;
                comparisonCoord.Y -= crushPointOffset.Y;

                dx = comparisonCoord.X - crusherPos.X;
                dy = comparisonCoord.Y - crusherPos.Y;

                var dot = dir.X * dx + dir.Y * dy;
                var distanceSquared = (dx * dx) + (dy * dy);

                if ((dot < 0) && (distanceSquared < distanceTooFarSquared))
                {
                    // Past target point, but not too far in distance or angle.
                    crushIt = true;
                }
            }

            if (crushIt)
            {
                // Do a boat load of crush damage, and the OnDie will handle
                // cases like crushed car object.
                var damageInfo = new DamageData();
                damageInfo.Request.DamageType = DamageType.Crush;
                damageInfo.Request.DeathType = DeathType.Crushed;
                damageInfo.Request.DamageDealer = crusherMe.Id;
                damageInfo.Request.DamageToDeal = DamageConstants.HugeDamageAmount; // Make sure they die
                crusheeOther.AttemptDamage(ref damageInfo);
            }
        }

        return true;
    }

    private void AddOverlap(GameObject obj)
    {
        if (obj != null && !IsCurrentlyOverlapped(obj))
        {
            _currentOverlap = obj.Id;
        }
    }

    private bool IsCurrentlyOverlapped(GameObject obj)
    {
        return obj != null && obj.Id == _currentOverlap;
    }

    private bool WasPreviouslyOverlapped(GameObject obj)
    {
        return obj != null && obj.Id == _previousOverlap;
    }

    private static bool PerpsLogicallyEqual(float perpOne, float perpTwo)
    {
        // Equality with a wiggle fudge.
        const float perpRange = 0.15f;
        return MathF.Abs(perpOne - perpTwo) <= perpRange;
    }

    /// <summary>
    /// Returns the current velocity magnitude in the forward direction.
    /// If velocity is opposite facing vector, the returned value will be
    /// negative.
    /// </summary>
    public float GetForwardSpeed2D()
    {
        var dir = GameObject.UnitDirectionVector2D;

        var vx = _velocity.X * dir.X;
        var vy = _velocity.Y * dir.Y;

        var dot = vx + vy;

        var speedSquared = vx * vx + vy * vy;

        var speed = MathF.Sqrt(speedSquared);

        return dot >= 0.0f
            ? speed
            : -speed;
    }

    /// <summary>
    /// Returns the current velocity magnitude in the forward direction.
    /// If velocity is opposite facing vector, the returned value will be
    /// negative.
    /// </summary>
    public float GetForwardSpeed3D()
    {
        var dir = GameObject.TransformMatrix.GetXVector();

        var vx = _velocity.X * dir.X;
        var vy = _velocity.Y * dir.Y;
        var vz = _velocity.Z * dir.Z;

        var dot = vx + vy + vz;

        var speed = MathF.Sqrt(vx * vx + vy * vy + vz * vz);

        return dot >= 0.0f
            ? speed
            : -speed;
    }

    public void ScrubVelocityZ(float desiredVelocity)
    {
        if (MathF.Abs(desiredVelocity) < 0.001f)
        {
            _velocity.Z = 0;
        }
        else
        {
            if ((desiredVelocity < 0 && _velocity.Z < desiredVelocity) || (desiredVelocity > 0 && _velocity.Z > desiredVelocity))
            {
                _velocity.Z = desiredVelocity;
            }
        }
        _velocityMagnitude = InvalidVelocityMagnitude;
    }

    public void ScrubVelocity2D(float desiredVelocity)
    {
        if (desiredVelocity < 0.001f)
        {
            _velocity.X = 0;
            _velocity.Y = 0;
        }
        else
        {
            var curVelocity = MathF.Sqrt(_velocity.X * _velocity.X + _velocity.Y * _velocity.Y);
            if (desiredVelocity > curVelocity)
            {
                return;
            }
            desiredVelocity /= curVelocity;
            _velocity.X *= desiredVelocity;
            _velocity.Y *= desiredVelocity;
        }
        _velocityMagnitude = InvalidVelocityMagnitude;
    }

    public void TransferVelocityTo(PhysicsBehavior that)
    {
        that._velocity += _velocity;
        that._velocityMagnitude = InvalidVelocityMagnitude;
    }

    public void AddVelocityTo(in Vector3 velocity)
    {
        _velocity += velocity;
    }

    internal override void DrawInspector()
    {
        base.DrawInspector();

        ImGui.InputFloat("Mass", ref _mass);
        ImGui.DragFloat3("Acceleration", ref _acceleration);
        ImGui.DragFloat3("Previous acceleration", ref _previousAcceleration);
        ImGui.DragFloat3("Velocity", ref _velocity);
        ImGui.LabelText("Velocity magnitude", _velocityMagnitude.ToString());
        ImGui.InputFloat("Yaw rate", ref _yawRate);
        ImGui.InputFloat("Roll rate", ref _rollRate);
        ImGui.InputFloat("Pitch rate", ref _pitchRate);
        ImGuiUtility.ComboEnum("Turning", ref _turning);
        ImGui.LabelText("Motive force expires", _motiveForceExpires.Value.ToString());
        ImGui.InputFloat("Extra bounciness", ref _extraBounciness);
        ImGui.InputFloat("Extra friction", ref _extraFriction);
    }

    internal override void Load(StatePersister reader)
    {
        var version = reader.PersistVersion(2);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistSingle(ref _yawRate);
        reader.PersistSingle(ref _rollRate);
        reader.PersistSingle(ref _pitchRate);
        reader.PersistVector3(ref _acceleration);
        reader.PersistVector3(ref _previousAcceleration);
        reader.PersistVector3(ref _velocity);

        // We've never seen version 1 in the wild.
        if (version < 2)
        {
            var tmp = Vector3.Zero;
            reader.PersistVector3(ref tmp);
        }

        reader.PersistEnum(ref _turning);
        reader.PersistObjectId(ref _ignoreCollisionsWith);
        reader.PersistEnumFlags(ref _flags);
        reader.PersistSingle(ref _mass);
        reader.PersistObjectId(ref _currentOverlap);
        reader.PersistObjectId(ref _previousOverlap);
        reader.PersistLogicFrame(ref _motiveForceExpires);
        reader.PersistSingle(ref _extraBounciness);
        reader.PersistSingle(ref _extraFriction);
        reader.PersistSingle(ref _velocityMagnitude);
    }
}

public enum PhysicsTurningType
{
    Negative = -1,
    None = 0,
    Positive = 1,
}

[Flags]
internal enum PhysicsFlagType
{
    None = 0,
    StickToGround = 0x1,
    AllowBounce = 0x2,
    ApplyFriction2DWhenAirborne = 0x4,
    UpdateEverRun = 0x8,
    WasAirborneLastFrame = 0x10,
    AllowCollideForce = 0x20,
    AllowToFall = 0x40,
    HasPitchRollYaw = 0x80,
    ImmuneToFallingDamage = 0x100,
    IsInFreefall = 0x200,
    IsInUpdate = 0x400,

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    IsStunned = 0x800,
}

public class PhysicsBehaviorModuleData : UpdateModuleData
{
    private const float DefaultMass = 1.0f;

    private const float DefaultShockYaw = 0.05f;
    private const float DefaultShockPitch = 0.025f;
    private const float DefaultShockRoll = 0.025f;

    private const float DefaultForwardFriction = 0.15f;
    public const float DefaultLateralFriction = 0.15f;
    private const float DefaultZFriction = 0.8f;
    private const float DefaultAeroFriction = 0.0f;

    internal static PhysicsBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(
        FieldParseTable,
        new PhysicsBehaviorModuleData
        {
            MinFallSpeedForDamage = HeightToSpeed(parser, 40.0f),
        });

    internal static readonly IniParseTable<PhysicsBehaviorModuleData> FieldParseTable = new IniParseTable<PhysicsBehaviorModuleData>
    {
        { "Mass", (parser, x) => x.Mass = parser.ParseFloat() },
        { "ShockResistance", (parser, x) => x.ShockResistance = parser.ParseFloat() },
        { "ShockMaxYaw", (parser, x) => x.ShockMaxYaw = parser.ParseFloat() },
        { "ShockMaxPitch", (parser, x) => x.ShockMaxPitch = parser.ParseFloat() },
        { "ShockMaxRoll", (parser, x) => x.ShockMaxRoll = parser.ParseFloat() },
        { "ForwardFriction", (parser, x) => x.ForwardFriction = parser.ParseFrictionPerSec() },
        { "LateralFriction", (parser, x) => x.LateralFriction = parser.ParseFrictionPerSec() },
        { "ZFriction", (parser, x) => x.ZFriction = parser.ParseFrictionPerSec() },
        { "AerodynamicFriction", (parser, x) => x.AerodynamicFriction = parser.ParseFrictionPerSec() },
        { "CenterOfMassOffset", (parser, x) => x.CenterOfMassOffset = parser.ParseFloat() },
        { "AllowBouncing", (parser, x) => x.AllowBouncing = parser.ParseBoolean() },
        { "KillWhenRestingOnGround", (parser, x) => x.KillWhenRestingOnGround = parser.ParseBoolean() },
        { "AllowCollideForce", (parser, x) => x.AllowCollideForce = parser.ParseBoolean() },
        { "MinFallHeightForDamage", (parser, x) => x.MinFallSpeedForDamage = ParseHeightToSpeed(parser) },
        { "FallHeightDamageFactor", (parser, x) => x.FallHeightDamageFactor = parser.ParseFloat() },
        { "PitchRollYawFactor", (parser, x) => x.PitchRollYawFactor = parser.ParseFloat() },
        { "GravityMult", (parser, x) => x.GravityMult = parser.ParseFloat() },
        { "ShockStandingTime", (parser, x) => x.ShockStandingTime = parser.ParseInteger() },
        { "ShockStunnedTimeLow", (parser, x) => x.ShockStunnedTimeLow = parser.ParseInteger() },
        { "ShockStunnedTimeHigh", (parser, x) => x.ShockStunnedTimeHigh = parser.ParseInteger() },
        { "OrientToFlightPath", (parser, x) => x.OrientToFlightPath = parser.ParseBoolean() },
        { "FirstHeight", (parser, x) => x.FirstHeight = parser.ParseInteger() },
        { "SecondHeight", (parser, x) => x.SecondHeight = parser.ParseInteger() }
    };

    public float Mass { get; internal set; } = DefaultMass;
    public float ForwardFriction { get; private set; } = DefaultForwardFriction;
    public float LateralFriction { get; private set; } = DefaultLateralFriction;
    public float ZFriction { get; private set; } = DefaultZFriction;
    public float AerodynamicFriction { get; private set; } = DefaultAeroFriction;
    public float CenterOfMassOffset { get; private set; } = 0.0f;
    public bool AllowBouncing { get; private set; } = false;
    public bool KillWhenRestingOnGround { get; private set; } = false;
    public bool AllowCollideForce { get; private set; } = true;
    public float MinFallSpeedForDamage { get; private set; }
    public float FallHeightDamageFactor { get; private set; } = 1.0f;

    // Original comment says:
    // thru some bizarre editing mishap, we have been double-apply pitch/roll/yaw rates
    // to objects for, well, a long time, it looks like. I have corrected that problem
    // in the name of efficiency, but to maintain the same visual appearance without having
    // to edit every freaking INI in the world at this point, I am just multiplying
    // all the results by a factor so that the effect is the same (but with less execution time).
    // I have put this factor into INI in the unlikely event we ever need to change it,
    // but defaulting it to 2 is, in fact, the right thing for now... (srj)
    public float PitchRollYawFactor { get; private set; } = 2.0f;

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public float ShockResistance { get; private set; } = 0.0f;

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public float ShockMaxYaw { get; private set; } = DefaultShockYaw;

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public float ShockMaxPitch { get; private set; } = DefaultShockPitch;

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public float ShockMaxRoll { get; private set; } = DefaultShockRoll;

    [AddedIn(SageGame.Bfme)]
    public float GravityMult { get; private set; } = 1.0f;

    [AddedIn(SageGame.Bfme)]
    public int ShockStandingTime { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int ShockStunnedTimeLow { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int ShockStunnedTimeHigh { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public bool OrientToFlightPath { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int FirstHeight { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int SecondHeight { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new PhysicsBehavior(gameObject, gameEngine, this);
    }

    private static float HeightToSpeed(IniParser parser, float height)
    {
        // v = sqrt(2*g*h)
        return MathF.Sqrt(2.0f * parser.GameData.Gravity * height);
    }

    private static float ParseHeightToSpeed(IniParser parser)
    {
        return HeightToSpeed(parser, parser.ParseFloat());
    }
}
