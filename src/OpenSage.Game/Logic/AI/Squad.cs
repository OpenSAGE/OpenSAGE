#nullable enable

using System.Collections.Generic;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI;

public sealed class Squad(IGame game) : IPersistableObject
{
    private readonly IGame _game = game;

    private List<ObjectId> _objectIds = [];

    public int SizeOfGroup => _objectIds.Count;

    public void AddObject(GameObject gameObject)
    {
        _objectIds.Add(gameObject.Id);
    }

    public void AddObjectId(ObjectId objectId)
    {
        _objectIds.Add(objectId);
    }

    public void RemoveObject(GameObject gameObject)
    {
        _objectIds.Remove(gameObject.Id);
    }

    public void ClearSquad()
    {
        _objectIds.Clear();
    }

    // TODO: C++ version of this doesn't allocate, and instead just recycles one vector instance (for both this and GetLiveObjects).
    // That's good for performance if this gets called a lot, but seems very easy to misuse.

    /// <summary>
    /// Returns a list of all objects in the squad.
    /// Removes dead objects from the squad as a side effect.
    /// </summary>
    public List<GameObject> GetAllObjects()
    {
        var gameObjects = new List<GameObject>(_objectIds.Count);
        var idsToRemove = new List<ObjectId>(0);

        foreach (var objectId in _objectIds)
        {
            var gameObject = _game.GameLogic.GetObjectById(objectId);
            if (gameObject != null)
            {
                gameObjects.Add(gameObject);
            }
            else
            {
                idsToRemove.Add(objectId);
            }
        }

        foreach (var objectId in idsToRemove)
        {
            _objectIds.Remove(objectId);
        }

        return gameObjects;
    }

    /// <summary>
    /// Returns a list of all objects in the squad that are selectable.
    /// Removes dead objects from the squad as a side effect.
    /// </summary>
    public List<GameObject> GetLiveObjects()
    {
        var allObjects = GetAllObjects();
        allObjects.RemoveAll(obj => !obj.IsSelectable);
        return allObjects;
    }

    public bool IsOnSquad(GameObject gameObject)
    {
        return _objectIds.Contains(gameObject.Id);
    }

    public void SquadFromTeam(Team fromTeam, bool clearSquadFirst)
    {
        if (clearSquadFirst)
        {
            ClearSquad();
        }

        foreach (var objectId in fromTeam.ObjectIds)
        {
            // TODO: C++ version of Team stores objects, not IDs. Does that matter?
            var gameObject = _game.GameLogic.GetObjectById(objectId);
            if (gameObject != null)
            {
                _objectIds.Add(objectId);
            }
        }
    }

    public void SquadFromAIGroup(AIGroup fromAIGroup, bool clearSquadFirst)
    {
        if (clearSquadFirst)
        {
            ClearSquad();
        }

        _objectIds = fromAIGroup.GetAllIds();
    }

    public void AIGroupFromSquad(AIGroup aiGroupToFill)
    {
        foreach (var obj in GetLiveObjects())
        {
            aiGroupToFill.Add(obj);
        }
    }

    public void Persist(StatePersister persister)
    {
        persister.PersistVersion(1);

        persister.PersistList(
            _objectIds,
            static (StatePersister persister, ref ObjectId item) =>
            {
                persister.PersistObjectIdValue(ref item);
            }
        );
    }
}

// TODO(Port): This is a placeholder for now.
public sealed record class AIGroup
{
    private readonly List<ObjectId> _memberList = [];

    public List<ObjectId> GetAllIds()
    {
        return _memberList;
    }

    public void Add(GameObject gameObject)
    {
        _memberList.Add(gameObject.Id);
    }
}
