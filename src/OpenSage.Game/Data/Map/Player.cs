#nullable enable

using System.Diagnostics;
using System.IO;
using OpenSage.Logic.Map;
using OpenSage.Scripting;

namespace OpenSage.Data.Map;

// This corresponds to the SidesInfo class in Generals
[DebuggerDisplay("Player '{Name}'")]
public sealed class Player
{
    public Player(AssetPropertyCollection properties)
    {
        ParseProperties(properties);
    }

    public Player() { }

    public BuildListInfo[] BuildList { get; internal set; } = [];
    public ScriptList Scripts { get; internal set; } = new ScriptList();

    /// <summary>
    /// internal identifier for player.
    /// NOTE: an empty string for playerName is reserved to denote the Neutral player.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// true if this player is to be human-controlled. false if computer-controlled.
    /// </summary>
    public bool IsHuman { get; set; }

    /// <summary>
    /// true if this player is in skirmish or multiplayer, rather than solo.
    /// </summary>
    public bool IsSkirmish { get; set; }

    /// <summary>
    /// playertemplate to use to construct the player
    /// </summary>
    public string Faction { get; set; } = string.Empty;

    /// <summary>
    /// displayable name for player.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// whitespace-separated list of player(s) we start as enemies with.
    /// </summary>
    public string Enemies { get; set; } = string.Empty;

    /// <summary>
    /// whitespace-separated list of player(s) we start as allies with
    /// </summary>
    public string Allies { get; set; } = string.Empty;

    /// <summary>
    /// (optional) if present, amount of money the player starts with
    /// </summary>
    public int? StartMoney { get; set; }

    /// <summary>
    /// (optional) if present, color to use for player (overrides player color)
    /// </summary>
    public int? Color { get; set; }

    /// <summary>
    /// (optional) if present, color to use for player at night (overrides player color)
    /// </summary>
    public int? NightColor { get; set; }

    /// <summary>
    /// (optional) if present, Player_*_Start waypoint to use for player (overrides InitialCameraPosition)
    /// </summary>
    public int? MultiplayerStartIndex { get; set; }

    /// <summary>
    /// (optional) if present, Difficulty level to use for player (overrides global difficulty)
    /// </summary>
    public int? SkirmishDifficulty { get; set; }

    /// <summary>
    /// (optional) if present, signifies if the player is the local player
    /// </summary>
    public bool? MultiplayerIsLocal { get; set; }

    /// <summary>
    /// (optional) if present, signifies if the player has preordered
    /// </summary>
    public bool? IsPreorder { get; set; }

    private void ParseProperties(AssetPropertyCollection properties)
    {
        Name = properties.GetPropOrNull("playerName")?.GetAsciiString() ?? string.Empty;
        IsHuman = properties.GetPropOrNull("playerIsHuman")?.GetBoolean() ?? false;
        IsSkirmish = properties.GetPropOrNull("playerIsSkirmish")?.GetBoolean() ?? false;
        Faction = properties.GetPropOrNull("playerFaction")?.GetAsciiString() ?? string.Empty;
        DisplayName = properties.GetPropOrNull("playerDisplayName")?.GetUnicodeString() ?? string.Empty;
        Enemies = properties.GetPropOrNull("playerEnemies")?.GetAsciiString() ?? string.Empty;
        Allies = properties.GetPropOrNull("playerAllies")?.GetAsciiString() ?? string.Empty;
        StartMoney = properties.GetPropOrNull("playerStartMoney")?.GetInteger();
        Color = properties.GetPropOrNull("playerColor")?.GetInteger();
        NightColor = properties.GetPropOrNull("playerNightColor")?.GetInteger();
        MultiplayerStartIndex = properties.GetPropOrNull("multiplayerStartIndex")?.GetInteger();
        SkirmishDifficulty = properties.GetPropOrNull("skirmishDifficulty")?.GetInteger();
        MultiplayerIsLocal = properties.GetPropOrNull("multiplayerIsLocal")?.GetBoolean();
        IsPreorder = properties.GetPropOrNull("playerIsPreorder")?.GetBoolean();
    }

    private void SerializeProperties(AssetPropertyCollection properties)
    {
        properties.AddAsciiString("playerName", Name);
        properties.AddBoolean("playerIsHuman", IsHuman);
        properties.AddBoolean("playerIsSkirmish", IsSkirmish);
        properties.AddAsciiString("playerFaction", Faction);
        properties.AddUnicodeString("playerDisplayName", DisplayName);
        properties.AddAsciiString("playerEnemies", Enemies);
        properties.AddAsciiString("playerAllies", Allies);
        properties.AddNullableInteger("playerStartMoney", StartMoney);
        properties.AddNullableInteger("playerColor", Color);
        properties.AddNullableInteger("playerNightColor", NightColor);
        properties.AddNullableInteger("multiplayerStartIndex", MultiplayerStartIndex);
        properties.AddNullableInteger("skirmishDifficulty", SkirmishDifficulty);
        properties.AddNullableBoolean("multiplayerIsLocal", MultiplayerIsLocal);
        properties.AddNullableBoolean("playerIsPreorder", IsPreorder);
    }

    internal static Player CreateNeutralPlayer() => CreatePlayer("Neutral");

    internal static Player CreateCivilianPlayer() => CreatePlayer("Civilian");

    private static Player CreatePlayer(string side)
    {
        var result = new Player
        {
            Name = $"plyr{side}",
            IsHuman = false,
            Faction = $"Faction{side}",
            DisplayName = side
        };

        return result;
    }

    internal static Player Parse(BinaryReader reader, MapParseContext context, ushort sidesListVersion, bool mapHasAssetList)
    {
        var player = new Player(AssetPropertyCollection.Parse(reader, context));
        var numBuildListItems = reader.ReadUInt32();
        var buildListItems = new BuildListInfo[numBuildListItems];
        var buildListInfoFields = GetBuildListInfoFields(sidesListVersion, mapHasAssetList);

        for (var i = 0; i < numBuildListItems; i++)
        {
            buildListItems[i] = BuildListInfo.Parse(reader, buildListInfoFields);
        }

        player.BuildList = buildListItems;

        return player;
    }

    internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames, ushort sidesListVersion, bool mapHasAssetList)
    {
        var properties = new AssetPropertyCollection();
        SerializeProperties(properties);
        properties.WriteTo(writer, assetNames);

        writer.Write((uint)BuildList.Length);

        var buildListInfoFields = GetBuildListInfoFields(sidesListVersion, mapHasAssetList);

        foreach (var buildListItem in BuildList)
        {
            buildListItem.WriteTo(writer, buildListInfoFields);
        }
    }

    private static BuildListInfo.IncludeFields GetBuildListInfoFields(ushort sidesListVersion, bool mapHasAssetList)
    {
        // TODO: Is this correct for both BFME and C&C3?
        // See similar method in BuildLists.cs
        return (sidesListVersion, mapHasAssetList) switch
        {
            ( >= 6, true) => BuildListInfo.IncludeFields.Cnc3UnknownBoolean,
            _ => BuildListInfo.IncludeFields.Default
        };
    }
}
