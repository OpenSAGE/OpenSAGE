#nullable enable

using System;
using System.Collections.Generic;
using OpenSage.Logic.Map;

namespace OpenSage.Logic;

public sealed class TeamFactory : IPersistableObject
{
    private readonly IGame _game;

    private readonly List<TeamPrototype> _teamPrototypes;
    private readonly Dictionary<TeamPrototypeId, TeamPrototype> _teamPrototypesById;
    private readonly Dictionary<string, TeamPrototype> _teamPrototypesByName;

    private TeamPrototypeId _uniqueTeamPrototypeId;
    private TeamId _uniqueTeamId;

    public TeamFactory(IGame game)
    {
        _game = game;

        _teamPrototypes = [];
        _teamPrototypesById = [];
        _teamPrototypesByName = [];
        _uniqueTeamId = TeamId.Invalid;
        _uniqueTeamPrototypeId = TeamPrototypeId.Invalid;
    }

    public void InitFromSides(SidesList sides)
    {
        Clear();

        foreach (var mapTeam in sides.Teams)
        {
            InitTeam(mapTeam);
        }
    }

    public void Clear()
    {
        _teamPrototypes.Clear();
        _teamPrototypesById.Clear();
        _teamPrototypesByName.Clear();
    }

    private TeamId GetNextTeamId()
    {
        _uniqueTeamId++;
        return _uniqueTeamId;
    }

    private TeamPrototypeId GetNextTeamPrototypeId()
    {
        _uniqueTeamPrototypeId++;
        return _uniqueTeamPrototypeId;
    }

    private void InitTeam(Data.Map.Team mapTeam)
    {
        var id = GetNextTeamPrototypeId();

        if (mapTeam.Name == null)
        {
            throw new InvalidStateException("Team name is null");
        }

        if (_teamPrototypesByName.ContainsKey(mapTeam.Name))
        {
            // This is a debug only assertion in C++, should it be here too?
            // This seems like a situation that if it ever happens the game state is irreparably corrupted.
            throw new InvalidOperationException($"Duplicate team name: {mapTeam.Name}");
        }

        var owner = mapTeam.Owner != null ? _game.PlayerList.FindPlayerWithName(mapTeam.Owner) : null;
        if (owner == null)
        {
            // Another debug only assertion in C++, but at least this one has a fallback.
            owner = _game.PlayerList.NeutralPlayer;
        }

        var teamPrototype = new TeamPrototype(
            _game,
            this,
            mapTeam.Name,
            owner,
            mapTeam.IsSingleton,
            mapTeam.Properties,
            id
        );

        // In C++ TeamPrototype's constructor adds itself to a global list as a side effect.
        // We'll do it explicitly here instead.
        _teamPrototypes.Add(teamPrototype);
        _teamPrototypesById.Add(id, teamPrototype);
        _teamPrototypesByName.Add(mapTeam.Name, teamPrototype);

        if (mapTeam.IsSingleton)
        {
            CreateInactiveTeam(mapTeam.Name);
        }
    }

    private Team CreateInactiveTeam(string name)
    {
        void RunProductionConditionScriptIfExists(TeamPrototype prototype)
        {
            if (!prototype.TemplateInfo.ExecuteActions)
            {
                return;
            }

            var script = _game.Scripting.FindScript(prototype.TemplateInfo.ProductionCondition);
            if (script != null)
            {
                _game.Scripting.ExecuteAction(script.ActionsIfTrue);
            }
        }

        var prototype = FindTeamPrototype(name);

        if (prototype == null)
        {
            throw new InvalidOperationException($"Team prototype not found: {name}");
        }

        if (prototype.IsSingleton)
        {
            var team = prototype.FirstTeam;
            if (team != null)
            {
                RunProductionConditionScriptIfExists(prototype);
                return team;
            }
        }

        var id = GetNextTeamId();
        var newTeam = new Team(_game, prototype, id);
        prototype.AddTeam(newTeam);
        RunProductionConditionScriptIfExists(prototype);
        return newTeam;
    }

    private TeamPrototype? FindTeamPrototype(string name)
    {
        if (_teamPrototypesByName.TryGetValue(name, out var result))
        {
            return result;
        }
        return null;
    }

    internal Team AddTeam(TeamPrototype teamPrototype)
    {
        var id = GetNextTeamId();
        var team = new Team(_game, teamPrototype, id);

        teamPrototype.AddTeam(team);

        return team;
    }

    internal Team AddTeamWithId(TeamPrototype teamPrototype, TeamId id)
    {
        _uniqueTeamId = TeamId.Max(_uniqueTeamId, id);

        var team = new Team(_game, teamPrototype, id);

        teamPrototype.AddTeam(team);

        return team;
    }

    public TeamPrototype? FindTeamPrototypeByName(string name)
    {
        if (_teamPrototypesByName.TryGetValue(name, out var result))
        {
            return result;
        }
        return null;
    }

    public TeamPrototype? FindTeamPrototypeById(TeamPrototypeId id)
    {
        if (_teamPrototypesById.TryGetValue(id, out var result))
        {
            return result;
        }
        return null;
    }

    public Team? FindTeamById(TeamId id)
    {
        foreach (var teamPrototype in _teamPrototypes)
        {
            var team = teamPrototype.FindTeamById(id);
            if (team != null)
            {
                return team;
            }
        }
        return null;
    }

    public Team? FindTeam(string name)
    {
        var prototype = FindTeamPrototype(name);

        if (prototype != null)
        {
            var team = prototype.FirstTeam;
            if (team == null || !prototype.IsSingleton)
            {
                return CreateInactiveTeam(name);
            }
            return team;
        }
        return null;
    }

    public void Reset()
    {
        _uniqueTeamPrototypeId = TeamPrototypeId.Invalid;
        _uniqueTeamId = TeamId.Invalid;
        Clear();
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistTeamId(ref _uniqueTeamId);

        var count = (ushort)_teamPrototypes.Count;
        reader.PersistUInt16(ref count, "TeamPrototypesCount");

        if (count != _teamPrototypes.Count)
        {
            throw new InvalidStateException();
        }

        reader.BeginArray("TeamPrototypes");
        if (reader.Mode == StatePersistMode.Read)
        {
            for (var i = 0; i < count; i++)
            {
                reader.BeginObject();

                var id = TeamPrototypeId.Invalid;
                reader.PersistTeamPrototypeId(ref id);

                var teamPrototype = _teamPrototypesById[id];
                reader.PersistObject(teamPrototype);

                reader.EndObject();
            }
        }
        else
        {
            foreach (var teamPrototype in _teamPrototypes)
            {
                reader.BeginObject();

                var id = teamPrototype.Id;
                reader.PersistTeamPrototypeId(ref id);

                reader.PersistObject(teamPrototype);

                reader.EndObject();
            }
        }
        reader.EndArray();
    }
}
