﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Map;
using OpenSage.Data.Scb;
using OpenSage.Scripting;

namespace OpenSage.Logic.Map;

using Player = Data.Map.Player;
using Team = Data.Map.Team;

internal static class SidesListUtility
{
    public static void SetupGameSides(
        IGame game,
        MapFile mapFile,
        PlayerSetting[] playerSettings,
        GameType gameType,
        out Player[] mapPlayers,
        out Team[] mapTeams,
        out ScriptList[] mapScriptLists)
    {
        var tempMapPlayers = new List<Player>();
        var tempMapTeams = new List<Team>();
        var tempMapScriptLists = new List<ScriptList>();

        if (gameType == GameType.SinglePlayer)
        {
            SetupSinglePlayerGameSides(
                mapFile,
                tempMapPlayers,
                tempMapTeams,
                tempMapScriptLists);
        }
        else
        {
            SetupSkirmishGameSides(
                game,
                mapFile,
                playerSettings,
                gameType,
                tempMapPlayers,
                tempMapTeams,
                tempMapScriptLists);
        }

        // TODO: Probably don't add replay observer player and team when viewing a replay?
        tempMapPlayers.Add(new Player
        {
            Faction = "FactionObserver",
            Name = "ReplayObserver",
            DisplayName = "Observer",
            IsHuman = false,
            Allies = "",
            Enemies = ""
        });

        tempMapScriptLists.Add(new ScriptList());

        mapPlayers = tempMapPlayers.ToArray();
        mapTeams = tempMapTeams.ToArray();
        mapScriptLists = tempMapScriptLists.ToArray();
    }

    private static Team CreateTeam(string name, string owner, bool isSingleton)
    {
        return new Team
        {
            Name = name,
            Owner = owner,
            IsSingleton = isSingleton,
        };
    }

    private static Team CreateDefaultTeam(string owner)
    {
        return CreateTeam($"team{owner}", owner, true);
    }

    private static void SetupSkirmishGameSides(
        IGame game,
        MapFile mapFile,
        PlayerSetting[] playerSettings,
        GameType gameType,
        List<Player> mapPlayers,
        List<Team> mapTeams,
        List<ScriptList> mapScriptLists)
    {
        var originalMapPlayers = mapFile.SidesList.Players;

        // Neutral player.
        mapPlayers.Add(originalMapPlayers[0]);

        // Civilian player. It isn't necessarily the second player.
        // TODO: There might be more than one civilian player.
        mapPlayers.Add(originalMapPlayers.FirstOrDefault(x => x.Faction == "FactionCivilian"));

        //var hasAIPlayer = false;
        for (var i = 0; i < playerSettings.Length; i++)
        {
            var playerSetting = playerSettings[i];

            var factionPlayer = originalMapPlayers.FirstOrDefault(x => x.Faction == playerSetting.SideName);

            var isHuman = playerSetting.Owner == PlayerOwner.Player;

            var playerName = isHuman
                ? $"player{i}"
                : $"{factionPlayer.Name}{i}";

            //if (!isHuman)
            //{
            //    hasAIPlayer = true;
            //}


            mapPlayers.Add(new Player
            {
                BuildList = factionPlayer.BuildList,
                Faction = factionPlayer.Faction,
                Name = playerName,
                DisplayName = factionPlayer.DisplayName,
                IsHuman = isHuman,
                Color = playerSetting.Color.ToInt32(),
                // TODO: Other player properties.
            });
        }

        // Setup player relationships.
        var playerAllies = new List<string>[playerSettings.Length];
        var playerEnemies = new List<string>[playerSettings.Length];
        for (var i = 0; i < playerSettings.Length; i++)
        {
            playerAllies[i] = new List<string>();
            playerEnemies[i] = new List<string>();
        }
        for (var i = 0; i < playerSettings.Length; i++)
        {
            var outerPlayer = playerSettings[i];

            for (var j = i + 1; j < playerSettings.Length; j++)
            {
                var innerPlayer = playerSettings[j];
                if (outerPlayer.Team == innerPlayer.Team && outerPlayer.Team != -1) // -1 is team None
                {
                    playerAllies[i].Add(mapPlayers[j + 2].Name);
                    playerAllies[j].Add(mapPlayers[i + 2].Name);
                }
                else
                {
                    playerEnemies[i].Add(mapPlayers[j + 2].Name);
                    playerEnemies[j].Add(mapPlayers[i + 2].Name);
                }
            }
        }
        for (var i = 0; i < playerSettings.Length; i++)
        {
            mapPlayers[i + 2].Allies = string.Join(' ', playerAllies[i]);
            mapPlayers[i + 2].Enemies = string.Join(' ', playerEnemies[i]);
        }

        var originalMapScriptLists = mapFile.GetPlayerScriptsList().ToArray();

        // TODO: Is this really correct? The variable name refers to player names and CopyScripts expects player names,
        // but the list contains factions.
        var playerNames = mapPlayers.Select(p => p.Faction).ToArray();

        while (mapScriptLists.Count < mapPlayers.Count)
        {
            mapScriptLists.Add(new ScriptList());
        }

        // Copy neutral player scripts.
        var neutralPlayerName = mapPlayers[0].Name;
        CopyScripts(
            originalMapScriptLists,
            playerNames,
            neutralPlayerName,
            mapScriptLists,
            0,
            appendIndex: false);

        // Copy civilian player scripts.
        var civilianPlayerName = mapPlayers[1].Name;
        CopyScripts(
            originalMapScriptLists,
            playerNames,
            civilianPlayerName,
            mapScriptLists,
            1,
            appendIndex: false);

        var hadScripts = false;
        switch (game.SageGame)
        {
            case SageGame.CncGenerals:
            case SageGame.CncGeneralsZeroHour:
                CreateTeamsFromScbFile(game, originalMapScriptLists, playerNames, mapPlayers, mapTeams, mapScriptLists, playerSettings, neutralPlayerName, civilianPlayerName, out hadScripts);
                break;
            case SageGame.Bfme:
            case SageGame.Bfme2:
            case SageGame.Bfme2Rotwk:
                mapTeams.AddRange(mapFile.GetTeams());
                break;
        }

        if (!hadScripts && playerSettings.Length > 1)
        {
            var multiplayerScriptsEntry = game.ContentManager.GetScriptEntry(@"Data\Scripts\MultiplayerScripts.scb");

            if (multiplayerScriptsEntry != null)
            {
                using var stream = multiplayerScriptsEntry.Open();
                var multiplayerScripts = ScbFile.FromStream(stream);

                // TODO: This is a bit hardcoded.
                mapScriptLists[0] = multiplayerScripts.PlayerScripts.ScriptLists[0];
            }
        }
    }

