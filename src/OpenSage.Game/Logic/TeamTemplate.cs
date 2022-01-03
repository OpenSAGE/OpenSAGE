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
            reader.PersistVersion(2);

            // This will be the same as the existing Owner, unless control of this team has been transferred.
            var ownerPlayerId = Owner.Id;
            reader.PersistUInt32(ref ownerPlayerId);
            Owner = players.GetPlayerByIndex(ownerPlayerId);

            reader.PersistAsciiString(ref _attackPriorityName);
            reader.PersistBoolean("Unknown1", ref _unknown1);

            _templateData.Load(reader);

            var teamCount = (ushort) _teams.Count;
            reader.PersistUInt16(ref teamCount);

            if (reader.Mode == StatePersistMode.Read)
            {
                for (var i = 0; i < teamCount; i++)
                {
                    var id = 0u;
                    reader.PersistUInt32(ref id);

                    var team = FindTeamById(id);
                    if (team == null)
                    {
                        team = TeamFactory.AddTeamWithId(this, id);
                    }

                    team.Load(reader);
                }
            }
            else
            {
                foreach (var team in _teams)
                {
                    var id = team.Id;
                    reader.PersistUInt32(ref id);

                    team.Load(reader);
                }
            }
        }
    }

    internal sealed class TeamTemplateData
    {
        public int ProductionPriority;

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistInt32(ref ProductionPriority);
        }
    }
}
