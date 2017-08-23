using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows use of the PACKING and UNPACKING condition states and allows the use of Deploy and 
    /// Undeploy within UnitSpecificSounds section of the object.
    /// </summary>
    public sealed class DeployStyleAIUpdate : ObjectBehavior
    {
        internal static DeployStyleAIUpdate Parse(IniParser parser) => parser.ParseBlock(BaseFieldParseTable);

        internal static readonly IniParseTable<DeployStyleAIUpdate> BaseFieldParseTable = new IniParseTable<DeployStyleAIUpdate>
        {
            { "Turret", (parser, x) => x.Turret = TurretAIData.Parse(parser) },

            { "AutoAcquireEnemiesWhenIdle", (parser, x) => x.AutoAcquireEnemiesWhenIdle = AutoAcquireEnemies.Parse(parser) },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "TurretsFunctionOnlyWhenDeployed", (parser, x) => x.TurretsFunctionOnlyWhenDeployed = parser.ParseBoolean() },
            { "TurretsMustCenterBeforePacking", (parser, x) => x.TurretsMustCenterBeforePacking = parser.ParseBoolean() }
        };

        public TurretAIData Turret { get; private set; }

        public AutoAcquireEnemies AutoAcquireEnemiesWhenIdle { get; private set; }
        public int PackTime { get; private set; }
        public int UnpackTime { get; private set; }
        public bool TurretsFunctionOnlyWhenDeployed { get; private set; }
        public bool TurretsMustCenterBeforePacking { get; private set; }
    }
}
