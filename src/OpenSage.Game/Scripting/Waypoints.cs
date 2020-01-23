using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Scripting
{
    public sealed class WaypointCollection  
    {
        private readonly Dictionary<uint, Waypoint> _waypointsByID;
        private readonly Dictionary<string, Waypoint> _waypointsByName;
        private readonly Dictionary<string, HashSet<Waypoint>> _waypointsByPathLabel;

        public Waypoint this[string name] => _waypointsByName[name];
        public Waypoint this[uint id] => _waypointsByID[id];

        public WaypointCollection()
        {
            // Note that we explicitly allow duplicate waypoint names.

            _waypointsByName = new Dictionary<string, Waypoint>();
            _waypointsByID = new Dictionary<uint, Waypoint>();
            _waypointsByPathLabel = new Dictionary<string, HashSet<Waypoint>>();
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
                        collection = new HashSet<Waypoint>();
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

        public bool TryGetByName(string name, out Waypoint waypoint)
        {
            return _waypointsByName.TryGetValue(name, out waypoint);
        }

        public IReadOnlyCollection<Waypoint> GetByPathLabel(string pathLabel)
        {
            return _waypointsByPathLabel.TryGetValue(pathLabel, out var waypoints) ?
                (IReadOnlyCollection<Waypoint>) waypoints :
                Array.Empty<Waypoint>();
        }
    }

    [DebuggerDisplay("ID = {ID}, Name = {Name}")]
    public sealed class Waypoint
    {
        public const string ObjectTypeName = "*Waypoints/Waypoint";

        private List<Waypoint> _connectedWaypoints;

        public uint ID { get; }
        public string Name { get; }
        public Vector3 Position { get; }

        public IEnumerable<string> PathLabels { get; }

        internal Waypoint(MapObject mapObject)
        {
            ID = (uint) mapObject.Properties["waypointID"].Value;
            Name = (string) mapObject.Properties["waypointName"].Value;
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
                PathLabels = Enumerable.Empty<string>();
            }
        }

        public void AddConnectionTo(Waypoint waypoint)
        {
            _connectedWaypoints ??= new List<Waypoint>();
            _connectedWaypoints.Add(waypoint);
        }

        public IReadOnlyList<Waypoint> ConnectedWaypoints =>
            (IReadOnlyList<Waypoint>) _connectedWaypoints ?? Array.Empty<Waypoint>();

        /// <summary>
        /// Follows a waypoint path starting with this waypoint.
        /// A path can contain branches and loops, which means that
        /// a) we need a random number generator to pick a path when there is more than one and
        /// b) the returned enumerable is potentially infinite, so avoid materializing it by
        /// calling <see cref="Enumerable.ToList"/> and co.
        /// </summary>
        public IEnumerable<Vector3> FollowPath(Random random)
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
                    int n => connectedWaypoints[random.Next(n)]
                };
            }
        }
    }
}
