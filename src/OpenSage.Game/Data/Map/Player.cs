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
    public AssetPropertyCollection Properties { get; private set; }

    public Player(AssetPropertyCollection properties)
    {
        Properties = properties;
    }

    public Player()
    {
        Properties = [];
    }

    public BuildListInfo[]? BuildList { get; internal set; } = [];
    public ScriptList? Scripts { get; internal set; } = new ScriptList();

    /// <summary>
    /// internal identifier for player.
    /// NOTE: an empty string for playerName is reserved to denote the Neutral player.
    /// </summary>
    public string? Name => Properties.GetPropOrNull(PlayerKeys.Name)?.GetAsciiString();

    /// <summary>
    /// true if this player is to be human-controlled. false if computer-controlled.
    /// </summary>
    public bool? IsHuman => Properties.GetPropOrNull(PlayerKeys.IsHuman)?.GetBoolean();

    /// <summary>
    /// true if this player is in skirmish or multiplayer, rather than solo.
    /// </summary>
    public bool? IsSkirmish => Properties.GetPropOrNull(PlayerKeys.IsSkirmish)?.GetBoolean();

    /// <summary>
    /// playertemplate to use to construct the player
    /// </summary>
    public string? Faction => Properties.GetPropOrNull(PlayerKeys.Faction)?.GetAsciiString();

    /// <summary>
    /// displayable name for player.
    /// </summary>
    public string? DisplayName => Properties.GetPropOrNull(PlayerKeys.DisplayName)?.GetUnicodeString();

    /// <summary>
    /// whitespace-separated list of player(s) we start as allies with
    /// </summary>
    public string? Allies
    {
        get
        {
            return Properties.GetPropOrNull(PlayerKeys.Allies)?.GetAsciiString();
        }
        set
        {
            Properties.TryGetValue(PlayerKeys.Allies, out var property);
            if (property == null)
            {
                Properties.AddAsciiString(PlayerKeys.Allies, value ?? string.Empty);
            }
            else
            {
                property.UpdateValue(value ?? string.Empty);
            }
        }
    }

    /// <summary>
    /// whitespace-separated list of player(s) we start as enemies with.
    /// </summary>
    public string? Enemies
    {
        get
        {
            return Properties.GetPropOrNull(PlayerKeys.Enemies)?.GetAsciiString();
        }
        set
        {
            Properties.TryGetValue(PlayerKeys.Enemies, out var property);
            if (property == null)
            {
                Properties.AddAsciiString(PlayerKeys.Enemies, value ?? string.Empty);
            }
            else
            {
                property.UpdateValue(value ?? string.Empty);
            }
        }
    }

    /// <summary>
    /// (optional) if present, amount of money the player starts with
    /// </summary>
    public int? StartMoney => Properties.GetPropOrNull(PlayerKeys.StartMoney)?.GetInteger();

    /// <summary>
    /// (optional) if present, color to use for player (overrides player color)
    /// </summary>
    public int? Color => Properties.GetPropOrNull(PlayerKeys.Color)?.GetInteger();

    /// <summary>
    /// (optional) if present, color to use for player at night (overrides player color)
    /// </summary>
    public int? NightColor => Properties.GetPropOrNull(PlayerKeys.NightColor)?.GetInteger();

    /// <summary>
    /// (optional) if present, Player_*_Start waypoint to use for player (overrides InitialCameraPosition)
    /// </summary>
    public int? MultiplayerStartIndex => Properties.GetPropOrNull(PlayerKeys.MultiplayerStartIndex)?.GetInteger();

    /// <summary>
    /// (optional) if present, Difficulty level to use for player (overrides global difficulty)
    /// </summary>
    public int? SkirmishDifficulty => Properties.GetPropOrNull(PlayerKeys.SkirmishDifficulty)?.GetInteger();

    /// <summary>
    /// (optional) if present, signifies if the player is the local player
    /// </summary>
    public bool? MultiplayerIsLocal => Properties.GetPropOrNull(PlayerKeys.MultiplayerIsLocal)?.GetBoolean();

    /// <summary>
    /// (optional) if present, signifies if the player has preordered
    /// </summary>
    public bool? IsPreorder => Properties.GetPropOrNull(PlayerKeys.IsPreorder)?.GetBoolean();

    internal static Player CreateNeutralPlayer() => CreatePlayer("Neutral");

    internal static Player CreateCivilianPlayer() => CreatePlayer("Civilian");

    private static Player CreatePlayer(string side)
    {
        var result = new Player(new AssetPropertyCollection {
            {"Name", $"plyr{side}" },
            {"IsHuman", false },
            {"Faction", $"Faction{side}" },
            {"DisplayName", side },
        });

        return result;
    }

    public void Init(AssetPropertyCollection? properties)
    {
        BuildList = null;
        Scripts = null;

        if (properties != null)
        {
            Properties = properties;
        }
        else
        {
            Properties.Clear();
        }
    }

    public void Clear()
    {
        Init(null);
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
        Properties.WriteTo(writer, assetNames);

        writer.Write((uint)(BuildList?.Length ?? 0));

        var buildListInfoFields = GetBuildListInfoFields(sidesListVersion, mapHasAssetList);

        foreach (var buildListItem in BuildList ?? [])
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

public static class PlayerKeys
{
    public const string Name = "playerName";
    public const string IsHuman = "playerIsHuman";
    public const string IsSkirmish = "playerIsSkirmish";
    public const string Faction = "playerFaction";
    public const string DisplayName = "playerDisplayName";
    public const string Enemies = "playerEnemies";
    public const string Allies = "playerAllies";
    public const string StartMoney = "playerStartMoney";
    public const string Color = "playerColor";
    public const string NightColor = "playerNightColor";
    public const string MultiplayerStartIndex = "multiplayerStartIndex";
    public const string SkirmishDifficulty = "skirmishDifficulty";
    public const string MultiplayerIsLocal = "multiplayerIsLocal";
    public const string IsPreorder = "playerIsPreorder";
}
