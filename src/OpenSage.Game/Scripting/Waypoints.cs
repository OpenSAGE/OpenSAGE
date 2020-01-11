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
        private readonly Dictionary<string, Waypoint> _waypointsByName;
        private readonly Dictionary<uint, Waypoint> _waypointsByID;

        public Waypoint this[string name] => _waypointsByName[name];
        public Waypoint this[uint id] => _waypointsByID[id];

        public WaypointCollection()
        {
            // Note that we explicitly allow duplicate waypoint names.

            _waypointsByName = new Dictionary<string, Waypoint>();
            _waypointsByID = new Dictionary<uint, Waypoint>();
        }

        public WaypointCollection(IEnumerable<Waypoint> waypoints)
            : this()
        {
            foreach (var waypoint in waypoints)
            {
                _waypointsByName[waypoint.Name] = waypoint;
                _waypointsByID[waypoint.ID] = waypoint;
            }
        }

        public bool TryGetByName(string name, out Waypoint waypoint)
        {
            return _waypointsByName.TryGetValue(name, out waypoint);
        }
    }

    [DebuggerDisplay("ID = {ID}, Name = {Name}")]
    public sealed class Waypoint
    {
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
    }

    public sealed class WaypointPathCollection
    {
        private readonly Dictionary<string, WaypointPath> _waypointPathsByLabel;
        private readonly Dictionary<Waypoint, WaypointPath> _waypointPathsByFirstNode;

        public WaypointPath this[Waypoint firstNode] => _waypointPathsByFirstNode.TryGetValue(firstNode, out var path) ? path : null;
        public WaypointPath this[string label] => _waypointPathsByLabel.TryGetValue(label, out var path) ? path : null;

        public WaypointPathCollection()
        {
            _waypointPathsByLabel = new Dictionary<string, WaypointPath>();
            _waypointPathsByFirstNode = new Dictionary<Waypoint, WaypointPath>();
        }

        public WaypointPathCollection(WaypointCollection waypoints, Data.Map.WaypointPath[] paths)
            : this()
        {
            foreach (var waypointPath in paths)
            {
                var start = waypoints[waypointPath.StartWaypointID];
                var end = waypoints[waypointPath.EndWaypointID];
                var path = new WaypointPath(start, end);

                foreach (var label in path.Start.PathLabels)
                {
                    _waypointPathsByLabel[label] = path;
                }

                _waypointPathsByFirstNode[path.Start] = path;
            }
        }

        /// <summary>
        /// Iterates through the waypoint path, starting at given node.
        /// </summary>
        public IEnumerable<Waypoint> GetFullPath(Waypoint node)
        {
            while (node != null)
            {
                yield return node;
                node = this[node]?.End;
            }
        }

        public IEnumerable<Waypoint> GetFullPath(string label) => GetFullPath(this[label]?.Start);
    }

    // Note: this is not actually a path despite the name, just a node of a doubly linked list.
    public sealed class WaypointPath
    {
        public Waypoint Start { get; }
        public Waypoint End { get; }

        public WaypointPath(Waypoint start, Waypoint end)
        {
            Start = start;
            End = end;
        }
    }
}
