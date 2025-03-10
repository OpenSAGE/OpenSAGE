﻿using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;

namespace OpenSage.Logic.Object;

public sealed class FXListDie : DieModule
{
    private readonly FXListDieModuleData _moduleData;

    internal FXListDie(GameObject gameObject, GameContext context, FXListDieModuleData moduleData)
        : base(gameObject, context, moduleData)
    {
        _moduleData = moduleData;
    }

    private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
    {
        _moduleData.DeathFX.Value.Execute(new FXListExecutionContext(
            context.GameObject.Rotation,
            context.GameObject.Translation,
            context.GameContext));
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

public sealed class FXListDieModuleData : DieModuleData
{
    internal static FXListDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<FXListDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
        .Concat(new IniParseTable<FXListDieModuleData>
        {
            { "DeathFX", (parser, x) => x.DeathFX = parser.ParseFXListReference() },
            { "OrientToObject", (parser, x) => x.OrientToObject = parser.ParseBoolean() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() }
        });

    public LazyAssetReference<FXList> DeathFX { get; private set; }
    public bool OrientToObject { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public bool StartsActive { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public string[] ConflictsWith { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public string[] TriggeredBy { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
    {
        return new FXListDie(gameObject, context, this);
    }
}
