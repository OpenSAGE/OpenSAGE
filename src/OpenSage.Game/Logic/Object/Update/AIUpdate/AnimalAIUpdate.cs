using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

[AddedIn(SageGame.Bfme)]
public class AnimalAIUpdate : AIUpdate
{
    internal override AnimalAIUpdateModuleData ModuleData { get; }

    internal AnimalAIUpdate(GameObject gameObject, IGameEngine gameEngine, AnimalAIUpdateModuleData moduleData) : base(gameObject, gameEngine, moduleData)
    {
        ModuleData = moduleData;
    }
}


public sealed class AnimalAIUpdateModuleData : AIUpdateModuleData
{
    internal new static AnimalAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private new static readonly IniParseTable<AnimalAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
        .Concat(new IniParseTable<AnimalAIUpdateModuleData>
        {
            { "FleeRange", (parser, x) => x.FleeRange = parser.ParseInteger() },
            { "FleeDistance", (parser, x) => x.FleeDistance = parser.ParseInteger() },
            { "WanderPercentage", (parser, x) => x.WanderPercentage = parser.ParsePercentage() },
            { "MaxWanderDistance", (parser, x) => x.MaxWanderDistance = parser.ParseInteger() },
            { "MaxWanderRadius", (parser, x) => x.MaxWanderRadius = parser.ParseInteger() },
            { "UpdateTimer", (parser, x) => x.UpdateTimer = parser.ParseInteger() },
            { "AfraidOfCastles", (parser, x) => x.AfraidOfCastles = parser.ParseBoolean() }
        });

    public int FleeRange { get; private set; }
    public int FleeDistance { get; private set; }
    public Percentage WanderPercentage { get; private set; }
    public int MaxWanderDistance { get; private set; }
    public int MaxWanderRadius { get; private set; }
    public int UpdateTimer { get; private set; }
    public bool AfraidOfCastles { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new AnimalAIUpdate(gameObject, gameEngine, this); ;
    }
}
