using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Mathematics;
using FixedMath.NET;

namespace OpenSage.Logic.Object
{
    public class SlavedUpdateModule : UpdateModule
    {
        private readonly SlavedUpdateModuleData _moduleData;
        protected readonly GameObject _gameObject;
        public GameObject Master;

        private TimeSpan _waitUntil;
        private RepairStatus _repairStatus;

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

        internal SlavedUpdateModule(GameObject gameObject, SlavedUpdateModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            var masterIsMoving = Master.ModelConditionFlags.Get(ModelConditionFlag.Moving);
            var masterHealthPercent = Master.HealthPercentage;

            var offsetToMaster = Master.Translation - _gameObject.Translation;
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
                            _gameObject.AIUpdate.SetTargetPoint(Master.Translation);
                            _repairStatus = RepairStatus.GOING_TO_MASTER;
                        }

                        _gameObject.AIUpdate.SetLocomotor(LocomotorSetType.Panic);
                        break;
                    case RepairStatus.GOING_TO_MASTER:
                        if (!isMoving)
                        {
                            _repairStatus = RepairStatus.READY;
                            var readyDuration = (float) (context.GameContext.Random.NextDouble() * (_moduleData.RepairMaxReadyTime - _moduleData.RepairMinReadyTime) + _moduleData.RepairMinReadyTime);
                            _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(readyDuration);
                        }
                        break;
                    case RepairStatus.READY:
                        if (context.Time.TotalTime > _waitUntil)
                        {
                            var range = (float) (context.GameContext.Random.NextDouble() * _moduleData.RepairRange);
                            var height = (float) (context.GameContext.Random.NextDouble() * (_moduleData.RepairMaxAltitude - _moduleData.RepairMinAltitude) + _moduleData.RepairMinAltitude);
                            var angle = (float) (context.GameContext.Random.NextDouble() * (Math.PI * 2));

                            var offset = Vector3.Transform(new Vector3(range, 0.0f, height), Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle));
                            _gameObject.AIUpdate.SetTargetPoint(Master.Translation + offset);
                            _repairStatus = RepairStatus.IN_TRANSITION;
                        }
                        break;
                    case RepairStatus.IN_TRANSITION:
                        if (!isMoving)
                        {
                            var (modelInstance, bone) = _gameObject.Drawable.FindBone(_moduleData.RepairWeldingFXBone);
                            var transform = modelInstance.AbsoluteBoneTransforms[bone.Index];
                            _particleTemplate ??= context.GameContext.AssetLoadContext.AssetStore.FXParticleSystemTemplates.GetByName(_moduleData.RepairWeldingSys);

                            var particleSystem = context.GameContext.ParticleSystems.Create(
                                _particleTemplate,
                                transform);

                            particleSystem.Activate();

                            var weldDuration = (float) (context.GameContext.Random.NextDouble() * (_moduleData.RepairMaxWeldTime - _moduleData.RepairMinWeldTime) + _moduleData.RepairMinWeldTime);
                            _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(weldDuration);
                            _repairStatus = RepairStatus.WELDING;
                        }
                        break;
                    case RepairStatus.WELDING:
                        if (context.Time.TotalTime > _waitUntil)
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
                        Master.Health += (Fix64) (_moduleData.RepairRatePerSecond * context.Time.DeltaTime.TotalSeconds);
                        if (Master.Health > Master.MaxHealth)
                        {
                            Master.Health = Master.MaxHealth;
                            _repairStatus = RepairStatus.INITIAL;
                            _gameObject.AIUpdate.SetLocomotor(LocomotorSetType.Normal);
                        }
                        break;
                }
            }
            else if (_gameObject.ModelConditionFlags.Get(ModelConditionFlag.Attacking))
            {
                // stay near target
                var target = _gameObject.CurrentWeapon.CurrentTarget.TargetObject;

                if (target != null)
                {
                    var offsetToTarget = target.Translation - _gameObject.Translation;
                    var distanceToTarget = offsetToTarget.Length();

                    if (_gameObject.AIUpdate.TargetPoints.Count == 0 && distanceToTarget > _moduleData.AttackWanderRange)
                    {
                        _gameObject.AIUpdate.SetTargetPoint(Master.Translation);
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
                else if (Master.ModelConditionFlags.Get(ModelConditionFlag.Guarding))
                {
                    maxRange = _moduleData.GuardWanderRange;
                }
                else if (Master.ModelConditionFlags.Get(ModelConditionFlag.Attacking))
                {
                    maxRange = _moduleData.AttackRange;
                }

                if (_gameObject.AIUpdate.TargetPoints.Count == 0 && distanceToMaster > maxRange)
                {
                    _gameObject.AIUpdate.SetTargetPoint(Master.Translation);
                }
            }

            if (_moduleData.DieOnMastersDeath && Master.ModelConditionFlags.Get(ModelConditionFlag.Dying))
            {
                _gameObject.Die(DeathType.Exploded, context.Time);
            }
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
            { "RepairMinReadyTime", (parser, x) => x.RepairMinReadyTime = parser.ParseInteger() },
            { "RepairMaxReadyTime", (parser, x) => x.RepairMaxReadyTime = parser.ParseInteger() },
            { "RepairMinWeldTime", (parser, x) => x.RepairMinWeldTime = parser.ParseInteger() },
            { "RepairMaxWeldTime", (parser, x) => x.RepairMaxWeldTime = parser.ParseInteger() },
            { "RepairWeldingSys", (parser, x) => x.RepairWeldingSys = parser.ParseAssetReference() },
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
        public int RepairMinReadyTime { get; private set; }
        public int RepairMaxReadyTime { get; private set; }
        public int RepairMinWeldTime { get; private set; }
        public int RepairMaxWeldTime { get; private set; }
        public string RepairWeldingSys { get; private set; }
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
            return new SlavedUpdateModule(gameObject, this);
        }
    }
}
