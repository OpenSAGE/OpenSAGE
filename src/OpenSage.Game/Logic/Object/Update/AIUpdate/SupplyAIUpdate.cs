using System;
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

        public enum SupplyGatherStates
        {
            Default,
            SearchForSupplySource,
            ApproachSupplySource,
            RequestSupplies,
            GatheringSupplies,
            SearchForSupplyTarget,
            ApproachSupplyTarget,
            EnqueuedAtSupplyTarget,
            StartDumpingSupplies,
            DumpingSupplies,
            FinishedDumpingSupplies
        }

        private SupplyAIUpdateModuleData _moduleData;

        private GameObject _currentSupplySource;
        private SupplyWarehouseDockUpdate _currentSourceDockUpdate;
        private TimeSpan _waitUntil;
        private int _numBoxes;

        protected virtual int GetAdditionalValuePerSupplyBox(ScopedAssetCollection<UpgradeTemplate> upgrades) => 0;

        internal SupplyAIUpdate(GameObject gameObject, SupplyAIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
            SupplyGatherState = SupplyGatherStates.Default;
            _numBoxes = 0;
        }

        internal override void SetTargetPoint(Vector3 targetPoint)
        {
            SupplyGatherState = SupplyGatherStates.Default;
            base.SetTargetPoint(targetPoint);
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);

            var isMoving = GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);

            switch (SupplyGatherState)
            {
                case SupplyGatherStates.SearchForSupplySource:
                    if (isMoving) break;

                    if (_currentSupplySource == null || !_currentSourceDockUpdate.HasBoxes())
                    {
                        // TODO: also use KindOf SUPPLY_SOURCE_ON_PREVIEW ?
                        var supplySources = context.GameContext.GameObjects.GetObjectsByKindOf(ObjectKinds.SupplySource);

                        var distanceToCurrentSupplySource = float.PositiveInfinity;
                        foreach (var supplySource in supplySources)
                        {
                            var offsetToSource = supplySource.Transform.Translation - GameObject.Transform.Translation;
                            var distanceToSource = offsetToSource.Vector2XY().Length();

                            if (distanceToSource > _moduleData.SupplyWarehouseScanDistance || distanceToSource > distanceToCurrentSupplySource) continue;

                            var dockUpdate = supplySource.FindBehavior<SupplyWarehouseDockUpdate>() ?? null;

                            if (!dockUpdate?.HasBoxes() ?? false) continue;

                            _currentSupplySource = supplySource;
                            _currentSourceDockUpdate = dockUpdate;
                            distanceToCurrentSupplySource = distanceToSource;
                        }
                    }

                    if (_currentSupplySource == null) break;

                    SetTargetPoint(_currentSupplySource.Transform.Translation);
                    SupplyGatherState = SupplyGatherStates.ApproachSupplySource;
                    break;
                case SupplyGatherStates.ApproachSupplySource:
                    if (!isMoving)
                    {
                        SupplyGatherState = SupplyGatherStates.RequestSupplies;
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, true);
                    }
                    break;
                case SupplyGatherStates.RequestSupplies:
                    var boxesAvailable = _currentSourceDockUpdate?.HasBoxes() ?? false;

                    if (!boxesAvailable)
                    {
                        if (_numBoxes == 0)
                        {
                            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, false);
                            SupplyGatherState = SupplyGatherStates.SearchForSupplySource;
                            break;
                        }
                    }
                    else if (_numBoxes < _moduleData.MaxBoxes)
                    {
                        _currentSourceDockUpdate.GetBox();
                        _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.SupplyWarehouseActionDelay);
                        SupplyGatherState = SupplyGatherStates.GatheringSupplies;
                        break;
                    }
 
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, false);
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Carrying, true);
                    SupplyGatherState = SupplyGatherStates.SearchForSupplyTarget;
                    break;
                case SupplyGatherStates.GatheringSupplies:
                    if (context.Time.TotalTime > _waitUntil)
                    {
                        _numBoxes++;
                        SupplyGatherState = SupplyGatherStates.RequestSupplies;
                    }
                    break;
                case SupplyGatherStates.SearchForSupplyTarget:
                    if (CurrentSupplyTarget == null)
                    {
                        var supplyTargets = context.GameContext.GameObjects.GetObjectsByKindOf(ObjectKinds.CashGenerator);

                        var distanceToCurrentSupplyTarget = float.PositiveInfinity;
                        foreach (var supplyTarget in supplyTargets)
                        {
                            if (supplyTarget.Owner != GameObject.Owner) continue;

                            var offsetToTarget = supplyTarget.Transform.Translation - GameObject.Transform.Translation;
                            var distanceToTarget = offsetToTarget.Vector2XY().Length();

                            if (distanceToTarget > _moduleData.SupplyWarehouseScanDistance || distanceToTarget > distanceToCurrentSupplyTarget) continue;

                            var dockUpdate = supplyTarget.FindBehavior<SupplyCenterDockUpdate>() ?? null;
                            if (dockUpdate?.CanApproach() ?? false) continue;
                            
                            CurrentSupplyTarget = supplyTarget;
                            _currentTargetDockUpdate = dockUpdate;
                            distanceToCurrentSupplyTarget = distanceToTarget;
                        }
                    }

                    if (CurrentSupplyTarget == null) break;

                    if (_currentTargetDockUpdate == null)
                    {
                        _currentTargetDockUpdate = CurrentSupplyTarget.FindBehavior<SupplyCenterDockUpdate>();
                    }

                    if (!_currentTargetDockUpdate.CanApproach()) break;

                    SetTargetPoint(_currentTargetDockUpdate.GetApproachTargetPosition(this));
                    SupplyGatherState = SupplyGatherStates.ApproachSupplyTarget;
                    break;
                case SupplyGatherStates.ApproachSupplyTarget:
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
