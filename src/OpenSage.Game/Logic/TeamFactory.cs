using System.Collections.Generic;

namespace OpenSage.Logic
{
    public sealed class TeamFactory
    {
        private readonly List<TeamTemplate> _teamTemplates;
        private readonly Dictionary<uint, TeamTemplate> _teamTemplatesById;
        private readonly Dictionary<string, TeamTemplate> _teamTemplatesByName;

        private uint _lastTeamId;

        public TeamFactory()
        {
            _teamTemplates = new List<TeamTemplate>();
            _teamTemplatesById = new Dictionary<uint, TeamTemplate>();
            _teamTemplatesByName = new Dictionary<string, TeamTemplate>();

            _lastTeamId = 0;
        }

        public void Initialize(Data.Map.Team[] mapTeams, PlayerManager players)
        {
            _teamTemplates.Clear();
            _teamTemplatesById.Clear();
            _teamTemplatesByName.Clear();

            foreach (var mapTeam in mapTeams)
            {
                var name = mapTeam.Properties["teamName"].Value as string;

                var ownerName = mapTeam.Properties["teamOwner"].Value as string;
                var owner = players.GetPlayerByName(ownerName);

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

        internal void Load(SaveFileReader reader, PlayerManager players)
        {
            reader.ReadVersion(1);

            _lastTeamId = reader.ReadUInt32();

            var count = reader.ReadUInt16();
            if (count != _teamTemplatesById.Count)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < count; i++)
            {
                var id = reader.ReadUInt32();
                var teamTemplate = _teamTemplatesById[id];
                teamTemplate.Load(reader, players);
            }
        }
    }
}
