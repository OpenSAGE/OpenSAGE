using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Logic.AI;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class WorkerAIUpdate : SupplyAIUpdate, IBuilderAIUpdate
    {
        private WorkerAIUpdateModuleData _moduleData;
        private GameObject _buildTarget;

        private readonly DozerAndWorkerState _state = new();

        private readonly WorkerAIUpdateStateMachine2 _stateMachine2 = new();
        private uint _unknownObjectId;
        private int _unknown5;
        private readonly WorkerAIUpdateStateMachine3 _stateMachine3 = new();

        internal WorkerAIUpdate(GameObject gameObject, WorkerAIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        #region Dozer stuff

        public void SetBuildTarget(GameObject gameObject)
        {
            // note that the order here is important, as SetTargetPoint will clear any existing buildTarget
            // TODO: target should not be directly on the building, but rather a point along the foundation perimeter
            SetTargetPoint(gameObject.Translation);
            CurrentSupplyTarget = null;
            SupplyGatherState = SupplyGatherStates.Default;
            _buildTarget = gameObject;
        }

        internal override void SetTargetPoint(Vector3 targetPoint)
        {
            base.SetTargetPoint(targetPoint);
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, false);
            _buildTarget?.PauseConstruction();
            ClearBuildTarget();
        }

        protected override void ArrivedAtDestination()
        {
            base.ArrivedAtDestination();

            if (_buildTarget is not null)
            {
                _buildTarget.Construct();
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, true);
            }
        }

        private void ClearBuildTarget()
        {
            _buildTarget = null;
        }

        #endregion

        internal override void ClearConditionFlags()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.HarvestPreparation, false);
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.HarvestAction, false);
            base.ClearConditionFlags();
        }

        protected override int GetAdditionalValuePerSupplyBox(ScopedAssetCollection<UpgradeTemplate> upgrades)
        {
            // this is also hardcoded in original SAGE, replaced by BonusScience and BonusScienceMultiplier (SupplyCenterDockUpdate) in later games
            var upgradeDefinition = upgrades.GetByName("Upgrade_GLAWorkerShoes");
            return GameObject.HasUpgrade(upgradeDefinition) ? _moduleData.UpgradedSupplyBoost : 0;
        }

        internal override GameObject FindClosestSupplyWarehouse(BehaviorUpdateContext context)
        {
            if (_moduleData.HarvestTrees)
            {
                var nearbyTrees = context.GameContext.Game.PartitionCellManager.QueryObjects(
                    context.GameObject,
                    context.GameObject.Translation,
                    _moduleData.SupplyWarehouseScanDistance,
                    new PartitionQueries.KindOfQuery(ObjectKinds.Tree));

                GameObject closestTree = null;
                var closestDistance = float.MaxValue;

                foreach (var tree in nearbyTrees)
                {
                    if (!tree.Definition.IsHarvestable)
                    {
                        continue;
                    }

                    if (tree.Supply <= 0)
                    {
                        continue;
                    }

                    var distance = context.GameContext.Game.PartitionCellManager.GetDistanceBetweenObjectsSquared(context.GameObject, tree);

                    if (distance < closestDistance)
                    {
                        closestTree = tree;
                        closestDistance = distance;
                    }
                }

                return closestTree;
            }
            else
            {
                return base.FindClosestSupplyWarehouse(context);
            }
        }

        internal override float GetHarvestActivationRange() => _moduleData.HarvestActivationRange;
        internal override LogicFrameSpan GetPreparationTime() => _moduleData.HarvestPreparationTime;

        internal override bool SupplySourceHasBoxes(BehaviorUpdateContext context, SupplyWarehouseDockUpdate dockUpdate, GameObject supplySource)
        {
            if (_moduleData.HarvestTrees && supplySource.Definition.KindOf.Get(ObjectKinds.Tree))
            {
                return supplySource.Supply > 0;
            }
            return base.SupplySourceHasBoxes(context, dockUpdate, supplySource);
        }

        internal override void GetBox(BehaviorUpdateContext context)
        {
            if (_moduleData.HarvestTrees && _currentSupplySource.Definition.KindOf.Get(ObjectKinds.Tree))
            {
                _currentSupplySource.Supply -= context.GameContext.AssetLoadContext.AssetStore.GameData.Current.ValuePerSupplyBox;
                if (_currentSupplySource.Supply <= 0)
                {
                    _currentSupplySource.Update();
                    _currentSupplySource = null;
                }
                return;
            }
            base.GetBox(context);
        }

        internal override void SetGatheringConditionFlags()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.HarvestPreparation, true);
        }

        internal override LogicFrameSpan GetPickingUpTime() => _moduleData.HarvestActionTime;

        internal override void SetActionConditionFlags()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.HarvestPreparation, false);
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.HarvestAction, true);
        }

        internal override void ClearActionConditionFlags()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.HarvestAction, false);
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);

            if (_buildTarget != null && _buildTarget.BuildProgress >= 1)
            {
                ClearBuildTarget();
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, false);
            }
            else
            {
                var isMoving = GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);

                switch (SupplyGatherState)
                {
                    case SupplyGatherStates.Default:
                        if (!isMoving)
                        {
                            SupplyGatherState = SupplyGatherStateToResume;
                            break;
                        }
                        _waitUntil = context.LogicFrame + _moduleData.BoredTime;
                        break;
                }
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            _state.Persist(reader);

            reader.PersistObject(_stateMachine2);
            reader.PersistObjectID(ref _unknownObjectId);
            reader.PersistInt32(ref _unknown5);

            reader.SkipUnknownBytes(1);

            reader.PersistObject(_stateMachine3);
        }

        private sealed class WorkerAIUpdateStateMachine3 : StateMachineBase
        {
            public WorkerAIUpdateStateMachine3()
            {
                AddState(0, new WorkerUnknown0State());
                AddState(1, new WorkerUnknown1State());
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.BeginObject("Base");
                base.Persist(reader);
                reader.EndObject();
            }

            private sealed class WorkerUnknown0State : State
            {
                public override void Persist(StatePersister reader)
                {
                    // No version?
                }
            }

            private sealed class WorkerUnknown1State : State
            {
                public override void Persist(StatePersister reader)
                {
                    // No version?
                }
            }
        }
    }

    internal sealed class WorkerAIUpdateStateMachine1 : StateMachineBase
    {
        public WorkerAIUpdateStateMachine1()
        {
            AddState(0, new WorkerUnknown0State());
            AddState(1, new WorkerUnknown1State());
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();
        }

        private sealed class WorkerUnknown0State : State
        {
            private int _unknown1;
            private int _unknown2;
            private bool _unknown3;

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistInt32(ref _unknown1);
                reader.PersistInt32(ref _unknown2);
                reader.PersistBoolean(ref _unknown3);
            }
        }

        private sealed class WorkerUnknown1State : State
        {
            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.SkipUnknownBytes(4);

                var unknown2 = 1;
                reader.PersistInt32(ref unknown2);
                if (unknown2 != 1)
                {
                    throw new InvalidStateException();
                }

                reader.SkipUnknownBytes(1);
            }
        }
    }

    internal sealed class WorkerAIUpdateStateMachine2 : StateMachineBase
    {
        public WorkerAIUpdateStateMachine2()
        {
            AddState(0, new WorkerUnknown0State());
            AddState(1, new WorkerUnknown1State());
            AddState(4, new WorkerUnknown4State());
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();
        }

        private sealed class WorkerUnknown0State : State
        {
            public override void Persist(StatePersister reader)
            {

            }
        }

        private sealed class WorkerUnknown1State : State
        {
            public override void Persist(StatePersister reader)
            {

            }
        }

        private sealed class WorkerUnknown4State : State
        {
            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);
            }
        }
    }

    /// <summary>
    /// Allows the use of VoiceRepair, VoiceBuildResponse, VoiceSupply, VoiceNoBuild, and
    /// VoiceTaskComplete within UnitSpecificSounds section of the object.
    /// Requires Kindof = DOZER.
    /// </summary>
    public sealed class WorkerAIUpdateModuleData : SupplyAIUpdateModuleData
    {
        internal new static WorkerAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<WorkerAIUpdateModuleData> FieldParseTable = SupplyAIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<WorkerAIUpdateModuleData>
            {
                { "RepairHealthPercentPerSecond", (parser, x) => x.RepairHealthPercentPerSecond = parser.ParsePercentage() },
                { "BoredTime", (parser, x) => x.BoredTime = parser.ParseTimeMillisecondsToLogicFrames() },
                { "BoredRange", (parser, x) => x.BoredRange = parser.ParseInteger() },
                { "UpgradedSupplyBoost", (parser, x) => x.UpgradedSupplyBoost = parser.ParseInteger() },
                { "HarvestTrees", (parser, x) => x.HarvestTrees = parser.ParseBoolean() },
                { "HarvestActivationRange", (parser, x) => x.HarvestActivationRange = parser.ParseInteger() },
                { "HarvestPreparationTime", (parser, x) => x.HarvestPreparationTime = parser.ParseTimeMillisecondsToLogicFrames() },
                { "HarvestActionTime", (parser, x) => x.HarvestActionTime = parser.ParseTimeMillisecondsToLogicFrames() },
            });

        public Percentage RepairHealthPercentPerSecond { get; private set; }
        public LogicFrameSpan BoredTime { get; private set; }
        public int BoredRange { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int UpgradedSupplyBoost { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HarvestTrees { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int HarvestActivationRange { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LogicFrameSpan HarvestPreparationTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LogicFrameSpan HarvestActionTime { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new WorkerAIUpdate(gameObject, this);
        }
    }
}
