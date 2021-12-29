using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class SpawnBehavior : BehaviorModule
    {
        GameObject _gameObject;
        SpawnBehaviorModuleData _moduleData;

        private List<GameObject> _spawnedUnits;
        private bool _initial;
        private IProductionExit _productionExit;

        private bool _unknownBool1;
        private string _templateName;
        private int _unknownInt1;
        private int _unknownInt2;
        private readonly List<uint> _unknownIntList = new();
        private readonly List<uint> _unknownObjectList = new();
        private ushort _unknownInt3;
        private int _unknownInt4;

        internal SpawnBehavior(GameObject gameObject, GameContext context, SpawnBehaviorModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;

            _spawnedUnits = new List<GameObject>();
            _initial = true;
        }

        private void SpawnUnit()
        {
            _productionExit ??= _gameObject.FindBehavior<IProductionExit>();

            var spawnedObject = _gameObject.Parent.Add(_moduleData.SpawnTemplate.Value);
            spawnedObject.Owner = _gameObject.Owner;
            _spawnedUnits.Add(spawnedObject);

            var slavedUpdate = spawnedObject.FindBehavior<SlavedUpdateModule>();
            if (slavedUpdate != null)
            {
                slavedUpdate.Master = _gameObject;
            }

            if (_productionExit != null)
            {
                spawnedObject.SetTranslation(_gameObject.ToWorldspace(_productionExit.GetUnitCreatePoint()));

                var rallyPoint = _productionExit.GetNaturalRallyPoint();
                if (rallyPoint.HasValue)
                {
                    spawnedObject.AIUpdate?.AddTargetPoint(_gameObject.ToWorldspace(rallyPoint.Value));
                }
            }

            var productionUpdate = _gameObject.FindBehavior<ProductionUpdate>();
            if (productionUpdate != null)
            {
                ProductionUpdate.HandleHarvesterUnitCreation(_gameObject, spawnedObject);
            }
        }

        public void SpawnInitial()
        {
            for (var i = 0; i < _moduleData.SpawnNumber; i++)
            {
                SpawnUnit();
            }
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (_initial && !_gameObject.IsBeingConstructed())
            {
                SpawnInitial();
                _initial = false;
            }

            // TODO: respawn killed/dead units
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);

            _unknownBool1 = reader.ReadBoolean();

            _templateName = reader.ReadAsciiString();

            reader.ReadInt32(ref _unknownInt1);
            reader.ReadInt32(ref _unknownInt2);

            reader.SkipUnknownBytes(4);

            var unknownBool2 = reader.ReadBoolean();
            if (!unknownBool2)
            {
                throw new InvalidStateException();
            }

            var unknownInt4 = (ushort)_unknownIntList.Count;
            reader.ReadUInt16(ref unknownInt4);
            for (var i = 0; i < unknownInt4; i++)
            {
                _unknownIntList.Add(reader.ReadUInt32());
            }

            var unknownBool3 = reader.ReadBoolean();
            if (!unknownBool3)
            {
                throw new InvalidStateException();
            }

            var objectCount = (ushort)_unknownObjectList.Count;
            reader.ReadUInt16(ref objectCount);
            for (var i = 0; i < objectCount; i++)
            {
                uint objectId = 0;
                reader.ReadObjectID(ref objectId);
                _unknownObjectList.Add(objectId);
            }

            reader.ReadUInt16(ref _unknownInt3);
            if (_unknownInt3 != 0 && _unknownInt3 != 1)
            {
                throw new InvalidStateException();
            }

            reader.ReadInt32(ref _unknownInt4);
            if (_unknownInt4 != objectCount && _unknownInt4 != -1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(4);
        }
    }

    public sealed class SpawnBehaviorModuleData : UpgradeModuleData
    {
        internal static SpawnBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<SpawnBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<SpawnBehaviorModuleData>
            {
                { "SpawnNumber", (parser, x) => x.SpawnNumber = parser.ParseInteger() },
                { "SpawnReplaceDelay", (parser, x) => x.SpawnReplaceDelay = parser.ParseLong() },
                { "SpawnTemplateName", (parser, x) => x.SpawnTemplate = parser.ParseObjectReference() },
                { "OneShot", (parser, x) => x.OneShot = parser.ParseBoolean() },
                { "CanReclaimOrphans", (parser, x) => x.CanReclaimOrphans = parser.ParseBoolean() },
                { "SpawnedRequireSpawner", (parser, x) => x.SpawnedRequireSpawner = parser.ParseBoolean() },
                { "ExitByBudding", (parser, x) => x.ExitByBudding = parser.ParseBoolean() },
                { "InitialBurst", (parser, x) => x.InitialBurst = parser.ParseInteger() },
                { "AggregateHealth", (parser, x) => x.AggregateHealth = parser.ParseBoolean() },
                { "SlavesHaveFreeWill", (parser, x) => x.SlavesHaveFreeWill = parser.ParseBoolean() },
                { "RespectCommandLimit", (parser, x) => x.RespectCommandLimit = parser.ParseBoolean() },
                { "KillSpawnsBasedOnModelConditionState", (parser, x) => x.KillSpawnsBasedOnModelConditionState = parser.ParseBoolean() },
                { "ShareUpgrades", (parser, x) => x.ShareUpgrades = parser.ParseBoolean() },
                { "FadeInTime", (parser, x) => x.FadeInTime = parser.ParseInteger() },
                { "SpawnInsideBuilding", (parser, x) => x.SpawnInsideBuilding = parser.ParseBoolean() },
            });

        public int SpawnNumber { get; private set; }
        public long SpawnReplaceDelay { get; private set; }
        public LazyAssetReference<ObjectDefinition> SpawnTemplate { get; private set; }
        public bool OneShot { get; private set; }
        public bool CanReclaimOrphans { get; private set; }
        public bool SpawnedRequireSpawner { get; private set; }
        public bool ExitByBudding { get; private set; }
        public int InitialBurst { get; private set; }
        public bool AggregateHealth { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool SlavesHaveFreeWill { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool RespectCommandLimit { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool KillSpawnsBasedOnModelConditionState { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ShareUpgrades { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int FadeInTime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool SpawnInsideBuilding { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new SpawnBehavior(gameObject, context, this);
        }
    }
}
