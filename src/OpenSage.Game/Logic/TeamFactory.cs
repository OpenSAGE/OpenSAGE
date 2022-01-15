﻿using System;
using System.Collections.Generic;

namespace OpenSage.Logic
{
    public sealed class TeamFactory : IPersistableObject
    {
        private readonly Game _game;

        private readonly List<TeamTemplate> _teamTemplates;
        private readonly Dictionary<uint, TeamTemplate> _teamTemplatesById;
        private readonly Dictionary<string, TeamTemplate> _teamTemplatesByName;

        private uint _lastTeamId;

        public TeamFactory(Game game)
        {
            _game = game;

            _teamTemplates = new List<TeamTemplate>();
            _teamTemplatesById = new Dictionary<uint, TeamTemplate>();
            _teamTemplatesByName = new Dictionary<string, TeamTemplate>();

            _lastTeamId = 0;
        }

        public void Initialize(Data.Map.Team[] mapTeams)
        {
            _teamTemplates.Clear();
            _teamTemplatesById.Clear();
            _teamTemplatesByName.Clear();

            foreach (var mapTeam in mapTeams)
            {
                var name = mapTeam.Properties["teamName"].Value as string;

                var ownerName = mapTeam.Properties["teamOwner"].Value as string;
                var owner = _game.PlayerManager.GetPlayerByName(ownerName);

                var isSingleton = (bool) mapTeam.Properties["teamIsSingleton"].Value;

                AddTeamTemplate(name, owner, isSingleton);
            }
        }

        private void AddTeamTemplate(string name, Player owner, bool isSingleton)
        {
            var id = (uint) (_teamTemplatesById.Count + 1);

            var teamTemplate = new TeamTemplate(
                this,
                id,
                name,
                owner,
                isSingleton);

            _teamTemplates.Add(teamTemplate);
            _teamTemplatesById.Add(id, teamTemplate);
            _teamTemplatesByName.Add(name, teamTemplate);

            if (isSingleton)
            {
                AddTeam(teamTemplate);
            }
        }

        internal Team AddTeam(TeamTemplate teamTemplate)
        {
            _lastTeamId++;

            var team = new Team(teamTemplate, _lastTeamId);

            teamTemplate.AddTeam(team);

            return team;
        }

        internal Team AddTeamWithId(TeamTemplate teamTemplate, uint id)
        {
            _lastTeamId = Math.Max(_lastTeamId, id);

            var team = new Team(teamTemplate, id);

            teamTemplate.AddTeam(team);

            return team;
        }

        public TeamTemplate FindTeamTemplateByName(string name)
        {
            if (_teamTemplatesByName.TryGetValue(name, out var result))
            {
                return result;
            }
            return null;
        }

        public TeamTemplate FindTeamTemplateById(uint id)
        {
            if (_teamTemplatesById.TryGetValue(id, out var result))
            {
                return result;
            }
            return null;
        }

        public Team FindTeamById(uint id)
        {
            foreach (var teamTemplate in _teamTemplates)
            {
                var team = teamTemplate.FindTeamById(id);
                if (team != null)
                {
                    return team;
                }
            }
            return null;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt32(ref _lastTeamId);

            var count = (ushort)_teamTemplates.Count;
            reader.PersistUInt16(ref count, "TeamTemplatesCount");

            if (count != _teamTemplates.Count)
            {
                throw new InvalidStateException();
            }

            reader.BeginArray("TeamTemplates");
            if (reader.Mode == StatePersistMode.Read)
            {
                for (var i = 0; i < count; i++)
                {
                    reader.BeginObject();

                    var id = 0u;
                    reader.PersistUInt32(ref id);

                    var teamTemplate = _teamTemplatesById[id];
                    reader.PersistObject(teamTemplate);

                    reader.EndObject();
                }
            }
            else
            {
                foreach (var teamTemplate in _teamTemplates)
                {
                    reader.BeginObject();

                    var id = teamTemplate.ID;
                    reader.PersistUInt32(ref id);

                    reader.PersistObject(teamTemplate);

                    reader.EndObject();
                }
            }
            reader.EndArray();
        }
    }
}
