﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Scb;
using OpenSage.Logic;
using OpenSage.Scripting;

namespace OpenSage.Data.Map
{
    internal static class SidesListUtility
    {
        public static void SetupGameSides(
            Game game,
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
            var replayObserverPlayerProperties = new AssetPropertyCollection();
            replayObserverPlayerProperties.AddAsciiString("playerFaction", "FactionObserver");
            replayObserverPlayerProperties.AddAsciiString("playerName", "ReplayObserver");
            replayObserverPlayerProperties.AddAsciiString("playerDisplayName", "Observer");
            replayObserverPlayerProperties.AddBoolean("playerIsHuman", false);
            replayObserverPlayerProperties.AddAsciiString("playerAllies", "");
            replayObserverPlayerProperties.AddAsciiString("playerEnemies", "");
            tempMapPlayers.Add(new Player
            {
                Properties = replayObserverPlayerProperties
            });

            tempMapScriptLists.Add(new ScriptList());

            mapPlayers = tempMapPlayers.ToArray();
            mapTeams = tempMapTeams.ToArray();
            mapScriptLists = tempMapScriptLists.ToArray();
        }

        private static Team CreateTeam(string name, string owner, bool isSingleton)
        {
            var properties = new AssetPropertyCollection();
            properties.AddAsciiString("teamName", name);
            properties.AddAsciiString("teamOwner", owner);
            properties.AddBoolean("teamIsSingleton", isSingleton);
            return new Team
            {
                Properties = properties
            };
        }

        private static Team CreateDefaultTeam(string owner)
        {
            return CreateTeam($"team{owner}", owner, true);
        }

        private static void SetupSkirmishGameSides(
            Game game,
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
            mapPlayers.Add(originalMapPlayers.FirstOrDefault(x => (string)x.Properties["playerFaction"].Value == "FactionCivilian"));

            //var hasAIPlayer = false;
            for (var i = 0; i < playerSettings.Length; i++)
            {
                var playerSetting = playerSettings[i];

                var factionPlayer = originalMapPlayers.FirstOrDefault(x => (string) x.Properties["playerFaction"].Value == playerSetting.SideName);

                var isHuman = playerSetting.Owner == PlayerOwner.Player;

                var playerName = isHuman
                    ? $"player{i}"
                    : $"{factionPlayer.Properties["playerName"].Value}{i}";

                //if (!isHuman)
                //{
                //    hasAIPlayer = true;
                //}

                var playerProperties = new AssetPropertyCollection();
                playerProperties.AddAsciiString("playerFaction", (string) factionPlayer.Properties["playerFaction"].Value);
                playerProperties.AddAsciiString("playerName", playerName);
                playerProperties.AddAsciiString("playerDisplayName", (string) factionPlayer.Properties["playerDisplayName"].Value);
                playerProperties.AddBoolean("playerIsHuman", isHuman);
                playerProperties.AddInteger("playerColor", playerSetting.Color.ToUInt32());

                // TODO: Other player properties.

                mapPlayers.Add(new Player
                {
                    Properties = playerProperties,
                    BuildList = factionPlayer.BuildList,
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
                        playerAllies[i].Add(mapPlayers[j + 2].Properties["playerName"].Value.ToString());
                        playerAllies[j].Add(mapPlayers[i + 2].Properties["playerName"].Value.ToString());
                    }
                    else
                    {
                        playerEnemies[i].Add(mapPlayers[j + 2].Properties["playerName"].Value.ToString());
                        playerEnemies[j].Add(mapPlayers[i + 2].Properties["playerName"].Value.ToString());
                    }
                }
            }
            for (var i = 0; i < playerSettings.Length; i++)
            {
                mapPlayers[i + 2].Properties.AddAsciiString("playerAllies", string.Join(' ', playerAllies[i]));
                mapPlayers[i + 2].Properties.AddAsciiString("playerEnemies", string.Join(' ', playerEnemies[i]));
            }

            var originalMapScriptLists = mapFile.GetPlayerScriptsList().ScriptLists;

            var playerNames = mapPlayers
                .Select(p => p.Properties.GetPropOrNull("playerFaction")?.Value.ToString())
                .ToArray();

            while (mapScriptLists.Count < mapPlayers.Count)
            {
                mapScriptLists.Add(new ScriptList());
            }

            // Copy neutral player scripts.
            var neutralPlayerName = (string) mapPlayers[0].Properties["playerName"].Value;
            CopyScripts(
                originalMapScriptLists,
                playerNames,
                neutralPlayerName,
                mapScriptLists,
                0,
                appendIndex: false);

            // Copy civilian player scripts.
            var civilianPlayerName = (string) mapPlayers[1].Properties["playerName"].Value;
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
            mapScriptLists.AddRange(mapFile.GetPlayerScriptsList().ScriptLists);

            mapTeams.Add(CreateDefaultTeam("ReplayObserver"));
        }

        private static void CreateTeamsFromScbFile(
            Game game,
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
                    if ((bool)mapPlayers[i].Properties["playerIsHuman"].Value)
                    {
                        // Copy the scripts from the civilian player to all human players.
                        CopyScripts(
                            originalMapScriptLists,
                            originalMapPlayerNames,
                            civilianPlayerName,
                            mapScriptLists,
                            i,
                            appendIndex: true);

                        mapTeams.Add(CreateDefaultTeam((string)mapPlayers[i].Properties["playerName"].Value));
                    }
                }

                mapTeams.Add(CreateDefaultTeam("ReplayObserver"));

                mapTeams.Add(CreateDefaultTeam(neutralPlayerName));
                mapTeams.Add(CreateDefaultTeam(civilianPlayerName));

                for (var i = 2; i < mapPlayers.Count; i++)
                {
                    if (!(bool)mapPlayers[i].Properties["playerIsHuman"].Value)
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

                        mapTeams.Add(CreateDefaultTeam((string)mapPlayers[i].Properties["playerName"].Value));
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
                if ((bool)mapPlayers[i].Properties["playerIsHuman"].Value)
                {
                    // Copy the scripts from the civilian player to all human players.
                    CopyScripts(
                        skirmishScripts.PlayerScripts.ScriptLists,
                        skirmishScriptsPlayerNames,
                        civilianPlayerName,
                        mapScriptLists,
                        i,
                        appendIndex: true);

                    mapTeams.Add(CreateDefaultTeam((string)mapPlayers[i].Properties["playerName"].Value));
                }
            }

            mapTeams.Add(CreateDefaultTeam("ReplayObserver"));

            mapTeams.Add(CreateDefaultTeam(neutralPlayerName));
            mapTeams.Add(CreateDefaultTeam(civilianPlayerName));

            // Skip neutral and civilian players.
            for (var i = 2; i < mapPlayers.Count; i++)
            {
                if (!(bool)mapPlayers[i].Properties["playerIsHuman"].Value)
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
                        var teamOwner = (string)team.Properties["teamOwner"].Value;
                        if (teamOwner == sourcePlayerName)
                        {
                            var teamName = $"{team.Properties["teamName"].Value}{i}";
                            mapTeams.Add(CreateTeam(
                                teamName,
                                (string)mapPlayers[i].Properties["playerName"].Value,
                                (bool)team.Properties["teamIsSingleton"].Value));
                        }
                    }
                }
            }
        }
    }
}
