using System.Collections.Generic;
using System.Diagnostics;

namespace OpenSage.Logic
{
    [DebuggerDisplay("TeamTemplate '{Name}'")]
    public sealed class TeamTemplate : IPersistableObject
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

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(2);

            // This will be the same as the existing Owner, unless control of this team has been transferred.
            var ownerPlayerId = Owner.Id;
            reader.PersistUInt32(ref ownerPlayerId);
            Owner = reader.Game.Scene3D.PlayerManager.GetPlayerByIndex(ownerPlayerId);

            reader.PersistAsciiString(ref _attackPriorityName);
            reader.PersistBoolean(ref _unknown1);
            reader.PersistObject(_templateData);

            var teamCount = (ushort) _teams.Count;
            reader.PersistUInt16(ref teamCount);

            reader.BeginArray("Teams");
            if (reader.Mode == StatePersistMode.Read)
            {
                for (var i = 0; i < teamCount; i++)
                {
                    reader.BeginObject();

                    var id = 0u;
                    reader.PersistUInt32(ref id);

                    var team = FindTeamById(id);
                    if (team == null)
                    {
                        team = TeamFactory.AddTeamWithId(this, id);
                    }

                    reader.PersistObject(team, "Value");

                    reader.EndObject();
                }
            }
            else
            {
                foreach (var team in _teams)
                {
                    reader.BeginObject();

                    var id = team.Id;
                    reader.PersistUInt32(ref id);

                    reader.PersistObject(team, "Value");

                    reader.EndObject();
                }
            }
            reader.EndArray();
        }
    }

    internal sealed class TeamTemplateData : IPersistableObject
    {
        public int ProductionPriority;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistInt32(ref ProductionPriority);
        }
    }
}
