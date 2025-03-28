#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Scripting;

public sealed class WaypointCollection
{
    private readonly Dictionary<int, Waypoint> _waypointsByID;
    private readonly Dictionary<string, Waypoint> _waypointsByName;
    private readonly Dictionary<string, HashSet<Waypoint>> _waypointsByPathLabel;

    public Waypoint this[string name] => _waypointsByName[name];
    public Waypoint this[int id] => _waypointsByID[id];

    public WaypointCollection()
    {
        // Note that we explicitly allow duplicate waypoint names.

        _waypointsByName = [];
        _waypointsByID = [];
        _waypointsByPathLabel = [];
    }

    public WaypointCollection(IEnumerable<Waypoint> waypoints, IEnumerable<WaypointPath> paths)
        : this()
    {
        foreach (var waypoint in waypoints)
        {
            _waypointsByName[waypoint.Name] = waypoint;
            _waypointsByID[waypoint.ID] = waypoint;

            foreach (var pathLabel in waypoint.PathLabels)
            {
                if (!_waypointsByPathLabel.TryGetValue(pathLabel, out var collection))
                {
                    collection = [];
                    _waypointsByPathLabel.Add(pathLabel, collection);
                }

                collection.Add(waypoint);
            }
        }

        foreach (var path in paths)
        {
            if (_waypointsByID.TryGetValue(path.StartWaypointID, out var startWaypoint) &&
                _waypointsByID.TryGetValue(path.EndWaypointID, out var endWaypoint))
            {
                startWaypoint.AddConnectionTo(endWaypoint);
            }
        }
    }

    public bool TryGetByName(string name, out Waypoint? waypoint)
    {
        return _waypointsByName.TryGetValue(name, out waypoint);
    }

    public IReadOnlyCollection<Waypoint> GetByPathLabel(string pathLabel)
    {
        return _waypointsByPathLabel.TryGetValue(pathLabel, out var waypoints) ? waypoints : Array.Empty<Waypoint>();
    }

    public bool TryGetPlayerStart(int playerIndex, out Waypoint? waypoint)
    {
        // Generals uses 0-based player indices, but 1-based indices for waypoints.
        // However we already use 1-based indices for both, so we don't need to adjust here.
        return TryGetByName($"Player_{playerIndex}_Start", out waypoint);
    }
}


[DebuggerDisplay("ID = {ID}, Name = {Name}")]
public sealed class Waypoint
{
    public const string ObjectTypeName = "*Waypoints/Waypoint";

    private List<Waypoint>? _connectedWaypoints;

    public int ID { get; }
    public string Name { get; }
    public Vector3 Position { get; }

    public IEnumerable<string> PathLabels { get; }

    internal Waypoint(MapObject mapObject)
    {
        ID = (int)mapObject.Properties["waypointID"].Value;
        Name = (string)mapObject.Properties["waypointName"].Value;
        Position = mapObject.Position;

        // It seems that if one of the label properties exists, all of them do
        if (mapObject.Properties.TryGetValue("waypointPathLabel1", out var label1))
        {
            PathLabels = new[]
            {
                (string) label1.Value,
                (string) mapObject.Properties["waypointPathLabel2"].Value,
                (string) mapObject.Properties["waypointPathLabel3"].Value
            }.WhereNot(string.IsNullOrWhiteSpace).ToSet();
        }
        else
        {
            PathLabels = [];
        }
    }

    public void AddConnectionTo(Waypoint waypoint)
    {
        _connectedWaypoints ??= [];
        _connectedWaypoints.Add(waypoint);
    }

    public IReadOnlyList<Waypoint> ConnectedWaypoints =>
        (IReadOnlyList<Waypoint>?)_connectedWaypoints ?? [];

    /// <summary>
    /// Follows a waypoint path starting with this waypoint.
    /// A path can contain branches and loops, which means that
    /// a) we need a random number generator to pick a path when there is more than one and
    /// b) the returned enumerable is potentially infinite, so avoid materializing it by
    /// calling <see cref="Enumerable.ToList"/> and co.
    /// </summary>
    public IEnumerable<Vector3> FollowPath(IRandom random)
    {
        var currentWaypoint = this;
        while (currentWaypoint != null)
        {
            yield return currentWaypoint.Position;
            var connectedWaypoints = currentWaypoint.ConnectedWaypoints;
            currentWaypoint = connectedWaypoints.Count switch
            {
                0 => null,
                1 => connectedWaypoints[0],
                int n => connectedWaypoints[random.Next(0, n - 1)]
            };
        }
    }
}
