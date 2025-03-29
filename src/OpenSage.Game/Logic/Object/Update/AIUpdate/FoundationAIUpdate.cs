using System;
using System.Linq;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

[AddedIn(SageGame.Bfme)]
public class FoundationAIUpdate : AIUpdate
{
    internal override FoundationAIUpdateModuleData ModuleData { get; }

    private LogicFrame _waitUntil;
    private LogicFrameSpan _updateInterval;

    //TODO: rather notify this when the corresponding order is processed and update again when the object is dead/destroyed
    internal FoundationAIUpdate(GameObject gameObject, GameEngine gameEngine, FoundationAIUpdateModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        ModuleData = moduleData;
        _updateInterval = new LogicFrameSpan((uint)MathF.Ceiling(GameEngine.LogicFramesPerSecond / 2)); // 0.5s, we do not have to check every frame
    }

    internal override void Update(BehaviorUpdateContext context)
    {
        CheckForStructure(context, GameObject, ref _waitUntil, _updateInterval);
    }

    internal static void CheckForStructure(BehaviorUpdateContext context, GameObject obj, ref LogicFrame waitUntil, LogicFrameSpan interval)
    {
        if (context.LogicFrame < waitUntil)
        {
            return;
        }

        waitUntil = context.LogicFrame + interval;

        var collidingObjects = context.GameEngine.Game.PartitionCellManager.QueryObjects(
            obj,
            obj.Translation,
            obj.Geometry.BoundingCircleRadius,
            new PartitionQueries.KindOfQuery(ObjectKinds.Structure));

        if (collidingObjects.Any())
        {
            obj.SetSelectable(false);
            obj.Hidden = true;
            return;
        }

        obj.SetSelectable(true);
        obj.Hidden = false;
    }
}


public class FoundationAIUpdateModuleData : AIUpdateModuleData
{
    internal new static FoundationAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    internal new static readonly IniParseTable<FoundationAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
        .Concat(new IniParseTable<FoundationAIUpdateModuleData>
        {
            { "BuildVariation", (parser, x) => x.BuildVariation = parser.ParseInteger() },
        });

    [AddedIn(SageGame.Bfme2)]
    public int BuildVariation { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new FoundationAIUpdate(gameObject, gameEngine, this);
    }
}