    /// <summary>
    /// Copies source scripts to a target player, optionally renaming variables, script and team names based on the target player index.
    /// </summary>
    /// <param name="scriptsList">The scripts list.</param>
    /// <param name="playerNames">The source player names.</param>
    /// <param name="sourcePlayerName">The name of the source player to copy. This is used to find the index in the <paramref name="playerNames"/> array, which is then used to access the <paramref name="scriptsList"/> array.</param>
    /// <param name="targetScriptsList">The array of script lists to copy the scripts into.</param>
    /// <param name="targetPlayerIndex">The index in the <paramref name="targetScriptsList"/> array the scripts should be copied to.
    /// 0 .. Neutral
    /// 1 .. Civilian
    /// 2 .. player 1
    /// 3 .. player 2
    /// ...
    /// </param>.
    /// <param name="appendIndex">If set to <c>true</c>, the player index will be appended to all script, team and variable names in order to create unique names.</param>
    private static void CopyScripts(
        ScriptList[] scriptsList,
        string[] playerNames,
        string sourcePlayerName,
        List<ScriptList> targetScriptsList,
        int targetPlayerIndex,
        bool appendIndex)
    {
        var sourcePlayerIndex = Array.FindIndex(playerNames, p => p.Equals(sourcePlayerName, StringComparison.OrdinalIgnoreCase));
        if (sourcePlayerIndex >= 0)
        {
            // In script files, the neutral player at index 0 is not included in the player names list
            if (scriptsList.Length > playerNames.Length && playerNames[0] != "")
            {
                sourcePlayerIndex++;
            }

            // For player 1, we want to append "0" to all script names and variables, but her position in the array is 2.
            var appendix = appendIndex ? (targetPlayerIndex - 2).ToString() : null;
            targetScriptsList[targetPlayerIndex] = scriptsList[sourcePlayerIndex].Copy(appendix);
        }
    }

    private static void SetupSinglePlayerGameSides(
        MapFile mapFile,
        List<Player> mapPlayers,
        List<Team> mapTeams,
        List<ScriptList> mapScriptLists)
    {
        mapPlayers.AddRange(mapFile.SidesList.Players);
        mapTeams.AddRange(mapFile.GetTeams());
        mapScriptLists.AddRange(mapFile.GetPlayerScriptsList());

        mapTeams.Add(CreateDefaultTeam("ReplayObserver"));
    }

