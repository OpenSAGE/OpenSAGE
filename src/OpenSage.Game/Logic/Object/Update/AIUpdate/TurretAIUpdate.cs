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

        internal TurretAIUpdate(GameObject gameObject, TurretAIUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;

            _gameObject.TurretYaw = MathUtility.ToRadians(_moduleData.NaturalTurretAngle);
            _gameObject.TurretPitch = MathUtility.ToRadians(_moduleData.NaturalTurretPitch);
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            var deltaTime = (float) context.Time.DeltaTime.TotalSeconds;

            var target = _gameObject.CurrentWeapon.CurrentTarget;

            if (target != null)
            {
                var directionToTarget = (target.TargetPosition - _gameObject.Transform.Translation).Vector2XY();
                var targetYaw = MathUtility.GetYawFromDirection(directionToTarget);

                var deltaYaw = MathUtility.CalculateAngleDelta(targetYaw, _gameObject.TurretYaw - _gameObject.Transform.EulerAngles.Z);

                if (MathF.Abs(deltaYaw) > 0.15f)
                {
                    if (!_moduleData.FiresWhileTurning)
                    {
                        _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Attacking, false);
                    }
                    _gameObject.TurretYaw -= MathF.Sign(deltaYaw) * deltaTime * MathUtility.ToRadians(_moduleData.TurretTurnRate);
                }
                else
                {
                    if (!_moduleData.FiresWhileTurning)
                    {
                        _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Attacking, true);
                    }
                    _gameObject.TurretYaw -= deltaYaw;
                }
            }

            if (_moduleData.AllowsPitch)
            {
                var pitch = MathUtility.ToRadians(_moduleData.NaturalTurretPitch);

                if (target != null)
                {
                    if (target.TargetType == WeaponTargetType.Object &&
                        !target.TargetObject.Definition.KindOf.Get(ObjectKinds.Aircraft)) // == ground unit??
                    {
                        pitch = MathUtility.ToRadians(_moduleData.GroundUnitPitch);
                    }
                }

                var deltaPitch = _gameObject.TurretPitch - pitch;
                if (MathF.Abs(deltaPitch) > 0.05f)
                {
                    _gameObject.TurretPitch += deltaTime * MathUtility.ToRadians(_moduleData.TurretPitchRate);
                }
            }
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

        public int MinPhysicalPitch { get; private set; }

        /// <summary>
        /// Instead of aiming pitchwise at the target, it will aim here
        /// /// </summary>
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
