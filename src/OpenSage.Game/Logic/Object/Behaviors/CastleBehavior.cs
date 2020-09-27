using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class CastleBehaviorModule : BehaviorModule
    {
        GameObject _gameObject;
        CastleBehaviorModuleData _moduleData;
        bool _unpacked = false;

        internal CastleBehaviorModule(GameObject gameObject, GameContext context, CastleBehaviorModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
        }

        private void Unpack(GameContext context)
        {
            if (!_unpacked)
            {
                var castleEntry = FindCastle();

                if (castleEntry != null)
                {
                    var basePath = $"bases\\{castleEntry.Camp}\\{castleEntry.Camp}.bse";

                    var entry = context.AssetLoadContext.FileSystem.GetFile(basePath);
                    var mapFile = MapFile.FromFileSystemEntry(entry);
                    var mapObjects = mapFile.ObjectsList.Objects.ToList();

                    foreach (var castleTemplate in mapFile.CastleTemplates.Templates)
                    {
                        var mapObject = mapObjects.Find(x => x.TypeName == castleTemplate.TemplateName);
                        var viewAngle = MathUtility.ToRadians(_gameObject.Definition.PlacementViewAngle);
                        var offset = Vector4.Transform(new Vector4(castleTemplate.Offset.X, castleTemplate.Offset.Y, 0.0f, 1.0f), Quaternion.CreateFromAxisAngle(Vector3.UnitZ, viewAngle)).ToVector3();

                        var angle = viewAngle + castleTemplate.Angle;
                        mapObject.Position = new Vector3(_gameObject.Transform.Translation.X, _gameObject.Transform.Translation.Y, 0.0f) + offset;

                        var baseObject = GameObject.FromMapObject(
                            mapObject,
                            context.AssetLoadContext.AssetStore,
                            context.GameObjects,
                            context.Terrain.HeightMap,
                            useRotationAnchorOffset: false,
                            angle);

                        AssignOwner(baseObject);
                    }
                }

                _unpacked = true;
            }
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            // TODO: Figure out the other unpack conditions
            //if (_moduleData.InstantUnpack)
            {
                Unpack(context.GameContext);
                _gameObject.Destroy();
            }
        }

        private void AssignOwner(GameObject gameObject)
        {
            if (_moduleData.FilterValidOwnedEntries.Matches(gameObject))
            {
                gameObject.Owner = _gameObject.Owner;
                gameObject.Team = _gameObject.Team;
            }
        }

        private CastleEntry FindCastle()
        {
            // Use the gameobject side
            return _moduleData.CastleToUnpackForFactions[0];

            foreach (var entry in _moduleData.CastleToUnpackForFactions)
            {
                if (entry.FactionName == _gameObject.Owner.Side)
                {
                    return entry;
                }
            }

            return null;
        }

        internal override void DrawInspector()
        {
            var entry = FindCastle();

            ImGui.LabelText("Camp", entry.Camp);
            ImGui.LabelText("Unpacked", _unpacked.ToString());
        }
    }

    [AddedIn(SageGame.Bfme)]
    public class CastleBehaviorModuleData : BehaviorModuleData
    {
        internal static CastleBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<CastleBehaviorModuleData> FieldParseTable = new IniParseTable<CastleBehaviorModuleData>
        {
            { "SidesAllowed", (parser, x) => x.SidesAllowed.Add(Side.Parse(parser)) },
            { "UseTheNewCastleSystemInsteadOfTheClunkyBuildList", (parser, x) => x.UseTheNewCastleSystemInsteadOfTheClunkyBuildList = parser.ParseBoolean() },
            { "FilterValidOwnedEntries", (parser, x) => x.FilterValidOwnedEntries = ObjectFilter.Parse(parser) },
            { "UseSecondaryBuildList", (parser, x) => x.UseSecondaryBuildList = parser.ParseBoolean() },
            { "CastleToUnpackForFaction", (parser, x) => x.CastleToUnpackForFactions.Add(CastleEntry.Parse(parser)) },
            { "MaxCastleRadius", (parser, x) => x.MaxCastleRadius = parser.ParseFloat() },
            { "FadeTime", (parser, x) => x.FadeTime = parser.ParseFloat() },
            { "ScanDistance", (parser, x) => x.ScanDistance = parser.ParseInteger() },
            { "PreBuiltList", (parser, x) => x.PreBuiltList = PreBuildObject.Parse(parser) },
            { "PreBuiltPlyr", (parser, x) => x.PreBuiltPlayer = parser.ParseString() },
            { "FilterCrew", (parser, x) => x.FilterCrew = ObjectFilter.Parse(parser) },
            { "CrewReleaseFX", (parser, x) => x.CrewReleaseFX = parser.ParseAssetReference() },
            { "CrewPrepareFX", (parser, x) => x.CrewPrepareFX = parser.ParseAssetReference() },
            { "CrewPrepareInterval", (parser, x) => x.CrewPrepareInterval = parser.ParseInteger() },
            { "DisableStructureRotation", (parser, x) => x.DisableStructureRotation = parser.ParseBoolean() },
            { "FactionDecal", (parser, x) => x.FactionDecals.Add(CastleEntry.Parse(parser)) },
            { "InstantUnpack", (parser, x) => x.InstantUnpack = parser.ParseBoolean() },
            { "KeepDeathKillsEverything", (parser, x) => x.KeepDeathKillsEverything = parser.ParseBoolean() },
            { "EvaEnemyCastleSightedEvent", (parser, x) => x.EvaEnemyCastleSightedEvent = parser.ParseAssetReference() },
            { "UnpackDelayTime", (parser, x) => x.UnpackDelayTime = parser.ParseFloat() },
            { "Summoned", (parser, x) => x.Summoned = parser.ParseBoolean() }
        };

        public List<Side> SidesAllowed { get; } = new List<Side>();
        public bool UseTheNewCastleSystemInsteadOfTheClunkyBuildList { get; private set; }
        public ObjectFilter FilterValidOwnedEntries { get; private set; }
        public bool UseSecondaryBuildList { get; private set; }
        public List<CastleEntry> CastleToUnpackForFactions { get; } = new List<CastleEntry>();
        public float MaxCastleRadius { get; private set; }
        public float FadeTime { get; private set; }
        public int ScanDistance { get; private set; }
        public PreBuildObject PreBuiltList { get; private set; }
        public string PreBuiltPlayer { get; private set; }
        public ObjectFilter FilterCrew { get; private set; }
        public string CrewReleaseFX { get; private set; }
        public string CrewPrepareFX { get; private set; }
        public int CrewPrepareInterval { get; private set; }
        public bool DisableStructureRotation { get; private set; }
        public List<CastleEntry> FactionDecals { get; } = new List<CastleEntry>();

        [AddedIn(SageGame.Bfme2)]
        public bool InstantUnpack { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool KeepDeathKillsEverything { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaEnemyCastleSightedEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float UnpackDelayTime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool Summoned { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new CastleBehaviorModule(gameObject, context, this);
        }
    }

    public sealed class CastleEntry
    {
        internal static CastleEntry Parse(IniParser parser)
        {
            var result = new CastleEntry
            {
                FactionName = parser.ParseString(),
                Camp = parser.ParseAssetReference(),
                MaybeStartMoney = parser.GetFloatOptional()
            };
            return result;
        }

        public string FactionName { get; private set; }
        public string Camp { get; private set; }
        public float MaybeStartMoney { get; private set; }
    }

    public sealed class Side
    {
        internal static Side Parse(IniParser parser)
        {
            return new Side()
            {
                SideName = parser.ParseString(),
                CommandSourceTypes = parser.ParseEnumFlags<CommandSourceTypes>()
            };
        }

        public string SideName { get; private set; }
        public CommandSourceTypes CommandSourceTypes { get; private set; }
    }

    public sealed class PreBuildObject
    {
        internal static PreBuildObject Parse(IniParser parser)
        {
            return new PreBuildObject()
            {
                ObjectName = parser.ParseAssetReference(),
                Count = parser.ParseInteger()
            };
        }

        public string ObjectName { get; private set; }
        public int Count { get; private set; }
    }
}
