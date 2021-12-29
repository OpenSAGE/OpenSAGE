using System;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.Logic.Object
{
    public sealed class MissileAIUpdate : AIUpdate
    {
        private readonly MissileAIUpdateModuleData _moduleData;

        private MissileState _state;
        private TimeSpan _nextStateChangeTime;

        private Vector3 _unknownPosition;
        private uint _stateMaybe;
        private uint _unknownFrame1;
        private uint _launcherObjectId;
        private uint _unknownObjectId;
        private bool _unknownBool1;
        private uint _unknownFrame2;
        private float _unknownFloat1;
        private WeaponTemplate _weaponTemplate;
        private FXParticleSystemTemplate _exhaustParticleSystemTemplate;
        private bool _unknownBool2;
        private Vector3 _currentPositionMaybe;
        private int _unknownInt1;
        private int _unknownInt2;

        internal FXList DetonationFX { get; set; }

        internal MissileAIUpdate(GameObject gameObject, MissileAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
            _moduleData = moduleData;

            _state = MissileState.Inactive;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            switch (_state)
            {
                case MissileState.Inactive:
                    _nextStateChangeTime = context.Time.TotalTime + _moduleData.IgnitionDelay;
                    _state = MissileState.WaitingForIgnition;
                    goto case MissileState.WaitingForIgnition;

                case MissileState.WaitingForIgnition:
                    if (context.Time.TotalTime > _nextStateChangeTime)
                    {
                        _moduleData.IgnitionFX?.Value?.Execute(
                            new FXListExecutionContext(
                                GameObject.Rotation,
                                GameObject.Translation,
                                context.GameContext));

                        if (_moduleData.DistanceToTravelBeforeTurning > 0)
                        {
                            var pointToReachBeforeTurning = context.GameObject.Translation
                                + Vector3.TransformNormal(Vector3.UnitX, context.GameObject.TransformMatrix) * _moduleData.DistanceToTravelBeforeTurning;
                            AddTargetPoint(pointToReachBeforeTurning);
                        }

                        // TODO: What to do if target doesn't exist anymore?
                        if (context.GameObject.CurrentWeapon.CurrentTarget != null)
                        {
                            AddTargetPoint(context.GameObject.CurrentWeapon.CurrentTarget.TargetPosition);
                        }

                        context.GameObject.Speed = _moduleData.InitialVelocity;

                        _state = MissileState.Moving;
                    }
                    break;

                case MissileState.Moving:
                    // TODO: TryToFollowTarget
                    BezierProjectileBehavior.CheckForHit(context, _moduleData.DetonateCallsKill, DetonationFX);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            base.Update(context);
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(4);

            base.Load(reader);

            reader.ReadVector3(ref _unknownPosition);

            _stateMaybe = reader.ReadUInt32();

            reader.ReadFrame(ref _unknownFrame1);

            var unknownInt1 = int.MaxValue;
            reader.ReadInt32(ref unknownInt1);
            if (unknownInt1 != int.MaxValue)
            {
                throw new InvalidStateException();
            }

            reader.ReadObjectID(ref _launcherObjectId);
            reader.ReadObjectID(ref _unknownObjectId);
            reader.ReadBoolean(ref _unknownBool1);
            reader.ReadFrame(ref _unknownFrame2);
            reader.ReadSingle(ref _unknownFloat1);

            var unknownFloat2 = 99999.0f;
            reader.ReadSingle(ref unknownFloat2);
            if (unknownFloat2 != 99999.0f)
            {
                throw new InvalidStateException();
            }

            var weaponTemplateName = _weaponTemplate?.Name;
            reader.ReadAsciiString(ref weaponTemplateName);
            _weaponTemplate = reader.AssetStore.WeaponTemplates.GetByName(weaponTemplateName);

            var exhaustParticleSystemTemplateName = _exhaustParticleSystemTemplate?.Name;
            reader.ReadAsciiString(ref exhaustParticleSystemTemplateName);
            _exhaustParticleSystemTemplate = reader.AssetStore.FXParticleSystemTemplates.GetByName(exhaustParticleSystemTemplateName);

            reader.ReadBoolean(ref _unknownBool2);
            reader.ReadVector3(ref _currentPositionMaybe);
            reader.ReadInt32(ref _unknownInt1); // 0, 0x20000
            reader.ReadInt32(ref _unknownInt2); // 1960
        }

        private enum MissileState
        {
            Inactive,
            WaitingForIgnition,
            Moving,
        }
    }

    public sealed class MissileAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static MissileAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<MissileAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable.Concat(new IniParseTable<MissileAIUpdateModuleData>
        {
            { "TryToFollowTarget", (parser, x) => x.TryToFollowTarget = parser.ParseBoolean() },
            { "FuelLifetime", (parser, x) => x.FuelLifetime = parser.ParseInteger() },
            { "DetonateOnNoFuel", (parser, x) => x.DetonateOnNoFuel = parser.ParseBoolean() },
            { "InitialVelocity", (parser, x) => x.InitialVelocity = parser.ParseInteger() },
            { "IgnitionDelay", (parser, x) => x.IgnitionDelay = parser.ParseTimeMilliseconds() },
            { "DistanceToTravelBeforeTurning", (parser, x) => x.DistanceToTravelBeforeTurning = parser.ParseInteger() },
            { "DistanceToTargetBeforeDiving", (parser, x) => x.DistanceToTargetBeforeDiving = parser.ParseInteger() },
            { "DistanceToTargetForLock", (parser, x) => x.DistanceToTargetForLock = parser.ParseInteger() },
            { "GarrisonHitKillRequiredKindOf", (parser, x) => x.GarrisonHitKillRequiredKindOf = parser.ParseEnum<ObjectKinds>() },
            { "GarrisonHitKillForbiddenKindOf", (parser, x) => x.GarrisonHitKillForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
            { "GarrisonHitKillCount", (parser, x) => x.GarrisonHitKillCount = parser.ParseInteger() },
            { "GarrisonHitKillFX", (parser, x) => x.GarrisonHitKillFX = parser.ParseFXListReference() },
            { "DetonateCallsKill", (parser, x) => x.DetonateCallsKill = parser.ParseBoolean() },
            { "IgnitionFX", (parser, x) => x.IgnitionFX = parser.ParseFXListReference() },
            { "KillSelfDelay", (parser, x) => x.KillSelfDelay = parser.ParseInteger() },
            { "DistanceScatterWhenJammed", (parser, x) => x.DistanceScatterWhenJammed = parser.ParseInteger() },
        });

        public bool TryToFollowTarget { get; private set; }
        public int FuelLifetime { get; private set; }
        public bool DetonateOnNoFuel { get; private set; }
        public int InitialVelocity { get; private set; }
        public TimeSpan IgnitionDelay { get; private set; }
        public int DistanceToTravelBeforeTurning { get; private set; }
        public int DistanceToTargetBeforeDiving { get; private set; }
        public int DistanceToTargetForLock { get; private set; }
        public ObjectKinds GarrisonHitKillRequiredKindOf { get; private set; }
        public ObjectKinds GarrisonHitKillForbiddenKindOf { get; private set; }
        public int GarrisonHitKillCount { get; private set; }
        public LazyAssetReference<FXList> GarrisonHitKillFX { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool DetonateCallsKill { get; private set; }
        public LazyAssetReference<FXList> IgnitionFX { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int KillSelfDelay { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int DistanceScatterWhenJammed { get; private set; }

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new MissileAIUpdate(gameObject, this);
        }
    }
}
