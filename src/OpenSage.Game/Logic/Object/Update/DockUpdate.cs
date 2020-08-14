using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class DockUpdate : UpdateModule
    {
        private GameObject _gameObject;
        private DockUpdateModuleData _moduleData;

        private Queue<GameObject> _unitsApproaching;

        internal DockUpdate(GameObject gameObject, DockUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
            _unitsApproaching = new Queue<GameObject>();
        }

        public bool CanApproach() => _moduleData.NumberApproachPositions == -1 || _unitsApproaching.Count < _moduleData.NumberApproachPositions;

        public Vector3 GetApproachTargetPosition(GameObject obj)
        {
            if (_moduleData.NumberApproachPositions == -1)
            {
                return _gameObject.Transform.Translation;
            }

            if (_unitsApproaching.Count == 0)
            {
                // TODO: might also be DOCKSTART or DOCKEND
                var (_, actionBone) = _gameObject.FindBone($"DOCKACTION");
                return actionBone.Transform.Translation;
            }

            _unitsApproaching.Enqueue(obj);
            var boneID = _unitsApproaching.Count;
            var(_, bone) = _gameObject.FindBone($"DOCKWAITING0{boneID}"); //TODO: more than 9 bones
            return bone.Transform.Translation;
        }

        internal override void Update(BehaviorUpdateContext context)
        {


            // move objects forward in Queue

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
