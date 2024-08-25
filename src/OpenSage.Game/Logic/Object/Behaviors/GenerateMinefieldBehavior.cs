#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Logic.Object
{
    public sealed class GenerateMinefieldBehavior : BehaviorModule, IUpgradeableModule
    {
        internal UpgradeLogic UpgradeLogic { get; }
        internal bool Generated => _generated;
        internal bool Upgraded => _upgraded;
        internal Vector3? GenerationPosition => _hasGenerationPosition ? _generationPosition : null;
        internal IReadOnlyList<uint> GeneratedMineIds => _generatedMineIds;

        private readonly GameObject _gameObject;
        private readonly GameContext _context;
        private readonly GenerateMinefieldBehaviorModuleData _moduleData;

        // whether the minefield has been generated? (seems to always match _upgradeLogic.Triggered)
        private bool _generated;
        // whether the minefield has been upgraded (after being generated)
        private bool _upgraded;

        // a falling cluster bomb will have this set to true along with a position to generate at
        private bool _hasGenerationPosition;
        private Vector3 _generationPosition;

        private readonly List<uint> _generatedMineIds = [];

        internal GenerateMinefieldBehavior(GameObject gameObject, GameContext context, GenerateMinefieldBehaviorModuleData moduleData)
        {
            _gameObject = gameObject;
            _context = context;
            _moduleData = moduleData;
            UpgradeLogic = new UpgradeLogic(moduleData.UpgradeData, OnUpgrade);
        }

        public bool CanUpgrade(UpgradeSet existingUpgrades) => UpgradeLogic.CanUpgrade(existingUpgrades);

        public void TryUpgrade(UpgradeSet completedUpgrades) => UpgradeLogic.TryUpgrade(completedUpgrades);

        // todo: this behavior is "doubly" upgradable - the first upgrade creates the mines, and the second upgrade replaces them with a different template
        private void OnUpgrade()
        {
            if (_moduleData.GenerateOnlyOnDeath)
            {
                return;
            }

            GenerateMinefield(new BehaviorUpdateContext(_context, _gameObject));
        }

        internal override void OnDie(BehaviorUpdateContext context, DeathType deathType, BitArray<ObjectStatus> status)
        {
            if (_moduleData.GenerateOnlyOnDeath)
            {
                GenerateMinefield(context);
            }

            base.OnDie(context, deathType, status);
        }

        private void GenerateMinefield(BehaviorUpdateContext context)
        {
            _moduleData.GenerationFX?.Value.Execute(context);

            var mineTemplate = _moduleData.MineName?.Value;

            if (mineTemplate is null)
            {
                return;
            }

            var centerPoint = _hasGenerationPosition ? _generationPosition : _gameObject.Transform.Translation;
            var gameData = _context.Game.AssetStore.GameData.Current;
            var mineObjectSize = mineTemplate.Geometry.BoundingCircleRadius * 2;
            var outerPerimeter = _moduleData.DistanceAroundObject ?? gameData.StandardMinefieldDistance;
            var smartRadius = _gameObject.ShapedCollider.WorldBounds.Radius; // todo: this should be a rectangle when AlwaysCircular is false
            var innerPerimeter = 0f;

            if (_moduleData.SmartBorder)
            {
                outerPerimeter = Math.Max(outerPerimeter, smartRadius);
                if (_moduleData.SmartBorderSkipInterior)
                {
                    innerPerimeter = smartRadius;
                }
            }

            innerPerimeter += mineTemplate.Geometry.BoundingCircleRadius;
            outerPerimeter += mineTemplate.Geometry.BoundingCircleRadius;

            // just create a list of the xy coordinates we'd like to generate at first
            var mineCandidateLocations = new List<Vector2>();

            // potentially iterate over this multiple times to make multiple rings if necessary
            for (var radius = innerPerimeter; radius <= outerPerimeter; radius += mineObjectSize)
            {
                if (_moduleData.AlwaysCircular)
                {
                    // we're making circles
                    var circumference = Math.PI * radius * 2;
                    var minesToPlace = (int)Math.Floor(circumference / mineObjectSize); // this may not be technically correct since the chord distance is shorter than the arc distance, but in testing seems to be accurate
                    var radialGainPerMine = (float)(2 * Math.PI / minesToPlace);

                    //convert polar to cartesian coordinates
                    for (var mineIndex = 0; mineIndex < minesToPlace; mineIndex++)
                    {
                        var theta = mineIndex * radialGainPerMine;
                        var x = centerPoint.X + radius * float.Cos(theta);
                        var y = centerPoint.Y + radius * float.Sin(theta);

                        mineCandidateLocations.Add(new Vector2(x, y));
                    }
                }
                else
                {
                    // we're making rectangles - aligned with world object, NOT axis-aligned
                    // nothing in the game currently uses this - still, the engine _does_ support it
                    throw new NotImplementedException();
                }
            }

            // then take those xy coordinates and determine which of them we could actually spawn mines at
            // we don't spawn them yet to avoid issues with them potentially "colliding" with each other
            var newMineTransforms = mineCandidateLocations
                .Select(v => new Vector3(v.X, v.Y, _context.Terrain.HeightMap.GetHeight(v.X, v.Y)))
                .WhereNot(_context.Terrain.ImpassableAt) // this might not be exactly correct, but seems to be close?
                .Select(NormalTransformAtLocation)
                .Where(TransformIsValidMineLocation)
                .ToList();

            // now that we know where to actually spawn the mines, spawn them
            foreach (var transform in newMineTransforms)
            {
                var newMine = _gameObject.GameContext.GameLogic.CreateObject(mineTemplate, _gameObject.Owner);
                newMine.UpdateTransform(transform.Translation, transform.Rotation);
                newMine.CreatedByObjectID = _gameObject.ID;
                _generatedMineIds.Add(newMine.ID);
            }
        }

        private Transform NormalTransformAtLocation(Vector3 location)
        {
            // todo: should the normal matching be handled by KindOf STICK_TO_TERRAIN_SLOPE?
            var normal = _context.Terrain.HeightMap.GetNormal(location.X, location.Y);
            var rotation = (float)(_context.Random.NextDouble() * 2 * Math.PI);
            return new Transform(location, Quaternion.CreateFromAxisAngle(normal, rotation));
        }

        private bool TransformIsValidMineLocation(Transform transform)
        {
            var mineTemplate = _moduleData.MineName?.Value;

            if (mineTemplate is null)
            {
                return false;
            }

            var intersecting = _context.Quadtree.FindIntersecting(new SphereCollider(transform, mineTemplate.Geometry.BoundingCircleRadius));
            return !intersecting.Any(obj => obj != _gameObject && (obj.IsKindOf(ObjectKinds.Structure) || obj.IsKindOf(ObjectKinds.Mine)));
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistObject(UpgradeLogic);
            reader.PersistBoolean(ref _generated);

            if (_generated != UpgradeLogic.Triggered)
            {
                throw new InvalidStateException();
            }

            reader.PersistBoolean(ref _hasGenerationPosition);

            if (reader.SageGame >= SageGame.CncGeneralsZeroHour)
            {
                reader.PersistBoolean(ref _upgraded);
            }

            reader.PersistVector3(ref _generationPosition);

            if (reader.SageGame >= SageGame.CncGeneralsZeroHour)
            {
                reader.PersistListWithByteCount(_generatedMineIds, (StatePersister persister, ref uint item) => persister.PersistObjectIDValue(ref item));
            }
        }
    }

    public sealed class GenerateMinefieldBehaviorModuleData : UpdateModuleData
    {
        internal static GenerateMinefieldBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GenerateMinefieldBehaviorModuleData> FieldParseTable =
            new IniParseTableChild<GenerateMinefieldBehaviorModuleData, UpgradeLogicData>(x => x.UpgradeData, UpgradeLogicData.FieldParseTable)
            .Concat(new IniParseTable<GenerateMinefieldBehaviorModuleData>
            {
                { "MineName", (parser, x) => x.MineName = parser.ParseObjectReference() },
                { "DistanceAroundObject", (parser, x) => x.DistanceAroundObject = parser.ParseFloat() },
                { "GenerateOnlyOnDeath", (parser, x) => x.GenerateOnlyOnDeath = parser.ParseBoolean() },
                { "SmartBorder", (parser, x) => x.SmartBorder = parser.ParseBoolean() },
                { "SmartBorderSkipInterior", (parser, x) => x.SmartBorderSkipInterior = parser.ParseBoolean() },
                { "AlwaysCircular", (parser, x) => x.AlwaysCircular = parser.ParseBoolean() },
                { "GenerationFX", (parser, x) => x.GenerationFX = parser.ParseFXListReference() },
                { "Upgradable", (parser, x) => x.Upgradable = parser.ParseBoolean() },
                { "UpgradedTriggeredBy", (parser, x) => x.UpgradedTriggeredBy = parser.ParseUpgradeReference() },
                { "UpgradedMineName", (parser, x) => x.UpgradedMineName = parser.ParseObjectReference() },
            });

        public UpgradeLogicData UpgradeData { get; } = new();

        /// <summary>
        /// The mine object to generate.
        /// </summary>
        public LazyAssetReference<ObjectDefinition>? MineName { get; private set; }

        /// <summary>
        /// The outer perimeter of the mines. If <see cref="SmartBorder"/> is <c>true</c>, this is not necessarily the inner perimeter,
        /// and we may end up with a ring with some thickness. If <see cref="SmartBorder"/> is true and this value is less than the
        /// computed value of <see cref="SmartBorder"/>, this setting is ignored.
        /// </summary>
        /// <remarks>
        /// A setting of <c>0</c> and a setting of <c>null</c> are <b>not</b> the same thing.
        /// <list type="table">
        /// <item>
        /// <term><c>null</c></term>
        /// <description>Use <see cref="GameData.StandardMinefieldDistance"/></description>
        /// </item>
        /// <item>
        /// <term><c>0</c></term>
        /// <description>Set the outer perimeter to 0</description>
        /// </item>
        /// </list>
        /// </remarks>
        public float? DistanceAroundObject { get; private set; }

        /// <summary>
        /// Whether the minefield should be generated when the object dies (e.g. with a cluster mine bomb).
        /// </summary>
        public bool GenerateOnlyOnDeath { get; private set; }

        /// <summary>
        /// Whether to set an inner perimeter based on the object's bounding box (or bounding sphere if <see cref="AlwaysCircular"/> is <c>true</c>).
        /// If set to <c>false</c>, <see cref="DistanceAroundObject"/> is used as the inner perimeter.
        /// </summary>
        public bool SmartBorder { get; private set; }

        /// <summary>
        /// This seems to be whether <see cref="SmartBorder"/> should be used as an inner perimeter - if false, it is used as an outer perimeter
        /// (but can be overridden by <see cref="DistanceAroundObject"/>).
        /// </summary>
        /// <remarks>
        /// When set to <c>false</c> for a building with only <see cref="SmartBorder"/> and no <see cref="DistanceAroundObject"/>, no mines spawned.
        /// This seems to be because the game builds a grid of mine locations, and then doesn't place any in places where there are existing structures.
        /// </remarks>
        public bool SmartBorderSkipInterior { get; private set; } = true;

        /// <summary>
        /// Whether the mines should form a ring or a box around the generating object.
        /// </summary>
        public bool AlwaysCircular { get; private set; }

        /// <summary>
        /// FX to play upon generation.
        /// </summary>
        public LazyAssetReference<FXList>? GenerationFX { get; private set; } // used for e.g. cluster mines, but not China structure mines

        /// <summary>
        /// Whether the mines can be upgraded to a new mine type.
        /// </summary>
        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool Upgradable { get; private set; }

        /// <summary>
        /// The upgrade which triggers the mines to be replaced with <see cref="UpgradedMineName"/>.
        /// </summary>
        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public LazyAssetReference<UpgradeTemplate>? UpgradedTriggeredBy { get; private set; }

        /// <summary>
        /// The template with which to replace the existing mines upon <see cref="UpgradedTriggeredBy"/>.
        /// </summary>
        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public LazyAssetReference<ObjectDefinition>? UpgradedMineName { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new GenerateMinefieldBehavior(gameObject, context, this);
        }
    }
}
