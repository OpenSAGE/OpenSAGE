using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class SupplyAIUpdate : AIUpdate
    {
        public GameObject CurrentSupplyTarget;
        public SupplyGatherStates SupplyGatherState;

        public enum SupplyGatherStates
        {
            DEFAULT,
            SEARCH_FOR_SUPPLY_SOURCE,
            APPROACH_SUPPLY_SOURCE,
            REQUEST_SUPPLYS,
            GATHERING_SUPPLYS,
            SEARCH_FOR_SUPPLY_TARGET,
            APPROACH_SUPPLY_TARGET,
            DUMPING_SUPPLYS
        }

        private SupplyAIUpdateModuleData _moduleData;

        private GameObject _currentSupplySource;
        private SupplyWarehouseDockUpdate _currentSourceDockUpdate;
        private double _waitUntil;
        private int _numBoxes;

        internal SupplyAIUpdate(GameObject gameObject, SupplyAIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
            SupplyGatherState = SupplyGatherStates.DEFAULT;
            _numBoxes = 0;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);

            var isMoving = GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);

            switch (SupplyGatherState)
            {
                case SupplyGatherStates.SEARCH_FOR_SUPPLY_SOURCE:
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
                            var hasBoxes = dockUpdate?.HasBoxes() ?? false;

                            if (!hasBoxes) continue;

                            _currentSupplySource = supplySource;
                            _currentSourceDockUpdate = dockUpdate;
                            distanceToCurrentSupplySource = distanceToSource;
                        }
                    }

                    // TODO: proper DockUpdate handling (queue etc)
                    GameObject.AIUpdate.AppendPathToTargetPoint(_currentSupplySource.Transform.Translation);
                    SupplyGatherState = SupplyGatherStates.APPROACH_SUPPLY_SOURCE;
                    break;
                case SupplyGatherStates.APPROACH_SUPPLY_SOURCE:
                    if (!isMoving)
                    {
                        SupplyGatherState = SupplyGatherStates.REQUEST_SUPPLYS;
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, true);
                    }
                    break;
                case SupplyGatherStates.REQUEST_SUPPLYS:
                    var boxesAvailable = _currentSourceDockUpdate?.HasBoxes() ?? false;

                    if (!boxesAvailable)
                    {
                        if (_numBoxes == 0)
                        {
                            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, false);
                            SupplyGatherState = SupplyGatherStates.SEARCH_FOR_SUPPLY_SOURCE;
                            break;
                        }
                    }
                    else if (_numBoxes < _moduleData.MaxBoxes)
                    {
                        _currentSourceDockUpdate.GetBox();
                        _waitUntil = context.Time.TotalTime.TotalMilliseconds + _moduleData.SupplyWarehouseActionDelay;
                        SupplyGatherState = SupplyGatherStates.GATHERING_SUPPLYS;
                        break;
                    }
 
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, false);
                    SupplyGatherState = SupplyGatherStates.SEARCH_FOR_SUPPLY_TARGET;
                    break;
                case SupplyGatherStates.GATHERING_SUPPLYS:
                    if (context.Time.TotalTime.TotalMilliseconds > _waitUntil)
                    {
                        _numBoxes++;
                        SupplyGatherState = SupplyGatherStates.REQUEST_SUPPLYS;
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Carrying, true);
                    }
                    break;
                case SupplyGatherStates.SEARCH_FOR_SUPPLY_TARGET:
                    if (CurrentSupplyTarget == null)
                    {
                        var supplyTargets = context.GameContext.GameObjects.GetObjectsByKindOf(ObjectKinds.CashGenerator);

                        var distanceToCurrentSupplyTarget = float.PositiveInfinity;
                        foreach (var supplyTarget in supplyTargets)
                        {
                            if (supplyTarget.Owner != GameObject.Owner) continue;

                            var offsetToTarget = supplyTarget.Transform.Translation - GameObject.Transform.Translation;
                            var distanceToTarget = offsetToTarget.Vector2XY().Length();

                            if (distanceToTarget < _moduleData.SupplyWarehouseScanDistance && distanceToTarget < distanceToCurrentSupplyTarget)
                            {
                                CurrentSupplyTarget = supplyTarget;
                                distanceToCurrentSupplyTarget = distanceToTarget;
                            }
                        }
                    }

                    // TODO: proper DockUpdate handling (queue etc)
                    GameObject.AIUpdate.SetTargetPoint(CurrentSupplyTarget.Transform.Translation);
                    SupplyGatherState = SupplyGatherStates.APPROACH_SUPPLY_TARGET;
                    break;
                case SupplyGatherStates.APPROACH_SUPPLY_TARGET:
                    if (!isMoving)
                    {
                        _waitUntil = context.Time.TotalTime.TotalMilliseconds + _moduleData.SupplyCenterActionDelay;
                        SupplyGatherState = SupplyGatherStates.DUMPING_SUPPLYS;
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, true);
                        CurrentSupplyTarget.ModelConditionFlags.Set(ModelConditionFlag.DockingActive, true);
                    }
                    break;
                case SupplyGatherStates.DUMPING_SUPPLYS:
                    if (context.Time.TotalTime.TotalMilliseconds > _waitUntil)
                    {
                        SupplyGatherState = SupplyGatherStates.SEARCH_FOR_SUPPLY_SOURCE;

                        var gameData = context.GameContext.AssetLoadContext.AssetStore.GameData.Current;
                        GameObject.Owner.Money += (uint)(_numBoxes * gameData.ValuePerSupplyBox);
                        _numBoxes = 0;

                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, false);
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Carrying, false);

                        // since animation mode is ONCE that flag should be cleared after the animation has run
                        //CurrentSupplyTarget.ModelConditionFlags.Set(ModelConditionFlag.DockingActive, false);
                    }
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
