using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class DockUpdate : UpdateModule
    {
        private GameObject _gameObject;
        private DockUpdateModuleData _moduleData;

        private Queue<SupplyAIUpdate> _unitsApproaching;
        private bool _usesWaitingBones;

        internal DockUpdate(GameObject gameObject, DockUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
            _unitsApproaching = new Queue<SupplyAIUpdate>();
            _usesWaitingBones = _moduleData.NumberApproachPositions != -1;
        }

        private Vector3 GetActionBone()
        {
            // TODO: might also be DOCKSTART or DOCKEND
            var (actionModelInstance, actionBone) = _gameObject.FindBone($"DOCKACTION");

            if (actionModelInstance != null && actionBone != null)
            {
                return actionModelInstance.AbsoluteBoneTransforms[actionBone.Index].Translation;
            }
            return _gameObject.Transform.Translation;
        }

        private Vector3 GetDockWaitingBone(int id)
        {
            var identifier = id.ToString("D2");
            var (modelInstance, bone) = _gameObject.FindBone($"DOCKWAITING{identifier}");
            return modelInstance.AbsoluteBoneTransforms[bone.Index].Translation;
        }

        public bool CanApproach() => !_usesWaitingBones || _unitsApproaching.Count < _moduleData.NumberApproachPositions + 1;

        public Vector3 GetApproachTargetPosition(SupplyAIUpdate aiUpdate)
        {
            _unitsApproaching.Enqueue(aiUpdate);

            if (!_usesWaitingBones)
            {
                return _gameObject.Transform.Translation;
            }

            if (_unitsApproaching.Count == 1)
            {
                return GetActionBone();
            }

            return GetDockWaitingBone(_unitsApproaching.Count - 1);
        }

        private void MoveObjectsForward()
        {
            if (!_usesWaitingBones) return;

            var units = _unitsApproaching.ToList();
            for (var i = 0; i < units.Count; i++)
            {
                var aiUpdate = units[i];
                aiUpdate.SupplyGatherState = SupplyAIUpdate.SupplyGatherStates.APPROACH_SUPPLY_TARGET;

                if (i == 0)
                {
                    aiUpdate.AddTargetPoint(GetActionBone());
                }
                else
                {
                    aiUpdate.AddTargetPoint(GetDockWaitingBone(i));
                }
            }
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (_unitsApproaching.Count > 0)
            {
                var aiUpdate = _unitsApproaching.Peek();

                switch (aiUpdate.SupplyGatherState)
                {
                    case SupplyAIUpdate.SupplyGatherStates.ENQUEUED_AT_SUPPLY_TARGET:
                        aiUpdate.SupplyGatherState = SupplyAIUpdate.SupplyGatherStates.START_DUMPING_SUPPLYS;
                        break;
                    case SupplyAIUpdate.SupplyGatherStates.FINISHED_DUMPING_SUPPLYS:
                        _gameObject.ModelConditionFlags.Set(ModelConditionFlag.DockingActive, true);
                        aiUpdate.SupplyGatherState = SupplyAIUpdate.SupplyGatherStates.SEARCH_FOR_SUPPLY_SOURCE;
                        _unitsApproaching.Dequeue();
                        MoveObjectsForward();
                        break;
                }
            }
        }
    }

    public abstract class DockUpdateModuleData : UpdateModuleData
    {
        internal static readonly IniParseTable<DockUpdateModuleData> FieldParseTable = new IniParseTable<DockUpdateModuleData>
        {
            { "NumberApproachPositions", (parser, x) => x.NumberApproachPositions = parser.ParseInteger() },
            { "AllowsPassthrough", (parser, x) => x.AllowsPassthrough = parser.ParseBoolean() },
        };

        /// <summary>
        /// Number of approach bones in the model. If this is -1, infinite harvesters can approach.
        /// </summary>
        public int NumberApproachPositions { get; private set; }

        /// <summary>
        /// Can harvesters drive through this warehouse? Should be set to false if all dock points are external.
        /// </summary>
        public bool AllowsPassthrough { get; private set; } = true;
    }
}
