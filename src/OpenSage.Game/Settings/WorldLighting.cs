using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Graphics.Effects;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Settings
{
    public sealed class WorldLighting
    {
        public IReadOnlyDictionary<TimeOfDay, LightSettings> LightingConfigurations { get; }

        public TimeOfDay TimeOfDay { get; set; }

        public bool EnableCloudShadows { get; set; } = true;

        public LightSettings CurrentLightingConfiguration => LightingConfigurations[TimeOfDay];

        public static WorldLighting CreateDefault()
        {
            var lights = new LightingConstantsPS
            {
                Light0 = new Light
                {
                    Ambient = new Vector3(0.3f, 0.3f, 0.3f),
                    Direction = Vector3.Normalize(new Vector3(-0.3f, 0.2f, -0.8f)),
                    Color = new Vector3(0.7f, 0.7f, 0.8f)
                },
            };

            var lightingConfigurations = new Dictionary<TimeOfDay, LightSettings>
            {
                {
                    TimeOfDay.Morning,
                    new LightSettings(lights, lights)
                }
            };

            return new WorldLighting(lightingConfigurations, TimeOfDay.Morning);
        }

        public WorldLighting(
            IReadOnlyDictionary<TimeOfDay, LightSettings> lightingConfigurations,
            TimeOfDay timeOfDay)
        {
            LightingConfigurations = lightingConfigurations;
            TimeOfDay = timeOfDay;
        }
    }

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
    }

    [DebuggerDisplay("ID = {ID}, Name = {Name}")]
    public sealed class Waypoint
    {
        public uint ID { get; }
        public string Name { get; }
        public Vector3 Position { get; }

        public IEnumerable<string> PathLabels { get; }

        public Waypoint(uint id, string name, Vector3 position, IEnumerable<string> pathLabels = null)
        {
            ID = id;
            Name = name;
            Position = position;

            PathLabels = pathLabels != null
                ? pathLabels.WhereNot(string.IsNullOrWhiteSpace).ToSet()
                : Enumerable.Empty<string>();
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

        public WaypointPathCollection(IEnumerable<WaypointPath> paths)
            : this()
        {
            foreach (var path in paths)
            {
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
