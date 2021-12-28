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

        private Vector3 _position1;
        private Vector3 _position2;
        private Vector3 _position3;
        private int _numApproachPositions1;
        private bool _unknownBool;
        private readonly List<Vector3> _approachPositions = new();
        private readonly List<uint> _approachObjectIds = new();
        private readonly List<bool> _unknownBools = new();
        private uint _unknownObjectId;
        private ushort _unknownInt1;

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
            var (actionModelInstance, actionBone) = _gameObject.Drawable.FindBone($"DOCKACTION");

            if (actionModelInstance != null && actionBone != null)
            {
                return actionModelInstance.AbsoluteBoneTransforms[actionBone.Index].Translation;
            }
            return _gameObject.Translation;
        }

        private Vector3 GetDockWaitingBone(int id)
        {
            var identifier = id.ToString("D2");
            var (modelInstance, bone) = _gameObject.Drawable.FindBone($"DOCKWAITING{identifier}");
            return modelInstance.AbsoluteBoneTransforms[bone.Index].Translation;
        }

        public bool CanApproach() => !_usesWaitingBones || _unitsApproaching.Count < _moduleData.NumberApproachPositions + 1;

        public Vector3 GetApproachTargetPosition(SupplyAIUpdate aiUpdate)
        {
            _unitsApproaching.Enqueue(aiUpdate);

            if (!_usesWaitingBones)
            {
                return _gameObject.Translation;
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
                aiUpdate.SupplyGatherState = SupplyAIUpdate.SupplyGatherStates.ApproachingSupplyTarget;

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
            if (_gameObject.ModelConditionFlags.Get(ModelConditionFlag.DockingActive))
            {
                _gameObject.ModelConditionFlags.Set(ModelConditionFlag.DockingActive, false);
            }

            if (_unitsApproaching.Count > 0)
            {
                var aiUpdate = _unitsApproaching.Peek();

                switch (aiUpdate.SupplyGatherState)
                {
                    case SupplyAIUpdate.SupplyGatherStates.EnqueuedAtSupplyTarget:
                        aiUpdate.SupplyGatherState = SupplyAIUpdate.SupplyGatherStates.StartDumpingSupplies;
                        break;
                    case SupplyAIUpdate.SupplyGatherStates.FinishedDumpingSupplies:
                        _gameObject.ModelConditionFlags.Set(ModelConditionFlag.DockingActive, true);
                        aiUpdate.SupplyGatherState = SupplyAIUpdate.SupplyGatherStates.SearchingForSupplySource;
                        _unitsApproaching.Dequeue();
                        MoveObjectsForward();
                        break;
                }
            }
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadVector3(ref _position1);
            reader.ReadVector3(ref _position2);
            reader.ReadVector3(ref _position3);

            _numApproachPositions1 = reader.ReadInt32();

            _unknownBool = reader.ReadBoolean();

            var numberApproachPositions2 = reader.ReadInt32();
            for (var i = 0; i < numberApproachPositions2; i++)
            {
                Vector3 approachPosition = default;
                reader.ReadVector3(ref approachPosition);
                _approachPositions.Add(approachPosition);
            }

            var numberApproachPositions3 = reader.ReadInt32();
            for (var i = 0; i < numberApproachPositions3; i++)
            {
                uint objectId = 0;
                reader.ReadObjectID(ref objectId);
                _approachObjectIds.Add(objectId);
            }

            var numberApproachPositions4 = reader.ReadInt32();
            for (var i = 0; i < numberApproachPositions4; i++)
            {
                _unknownBools.Add(reader.ReadBoolean());
            }

            reader.ReadObjectID(ref _unknownObjectId);

            _unknownInt1 = reader.ReadUInt16();
            if (_unknownInt1 != 0 && _unknownInt1 != 1)
            {
                throw new InvalidStateException();
            }

            var unknown3 = reader.ReadBoolean();
            if (!unknown3)
            {
                throw new InvalidStateException();
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
