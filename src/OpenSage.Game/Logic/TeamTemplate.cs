using System.Collections.Generic;
using System.Diagnostics;
using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    [DebuggerDisplay("TeamTemplate '{Name}'")]
    public sealed class TeamTemplate
    {
        private readonly List<Team> _teams;

        private string _attackPriorityName;
        private bool _unknown1;
        private TeamTemplateData _templateData;

        public readonly TeamFactory TeamFactory;

        public readonly uint ID;
        public readonly string Name;
        public Player Owner { get; private set; }
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

        internal void Load(StatePersister reader, PlayerManager players)
        {
            reader.ReadVersion(2);

            // This will be the same as the existing Owner, unless control of this team has been transferred.
            var ownerPlayerId = Owner.Id;
            reader.ReadUInt32(ref ownerPlayerId);
            Owner = players.GetPlayerByIndex(ownerPlayerId);

            reader.ReadAsciiString(ref _attackPriorityName);
            reader.ReadBoolean(ref _unknown1);

            _templateData.Load(reader);

            var teamCount = (ushort) _teams.Count;
            reader.ReadUInt16(ref teamCount);

            for (var i = 0; i < teamCount; i++)
            {
                var id = 0u;
                reader.ReadUInt32(ref id);

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

        internal void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            reader.ReadInt32(ref ProductionPriority);
        }
    }
}
