using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Mathematics;
using FixedMath.NET;
using OpenSage.Content;

namespace OpenSage.Logic.Object
{
    public class SlavedUpdateModule : UpdateModule
    {
        private readonly SlavedUpdateModuleData _moduleData;
        private readonly GameObject _gameObject;
        private readonly GameContext _context;
        private GameObject _master;

        // todo: these two fields need to be fit into the persisted fields somehow
        private LogicFrame _waitUntil;
        private RepairStatus _repairStatus;

        private uint _parentObjectId;
        private Vector3 _nextRelativePosition; // next coordinates relative to the parent we should move to
        private int _unknownInt; // 1, 4, 5, 9, 13
        private int _unknownInt2; // 0, 3, 6
        private bool _unknownBool;

        private enum RepairStatus
        {
            INITIAL,
            GOING_TO_MASTER,
            READY,
            ZIP_AROUND,
            IN_TRANSITION,
            WELDING,
            DONE
        }

        private FXParticleSystemTemplate _particleTemplate;

        internal SlavedUpdateModule(GameObject gameObject, GameContext context, SlavedUpdateModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
            _context = context;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (_master == null)
            {
                // TODO: Should this ever be null?
                return;
            }

            var masterIsMoving = _master.ModelConditionFlags.Get(ModelConditionFlag.Moving);
            var masterHealthPercent = _master.HealthPercentage;

            var offsetToMaster = _master.Translation - _gameObject.Translation;
            var distanceToMaster = offsetToMaster.Vector2XY().Length();

            if (!masterIsMoving && (masterHealthPercent < (Fix64) (_moduleData.RepairWhenBelowHealthPercent / 100.0) || _repairStatus != RepairStatus.INITIAL))
            {
                // repair master
                var isMoving = _gameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);

                switch (_repairStatus)
                {
                    case RepairStatus.INITIAL:
                        // go to master
                        if (distanceToMaster > 1.0)
                        {
                            _gameObject.AIUpdate.SetTargetPoint(_master.Translation);
                            _repairStatus = RepairStatus.GOING_TO_MASTER;
                        }

                        _gameObject.AIUpdate.SetLocomotor(LocomotorSetType.Panic);
                        break;
                    case RepairStatus.GOING_TO_MASTER:
                        if (!isMoving)
                        {
                            _repairStatus = RepairStatus.READY;
                            var readyDuration = context.GameContext.GetRandomLogicFrameSpan(_moduleData.RepairMinReadyTime, _moduleData.RepairMaxReadyTime);
                            _waitUntil = context.LogicFrame + readyDuration;
                        }
                        break;
                    case RepairStatus.READY:
                        if (context.LogicFrame >= _waitUntil)
                        {
                            var range = (float) (context.GameContext.Random.NextDouble() * _moduleData.RepairRange);
                            var height = (float) (context.GameContext.Random.NextDouble() * (_moduleData.RepairMaxAltitude - _moduleData.RepairMinAltitude) + _moduleData.RepairMinAltitude);
                            var angle = (float) (context.GameContext.Random.NextDouble() * (Math.PI * 2));

                            var offset = Vector3.Transform(new Vector3(range, 0.0f, height), Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle));
                            _gameObject.AIUpdate.SetTargetPoint(_master.Translation + offset);
                            _repairStatus = RepairStatus.IN_TRANSITION;
                        }
                        break;
                    case RepairStatus.IN_TRANSITION:
                        if (!isMoving)
                        {
                            var (modelInstance, bone) = _gameObject.Drawable.FindBone(_moduleData.RepairWeldingFXBone);
                            var transform = modelInstance.AbsoluteBoneTransforms[bone.Index];
                            _particleTemplate ??= _moduleData.RepairWeldingSys.Value;

                            var particleSystem = context.GameContext.ParticleSystems.Create(
                                _particleTemplate,
                                transform);

                            particleSystem.Activate();

                            var weldDuration = context.GameContext.GetRandomLogicFrameSpan(_moduleData.RepairMinWeldTime, _moduleData.RepairMaxWeldTime);
                            _waitUntil = context.LogicFrame + weldDuration;
                            _repairStatus = RepairStatus.WELDING;
                        }
                        break;
                    case RepairStatus.WELDING:
                        if (context.LogicFrame >= _waitUntil)
                        {
                            _repairStatus = RepairStatus.READY;
                        }
                        break;
                }

