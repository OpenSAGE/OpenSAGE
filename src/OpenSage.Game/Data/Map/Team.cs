#nullable enable

using System.Diagnostics;
using System.IO;

namespace OpenSage.Data.Map;

[DebuggerDisplay("Team '{Name}'")]
public sealed class Team
{
    public Team(AssetPropertyCollection properties)
    {
        ParseProperties(properties);
    }

    public Team() { }

    /// <summary>
    /// name of team.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// name of the player-or-team that owns this team.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// if true, only one instance of this team can be made.
    /// </summary>
    public bool IsSingleton { get; set; }

    /// <summary>
    /// Name of waypoint where team spawns.
    /// </summary>
    public string Home { get; set; } = string.Empty;

    /// <summary>
    /// Thing name of first batch of units.
    /// </summary>
    public string? UnitType1 { get; set; }

    /// <summary>
    /// Minimum number of units of type 1 to build.
    /// </summary>
    public int? UnitMinCount1 { get; set; }

    /// <summary>
    /// Maximum number of units of type 1 to build.
    /// </summary>
    public int? UnitMaxCount1 { get; set; }

    /// <summary>
    /// Thing name of second batch of units.
    /// </summary>
    public string? UnitType2 { get; set; }

    /// <summary>
    /// Minimum number of units of type 2 to build.
    /// </summary>
    public int? UnitMinCount2 { get; set; }

    /// <summary>
    /// Maximum number of units of type 2 to build.
    /// </summary>
    public int? UnitMaxCount2 { get; set; }

    /// <summary>
    /// Thing name of third batch of units.
    /// </summary>
    public string? UnitType3 { get; set; }

    /// <summary>
    /// Minimum number of units of type 3 to build.
    /// </summary>
    public int? UnitMinCount3 { get; set; }

    /// <summary>
    /// Maximum number of units of type 3 to build.
    /// </summary>
    public int? UnitMaxCount3 { get; set; }

    /// <summary>
    /// Thing name of fourth batch of units.
    /// </summary>
    public string? UnitType4 { get; set; }

    /// <summary>
    /// Minimum number of units of type 4 to build.
    /// </summary>
    public int? UnitMinCount4 { get; set; }

    /// <summary>
    /// Maximum number of units of type 4 to build.
    /// </summary>
    public int? UnitMaxCount4 { get; set; }

    /// <summary>
    /// Thing name of fifth batch of units.
    /// </summary>
    public string? UnitType5 { get; set; }

    /// <summary>
    /// Minimum number of units of type 5 to build.
    /// </summary>
    public int? UnitMinCount5 { get; set; }

    /// <summary>
    /// Maximum number of units of type 5 to build.
    /// </summary>
    public int? UnitMaxCount5 { get; set; }

    /// <summary>
    /// Thing name of sixth batch of units.
    /// </summary>
    public string? UnitType6 { get; set; }

    /// <summary>
    /// Minimum number of units of type 6 to build.
    /// </summary>
    public int? UnitMinCount6 { get; set; }

    /// <summary>
    /// Maximum number of units of type 6 to build.
    /// </summary>
    public int? UnitMaxCount6 { get; set; }

    /// <summary>
    /// Thing name of seventh batch of units.
    /// </summary>
    public string? UnitType7 { get; set; }

    /// <summary>
    /// Minimum number of units of type 7 to build.
    /// </summary>
    public int? UnitMinCount7 { get; set; }

    /// <summary>
    /// Maximum number of units of type 7 to build.
    /// </summary>
    public int? UnitMaxCount7 { get; set; }

    /// <summary>
    /// Name of script to execute when team is created for AI.
    /// </summary>
    public string? OnCreateScript { get; set; }

    /// <summary>
    /// Name of script to execute when AI team is in idle state.
    /// </summary>
    public string? OnIdleScript { get; set; }

    /// <summary>
    /// Number of frames to wait after achieving the minimum number of units to try to recruit up to the maximum number of units.
    /// </summary>
    public int? InitialIdleFrames { get; set; }

    /// <summary>
    /// Name of a script to run each time a member on this team is destroyed.
    /// </summary>
    public string? OnUnitDestroyedScript { get; set; } = string.Empty;

    /// <summary>
    /// Name of script to execute when team is destroyed.
    /// </summary>
    public string? OnDestroyedScript { get; set; } = string.Empty;

    /// <summary>
    /// Percent destroyed to trigger OnDamagedScript. 1.0 = 100% = trigger when team is all destroyed. 0 = 0% = trigger when first team member is destroyed.
    /// </summary>
    public float? DestroyedThreshold { get; set; }

