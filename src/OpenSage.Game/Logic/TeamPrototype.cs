#nullable enable

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenSage.Data.Map;
using OpenSage.Scripting;

namespace OpenSage.Logic;

[DebuggerDisplay("TeamPrototype '{Name}'")]
public sealed class TeamPrototype : IPersistableObject
{
    /// <summary>
    /// The factory that created this team prototype.
    /// </summary>
    public readonly TeamFactory TeamFactory;

    /// <summary>
    /// The Player that currently controls this team prototype.
    /// Port note: Comment in C++ says this can be nullable, but the only place where it's set ensures it's not null.
    /// </summary>
    private Player _owningPlayer;

    /// <summary>
    /// Unique prototype ID.
    /// </summary>
    public readonly TeamPrototypeId Id;
    public readonly string Name;

    // In Generals this is a flags enum, but only one flag (IsSingleton) is defined.
    public readonly bool IsSingleton;

    private bool _productionConditionAlwaysFalse;
    private Script? _productionConditionScript;

    private bool _retrievedGenericScripts;
    private List<Script> _genericScriptsToRun = new();

    private TeamTemplateInfo _teamTemplate;
    public TeamTemplateInfo TemplateInfo => _teamTemplate;

    private string _attackPriorityName;

    private readonly List<Team> _teams;

    public Player ControllingPlayer
    {
        get => _owningPlayer;

        [MemberNotNull(nameof(_owningPlayer))]
        set
        {
            if (_owningPlayer == value)
            {
                return;
            }

            _owningPlayer?.RemoveTeamFromList(this);
            _owningPlayer = value;
            _owningPlayer.AddTeamToList(this);
        }
    }

    public TeamPrototype(IGame game, TeamFactory teamFactory, string name, Player owner, bool isSingleton, AssetPropertyCollection teamProperties, TeamPrototypeId id)
    {
        TeamFactory = teamFactory;
        Id = id;
        _teamTemplate = new TeamTemplateInfo(game, teamProperties);
        Name = _teamTemplate.Name;
        _teams = [];
        ControllingPlayer = owner;
        _attackPriorityName = "";
        IsSingleton = _teamTemplate.IsSingleton;
    }

    internal void AddTeam(Team team)
    {
        _teams.Add(team);
    }

    public Team? FindTeamById(TeamId id)
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

    // C++: getFirstItemIn_TeamInstanceList
    public Team? FirstTeam
    {
        get
        {
            if (_teams.Count == 0)
            {
                return null;
            }

            return _teams[0];
        }
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(2);

        // This will be the same as the existing Owner, unless control of this team has been transferred.
        var ownerPlayerId = ControllingPlayer.Index;
        reader.PersistPlayerIndex(ref ownerPlayerId);
        ControllingPlayer = reader.Game.PlayerManager.GetPlayerByIndex(ownerPlayerId);

        reader.PersistAsciiString(ref _attackPriorityName);
        reader.PersistBoolean(ref _productionConditionAlwaysFalse);
        reader.PersistObject(_teamTemplate);

        var teamCount = (ushort)_teams.Count;
        reader.PersistUInt16(ref teamCount);

        reader.BeginArray("Teams");
        if (reader.Mode == StatePersistMode.Read)
        {
            for (var i = 0; i < teamCount; i++)
            {
                reader.BeginObject();

                var id = TeamId.Invalid;
                reader.PersistTeamId(ref id);

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
                reader.PersistTeamId(ref id);

                reader.PersistObject(team, "Value");

                reader.EndObject();
            }
        }
        reader.EndArray();
    }
}
