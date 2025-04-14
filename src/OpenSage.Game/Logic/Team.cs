#nullable enable

using System;
using System.Collections.Generic;
using OpenSage.Logic.Object;
using OpenSage.Scripting;

namespace OpenSage.Logic;

public sealed class Team : IPersistableObject
{
    public const ushort MaxGenericScripts = 16;

    private readonly IGame _game;

    public readonly TeamPrototype Prototype;

    public readonly TeamId Id;

    public readonly List<GameObject> Members = [];

    /// <summary>
    /// Used to temporarily store the IDs of member objects when loading a save.
    /// </summary>
    private readonly List<ObjectId> _persistedMembersIdList = [];

    /// <summary>
    /// Name of the current AI state.
    /// </summary>
    private string _state = "";

    /// <summary>
    /// True if a team member entered or exited a trigger area this frame.
    /// </summary>
    private bool _enteredOrExited;

    /// <summary>
    /// True if the team is complete. False while members are being added.
    /// </summary>
    private bool _active;

    /// <summary>
    /// True when first activated.
    /// </summary>
    private bool _created;

    /// <summary>
    /// True if we have an on enemy sighted or all clear script.
    /// </summary>
    private bool _checkEnemySighted;
    private bool _seeEnemy;
    private bool _prevSeeEnemy;
    private bool _wasIdle;

    private int _destroyThreshold;
    private int _curUnits;

    private Waypoint? _currentWaypoint;

    private readonly bool[] _shouldAttemptGenericScript = new bool[MaxGenericScripts];

    private bool _isRecruitabilitySet;
    private bool _isRecruitable;

    private ObjectId _commonAttackTarget;

    public readonly PlayerRelationships<TeamId> TeamToTeamRelationships = new();
    public readonly PlayerRelationships<PlayerIndex> TeamToPlayerRelationships = new();

    internal Team(IGame game, TeamPrototype prototype, TeamId id)
    {
        _game = game;
        Prototype = prototype;
        Id = id;
    }

    internal void SetActive()
    {
        if (!_active)
        {
            _created = true;
            _active = true;
        }
    }

    public Player ControllingPlayer
    {
        get => Prototype.ControllingPlayer;
        set
        {
            // Comment from original Team.cpp:
            // This function (note: in this case setter) is used by one script, and it is kind of odd. The actual units
            // are not getting captured, the team they are on is being reassigned to a new player.
            // The Team doesn't change, it just starts to return a different answer when you ask for
            // the controlling player. I don't want to make the major change of onCapture on everyone,
            // so I will do the minor fix for the specific bug, which is harmless even when misused.

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            foreach (var obj in Members)
            {
                obj.HandlePartitionCellMaintenance();
            }
        }
    }

    public RelationshipType GetRelationship(Team that)
    {
        // Check team override
        if (TeamToTeamRelationships.TryGetValue(that.Id, out var relationship))
        {
            return relationship;
        }

        // Check player override
        if (TeamToPlayerRelationships.TryGetValue(ControllingPlayer.Index, out relationship))
        {
            return relationship;
        }

        // Otherwise use player's personal relationship
        return ControllingPlayer.GetRelationship(that);
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        var id = Id;
        reader.PersistTeamId(ref id);
        if (id != Id)
        {
            throw new InvalidStateException();
        }

        if (reader.Mode == StatePersistMode.Write)
        {
            reader.PersistList(
                Members,
                static (StatePersister persister, ref GameObject item) =>
                {
                    var objectId = item.Id;
                    persister.PersistObjectIdValue(ref objectId);
                }
            );
        }
        else if (reader.Mode == StatePersistMode.Read)
        {
            reader.PersistList(
                _persistedMembersIdList,
                static (StatePersister persister, ref ObjectId item) =>
                {
                    persister.PersistObjectIdValue(ref item);
                }
            );
        }

        reader.PersistAsciiString(ref _state);
        reader.PersistBoolean(ref _enteredOrExited);
        reader.PersistBoolean(ref _active);
        reader.PersistBoolean(ref _created);
        reader.PersistBoolean(ref _checkEnemySighted);
        reader.PersistBoolean(ref _seeEnemy);
        reader.PersistBoolean(ref _prevSeeEnemy);
        reader.PersistBoolean(ref _wasIdle);
        reader.PersistInt32(ref _destroyThreshold);
        reader.PersistInt32(ref _curUnits);

        var currentWaypointId = (uint)(_currentWaypoint?.Id ?? 0);
        reader.PersistUInt32(ref currentWaypointId);
        if (reader.Mode == StatePersistMode.Read)
        {
            _currentWaypoint = _game.TerrainLogic.GetWaypointById(currentWaypointId);
        }

        reader.PersistArrayWithUInt16Length(
            _shouldAttemptGenericScript,
            static (StatePersister persister, ref bool item) =>
            {
                persister.PersistBooleanValue(ref item);
            }
        );

        reader.PersistBoolean(ref _isRecruitabilitySet);
        reader.PersistBoolean(ref _isRecruitable);
        reader.PersistObjectId(ref _commonAttackTarget);
        reader.PersistObject(TeamToTeamRelationships);
        reader.PersistObject(TeamToPlayerRelationships);
    }

    // TODO(Port): Actually call this
    public void LoadPostProcess()
    {
        // In C++, this was previously (before Generals shipped) used to re-create the object references in Members.
        // However, in retail Generals / ZH, this function and _persistedMembersIdList is only used for sanity checks.
        // GameObject.Persist() should instead handle adding the object to the correct team.

        foreach (var objId in _persistedMembersIdList)
        {
            var obj = _game.GameLogic.GetObjectById(objId);

            if (obj == null)
            {
                throw new InvalidStateException($"Team member object with ID {objId} not found.");
            }

            if (!Members.Contains(obj))
            {
                throw new InvalidStateException($"Object {objId} should be in team {Id}, but is not.");
            }
        }
    }
}