    /// <summary>
    /// Name of script to execute when enemy is sighted.
    /// </summary>
    public string? EnemySightedScript { get; set; } = string.Empty;

    /// <summary>
    /// Name of script to execute when all clear.
    /// </summary>
    public string? AllClearScript { get; set; } = string.Empty;

    /// <summary>
    /// True if team auto reinforces.
    /// </summary>
    public bool? AutoReinforce { get; set; }

    /// <summary>
    /// True if team can be recruited from by other ai teams.
    /// </summary>
    public bool? IsAIRecruitable { get; set; }

    /// <summary>
    /// True if team can be recruited from by other ai teams.
    /// </summary>
    public bool? IsBaseDefense { get; set; }

    /// <summary>
    /// True if team can be recruited from by other ai teams.
    /// </summary>
    public bool? IsPerimeterDefense { get; set; }

    /// <summary>
    /// The aggressiveness of the team: -2 Sleep, -1 Passive, 0 Normal, 1 Alert, 2 Aggressive
    /// </summary>
    public int? Aggressiveness { get; set; }

    /// <summary>
    /// True if transports return to base.
    /// </summary>
    public bool? TransportsReturn { get; set; }

    /// <summary>
    /// True if the team avoids threats.
    /// </summary>
    public bool? AvoidThreats { get; set; }

    /// <summary>
    /// True if the team attacks the same target when auto-acquiring targets.
    /// </summary>
    public bool? AttackCommonTarget { get; set; }

    /// <summary>
    /// If a team is not singleton, max number of this team at one time.
    /// </summary>
    public int? MaxInstances { get; set; }

    /// <summary>
    /// Description comment field.
    /// </summary>
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Script that contains the conditions for team production.
    /// </summary>
    public string? ProductionCondition { get; set; } = string.Empty;

    /// <summary>
    /// Team's production priority.
    /// </summary>
    public int? ProductionPriority { get; set; }

    /// <summary>
    /// Team's production priority increase on success amount.
    /// </summary>
    public int? ProductionPrioritySuccessIncrease { get; set; }

    /// <summary>
    /// Team's production priority decrease on failure amount.
    /// </summary>
    public int? ProductionPriorityFailureDecrease { get; set; }

    /// <summary>
    /// Team's transport unit for reinforcements.
    /// </summary>
    public string? Transport { get; set; } = string.Empty;

    /// <summary>
    /// Team's origin for reinforcements.
    /// </summary>
    public string? ReinforcementOrigin { get; set; } = string.Empty;

    /// <summary>
    /// Team's starts full flag (pack into transports) for reinforcement teams.
    /// </summary>
    public bool? StartsFull { get; set; }

    /// <summary>
    /// Team's transports exit (leave the map) flag for reinforcement teams.
    /// </summary>
    public bool? TransportsExit { get; set; }

    /// <summary>
    /// What veterancy does this object have: 0 green, 1 normal, 2 veteran, 3 elite
    /// </summary>
    public int? Veterancy { get; set; }

    /// <summary>
    /// Does the team execute the actions in
    /// </summary>
    public bool? ExecutesActionsOnCreate { get; set; }

    /// <summary>
    /// This key is used in the same way that objectGrantUpgrades is used. See it for an explanation.
    /// </summary>
    public string? GenericScriptHook { get; set; }

