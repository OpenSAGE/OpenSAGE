﻿#nullable enable

using System;
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
        private IProductionExit? _productionExit;
        private OpenContainModule? _openContain;

        private bool _unknownBool1;
        private string? _templateName;
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
            var spawnedObject = _gameObject.GameContext.GameLogic.CreateObject(_moduleData.SpawnTemplate?.Value, _gameObject.Owner);
            _spawnedUnits.Add(spawnedObject);

            spawnedObject.CreatedByObjectID = _gameObject.ID;
            var slavedUpdate = spawnedObject.FindBehavior<SlavedUpdateModule>();
            slavedUpdate?.SetMaster(_gameObject);

            if (!TryTransformViaProductionExit(spawnedObject) &&
                !TryTransformViaOpenContainer(spawnedObject))
            {
                throw new Exception("Unable to set spawn point for spawned unit");
            }
        }

        private bool TryTransformViaProductionExit(GameObject spawnedObject)
        {
            _productionExit ??= _gameObject.FindBehavior<IProductionExit>();

            if (_productionExit == null)
            {
                return false;
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

            return true;
        }

        // GLATunnelNetwork has no production exit behavior - it's explicitly commented out, saying "... we don't appear to need it for the spawns because they use OpenContain instead"
        private bool TryTransformViaOpenContainer(GameObject spawnedObject)
        {
            _openContain ??= _gameObject.FindBehavior<OpenContainModule>();

            if (_openContain == null)
            {
                return false;
            }

            // spawn at container output
            var (exitStart, exitEnd) = _openContain.DefaultExitPath;

            if (exitStart.HasValue && exitEnd.HasValue)
            {
                spawnedObject.SetTranslation(_gameObject.ToWorldspace(exitStart.Value));
                spawnedObject.AIUpdate.AddTargetPoint(_gameObject.ToWorldspace(exitEnd.Value));
            }
            else
            {
                spawnedObject.SetTranslation(_gameObject.Translation);
            }

            // move to rally point
            var rallyPoint = _gameObject.RallyPoint;
            if (rallyPoint.HasValue)
            {
                spawnedObject.AIUpdate?.AddTargetPoint(rallyPoint.Value);
            }

            return true;
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

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistAsciiString(ref _templateName);
            reader.PersistInt32(ref _unknownInt1);
            reader.PersistInt32(ref _unknownInt2);

            reader.SkipUnknownBytes(4);

            var unknownBool2 = true;
            reader.PersistBoolean(ref unknownBool2);
            if (!unknownBool2)
            {
                throw new InvalidStateException();
            }

            reader.PersistList(
                _unknownIntList,
                static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistUInt32Value(ref item);
                });

            var unknownBool3 = true;
            reader.PersistBoolean(ref unknownBool3);
            if (!unknownBool3)
            {
                throw new InvalidStateException();
            }

            reader.PersistList(
                _unknownObjectList,
                static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistObjectIDValue(ref item);
                });

            reader.PersistUInt16(ref _unknownInt3);
            if (_unknownInt3 != 0 && _unknownInt3 != 1)
            {
                throw new InvalidStateException();
            }

            reader.PersistInt32(ref _unknownInt4);
            if (_unknownInt4 != _unknownObjectList.Count && _unknownInt4 != -1)
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
        public LazyAssetReference<ObjectDefinition>? SpawnTemplate { get; private set; }
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
