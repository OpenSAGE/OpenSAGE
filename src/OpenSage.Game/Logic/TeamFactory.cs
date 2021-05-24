using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    public sealed class TeamFactory
    {
        private readonly List<TeamTemplate> _teamTemplates;
        private readonly Dictionary<uint, TeamTemplate> _teamTemplatesById;
        private readonly Dictionary<string, TeamTemplate> _teamTemplatesByName;

        private readonly Dictionary<uint, Team> _teamsById;
        private uint _nextTeamId;

        public TeamFactory()
        {
            _teamTemplates = new List<TeamTemplate>();
            _teamTemplatesById = new Dictionary<uint, TeamTemplate>();
            _teamTemplatesByName = new Dictionary<string, TeamTemplate>();

            _teamsById = new Dictionary<uint, Team>();
            _nextTeamId = 1;
        }

        public void Initialize(Data.Map.Team[] mapTeams, IReadOnlyList<Player> players)
        {
            _teamTemplates.Clear();
            _teamTemplatesById.Clear();
            _teamTemplatesByName.Clear();

            foreach (var mapTeam in mapTeams)
            {
                var name = mapTeam.Properties["teamName"].Value as string;

                var ownerName = mapTeam.Properties["teamOwner"].Value as string;
                var owner = players.FirstOrDefault(player => player.Name == ownerName);

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
            var team = new Team(teamTemplate, _nextTeamId++);

            _teamsById.Add(team.Id, team);

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
            if (_teamsById.TryGetValue(id, out var result))
            {
                return result;
            }
            return null;
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknown1 = reader.ReadUInt32();

            var count = reader.ReadUInt16();
            if (count != _teamTemplatesById.Count)
            {
                throw new InvalidDataException();
            }

            System.Diagnostics.Debug.WriteLine("TeamTemplates: " + count);

            for (var i = 0; i < count; i++)
            {
                var id = reader.ReadUInt32();

                System.Diagnostics.Debug.WriteLine("- TeamTemplateId: " + id);

                var teamTemplate = _teamTemplatesById[id];

                teamTemplate.Load(reader);
            }
        }
    }
}