    private void ParseProperties(AssetPropertyCollection properties)
    {
        Name = properties.GetPropOrNull("teamName")?.GetAsciiString() ?? string.Empty;
        Owner = properties.GetPropOrNull("teamOwner")?.GetAsciiString();
        IsSingleton = properties.GetPropOrNull("teamIsSingleton")?.GetBoolean() ?? false;
        Home = properties.GetPropOrNull("teamHome")?.GetAsciiString() ?? string.Empty;
        UnitType1 = properties.GetPropOrNull("teamUnitType1")?.GetAsciiString();
        UnitMinCount1 = properties.GetPropOrNull("teamUnitMinCount1")?.GetInteger();
        UnitMaxCount1 = properties.GetPropOrNull("teamUnitMaxCount1")?.GetInteger();
        UnitType2 = properties.GetPropOrNull("teamUnitType2")?.GetAsciiString();
        UnitMinCount2 = properties.GetPropOrNull("teamUnitMinCount2")?.GetInteger();
        UnitMaxCount2 = properties.GetPropOrNull("teamUnitMaxCount2")?.GetInteger();
        UnitType3 = properties.GetPropOrNull("teamUnitType3")?.GetAsciiString();
        UnitMinCount3 = properties.GetPropOrNull("teamUnitMinCount3")?.GetInteger();
        UnitMaxCount3 = properties.GetPropOrNull("teamUnitMaxCount3")?.GetInteger();
        UnitType4 = properties.GetPropOrNull("teamUnitType4")?.GetAsciiString();
        UnitMinCount4 = properties.GetPropOrNull("teamUnitMinCount4")?.GetInteger();
        UnitMaxCount4 = properties.GetPropOrNull("teamUnitMaxCount4")?.GetInteger();
        UnitType5 = properties.GetPropOrNull("teamUnitType5")?.GetAsciiString();
        UnitMinCount5 = properties.GetPropOrNull("teamUnitMinCount5")?.GetInteger();
        UnitMaxCount5 = properties.GetPropOrNull("teamUnitMaxCount5")?.GetInteger();
        UnitType6 = properties.GetPropOrNull("teamUnitType6")?.GetAsciiString();
        UnitMinCount6 = properties.GetPropOrNull("teamUnitMinCount6")?.GetInteger();
        UnitMaxCount6 = properties.GetPropOrNull("teamUnitMaxCount6")?.GetInteger();
        UnitType7 = properties.GetPropOrNull("teamUnitType7")?.GetAsciiString();
        UnitMinCount7 = properties.GetPropOrNull("teamUnitMinCount7")?.GetInteger();
        UnitMaxCount7 = properties.GetPropOrNull("teamUnitMaxCount7")?.GetInteger();
        OnCreateScript = properties.GetPropOrNull("teamOnCreateScript")?.GetAsciiString();
        OnIdleScript = properties.GetPropOrNull("teamOnIdleScript")?.GetAsciiString();
        InitialIdleFrames = properties.GetPropOrNull("teamInitialIdleFrames")?.GetInteger();
        OnUnitDestroyedScript = properties.GetPropOrNull("teamOnUnitDestroyedScript")?.GetAsciiString();
        OnDestroyedScript = properties.GetPropOrNull("teamOnDestroyedScript")?.GetAsciiString();
        DestroyedThreshold = properties.GetPropOrNull("teamDestroyedThreshold")?.GetReal();
        EnemySightedScript = properties.GetPropOrNull("teamEnemySightedScript")?.GetAsciiString();
        AllClearScript = properties.GetPropOrNull("teamAllClearScript")?.GetAsciiString();
        AutoReinforce = properties.GetPropOrNull("teamAutoReinforce")?.GetBoolean();
        IsAIRecruitable = properties.GetPropOrNull("teamIsAIRecruitable")?.GetBoolean();
        IsBaseDefense = properties.GetPropOrNull("teamIsBaseDefense")?.GetBoolean();
        IsPerimeterDefense = properties.GetPropOrNull("teamIsPerimeterDefense")?.GetBoolean();
        Aggressiveness = properties.GetPropOrNull("teamAggressiveness")?.GetInteger();
        TransportsReturn = properties.GetPropOrNull("teamTransportsReturn")?.GetBoolean();
        AvoidThreats = properties.GetPropOrNull("teamAvoidThreats")?.GetBoolean();
        AttackCommonTarget = properties.GetPropOrNull("teamAttackCommonTarget")?.GetBoolean();
        MaxInstances = properties.GetPropOrNull("teamMaxInstances")?.GetInteger();
        Description = properties.GetPropOrNull("teamDescription")?.GetAsciiString();
        ProductionCondition = properties.GetPropOrNull("teamProductionCondition")?.GetAsciiString();
        ProductionPriority = properties.GetPropOrNull("teamProductionPriority")?.GetInteger();
        ProductionPrioritySuccessIncrease = properties.GetPropOrNull("teamProductionPrioritySuccessIncrease")?.GetInteger();
        ProductionPriorityFailureDecrease = properties.GetPropOrNull("teamProductionPriorityFailureDecrease")?.GetInteger();
        Transport = properties.GetPropOrNull("teamTransport")?.GetAsciiString();
        ReinforcementOrigin = properties.GetPropOrNull("teamReinforcementOrigin")?.GetAsciiString();
        StartsFull = properties.GetPropOrNull("teamStartsFull")?.GetBoolean();
        TransportsExit = properties.GetPropOrNull("teamTransportsExit")?.GetBoolean();
        Veterancy = properties.GetPropOrNull("teamVeterancy")?.GetInteger();
        ExecutesActionsOnCreate = properties.GetPropOrNull("teamExecutesActionsOnCreate")?.GetBoolean();
        GenericScriptHook = properties.GetPropOrNull("teamGenericScriptHook")?.GetAsciiString();
    }

