#nullable enable

using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Logic.AI;
using OpenSage.Logic.Object;

namespace OpenSage.Logic;

public sealed class TeamTemplateInfo : IPersistableObject
{
    private IGame _game;

    public const uint MaxUnitTypes = 7;

    /// <summary>
    /// Name of the team.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// Name of the player-or-team that owns this team.
    /// </summary>
    public readonly string Owner;

    /// <summary>
    /// if true, only one instance of this team can be made.
    /// </summary>
    public readonly bool IsSingleton;

    public readonly string? Description;

    /// <summary>
    /// Quantity of units to create or build.
    /// </summary>
    public readonly List<CreateUnitsInfo> CreateUnitsInfoList = new((int)MaxUnitTypes);

    private readonly string? _homeWaypointName;

    /// <summary>
    /// Spawn location for team.
    /// </summary>
    public readonly Vector3? HomeLocation;

    public bool HasHomeLocation => HomeLocation.HasValue;

    /// <summary>
    /// Script executed when team is created.
    /// </summary>
    public readonly string ScriptOnCreate;

    /// <summary>
    /// Script executed when team is idle.
    /// </summary>
    public readonly string ScriptOnIdle;

    /// <summary>
    /// Number of frames to continue recruiting after the minimum team size is achieved.
    /// </summary>
    public readonly int InitialIdleFrames;

    /// <summary>
    /// Script executed when enemy is sighted.
    /// </summary>
    public readonly string ScriptOnEnemySighted;

    /// <summary>
    /// Script executed when enemy is no longer visible.
    /// </summary>
    public readonly string ScriptOnAllClear;

    /// <summary>
    /// Script executed each time a unit on this team dies.
    /// </summary>
    public readonly string ScriptOnUnitDestroyed;

    /// <summary>
    /// Script executed when DestroyedThreshold of member units are destroyed.
    /// </summary>
    public readonly string ScriptOnDestroyed;

    /// <summary>
    /// OnDestroyed threshold - 1.0 = 100% = all destroyed, .5 = 50% = half of the units destroyed, 0 = useless.
    /// </summary>
    public readonly float DestroyedThreshold;

    /// <summary>
    /// True if other AI teams can recruit.
    /// </summary>
    public readonly bool IsAIRecruitable;

    /// <summary>
    /// True if is base defense team.
    /// </summary>
    public readonly bool IsBaseDefense;

    /// <summary>
    /// True if is a perimeter base defense team.
    /// </summary>
    public readonly bool IsPerimeterDefense;

    /// <summary>
    /// True if team automatically tries to reinforce.
    /// </summary>
    public readonly bool AutomaticallyReinforce;

    /// <summary>
    /// True if transports return to base after unloading.
    /// </summary>
    public readonly bool TransportsReturn;

    /// <summary>
    /// True if the team avoids threats.
    /// </summary>
    public readonly bool AvoidThreats;

    /// <summary>
    /// True if the team attacks the same target unit.
    /// </summary>
    public readonly bool AttackCommonTarget;

    /// <summary>
    /// Maximum number of instances of a team that is not singleton.
    /// </summary>
    public readonly int MaxInstances;

    public int ProductionPriority;

    /// <summary>
    /// Production priority increase on success.
    /// </summary>
    public readonly int ProductionPrioritySuccessIncrease;

    /// <summary>
    /// Production priority decrease on failure.
    /// </summary>
    public readonly int ProductionPriorityFailureDecrease;

    /// <summary>
    /// The initial team attitude.
    /// </summary>
    public readonly AttitudeType InitialTeamAttitude;

    /// <summary>
    /// Unit used to transport the team.
    /// </summary>
    public readonly string TransportUnitType;

    /// <summary>
    /// Waypoint where the reinforcement team starts.
    /// </summary>
    public readonly string StartReinforceWaypoint;

    /// <summary>
    /// If true, team loads into member transports.
    /// </summary>
    public readonly bool TeamStartsFull;

    /// <summary>
    /// True if the transports leave after deploying team.
    /// </summary>
    public readonly bool TransportsExit;

    public readonly VeterancyLevel Veterancy;

    /// <summary>
    /// Script that contains the production conditions.
    /// </summary>
    public readonly string ProductionCondition;

