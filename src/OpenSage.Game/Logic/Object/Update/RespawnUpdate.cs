using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class RespawnUpdateModuleData : UpdateModuleData
    {
        internal static RespawnUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RespawnUpdateModuleData> FieldParseTable = new IniParseTable<RespawnUpdateModuleData>
        {
            { "DeathAnim", (parser, x) => x.DeathAnim = parser.ParseEnum<ModelConditionFlag>() },
            { "DeathFX", (parser, x) => x.DeathFX = parser.ParseAssetReference() },
            { "DeathAnimationTime", (parser, x) => x.DeathAnimationTime = parser.ParseInteger() },
            { "InitialSpawnFX", (parser, x) => x.InitialSpawnFX = parser.ParseAssetReference() },
            { "RespawnAnim", (parser, x) => x.RespawnAnim = parser.ParseEnum<ModelConditionFlag>() },
            { "RespawnFX", (parser, x) => x.RespawnFX = parser.ParseAssetReference() },
            { "RespawnAnimationTime", (parser, x) => x.RespawnAnimationTime = parser.ParseInteger() },
            { "AutoRespawnAtObjectFilter", (parser, x) => x.AutoRespawnAtObjectFilter = ObjectFilter.Parse(parser) },
            { "ButtonImage", (parser, x) => x.ButtonImage = parser.ParseAssetReference() },
            { "RespawnRules", (parser, x) => x.RespawnRules = RespawnRules.Parse(parser) },
            { "RespawnEntry", (parser, x) => x.RespawnEntries.Add(RespawnEntry.Parse(parser)) },
            { "RespawnAsTemplate", (parser, x) => x.RespawnAsTemplate = parser.ParseAssetReference() },
        };

        public ModelConditionFlag DeathAnim { get; private set; }
        public string DeathFX { get; private set; }
        public int DeathAnimationTime { get; private set; }
        public string InitialSpawnFX { get; private set; }
        public ModelConditionFlag RespawnAnim { get; private set; }
        public string RespawnFX { get; private set; }
        public int RespawnAnimationTime { get; private set; }
        public ObjectFilter AutoRespawnAtObjectFilter { get; private set; }
        public string ButtonImage { get; private set; }
        public RespawnRules RespawnRules { get; private set; }
        public List<RespawnEntry> RespawnEntries { get; } = new List<RespawnEntry>();
        public string RespawnAsTemplate { get; private set; }
    }

    public sealed class RespawnRules
    {
        internal static RespawnRules Parse(IniParser parser)
        {
            return new RespawnRules()
            {
                AutoSpawn = parser.ParseAttributeBoolean("AutoSpawn"),
                Cost = parser.ParseAttributeInteger("Cost"),
                Time = parser.ParseAttributeInteger("Time"),
                Health = parser.ParseAttributePercentage("Health")
            };
        }

        public bool AutoSpawn { get; private set; }
        public int Cost { get; private set; }
        public int Time { get; private set; }
        public Percentage Health { get; private set; }
    }

    public sealed class RespawnEntry
    {
        internal static RespawnEntry Parse(IniParser parser)
        {
            return new RespawnEntry()
            {
                Level = parser.ParseAttributeInteger("Level"),
                Cost = parser.ParseAttributeInteger("Cost"),
                Time = parser.ParseAttributeInteger("Time")
            };
        }

        public int Level { get; private set; }
        public int Cost { get; private set; }
        public int Time { get; private set; }
    }
}
