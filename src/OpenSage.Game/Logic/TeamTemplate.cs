using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    [DebuggerDisplay("TeamTemplate '{Name}'")]
    public sealed class TeamTemplate
    {
        private readonly List<Team> _teams;

        private string _attackPriorityName;
        private TeamTemplateData _templateData;

        public readonly TeamFactory TeamFactory;

        public readonly uint ID;
        public readonly string Name;
        public readonly Player Owner;
        public readonly bool IsSingleton;

        public TeamTemplate(TeamFactory teamFactory, uint id, string name, Player owner, bool isSingleton)
        {
            TeamFactory = teamFactory;
            ID = id;
            Name = name;
            Owner = owner;
            IsSingleton = isSingleton;

            // TODO: Read this from map data.
            _templateData = new TeamTemplateData();

            _teams = new List<Team>();
        }

        internal void AddTeam(Team team)
        {
            _teams.Add(team);
        }

        public Team FindTeamById(uint id)
        {
            foreach (var team in _teams)
            {
                if (team.Id == id)
                {
                    return team;
                }
            }
            return null;
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            var playerId = reader.ReadUInt32();
            if (playerId != Owner.Id)
            {
                throw new InvalidDataException();
            }

            _attackPriorityName = reader.ReadAsciiString();

            var unknown2 = reader.ReadBoolean();

            _templateData.Load(reader);

            var teamCount = reader.ReadUInt16();

            for (var i = 0; i < teamCount; i++)
            {
                var id = reader.ReadUInt32();

                var team = FindTeamById(id);
                if (team == null)
                {
                    team = TeamFactory.AddTeam(this);
                    team.Id = id;
                }

                team.Load(reader);
            }
        }
    }

    internal sealed class TeamTemplateData
    {
        public int ProductionPriority;

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            ProductionPriority = reader.ReadInt32();
        }
    }
}
