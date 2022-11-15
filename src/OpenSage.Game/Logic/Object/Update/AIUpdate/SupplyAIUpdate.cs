using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class SupplyAIUpdate : AIUpdate
    {
        public GameObject CurrentSupplyTarget;
        private SupplyCenterDockUpdate _currentTargetDockUpdate;
        public SupplyGatherStates SupplyGatherState;
        protected SupplyGatherStates SupplyGatherStateToResume;

        public enum SupplyGatherStates
        {
            Default,
            SearchingForSupplySource,
            ApproachingSupplySource,
            RequestingSupplies,
            GatheringSupplies,
            PickingUpSupplies,
            SearchingForSupplyTarget,
            ApproachingSupplyTarget,
            EnqueuedAtSupplyTarget,
            StartDumpingSupplies,
            DumpingSupplies,
            FinishedDumpingSupplies
        }

        private SupplyAIUpdateModuleData _moduleData;

        public GameObject CurrentSupplySource { get; set; }
        private SupplyWarehouseDockUpdate _currentSourceDockUpdate;
        protected LogicFrame _waitUntil;
        protected int _numBoxes;

        public int SupplyWarehouseScanDistance => _moduleData.SupplyWarehouseScanDistance;

        protected virtual int GetAdditionalValuePerSupplyBox(ScopedAssetCollection<UpgradeTemplate> upgrades) => 0;

        internal SupplyAIUpdate(GameObject gameObject, SupplyAIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
            SupplyGatherState = SupplyGatherStates.Default;
            SupplyGatherStateToResume = SupplyGatherStates.SearchingForSupplySource;
            _numBoxes = 0;
        }

        internal override void SetTargetPoint(Vector3 targetPoint)
        {
            SupplyGatherStateToResume = SupplyGatherState;
            SupplyGatherState = SupplyGatherStates.Default;
            ClearConditionFlags();
            base.SetTargetPoint(targetPoint);
        }

        internal virtual void ClearConditionFlags()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, false);
        }

        internal virtual float GetHarvestActivationRange() => 0.0f;
        internal virtual LogicFrameSpan GetPreparationTime() => LogicFrameSpan.Zero;

        internal virtual bool SupplySourceHasBoxes(BehaviorUpdateContext context, SupplyWarehouseDockUpdate dockUpdate, GameObject supplySource)
        {
            return dockUpdate?.HasBoxes() ?? false;
        }

        internal virtual void GetBox(BehaviorUpdateContext context)
        {
            _currentSourceDockUpdate?.GetBox();
        }

        internal virtual void SetGatheringConditionFlags()
        {

        }

        internal virtual LogicFrameSpan GetPickingUpTime() => LogicFrameSpan.Zero;

        internal virtual void SetActionConditionFlags()
        {

        }

        internal virtual void ClearActionConditionFlags()
        {

        }

        internal virtual GameObject FindClosestSupplyWarehouse(BehaviorUpdateContext context)
        {
            return context.GameObject.Owner.SupplyManager.FindClosestSupplyWarehouse(context.GameObject);
        }

        private static GameObject FindClosestSupplyCenter(BehaviorUpdateContext context)
        {
            return context.GameObject.Owner.SupplyManager.FindClosestSupplyCenter(context.GameObject);
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);

            var isMoving = GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);

            switch (SupplyGatherState)
            {
                case SupplyGatherStates.SearchingForSupplySource:
                    if (isMoving)
                    {
                        break;
                    }

                    if (CurrentSupplySource == null
                        || (_currentSourceDockUpdate != null && !_currentSourceDockUpdate.HasBoxes()))
                    {
                        CurrentSupplySource = FindClosestSupplyWarehouse(context);
                    }

                    if (CurrentSupplySource == null)
                    {
                        break;
                    }

                    _currentSourceDockUpdate = CurrentSupplySource.FindBehavior<SupplyWarehouseDockUpdate>();

                    var direction = Vector3.Normalize(CurrentSupplySource.Translation - GameObject.Translation);

                    SetTargetPoint(CurrentSupplySource.Translation - direction * GetHarvestActivationRange());
                    SupplyGatherState = SupplyGatherStates.ApproachingSupplySource;
                    break;

                case SupplyGatherStates.ApproachingSupplySource:
                    if (!isMoving)
                    {
                        SupplyGatherState = SupplyGatherStates.RequestingSupplies;
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, true);
                    }
                    break;

                case SupplyGatherStates.RequestingSupplies:
                    var boxesAvailable = SupplySourceHasBoxes(context, _currentSourceDockUpdate, CurrentSupplySource);

                    if (!boxesAvailable)
                    {
                        CurrentSupplySource = null;
                        if (_numBoxes == 0)
                        {
                            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, false);
                            SupplyGatherState = SupplyGatherStates.SearchingForSupplySource;
                            break;
                        }
                    }
                    else if (_numBoxes < _moduleData.MaxBoxes)
                    {
                        GetBox(context);
                        var waitTime = _moduleData.SupplyWarehouseActionDelay + GetPreparationTime();
                        _waitUntil = context.LogicFrame + waitTime;
                        SupplyGatherState = SupplyGatherStates.GatheringSupplies;
                        SetGatheringConditionFlags();
                        break;
                    }

                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, false);
                    SetActionConditionFlags();
                    _waitUntil = context.LogicFrame + GetPickingUpTime();
                    SupplyGatherState = SupplyGatherStates.PickingUpSupplies;
                    break;

                case SupplyGatherStates.GatheringSupplies:
                    if (context.LogicFrame >= _waitUntil)
                    {
                        _numBoxes++;
                        GameObject.Supply = _numBoxes;
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Carrying, true);
                        SupplyGatherState = SupplyGatherStates.RequestingSupplies;
                    }
                    break;

                case SupplyGatherStates.PickingUpSupplies:
                    if (context.LogicFrame >= _waitUntil)
                    {
                        SupplyGatherState = SupplyGatherStates.SearchingForSupplyTarget;
                        ClearActionConditionFlags();
                    }
                    break;

                case SupplyGatherStates.SearchingForSupplyTarget:
                    if (CurrentSupplyTarget == null)
                    {
                        CurrentSupplyTarget = FindClosestSupplyCenter(context);
                    }

                    if (CurrentSupplyTarget == null)
                    {
                        break;
                    }

                    _currentTargetDockUpdate = CurrentSupplyTarget.FindBehavior<SupplyCenterDockUpdate>();

                    if (!_currentTargetDockUpdate.CanApproach())
                    {
                        break;
                    }

                    SetTargetPoint(_currentTargetDockUpdate.GetApproachTargetPosition(this));
                    SupplyGatherState = SupplyGatherStates.ApproachingSupplyTarget;
                    break;

                case SupplyGatherStates.ApproachingSupplyTarget:
                    if (!isMoving)
                    {
                        SupplyGatherState = SupplyGatherStates.EnqueuedAtSupplyTarget;
                    }
                    break;

                case SupplyGatherStates.EnqueuedAtSupplyTarget:
                    // wait until the DockUpdate moves us forward
                    break;

                case SupplyGatherStates.StartDumpingSupplies:
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, true);
                    SupplyGatherState = SupplyGatherStates.DumpingSupplies;
                    // todo: this might not be entirely accurate since partial loads can be deposited if unloading is manually aborted early
                    _waitUntil = context.LogicFrame + _moduleData.SupplyCenterActionDelay * _numBoxes;
                    break;

                case SupplyGatherStates.DumpingSupplies:
                    if (context.LogicFrame >= _waitUntil)
                    {
                        SupplyGatherState = SupplyGatherStates.FinishedDumpingSupplies;

                        var assetStore = context.GameContext.AssetLoadContext.AssetStore;
                        var bonusAmountPerBox = GetAdditionalValuePerSupplyBox(assetStore.Upgrades);
                        _currentTargetDockUpdate.DumpBoxes(assetStore, ref _numBoxes, bonusAmountPerBox);

                        GameObject.Supply = _numBoxes;
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, false);
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Carrying, false);
                    }
                    break;

                case SupplyGatherStates.FinishedDumpingSupplies:
                    break;
            }
        }
    }

    /// <summary>
    /// Requires the object to have KindOf = HARVESTER.
    /// </summary>
    public abstract class SupplyAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static readonly IniParseTable<SupplyAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<SupplyAIUpdateModuleData>
            {
                { "MaxBoxes", (parser, x) => x.MaxBoxes = parser.ParseInteger() },
                { "SupplyCenterActionDelay", (parser, x) => x.SupplyCenterActionDelay = parser.ParseTimeMillisecondsToLogicFrames() },
                { "SupplyWarehouseActionDelay", (parser, x) => x.SupplyWarehouseActionDelay = parser.ParseTimeMillisecondsToLogicFrames() },
                { "SupplyWarehouseScanDistance", (parser, x) => x.SupplyWarehouseScanDistance = parser.ParseInteger() },
                { "SuppliesDepletedVoice", (parser, x) => x.SuppliesDepletedVoice = parser.ParseAssetReference() }
            });

        public int MaxBoxes { get; private set; }
        // ms for whole thing (one transaction)
        public LogicFrameSpan SupplyCenterActionDelay { get; private set; }
        // ms per box (many small transactions)
        public LogicFrameSpan SupplyWarehouseActionDelay { get; private set; }
        // Max distance to look for a warehouse, or we go home.  (Direct dock command on warehouse overrides, and no max on Center Scan)
        public int SupplyWarehouseScanDistance { get; private set; }
        public string SuppliesDepletedVoice { get; private set; }
    }
}
