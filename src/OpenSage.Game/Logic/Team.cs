using System.Collections.Generic;

namespace OpenSage.Logic
{
    public sealed class Team
    {
        private readonly List<Entity> _teamMembers;

        public IReadOnlyList<Entity> TeamMembers => _teamMembers;

        public Team()
        {
            _teamMembers = new List<Entity>();
        }

        public void AddTeamMember(Entity teamMember)
        {
            _teamMembers.Add(teamMember);
        }
    }
}