                switch (_repairStatus)
                {
                    case RepairStatus.ZIP_AROUND:
                    case RepairStatus.IN_TRANSITION:
                    case RepairStatus.WELDING:
                        _master.Health += (Fix64) (_moduleData.RepairRatePerSecond / Game.LogicFramesPerSecond);
                        if (_master.Health > _master.MaxHealth)
                        {
                            _master.Health = _master.MaxHealth;
                            _repairStatus = RepairStatus.INITIAL;
                            _gameObject.AIUpdate.SetLocomotor(LocomotorSetType.Normal);
                        }
                        break;
                }
            }
            else if (_gameObject.ModelConditionFlags.Get(ModelConditionFlag.Attacking))
            {
                // stay near target
                var target = _gameObject.CurrentWeapon.CurrentTarget.GetTargetObject();

                if (target != null)
                {
                    var offsetToTarget = target.Translation - _gameObject.Translation;
                    var distanceToTarget = offsetToTarget.Length();

                    if (_gameObject.AIUpdate.TargetPoints.Count == 0 && distanceToTarget > _moduleData.AttackWanderRange)
                    {
                        _gameObject.AIUpdate.SetTargetPoint(_master.Translation);
                    }
                }
            }
            else
            {
                // stay near master
                var maxRange = _moduleData.GuardMaxRange;
                if (masterIsMoving)
                {
                    maxRange = _moduleData.ScoutRange;
                }
                else if (_master.ModelConditionFlags.Get(ModelConditionFlag.Guarding))
                {
                    maxRange = _moduleData.GuardWanderRange;
                }
                else if (_master.ModelConditionFlags.Get(ModelConditionFlag.Attacking))
                {
                    maxRange = _moduleData.AttackRange;
                }

                if (_gameObject.AIUpdate?.TargetPoints.Count == 0 && distanceToMaster > maxRange)
                {
                    _gameObject.AIUpdate.SetTargetPoint(_master.Translation);
                }
            }

            // prior to bfme2, die on master death seems to be the default?
            if (_master.IsDead && (_context.Game.SageGame is not SageGame.Bfme2 || _moduleData.DieOnMastersDeath))
            {
                _gameObject.Die(DeathType.Exploded);
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistObjectID(ref _parentObjectId);
            reader.PersistVector3(ref _nextRelativePosition);

            reader.PersistInt32(ref _unknownInt);
            reader.PersistInt32(ref _unknownInt2);

            reader.PersistBoolean(ref _unknownBool);

            reader.Game.GameLogic.GetObjectById(_parentObjectId);
        }