    /// <summary>
    /// If this is true, then when the production condition becomes true, we also execute the actions.
    /// </summary>
    public readonly bool ExecuteActions;

    /// <summary>
    /// Generic scripts for the team.
    /// </summary>
    public readonly string[] TeamGenericScripts;

    public TeamTemplateInfo(IGame game, AssetPropertyCollection dict)
    {
        _game = game;

        Name = dict.GetPropOrNull(TeamKeys.Name)?.GetAsciiString() ?? string.Empty;
        Owner = dict.GetPropOrNull(TeamKeys.Owner)?.GetAsciiString() ?? string.Empty;
        IsSingleton = dict.GetPropOrNull(TeamKeys.IsSingleton)?.GetBoolean() ?? false;
        Description = dict.GetPropOrNull(TeamKeys.Description)?.GetAsciiString() ?? string.Empty;

        for (var i = 1; i <= MaxUnitTypes; i++)
        {
            var min = dict.GetPropOrNull($"{TeamKeys.UnitMinCountPrefix}{i}")?.GetInteger() ?? 0;
            var max = dict.GetPropOrNull($"{TeamKeys.UnitMaxCountPrefix}{i}")?.GetInteger() ?? 0;
            var templateName = dict.GetPropOrNull($"{TeamKeys.UnitTypePrefix}{i}")?.GetAsciiString();

            if (templateName != null && max > 0)
            {
                CreateUnitsInfoList.Add(new CreateUnitsInfo(min, max, templateName));
            }
        }

        _homeWaypointName = dict.GetPropOrNull(TeamKeys.Home)?.GetAsciiString();
        if (_homeWaypointName != null)
        {
            var waypoint = _game.TerrainLogic.GetWaypointByName(_homeWaypointName);

            if (waypoint != null)
            {
                HomeLocation = waypoint.Position;
            }
        }

        ScriptOnCreate = dict.GetPropOrNull(TeamKeys.OnCreateScript)?.GetAsciiString() ?? string.Empty;
        IsAIRecruitable = dict.GetPropOrNull(TeamKeys.IsAIRecruitable)?.GetBoolean() ?? false;
        IsBaseDefense = dict.GetPropOrNull(TeamKeys.IsBaseDefense)?.GetBoolean() ?? false;
        IsPerimeterDefense = dict.GetPropOrNull(TeamKeys.IsPerimeterDefense)?.GetBoolean() ?? false;
        AutomaticallyReinforce = dict.GetPropOrNull(TeamKeys.AutoReinforce)?.GetBoolean() ?? false;

        InitialTeamAttitude = (AttitudeType)(dict.GetPropOrNull(TeamKeys.Aggressiveness)?.GetInteger() ?? 0);

        TransportsReturn = dict.GetPropOrNull(TeamKeys.TransportsReturn)?.GetBoolean() ?? false;
        AvoidThreats = dict.GetPropOrNull(TeamKeys.AvoidThreats)?.GetBoolean() ?? false;
        AttackCommonTarget = dict.GetPropOrNull(TeamKeys.AttackCommonTarget)?.GetBoolean() ?? false;
        MaxInstances = dict.GetPropOrNull(TeamKeys.MaxInstances)?.GetInteger() ?? 0;

        ScriptOnIdle = dict.GetPropOrNull(TeamKeys.OnIdleScript)?.GetAsciiString() ?? string.Empty;
        InitialIdleFrames = dict.GetPropOrNull(TeamKeys.InitialIdleFrames)?.GetInteger() ?? 0;
        ScriptOnEnemySighted = dict.GetPropOrNull(TeamKeys.EnemySightedScript)?.GetAsciiString() ?? string.Empty;
        ScriptOnAllClear = dict.GetPropOrNull(TeamKeys.AllClearScript)?.GetAsciiString() ?? string.Empty;
        ScriptOnDestroyed = dict.GetPropOrNull(TeamKeys.OnDestroyedScript)?.GetAsciiString() ?? string.Empty;
        DestroyedThreshold = dict.GetPropOrNull(TeamKeys.DestroyedThreshold)?.GetReal() ?? 0f;
        ScriptOnUnitDestroyed = dict.GetPropOrNull(TeamKeys.OnUnitDestroyedScript)?.GetAsciiString() ?? string.Empty;

        ProductionPriority = dict.GetPropOrNull(TeamKeys.ProductionPriority)?.GetInteger() ?? 0;
        ProductionPrioritySuccessIncrease = dict.GetPropOrNull(TeamKeys.ProductionPrioritySuccessIncrease)?.GetInteger() ?? 0;
        ProductionPriorityFailureDecrease = dict.GetPropOrNull(TeamKeys.ProductionPriorityFailureDecrease)?.GetInteger() ?? 0;

        ProductionCondition = dict.GetPropOrNull(TeamKeys.ProductionCondition)?.GetAsciiString() ?? string.Empty;
        ExecuteActions = dict.GetPropOrNull(TeamKeys.ExecutesActionsOnCreate)?.GetBoolean() ?? false;

        TeamGenericScripts = new string[Team.MaxGenericScripts];
        for (var i = 0; i < Team.MaxGenericScripts; i++)
        {
            var keyName = $"{TeamKeys.GenericScriptHookPrefix}{i}";
            TeamGenericScripts[i] = dict.GetPropOrNull(keyName)?.GetAsciiString() ?? string.Empty;
        }

        TransportUnitType = dict.GetPropOrNull(TeamKeys.Transport)?.GetAsciiString() ?? string.Empty;
        TransportsExit = dict.GetPropOrNull(TeamKeys.TransportsExit)?.GetBoolean() ?? false;
        TeamStartsFull = dict.GetPropOrNull(TeamKeys.StartsFull)?.GetBoolean() ?? false;
        StartReinforceWaypoint = dict.GetPropOrNull(TeamKeys.ReinforcementOrigin)?.GetAsciiString() ?? string.Empty;
        Veterancy = (VeterancyLevel)(dict.GetPropOrNull(TeamKeys.Veterancy)?.GetInteger() ?? 0);
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistInt32(ref ProductionPriority);
    }
}