    private void SerializeProperties(AssetPropertyCollection properties)
    {
        properties.AddAsciiString("teamName", Name);
        properties.AddNullableAsciiString("teamOwner", Owner);
        properties.AddBoolean("teamIsSingleton", IsSingleton);
        properties.AddAsciiString("teamHome", Home);
        properties.AddNullableAsciiString("teamUnitType1", UnitType1);
        properties.AddNullableInteger("teamUnitMinCount1", UnitMinCount1);
        properties.AddNullableInteger("teamUnitMaxCount1", UnitMaxCount1);
        properties.AddNullableAsciiString("teamUnitType2", UnitType2);
        properties.AddNullableInteger("teamUnitMinCount2", UnitMinCount2);
        properties.AddNullableInteger("teamUnitMaxCount2", UnitMaxCount2);
        properties.AddNullableAsciiString("teamUnitType3", UnitType3);
        properties.AddNullableInteger("teamUnitMinCount3", UnitMinCount3);
        properties.AddNullableInteger("teamUnitMaxCount3", UnitMaxCount3);
        properties.AddNullableAsciiString("teamUnitType4", UnitType4);
        properties.AddNullableInteger("teamUnitMinCount4", UnitMinCount4);
        properties.AddNullableInteger("teamUnitMaxCount4", UnitMaxCount4);
        properties.AddNullableAsciiString("teamUnitType5", UnitType5);
        properties.AddNullableInteger("teamUnitMinCount5", UnitMinCount5);
        properties.AddNullableInteger("teamUnitMaxCount5", UnitMaxCount5);
        properties.AddNullableAsciiString("teamUnitType6", UnitType6);
        properties.AddNullableInteger("teamUnitMinCount6", UnitMinCount6);
        properties.AddNullableInteger("teamUnitMaxCount6", UnitMaxCount6);
        properties.AddNullableAsciiString("teamUnitType7", UnitType7);
        properties.AddNullableInteger("teamUnitMinCount7", UnitMinCount7);
        properties.AddNullableInteger("teamUnitMaxCount7", UnitMaxCount7);
        properties.AddNullableAsciiString("teamOnCreateScript", OnCreateScript);
        properties.AddNullableAsciiString("teamOnIdleScript", OnIdleScript);
        properties.AddNullableInteger("teamInitialIdleFrames", InitialIdleFrames);
        properties.AddNullableAsciiString("teamOnUnitDestroyedScript", OnUnitDestroyedScript);
        properties.AddNullableAsciiString("teamOnDestroyedScript", OnDestroyedScript);
        properties.AddNullableReal("teamDestroyedThreshold", DestroyedThreshold);
        properties.AddNullableAsciiString("teamEnemySightedScript", EnemySightedScript);
        properties.AddNullableAsciiString("teamAllClearScript", AllClearScript);
        properties.AddNullableBoolean("teamAutoReinforce", AutoReinforce);
        properties.AddNullableBoolean("teamIsAIRecruitable", IsAIRecruitable);
        properties.AddNullableBoolean("teamIsBaseDefense", IsBaseDefense);
        properties.AddNullableBoolean("teamIsPerimeterDefense", IsPerimeterDefense);
        properties.AddNullableInteger("teamAggressiveness", Aggressiveness);
        properties.AddNullableBoolean("teamTransportsReturn", TransportsReturn);
        properties.AddNullableBoolean("teamAvoidThreats", AvoidThreats);
        properties.AddNullableBoolean("teamAttackCommonTarget", AttackCommonTarget);
        properties.AddNullableInteger("teamMaxInstances", MaxInstances);
        properties.AddNullableAsciiString("teamDescription", Description);
        properties.AddNullableAsciiString("teamProductionCondition", ProductionCondition);
        properties.AddNullableInteger("teamProductionPriority", ProductionPriority);
        properties.AddNullableInteger("teamProductionPrioritySuccessIncrease", ProductionPrioritySuccessIncrease);
        properties.AddNullableInteger("teamProductionPriorityFailureDecrease", ProductionPriorityFailureDecrease);
        properties.AddNullableAsciiString("teamTransport", Transport);
        properties.AddNullableAsciiString("teamReinforcementOrigin", ReinforcementOrigin);
        properties.AddNullableBoolean("teamStartsFull", StartsFull);
        properties.AddNullableBoolean("teamTransportsExit", TransportsExit);
        properties.AddNullableInteger("teamVeterancy", Veterancy);
        properties.AddNullableBoolean("teamExecutesActionsOnCreate", ExecutesActionsOnCreate);
        properties.AddNullableAsciiString("teamGenericScriptHook", GenericScriptHook);
    }

    internal static Team Parse(BinaryReader reader, MapParseContext context)
    {
        return new Team(AssetPropertyCollection.Parse(reader, context));
    }

    internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
    {
        var properties = new AssetPropertyCollection();
        SerializeProperties(properties);
        properties.WriteTo(writer, assetNames);
    }
}
