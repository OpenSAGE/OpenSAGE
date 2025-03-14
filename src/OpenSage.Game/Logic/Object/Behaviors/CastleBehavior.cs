﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

internal sealed class CastleBehavior : FoundationAIUpdate
{
    private GameObject _gameObject;
    private CastleBehaviorModuleData _moduleData;
    private GameContext _context;
    private LogicFrame _waitUntil;
    private LogicFrameSpan _updateInterval;
    private Player _nativePlayer;

    public bool IsUnpacked { get; set; }

    internal CastleBehavior(GameObject gameObject, GameContext context, CastleBehaviorModuleData moduleData)
        : base(gameObject, context, moduleData)
    {
        IsUnpacked = false;
        _moduleData = moduleData;
        _gameObject = gameObject;
        _context = context;
        _updateInterval = new LogicFrameSpan((uint)MathF.Ceiling(Game.LogicFramesPerSecond / 2)); // 0.5s
        _nativePlayer = _gameObject.Owner;
    }

    public float GetUnpackCost(Player player)
    {
        var castleEntry = FindCastle(player.Side);
        return castleEntry.UnpackCost;
    }

    public void Unpack(Player player, bool instant = false)
    {
        if (IsUnpacked)
        {
            return;
        }

        _gameObject.Hidden = true;
        _gameObject.IsSelectable = false;

        var castleEntry = FindCastle(player.Template.Side);

        if (castleEntry != null)
        {
            var basePath = $"bases\\{castleEntry.Camp}\\{castleEntry.Camp}.bse";

            var entry = _context.AssetLoadContext.FileSystem.GetFile(basePath);
            var mapFile = MapFile.FromFileSystemEntry(entry);
            var mapObjects = mapFile.ObjectsList.Objects.ToList();

            foreach (var castleTemplate in mapFile.CastleTemplates.Templates)
            {
                var mapObject = mapObjects.Find(x => x.TypeName == castleTemplate.TemplateName);

                var viewAngle = MathUtility.ToRadians(_gameObject.Definition.PlacementViewAngle);

                var offset = Vector4.Transform(new Vector4(castleTemplate.Offset.X, castleTemplate.Offset.Y, 0.0f, 1.0f), Quaternion.CreateFromAxisAngle(Vector3.UnitZ, viewAngle)).ToVector3();

                var angle = viewAngle + castleTemplate.Angle;
                mapObject.Position = new Vector3(_gameObject.Translation.X, _gameObject.Translation.Y, 0.0f) + offset;

                var baseObject = GameObject.FromMapObject(mapObject, _context, false, angle);

                if (!instant)
                {
                    baseObject.PrepareConstruction();
                    baseObject.SetIsBeingConstructed();
                    baseObject.BuildProgress = 0.0f;
                }

                if (_moduleData.FilterValidOwnedEntries.Matches(baseObject))
                {
                    baseObject.Owner = player;
                }
            }
        }
        IsUnpacked = true;
    }

    internal override void Update(BehaviorUpdateContext context)
    {
        if (IsUnpacked)
        {
            // not sure if this is correct, or does any object with BASE_FOUNDATION or BASE_SITE automatically get a FoundationAIUpdate?
            FoundationAIUpdate.CheckForStructure(context, _gameObject, ref _waitUntil, _updateInterval);
            return;
        }

        // TODO: Figure out the other unpack conditions
        if (_moduleData.InstantUnpack)
        {
            Unpack(_gameObject.Owner, instant: true);
        }

        var nearbyUnits = context.GameContext.Game.PartitionCellManager.QueryObjects(
            _gameObject,
            _gameObject.Translation,
            _moduleData.ScanDistance,
            new PartitionQueries.TrueQuery());

        if (!nearbyUnits.Any())
        {
            _gameObject.Owner = _nativePlayer;
            return;
        }

        // TODO: check if all nearby units are from the same owner
        foreach (var unit in nearbyUnits)
        {
            if (unit == _gameObject)
            {
                continue;
            }

            var distance = (unit.Translation - _gameObject.Translation).Length();
            if (distance < _moduleData.ScanDistance)
            {
                _gameObject.Owner = unit.Owner;
                return;
            }
        }
    }

    private CastleEntry FindCastle(string side)
    {
        foreach (var entry in _moduleData.CastleToUnpackForFactions)
        {
            if (side == entry.FactionName)
            {
                return entry;
            }
        }

        return null;
    }

    internal override void DrawInspector()
    {
        //var entry = FindCastle();

        //ImGui.LabelText("Camp", entry.Camp);
        ImGui.LabelText("Unpacked", IsUnpacked.ToString());
    }
}

[AddedIn(SageGame.Bfme)]
public class CastleBehaviorModuleData : FoundationAIUpdateModuleData
{
    internal new static CastleBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    internal new static readonly IniParseTable<CastleBehaviorModuleData> FieldParseTable = FoundationAIUpdateModuleData.FieldParseTable
        .Concat(new IniParseTable<CastleBehaviorModuleData>
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
        });

    public List<Side> SidesAllowed { get; } = new List<Side>();
    public bool UseTheNewCastleSystemInsteadOfTheClunkyBuildList { get; private set; }
    public ObjectFilter FilterValidOwnedEntries { get; private set; }
    public bool UseSecondaryBuildList { get; private set; }
    public List<CastleEntry> CastleToUnpackForFactions { get; } = new List<CastleEntry>();
    public float MaxCastleRadius { get; private set; }
    public float FadeTime { get; private set; }
    public int ScanDistance { get; private set; } = 100; // TODO: correct?
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
        return new CastleBehavior(gameObject, context, this);
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
            UnpackCost = parser.GetFloatOptional()
        };
        return result;
    }

    public string FactionName { get; private set; }
    public string Camp { get; private set; }
    public float UnpackCost { get; private set; }
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
