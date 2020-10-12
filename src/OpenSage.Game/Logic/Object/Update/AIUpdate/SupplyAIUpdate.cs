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

        protected GameObject _currentSupplySource;
        private SupplyWarehouseDockUpdate _currentSourceDockUpdate;
        protected TimeSpan _waitUntil;
        protected int _numBoxes;

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

        internal virtual List<GameObject> GetNearbySupplySources(BehaviorUpdateContext context)
        {
            var nearbyObjects = context.GameContext.Scene3D.Quadtree.FindNearby(GameObject, GameObject.Transform, _moduleData.SupplyWarehouseScanDistance);
            // TODO: also use KindOf SUPPLY_SOURCE_ON_PREVIEW ?
            return nearbyObjects.Where(x => x.Definition.KindOf.Get(ObjectKinds.SupplySource)).ToList();
        }

        internal virtual void ClearConditionFlags()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, false);
        }

        internal virtual void FindNearbySupplySource(BehaviorUpdateContext context)
        {
            var supplySources = GetNearbySupplySources(context);

            var distanceToCurrentSupplySource = float.PositiveInfinity;
            foreach (var supplySource in supplySources)
            {
                var offsetToSource = supplySource.Translation - GameObject.Translation;
                var distanceToSource = offsetToSource.Vector2XY().Length();

                if (distanceToSource > distanceToCurrentSupplySource)
                {
                    continue;
                }

                var dockUpdate = supplySource.FindBehavior<SupplyWarehouseDockUpdate>() ?? null;

                if (!SupplySourceHasBoxes(context, dockUpdate, supplySource))
                {
                    continue;
                }

                _currentSupplySource = supplySource;
                _currentSourceDockUpdate = dockUpdate;
                distanceToCurrentSupplySource = distanceToSource;
            }
        }

        internal virtual float GetHarvestActivationRange() => 0.0f;
        internal virtual float GetPreparationTime() => 0.0f;

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

        internal virtual float GetPickingUpTime() => 0.0f;

        internal virtual void SetActionConditionFlags()
        {

        }

        internal virtual void ClearActionConditionFlags()
        {

        }

        internal virtual List<GameObject> GetNearbySupplyCenters(BehaviorUpdateContext context)
        {
            var nearbyObjects = context.GameContext.Scene3D.Quadtree.FindNearby(GameObject, GameObject.Transform, _moduleData.SupplyWarehouseScanDistance);
            return nearbyObjects.Where(x => x.Definition.KindOf.Get(ObjectKinds.CashGenerator)).ToList();
        }

        internal virtual void FindNearbySupplyTarget(BehaviorUpdateContext context)
        {
            var supplyTargets = GetNearbySupplyCenters(context);

            var distanceToCurrentSupplyTarget = float.PositiveInfinity;
            foreach (var supplyTarget in supplyTargets)
            {
                if (supplyTarget.Owner != GameObject.Owner)
                {
                    continue;
                }

                var offsetToTarget = supplyTarget.Translation - GameObject.Translation;
                var distanceToTarget = offsetToTarget.Vector2XY().Length();

                if (distanceToTarget > distanceToCurrentSupplyTarget)
                {
                    continue;
                }

                var dockUpdate = supplyTarget.FindBehavior<SupplyCenterDockUpdate>() ?? null;
                if (!dockUpdate?.CanApproach() ?? false)
                {
                    continue;
                }

                CurrentSupplyTarget = supplyTarget;
                _currentTargetDockUpdate = dockUpdate;
                distanceToCurrentSupplyTarget = distanceToTarget;
            }
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

                    if (_currentSupplySource == null
                        || (_currentSourceDockUpdate != null && !_currentSourceDockUpdate.HasBoxes()))
                    {
                        FindNearbySupplySource(context);
                    }

                    if (_currentSupplySource == null)
                    {
                        break;
                    }

                    var direction = Vector3.Normalize(_currentSupplySource.Translation - GameObject.Translation);

                    SetTargetPoint(_currentSupplySource.Translation - direction * GetHarvestActivationRange());
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
                    var boxesAvailable = SupplySourceHasBoxes(context, _currentSourceDockUpdate, _currentSupplySource);

                    if (!boxesAvailable)
                    {
                        _currentSupplySource = null;
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
                        _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(waitTime);
                        SupplyGatherState = SupplyGatherStates.GatheringSupplies;
                        SetGatheringConditionFlags();
                        break;
                    }

                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, false);
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Carrying, true);
                    SetActionConditionFlags();
                    _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(GetPickingUpTime());
                    SupplyGatherState = SupplyGatherStates.PickingUpSupplies;
                    break;

                case SupplyGatherStates.GatheringSupplies:
                    if (context.Time.TotalTime > _waitUntil)
                    {
                        _numBoxes++;
                        SupplyGatherState = SupplyGatherStates.RequestingSupplies;
                    }
                    break;

                case SupplyGatherStates.PickingUpSupplies:
                    if (context.Time.TotalTime > _waitUntil)
                    {
                        SupplyGatherState = SupplyGatherStates.SearchingForSupplyTarget;
                        ClearActionConditionFlags();
                    }
                    break;

                case SupplyGatherStates.SearchingForSupplyTarget:
                    if (CurrentSupplyTarget == null)
                    {
                        FindNearbySupplyTarget(context);
                    }

                    if (CurrentSupplyTarget == null)
                    {
                        break;
                    }

                    _currentTargetDockUpdate ??= CurrentSupplyTarget.FindBehavior<SupplyCenterDockUpdate>();

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
                    _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.SupplyCenterActionDelay);
                    break;

                case SupplyGatherStates.DumpingSupplies:
                    if (context.Time.TotalTime > _waitUntil)
                    {
                        SupplyGatherState = SupplyGatherStates.FinishedDumpingSupplies;

                        var assetStore = context.GameContext.AssetLoadContext.AssetStore;
                        var bonusAmountPerBox = GetAdditionalValuePerSupplyBox(assetStore.Upgrades);
                        _currentTargetDockUpdate.DumpBoxes(assetStore, ref _numBoxes, bonusAmountPerBox);

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
        internal static new readonly IniParseTable<SupplyAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<SupplyAIUpdateModuleData>
            {
                { "MaxBoxes", (parser, x) => x.MaxBoxes = parser.ParseInteger() },
                { "SupplyCenterActionDelay", (parser, x) => x.SupplyCenterActionDelay = parser.ParseInteger() },
                { "SupplyWarehouseActionDelay", (parser, x) => x.SupplyWarehouseActionDelay = parser.ParseInteger() },
                { "SupplyWarehouseScanDistance", (parser, x) => x.SupplyWarehouseScanDistance = parser.ParseInteger() },
                { "SuppliesDepletedVoice", (parser, x) => x.SuppliesDepletedVoice = parser.ParseAssetReference() }
            });

        public int MaxBoxes { get; private set; }
        // ms for whole thing (one transaction)
        public int SupplyCenterActionDelay { get; private set; }
        // ms per box (many small transactions)
        public int SupplyWarehouseActionDelay { get; private set; }
        // Max distance to look for a warehouse, or we go home.  (Direct dock command on warehouse overrides, and no max on Center Scan)
        public int SupplyWarehouseScanDistance { get; private set; }
        public string SuppliesDepletedVoice { get; private set; }
    }
}