    // TODO(Port): SidesList.PrepareForMpOrSkirmish contains a ported near-equivalent of this function
    private static void CreateTeamsFromScbFile(
        IGame game,
        ScriptList[] originalMapScriptLists,
        string[] originalMapPlayerNames,
        List<Player> mapPlayers,
        List<Team> mapTeams,
        List<ScriptList> mapScriptLists,
        PlayerSetting[] playerSettings,
        string neutralPlayerName,
        string civilianPlayerName,

        out bool hadScripts)
    {
        // If we already have scripts defined in World Builder for this map, don't overwrite them.
        hadScripts = originalMapScriptLists.Any(x => x != null && (x.ScriptGroups.Length > 0 || x.Scripts.Length > 0));
        if (hadScripts)
        {
            // ... but we still need to create teams.
            for (var i = 2; i < mapPlayers.Count; i++)
            {
                if (mapPlayers[i].IsHuman)
                {
                    // Copy the scripts from the civilian player to all human players.
                    CopyScripts(
                        originalMapScriptLists,
                        originalMapPlayerNames,
                        civilianPlayerName,
                        mapScriptLists,
                        i,
                        appendIndex: true);

                    mapTeams.Add(CreateDefaultTeam(mapPlayers[i].Name));
                }
            }

            mapTeams.Add(CreateDefaultTeam("ReplayObserver"));

            mapTeams.Add(CreateDefaultTeam(neutralPlayerName));
            mapTeams.Add(CreateDefaultTeam(civilianPlayerName));

            for (var i = 2; i < mapPlayers.Count; i++)
            {
                if (!mapPlayers[i].IsHuman)
                {
                    var playerSide = game.AssetStore.PlayerTemplates.GetByName(playerSettings[i - 2].SideName).Side;
                    var sourcePlayerName = $"Faction{playerSide}";

                    // Copy the scripts from the according skirmish player for all AI players.
                    CopyScripts(
                        originalMapScriptLists,
                        originalMapPlayerNames,
                        sourcePlayerName,
                        mapScriptLists,
                        i,
                        appendIndex: true);

                    mapTeams.Add(CreateDefaultTeam(mapPlayers[i].Name));
                }
            }

            return;
        }

        var skirmishScriptsEntry = game.ContentManager.GetScriptEntry(@"Data\Scripts\SkirmishScripts.scb");

        using var stream = skirmishScriptsEntry.Open();
        var skirmishScripts = ScbFile.FromStream(stream);

        // This probably isn't right, but it does make the teams match those in .sav files.
        // We first add human player(s) teams, then the replay observer team,
        // then neutral and civilian teams, and then finally AI skirmish players.

        var skirmishScriptsPlayerNames = skirmishScripts.ScriptsPlayers.Players.Select(p => p.Name).ToArray();

        // Skip neutral and civilian players.
        for (var i = 2; i < mapPlayers.Count; i++)
        {
            if (mapPlayers[i].IsHuman)
            {
                // Copy the scripts from the civilian player to all human players.
                CopyScripts(
                    skirmishScripts.PlayerScripts.ScriptLists,
                    skirmishScriptsPlayerNames,
                    civilianPlayerName,
                    mapScriptLists,
                    i,
                    appendIndex: true);

                mapTeams.Add(CreateDefaultTeam(mapPlayers[i].Name));
            }
        }

        mapTeams.Add(CreateDefaultTeam("ReplayObserver"));

        mapTeams.Add(CreateDefaultTeam(neutralPlayerName));
        mapTeams.Add(CreateDefaultTeam(civilianPlayerName));

        // Skip neutral and civilian players.
        for (var i = 2; i < mapPlayers.Count; i++)
        {
            if (!mapPlayers[i].IsHuman)
            {
                var playerSide = game.AssetStore.PlayerTemplates.GetByName(playerSettings[i - 2].SideName).Side;
                var sourcePlayerName = $"Skirmish{playerSide}";

                // Copy the scripts from the according skirmish player for all AI players.
                CopyScripts(
                    skirmishScripts.PlayerScripts.ScriptLists,
                    skirmishScriptsPlayerNames,
                    sourcePlayerName,
                    mapScriptLists,
                    i,
                    appendIndex: true);

                // TODO: Not sure about the order the teams are added.
                foreach (var team in skirmishScripts.Teams.Teams)
                {
                    var teamOwner = team.Owner;
                    if (teamOwner == sourcePlayerName)
                    {
                        var teamName = $"{team.Name}{i}";
                        mapTeams.Add(CreateTeam(teamName, mapPlayers[i].Name, team.IsSingleton));
                    }
                }
            }
        }
    }
}
