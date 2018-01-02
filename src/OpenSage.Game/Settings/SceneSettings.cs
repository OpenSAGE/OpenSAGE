using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Graphics.Effects;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Settings
{
    public sealed class SceneSettings
    {
        public Dictionary<TimeOfDay, LightSettings> LightingConfigurations { get; set; }

        public TimeOfDay TimeOfDay { get; set; }

        internal LightSettings CurrentLightingConfiguration => LightingConfigurations[TimeOfDay];

        public WaypointCollection Waypoints { get; set; }
        public WaypointPathCollection WaypointPaths { get; set; }

        public SceneSettings()
        {
            var lights = new LightingConstants
            {
                Light0 = new Light
                {
                    Ambient = new Vector3(0.3f, 0.3f, 0.3f),
                    Direction = Vector3.Normalize(new Vector3(-0.3f, 0.2f, -0.8f)),
                    Color = new Vector3(0.7f, 0.7f, 0.8f)
                }
            };

            LightingConfigurations = new Dictionary<TimeOfDay, LightSettings>
            {
                {
                    TimeOfDay.Morning,
                    new LightSettings
                    {
                        TerrainLights = lights,
                        ObjectLights = lights
                    }
                }
            };

            TimeOfDay = TimeOfDay.Morning;

            Waypoints = new WaypointCollection(new Waypoint[0]);
            WaypointPaths = new WaypointPathCollection(new WaypointPath[0]);
        }
    }

    public sealed class WaypointCollection
    {
        private readonly Dictionary<string, Waypoint> _waypointsByName;
        private readonly Dictionary<uint, Waypoint> _waypointsByID;

        public Waypoint this[string name] => _waypointsByName[name];
        public Waypoint this[uint id] => _waypointsByID[id];

        public WaypointCollection(IEnumerable<Waypoint> waypoints)
        {
            // Note that we explicitly allow duplicate waypoint names.

            _waypointsByName = new Dictionary<string, Waypoint>();
            _waypointsByID = new Dictionary<uint, Waypoint>();

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

        public WaypointPath this[Waypoint firstNode] => _waypointPathsByFirstNode[firstNode];
        public WaypointPath this[string label] => _waypointPathsByLabel[label];

        public WaypointPathCollection(IEnumerable<WaypointPath> paths)
        {
            _waypointPathsByLabel = new Dictionary<string, WaypointPath>();
            _waypointPathsByFirstNode = new Dictionary<Waypoint, WaypointPath>();

            foreach (var path in paths)
            {
                foreach (var label in path.Start.PathLabels)
                {
                    _waypointPathsByLabel[label] = path;
                }

                _waypointPathsByFirstNode[path.Start] = path;
            }
        }
    }

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
