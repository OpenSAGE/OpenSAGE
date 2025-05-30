﻿using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

[AddedIn(SageGame.Bfme)]
public class PassiveAreaEffectBehavior : UpdateModule
{
    private readonly PassiveAreaEffectBehaviorModuleData _moduleData;
    private LogicFrame _nextPing;

    public PassiveAreaEffectBehavior(GameObject gameObject, IGameEngine gameEngine, PassiveAreaEffectBehaviorModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;
    }

    public override UpdateSleepTime Update()
    {
        if (GameEngine.GameLogic.CurrentFrame < _nextPing)
        {
            // TODO(Port): Use correct value.
            return UpdateSleepTime.None;
        }
        _nextPing = GameEngine.GameLogic.CurrentFrame + _moduleData.PingDelay;

        var nearbyObjects = GameEngine.Game.PartitionCellManager.QueryObjects(
            GameObject,
            GameObject.Translation,
            _moduleData.EffectRadius,
            new PartitionQueries.ObjectFilterQuery(_moduleData.AllowFilter));

        foreach (var nearbyObject in nearbyObjects)
        {
            // TODO: HealPercentPerSecond, UpgradeRequired, NonStackable, HealFX, AntiCategories

            foreach (var modifier in _moduleData.Modifiers)
            {
                nearbyObject.AddAttributeModifier(modifier.Value.Name, new AttributeModifier(modifier.Value));
            }
        }

        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }
}


public sealed class PassiveAreaEffectBehaviorModuleData : UpdateModuleData
{
    internal static PassiveAreaEffectBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<PassiveAreaEffectBehaviorModuleData> FieldParseTable = new IniParseTable<PassiveAreaEffectBehaviorModuleData>
    {
        { "EffectRadius", (parser, x) => x.EffectRadius = parser.ParseLong() },
        { "PingDelay", (parser, x) => x.PingDelay = parser.ParseTimeMillisecondsToLogicFrames() },
        { "HealPercentPerSecond", (parser, x) => x.HealPercentPerSecond = parser.ParsePercentage() },
        { "AllowFilter", (parser, x) => x.AllowFilter = ObjectFilter.Parse(parser) },
        { "ModifierName", (parser, x) => x.Modifiers.Add(parser.ParseModifierListReference()) },
        { "UpgradeRequired", (parser, x) => x.UpgradeRequired = parser.ParseUpgradeReference() },
        { "NonStackable", (parser, x) => x.NonStackable = parser.ParseBoolean() },
        { "HealFX", (parser, x) => x.HealFX = parser.ParseFXListReference() },
        { "AntiCategories", (parser, x) => x.AntiCategories = parser.ParseEnumBitArray<ModifierCategory>() }
    };

    public long EffectRadius { get; private set; }
    public LogicFrameSpan PingDelay { get; private set; }
    public Percentage HealPercentPerSecond { get; private set; }
    public ObjectFilter AllowFilter { get; private set; }
    public List<LazyAssetReference<ModifierList>> Modifiers { get; } = new List<LazyAssetReference<ModifierList>>();

    [AddedIn(SageGame.Bfme2)]
    public LazyAssetReference<UpgradeTemplate> UpgradeRequired { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public bool NonStackable { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public LazyAssetReference<FXList> HealFX { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public BitArray<ModifierCategory> AntiCategories { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new PassiveAreaEffectBehavior(gameObject, gameEngine, this);
    }
}