public record struct CreateUnitsInfo(int MinUnits, int MaxUnits, string UnitThingName);

public static class TeamKeys
{
    public const string Name = "teamName";
    public const string Owner = "teamOwner";
    public const string IsSingleton = "teamIsSingleton";
    public const string Description = "teamDescription";
    public const string Home = "teamHome";

    // Unit type keys have a consistent prefix followed by a number (1-7)
    public const string UnitTypePrefix = "teamUnitType";
    public const string UnitMinCountPrefix = "teamUnitMinCount";
    public const string UnitMaxCountPrefix = "teamUnitMaxCount";

    public const string OnCreateScript = "teamOnCreateScript";
    public const string OnIdleScript = "teamOnIdleScript";
    public const string InitialIdleFrames = "teamInitialIdleFrames";
    public const string OnUnitDestroyedScript = "teamOnUnitDestroyedScript";
    public const string OnDestroyedScript = "teamOnDestroyedScript";
    public const string DestroyedThreshold = "teamDestroyedThreshold";
    public const string EnemySightedScript = "teamEnemySightedScript";
    public const string AllClearScript = "teamAllClearScript";
    public const string IsAIRecruitable = "teamIsAIRecruitable";
    public const string IsBaseDefense = "teamIsBaseDefense";
    public const string IsPerimeterDefense = "teamIsPerimeterDefense";
    public const string AutoReinforce = "teamAutoReinforce";
    public const string Aggressiveness = "teamAggressiveness";
    public const string TransportsReturn = "teamTransportsReturn";
    public const string AvoidThreats = "teamAvoidThreats";
    public const string AttackCommonTarget = "teamAttackCommonTarget";
    public const string MaxInstances = "teamMaxInstances";
    public const string ProductionCondition = "teamProductionCondition";
    public const string ProductionPriority = "teamProductionPriority";
    public const string ProductionPrioritySuccessIncrease = "teamProductionPrioritySuccessIncrease";
    public const string ProductionPriorityFailureDecrease = "teamProductionPriorityFailureDecrease";
    public const string Transport = "teamTransport";
    public const string ReinforcementOrigin = "teamReinforcementOrigin";
    public const string StartsFull = "teamStartsFull";
    public const string TransportsExit = "teamTransportsExit";
    public const string Veterancy = "teamVeterancy";
    public const string ExecutesActionsOnCreate = "teamExecutesActionsOnCreate";

    // Generic script hooks have a consistent prefix followed by a number
    public const string GenericScriptHookPrefix = "teamGenericScriptHook";
}
