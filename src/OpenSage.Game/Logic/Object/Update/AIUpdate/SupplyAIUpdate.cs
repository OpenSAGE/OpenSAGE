using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class SupplyAIUpdate : AIUpdate
    {
        public GameObject CurrentSupplyTarget;

        private SupplyAIUpdateModuleData _moduleData;

        private GameObject _currentSupplySource;
        private SupplyGatherState _supplyGatherState;
        private double _waitUntil;
        private int _numBoxes;

        private enum SupplyGatherState
        {
            DEFAULT,
            SEARCH_FOR_SUPPLY_SOURCE,
            APPROACH_SUPPLY_SOURCE,
            REQUEST_SUPPLYS,
            GATHERING_SUPPLYS,
            SEARCH_FOR_SUPPLY_TARGET,
            APPROACH_SUPPLY_TARGET,
            DUMPING_SUPPLYS,

            DOCKING_AT_SUPPLY_SOURCE,
            DOCK_AT_SUPPLY_TARGET,
        }

        internal SupplyAIUpdate(GameObject gameObject, SupplyAIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
            _supplyGatherState = SupplyGatherState.SEARCH_FOR_SUPPLY_SOURCE;
            _numBoxes = 0;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);

            var isMoving = GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);

            switch (_supplyGatherState)
            {
                case SupplyGatherState.SEARCH_FOR_SUPPLY_SOURCE:
                    if (_currentSupplySource == null)
                    {
                        // TODO: also use KindOf SUPPLY_SOURCE_ON_PREVIEW ?
                        var supplySources = context.GameContext.GameObjects.GetObjectsByKindOf(ObjectKinds.SupplySource);

                        var distanceToCurrentSupplySource = float.PositiveInfinity;
                        foreach (var supplySource in supplySources)
                        {
                            var offsetToSource = supplySource.Transform.Translation - GameObject.Transform.Translation;
                            var distanceToSource = offsetToSource.Vector2XY().Length();

                            if (distanceToSource < _moduleData.SupplyWarehouseScanDistance && distanceToSource < distanceToCurrentSupplySource)
                            {
                                _currentSupplySource = supplySource;
                                distanceToCurrentSupplySource = distanceToSource;
                            }
                        }
                    }

                    GameObject.AIUpdate.AddTargetPoint(_currentSupplySource.Transform.Translation);
                    _supplyGatherState = SupplyGatherState.APPROACH_SUPPLY_SOURCE;
                    break;
                case SupplyGatherState.APPROACH_SUPPLY_SOURCE:
                    if (!isMoving)
                    {
                        _supplyGatherState = SupplyGatherState.REQUEST_SUPPLYS;
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, true);
                    }
                    break;
                case SupplyGatherState.REQUEST_SUPPLYS:
                    if (_numBoxes < _moduleData.MaxBoxes)
                    {
                        _waitUntil = context.Time.TotalTime.TotalMilliseconds + _moduleData.SupplyWarehouseActionDelay;
                        _supplyGatherState = SupplyGatherState.GATHERING_SUPPLYS;
                    }
                    else
                    {
                        _supplyGatherState = SupplyGatherState.SEARCH_FOR_SUPPLY_TARGET;
                    }
                    break;
                case SupplyGatherState.GATHERING_SUPPLYS:
                    if (context.Time.TotalTime.TotalMilliseconds > _waitUntil)
                    {
                        _numBoxes++;
                        _supplyGatherState = SupplyGatherState.REQUEST_SUPPLYS;
                    }
                    break;
                case SupplyGatherState.SEARCH_FOR_SUPPLY_TARGET:
                    if (CurrentSupplyTarget == null)
                    {
                        var supplyTargets = context.GameContext.GameObjects.GetObjectsByKindOf(ObjectKinds.CashGenerator);

                        var distanceToCurrentSupplyTarget = float.PositiveInfinity;
                        foreach (var supplyTarget in supplyTargets)
                        {
                            if (supplyTarget.Owner != GameObject.Owner)
                            {
                                continue;
                            }

                            var offsetToTarget = supplyTarget.Transform.Translation - GameObject.Transform.Translation;
                            var distanceToTarget = offsetToTarget.Vector2XY().Length();

                            if (distanceToTarget < _moduleData.SupplyWarehouseScanDistance && distanceToTarget < distanceToCurrentSupplyTarget)
                            {
                                CurrentSupplyTarget = supplyTarget;
                                distanceToCurrentSupplyTarget = distanceToTarget;
                            }
                        }
                    }

                    GameObject.AIUpdate.SetTargetPoint(CurrentSupplyTarget.Transform.Translation);
                    _supplyGatherState = SupplyGatherState.APPROACH_SUPPLY_TARGET;
                    break;
                case SupplyGatherState.APPROACH_SUPPLY_TARGET:
                    if (!isMoving)
                    {
                        _waitUntil = context.Time.TotalTime.TotalMilliseconds + _moduleData.SupplyCenterActionDelay;
                        _supplyGatherState = SupplyGatherState.DUMPING_SUPPLYS;
                        //GameObject.ModelConditionFlags.Set(ModelConditionFlag.Docking, true);
                    }
                    break;
                case SupplyGatherState.DUMPING_SUPPLYS:
                    if (context.Time.TotalTime.TotalMilliseconds > _waitUntil)
                    {
                        _supplyGatherState = SupplyGatherState.SEARCH_FOR_SUPPLY_SOURCE;
                        // TOOD: increase player money
                        _numBoxes = 0;
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