        public void SetMaster(GameObject gameObject)
        {
            _master = gameObject;
            _parentObjectId = gameObject.ID;
        }
    }

    public sealed class SlavedUpdateModuleData : UpdateModuleData
    {
        internal static SlavedUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SlavedUpdateModuleData> FieldParseTable = new IniParseTable<SlavedUpdateModuleData>
        {
            { "GuardMaxRange", (parser, x) => x.GuardMaxRange = parser.ParseFloat() },
            { "GuardWanderRange", (parser, x) => x.GuardWanderRange = parser.ParseFloat() },
            { "AttackRange", (parser, x) => x.AttackRange = parser.ParseInteger() },
            { "AttackWanderRange", (parser, x) => x.AttackWanderRange = parser.ParseInteger() },
            { "ScoutRange", (parser, x) => x.ScoutRange = parser.ParseInteger() },
            { "ScoutWanderRange", (parser, x) => x.ScoutWanderRange = parser.ParseInteger() },
            { "RepairRange", (parser, x) => x.RepairRange = parser.ParseInteger() },
            { "RepairMinAltitude", (parser, x) => x.RepairMinAltitude = parser.ParseFloat() },
            { "RepairMaxAltitude", (parser, x) => x.RepairMaxAltitude = parser.ParseFloat() },
            { "RepairRatePerSecond", (parser, x) => x.RepairRatePerSecond = parser.ParseFloat() },
            { "RepairWhenBelowHealth%", (parser, x) => x.RepairWhenBelowHealthPercent = parser.ParseInteger() },
            { "RepairMinReadyTime", (parser, x) => x.RepairMinReadyTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "RepairMaxReadyTime", (parser, x) => x.RepairMaxReadyTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "RepairMinWeldTime", (parser, x) => x.RepairMinWeldTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "RepairMaxWeldTime", (parser, x) => x.RepairMaxWeldTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "RepairWeldingSys", (parser, x) => x.RepairWeldingSys = parser.ParseFXParticleSystemTemplateReference() },
            { "RepairWeldingFXBone", (parser, x) => x.RepairWeldingFXBone = parser.ParseBoneName() },
            { "DistToTargetToGrantRangeBonus", (parser, x) => x.DistToTargetToGrantRangeBonus = parser.ParseInteger() },
            { "StayOnSameLayerAsMaster", (parser, x) => x.StayOnSameLayerAsMaster = parser.ParseBoolean() },
            { "LeashRange", (parser, x) => x.LeashRange = parser.ParseInteger() },
            { "UseSlaverAsControlForEvaObjectSightedEvents", (parser, x) => x.UseSlaverAsControlForEvaObjectSightedEvents = parser.ParseBoolean() },
            { "DieOnMastersDeath", (parser, x) => x.DieOnMastersDeath = parser.ParseBoolean() },
            { "MarkUnselectable", (parser, x) => x.MarkUnselectable = parser.ParseBoolean() },
            { "GuardPositionOffset", (parser, x) => x.GuardPositionOffset = parser.ParseVector3() },
            { "FadeOutRange", (parser, x) => x.FadeOutRange = parser.ParseInteger() },
            { "FadeTime", (parser, x) => x.FadeTime = parser.ParseInteger() }
        };

        // How far away from master I'm allowed when master is idle (doesn't wander)
        public float GuardMaxRange { get; private set; }
        // How far away I'm allowed to wander from master while guarding.
        public float GuardWanderRange { get; private set; }
        // How far away from master I'm allowed when master is attacking a target.
        public int AttackRange { get; private set; }
        // How far I'm allowed to wander from target.
        public int AttackWanderRange { get; private set; }
        // How far away from master I'm allowed when master is moving.
        public int ScoutRange { get; private set; }
        // How far I'm allowed to wander from scout point.
        public int ScoutWanderRange { get; private set; }
        // How far I can zip around while repair (only moves when he stops welding)
        public int RepairRange { get; private set; }
        // My minimum repair hover altitude.
        public float RepairMinAltitude { get; private set; }
        // My maximum repair hover altitude.
        public float RepairMaxAltitude { get; private set; }
        // How many health points can I repair per second.
        public float RepairRatePerSecond { get; private set; }
        // How low should my master's health be (in %) before I should prioritize repairing.
        public int RepairWhenBelowHealthPercent { get; private set; }
        public LogicFrameSpan RepairMinReadyTime { get; private set; }
        public LogicFrameSpan RepairMaxReadyTime { get; private set; }
        public LogicFrameSpan RepairMinWeldTime { get; private set; }
        public LogicFrameSpan RepairMaxWeldTime { get; private set; }
        public LazyAssetReference<FXParticleSystemTemplate> RepairWeldingSys { get; private set; }
        public string RepairWeldingFXBone { get; private set; }
        // How close I have to be to the master's target in order to grant master a range bonus.
        public int DistToTargetToGrantRangeBonus { get; private set; }
        public bool StayOnSameLayerAsMaster { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int LeashRange { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseSlaverAsControlForEvaObjectSightedEvents { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool DieOnMastersDeath { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool MarkUnselectable { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Vector3 GuardPositionOffset { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int FadeOutRange { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int FadeTime { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new SlavedUpdateModule(gameObject, context, this);
        }
    }
}
