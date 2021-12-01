using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class RailroadBehavior : PhysicsBehavior
    {
        internal RailroadBehavior(GameObject gameObject, GameContext context, PhysicsBehaviorModuleData moduleData)
            : base(gameObject, context, moduleData)
        {
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            base.Load(reader);

            // TODO
        }
    }

    /// <summary>
    /// Requires object to follow waypoint path named with Tunnel, Disembark or Station with "Start"
    /// and "End" convention.
    /// </summary>
    public sealed class RailroadBehaviorModuleData : PhysicsBehaviorModuleData
    {
        internal new static RailroadBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<RailroadBehaviorModuleData> FieldParseTable = PhysicsBehaviorModuleData.FieldParseTable.Concat(new IniParseTable<RailroadBehaviorModuleData>
        {
            { "PathPrefixName", (parser, x) => x.PathPrefixName = parser.ParseAssetReference() },

            { "RunningGarrisonSpeedMax", (parser, x) => x.RunningGarrisonSpeedMax = parser.ParseInteger() },
            { "KillSpeedMin", (parser, x) => x.KillSpeedMin = parser.ParseInteger() },
            { "Friction", (parser, x) => x.Friction = parser.ParseFloat() },
            { "BigMetalBounceSound", (parser, x) => x.BigMetalBounceSound = parser.ParseAssetReference() },
            { "SmallMetalBounceSound", (parser, x) => x.SmallMetalBounceSound = parser.ParseAssetReference() },
            { "MeatyBounceSound", (parser, x) => x.MeatyBounceSound = parser.ParseAssetReference() },
            { "ClicketyClackSound", (parser, x) => x.ClicketyClackSound = parser.ParseAssetReference() },
            { "WhistleSound", (parser, x) => x.WhistleSound = parser.ParseAssetReference() },

            { "IsLocomotive", (parser, x) => x.IsLocomotive = parser.ParseBoolean() },
            { "SpeedMax", (parser, x) => x.SpeedMax = parser.ParseFloat() },
            { "Acceleration", (parser, x) => x.Acceleration = parser.ParseFloat() },
            { "WaitAtStationTime", (parser, x) => x.WaitAtStationTime = parser.ParseInteger() },
            { "Braking", (parser, x) => x.Braking = parser.ParseFloat() },
            { "RunningSound", (parser, x) => x.RunningSound = parser.ParseAssetReference() },
            { "CrashFXTemplateName", (parser, x) => x.CrashFXTemplateName = parser.ParseAssetReference() },

            { "CarriageTemplateName", (parser, x) => x.CarriageTemplateNames.Add(parser.ParseAssetReference()) },
        });

        /// <summary>
        /// Waypoint prefix name.
        /// </summary>
        public string PathPrefixName { get; private set; }

        // Parameters for all carriages

        public int RunningGarrisonSpeedMax { get; private set; }
        public int KillSpeedMin { get; private set; }
        public float Friction { get; private set; }
        public string BigMetalBounceSound { get; private set; }
        public string SmallMetalBounceSound { get; private set; }
        public string MeatyBounceSound { get; private set; }
        public string ClicketyClackSound { get; private set; }
        public string WhistleSound { get; private set; }

        // Parameters that are only applicable when IsLocomotive = True.

        public bool IsLocomotive { get; private set; }
        public float SpeedMax { get; private set; }
        public float Acceleration { get; private set; }
        public int WaitAtStationTime { get; private set; }
        public float Braking { get; private set; }
        public string RunningSound { get; private set; }
        public string CrashFXTemplateName { get; private set; }

        public List<string> CarriageTemplateNames { get; } = new List<string>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new RailroadBehavior(gameObject, context, this);
        }
    }
}
