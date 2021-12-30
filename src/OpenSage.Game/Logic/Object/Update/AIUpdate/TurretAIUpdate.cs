using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class TurretAIUpdate : UpdateModule
    {
        private readonly TurretAIUpdateModuleData _moduleData;
        private readonly GameObject _gameObject;

        private WeaponTarget _currentTarget;
        private TimeSpan _waitUntil;
        private TurretAIStates _turretAIstate;

        private float _unknownFloat1;
        private float _unknownFloat2;
        private uint _unknownFrame1;
        private uint _unknownInt1;
        private uint _unknownFrame2;
        private readonly bool[] _unknownBools = new bool[7];
        private uint _unknownFrame3;

        public enum TurretAIStates
        {
            Disabled,
            Idle,
            ScanningForTargets,
            Turning,
            Attacking,
            Recentering
        }

        internal TurretAIUpdate(GameObject gameObject, TurretAIUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;

            _gameObject.TurretYaw = MathUtility.ToRadians(_moduleData.NaturalTurretAngle);
            _gameObject.TurretPitch = MathUtility.ToRadians(_moduleData.NaturalTurretPitch);

            _turretAIstate = _moduleData.InitiallyDisabled ? TurretAIStates.Disabled : TurretAIStates.ScanningForTargets;
        }

        internal void Update(BehaviorUpdateContext context, BitArray<AutoAcquireEnemiesType> autoAcquireEnemiesWhenIdle)
        {
            var deltaTime = (float) context.Time.DeltaTime.TotalSeconds;

            var target = _gameObject.CurrentWeapon?.CurrentTarget;
            float targetYaw;

            if (_gameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving))
            {
                _turretAIstate = TurretAIStates.Recentering;
                _gameObject.CurrentWeapon?.SetTarget(null);
            }

            switch (_turretAIstate)
            {
                case TurretAIStates.Disabled:
                    break; // TODO: how does it get enabled?

                case TurretAIStates.Idle:
                    if (target != null)
                    {
                        _turretAIstate = TurretAIStates.Turning;
                        _currentTarget = target;
                    }
                    else if (context.Time.TotalTime > _waitUntil && (autoAcquireEnemiesWhenIdle?.Get(AutoAcquireEnemiesType.Yes) ?? true))
                    {
                        _turretAIstate = TurretAIStates.ScanningForTargets;
                    }
                    break;

                case TurretAIStates.ScanningForTargets:
                    if (target == null)
                    {
                        if (!FoundTargetWhileScanning(context, autoAcquireEnemiesWhenIdle))
                        {
                            var scanInterval =
                                context.GameContext.Random.NextDouble() *
                                (_moduleData.MaxIdleScanInterval - _moduleData.MinIdleScanInterval) +
                                _moduleData.MinIdleScanInterval;
                            _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(scanInterval);
                            _turretAIstate = TurretAIStates.Idle;
                            break;
                        }
                    }

                    if (!_moduleData.FiresWhileTurning)
                    {
                        _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Attacking, false);
                    }

                    _turretAIstate = TurretAIStates.Turning;
                    break;

                case TurretAIStates.Turning:
                    if (target == null)
                    {
                        _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.RecenterTime);
                        _turretAIstate = TurretAIStates.Recentering;
                        break;
                    }

                    var directionToTarget = (target.TargetPosition - _gameObject.Translation).Vector2XY();
                    targetYaw = MathUtility.GetYawFromDirection(directionToTarget) - _gameObject.Yaw;

                    if (Rotate(targetYaw, deltaTime))
                    {
                        break;
                    }

                    if (!_moduleData.FiresWhileTurning)
                    {
                        _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Attacking, true);
                    }

                    _turretAIstate = TurretAIStates.Attacking;
                    break;

                case TurretAIStates.Attacking:
                    if (target == null)
                    {
                        _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.RecenterTime);
                        _turretAIstate = TurretAIStates.Recentering;
                    }
                    else if (target != _currentTarget)
                    {
                        _turretAIstate = TurretAIStates.Turning;
                        _currentTarget = target;
                    }
                    break;

                case TurretAIStates.Recentering:
                    if (context.Time.TotalTime > _waitUntil)
                    {
                        targetYaw = MathUtility.ToRadians(_moduleData.NaturalTurretAngle) ;
                        if (!Rotate(targetYaw, deltaTime))
                        {
                            _turretAIstate = TurretAIStates.Idle;
                        }
                    }
                    break;
            }
        }

        private bool Rotate(float targetYaw, float deltaTime)
        {
            var deltaYaw = MathUtility.CalculateAngleDelta(targetYaw, _gameObject.TurretYaw);

            if (MathF.Abs(deltaYaw) > 0.15f)
            {
                _gameObject.TurretYaw -= MathF.Sign(deltaYaw) * deltaTime * MathUtility.ToRadians(_moduleData.TurretTurnRate);
                return true;
            }
            _gameObject.TurretYaw -= deltaYaw;
            return false;
        }

        private bool FoundTargetWhileScanning(BehaviorUpdateContext context, BitArray<AutoAcquireEnemiesType> autoAcquireEnemiesWhenIdle)
        {
            return false;

            //var attacksBuildings = autoAcquireEnemiesWhenIdle?.Get(AutoAcquireEnemiesType.AttackBuildings) ?? true;
            //var scanRange = _gameObject.CurrentWeapon.Template.AttackRange;

            //var restrictedByScanAngle = _moduleData.MinIdleScanAngle != 0 && _moduleData.MaxIdleScanAngle != 0;
            //var scanAngleOffset = context.GameContext.Random.NextDouble() *
            //                (_moduleData.MaxIdleScanAngle - _moduleData.MinIdleScanAngle) +
            //                _moduleData.MinIdleScanAngle;

            //var nearbyObjects = context.GameContext.Scene3D.Quadtree.FindNearby(_gameObject, _gameObject.Transform, scanRange);
            //foreach (var obj in nearbyObjects)
            //{
            //    if (obj.Definition.KindOf.Get(ObjectKinds.Structure) && !attacksBuildings)
            //    {
            //        continue;
            //    }

            //    if (restrictedByScanAngle)
            //    {
            //        // TODO: test with GLAVehicleTechnicalChassisOne
            //        var deltaTranslation = obj.Translation - _gameObject.Translation;
            //        var direction = deltaTranslation.Vector2XY();
            //        var angleToObject = MathUtility.GetYawFromDirection(direction);
            //        var angleDelta = MathUtility.CalculateAngleDelta(angleToObject, _gameObject.EulerAngles.Z + MathUtility.ToRadians(_moduleData.NaturalTurretAngle));

            //        if (angleDelta < -scanAngleOffset || scanAngleOffset < angleDelta)
            //        {
            //            continue;
            //        }
            //    }

            //    _gameObject.CurrentWeapon.SetTarget(new WeaponTarget(obj));
            //    return true;
            //}
            
            //return false;
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            var unknownBool1 = true;
            reader.PersistBoolean(ref unknownBool1);
            if (!unknownBool1)
            {
                throw new InvalidStateException();
            }

            // Angles maybe.
            reader.PersistSingle(ref _unknownFloat1);
            reader.PersistSingle(ref _unknownFloat2);

            reader.PersistFrame(ref _unknownFrame1);
            reader.PersistUInt32(ref _unknownInt1); // 0, 1
            reader.PersistFrame(ref _unknownFrame2);

            for (var i = 0; i < 7; i++)
            {
                reader.PersistBoolean(ref _unknownBools[i]);
            }

            reader.PersistFrame(ref _unknownFrame3);
        }
    }

    public sealed class TurretAIUpdateModuleData : UpdateModuleData
    {
        internal static TurretAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TurretAIUpdateModuleData> FieldParseTable = new IniParseTable<TurretAIUpdateModuleData>
        {
            { "InitiallyDisabled", (parser, x) => x.InitiallyDisabled = parser.ParseBoolean() },
            { "TurretTurnRate", (parser, x) => x.TurretTurnRate = parser.ParseInteger() },
            { "TurretPitchRate", (parser, x) => x.TurretPitchRate = parser.ParseInteger() },
            { "AllowsPitch", (parser, x) => x.AllowsPitch = parser.ParseBoolean() },
            { "FiresWhileTurning", (parser, x) => x.FiresWhileTurning = parser.ParseBoolean() },
            { "NaturalTurretPitch", (parser, x) => x.NaturalTurretPitch = parser.ParseInteger() },
            { "NaturalTurretAngle", (parser, x) => x.NaturalTurretAngle = parser.ParseInteger() },
            { "GroundUnitPitch", (parser, x) => x.GroundUnitPitch = parser.ParseInteger() },
            { "MinPhysicalPitch", (parser, x) => x.MinPhysicalPitch = parser.ParseInteger() },
            { "FirePitch", (parser, x) => x.FirePitch = parser.ParseInteger() },
            { "MinIdleScanAngle", (parser, x) => x.MinIdleScanAngle = parser.ParseInteger() },
            { "MaxIdleScanAngle", (parser, x) => x.MaxIdleScanAngle = parser.ParseInteger() },
            { "MinIdleScanInterval", (parser, x) => x.MinIdleScanInterval = parser.ParseInteger() },
            { "MaxIdleScanInterval", (parser, x) => x.MaxIdleScanInterval = parser.ParseInteger() },
            { "RecenterTime", (parser, x) => x.RecenterTime = parser.ParseInteger() },
            { "ControlledWeaponSlots", (parser, x) => x.ControlledWeaponSlots = parser.ParseEnumBitArray<WeaponSlot>() },

            { "TurretFireAngleSweep", (parser, x) => x.TurretFireAngleSweeps.Add(parser.ParseEnum<WeaponSlot>(), parser.ParseInteger()) },
            { "TurretSweepSpeedModifier", (parser, x) => x.TurretSweepSpeedModifiers.Add(parser.ParseEnum<WeaponSlot>(), parser.ParseFloat()) },

            { "TurretMaxDeflectionCW", (parser, x) => x.TurretMaxDeflectionCW = parser.ParseInteger() },
            { "TurretMaxDeflectionACW", (parser, x) => x.TurretMaxDeflectionACW = parser.ParseInteger() },
        };

        public bool InitiallyDisabled { get; private set; }

        /// <summary>
        /// Turn rate, in degrees per second.
        /// </summary>
        public int TurretTurnRate { get; private set; }

        public int TurretPitchRate { get; private set; }

        public bool AllowsPitch { get; private set; }

        public bool FiresWhileTurning { get; private set; }

        public int NaturalTurretPitch { get; private set; }

        public int NaturalTurretAngle { get; private set; }

        public int GroundUnitPitch { get; private set; }

        /// <summary>
        /// If allows pitch, the lowest I can dip down to shoot.defaults to 0 (horizontal)
        /// </summary>
        public int MinPhysicalPitch { get; private set; }

        /// <summary>
        /// Instead of aiming pitchwise at the target, it will aim here
        /// </summary>
        public int FirePitch { get; private set; }

        /// <summary>
        /// Minimum offset, in degrees, from <see cref="NaturalTurretAngle"/>.
        /// </summary>
        public int MinIdleScanAngle { get; private set; }

        /// <summary>
        /// Maximum offset, in degrees, from <see cref="NaturalTurretAngle"/>.
        /// </summary>
        public int MaxIdleScanAngle { get; private set; }

        public int MinIdleScanInterval { get; private set; }

        public int MaxIdleScanInterval { get; private set; }

        /// <summary>
        /// Time to wait when idling before recentering.
        /// </summary>
        public int RecenterTime { get; private set; }

        public BitArray<WeaponSlot> ControlledWeaponSlots { get; private set; }
        public Dictionary<WeaponSlot, int> TurretFireAngleSweeps { get; } = new Dictionary<WeaponSlot, int>();

        /// <summary>
        /// Sweep slower than you turn
        /// /// </summary>
        public Dictionary<WeaponSlot, float> TurretSweepSpeedModifiers { get; } = new Dictionary<WeaponSlot, float>();

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int TurretMaxDeflectionCW { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int TurretMaxDeflectionACW { get; private set; }

        internal TurretAIUpdate CreateTurretAIUpdate(GameObject gameObject)
        {
            return new TurretAIUpdate(gameObject, this);
        }
    }

    public sealed class TurretAITargetChooserData : BaseAITargetChooserData
    {

    }
}
