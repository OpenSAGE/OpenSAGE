using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Mathematics.FixedMath;

namespace OpenSage.Logic.Object
{
    public class SlavedUpdateModule : UpdateModule
    {
        private readonly SlavedUpdateModuleData _moduleData;
        protected readonly GameObject _gameObject;

        public GameObject Master;

        private bool _isWelding;

        private double _weldUntil;

        private FXParticleSystemTemplate _particleTemplate;

        internal SlavedUpdateModule(GameObject gameObject, SlavedUpdateModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
            _isWelding = false;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            var masterIsMoving = Master.ModelConditionFlags.Get(ModelConditionFlag.Moving);
            var masterHealthPercent = Master.Body.Health / Master.Body.MaxHealth;

            var offsetToMaster = Master.Transform.Translation - _gameObject.Transform.Translation;
            var distanceToMaster = offsetToMaster.Length();

            // repair master
            if (!masterIsMoving && true) //masterHealthPercent < (Fix64)_moduleData.RepairWhenBelowHealthPercent)
            {
                // TODO: what are 'RepairMinReadyTime' and 'RepairMaxReadyTime' for?
                var isMoving = _gameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);
                if (!isMoving)
                {
                    if (!_isWelding)
                    {
                        _isWelding = true;

                        // TODO: create FX from template
                        var (modelInstance, bone) = _gameObject.FindBone(_moduleData.RepairWeldingFXBone);
                        //var transform = modelInstance.AbsoluteBoneTransforms[bone.Index];
                        _particleTemplate ??= context.GameContext.AssetLoadContext.AssetStore.FXParticleSystemTemplates.GetByName(_moduleData.RepairWeldingSys);

                        //_moduleData.RepairWeldingSys.Value.Execute(new FXListExecutionContext(
                        //    Quaternion.Identity,
                        //    transform.Translation,
                        //    context.GameContext));

                        var weldDuration = (float) (context.GameContext.Random.NextDouble() * (_moduleData.RepairMaxWeldTime - _moduleData.RepairMinWeldTime) + _moduleData.RepairMinWeldTime);
                        _weldUntil = context.Time.TotalTime.TotalMilliseconds + weldDuration;
                    }

                    if (context.Time.TotalTime.TotalMilliseconds > _weldUntil)
                    {
                        _isWelding = false;

                        // zip around
                        var range = (float) (context.GameContext.Random.NextDouble() * _moduleData.RepairRange);
                        var height = (float) (context.GameContext.Random.NextDouble() * (_moduleData.RepairMaxAltitude - _moduleData.RepairMinAltitude) + _moduleData.RepairMinAltitude);
                        var angle = (float) (context.GameContext.Random.NextDouble() * (Math.PI * 2));

                        var offset = Vector3.Transform(new Vector3(range, 0.0f, height), Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle));
                        _gameObject.AIUpdate.SetTargetPoint(Master.Transform.Translation + offset);
                    }
                }

                Master.Body.Health += (Fix64)(_moduleData.RepairRatePerSecond * context.Time.DeltaTime.TotalSeconds);
            }
            else if (_gameObject.ModelConditionFlags.Get(ModelConditionFlag.Attacking))
            {
                // stay near target
                var target = _gameObject.CurrentWeapon.CurrentTarget.TargetObject;

                if (target != null)
                {
                    var offsetToTarget = target.Transform.Translation - _gameObject.Transform.Translation;
                    var distanceToTarget = offsetToTarget.Length();

                    if (_gameObject.AIUpdate.TargetPoints.Count == 0 && distanceToTarget > _moduleData.AttackWanderRange)
                    {
                        _gameObject.AIUpdate.SetTargetPoint(Master.Transform.Translation);
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
                    _gameObject.AIUpdate.SetTargetPoint(Master.Transform.Translation);
                }
            }

            // TODO
            //if (_moduleData.DieOnMastersDeath && Master.ModelConditionFlags.Get(ModelConditionFlag.Dying))
            //{
            //    _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Dying, true);
            //}
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
