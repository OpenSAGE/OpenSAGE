using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public class AIUpdateInterfaceBehavior : ObjectBehavior
    {
        internal static AIUpdateInterfaceBehavior Parse(IniParser parser) => parser.ParseBlock(BaseFieldParseTable);

        internal static readonly IniParseTable<AIUpdateInterfaceBehavior> BaseFieldParseTable = new IniParseTable<AIUpdateInterfaceBehavior>
        {
            { "Turret", (parser, x) => x.Turret = TurretAIData.Parse(parser) },
            { "AutoAcquireEnemiesWhenIdle", (parser, x) => x.AutoAcquireEnemiesWhenIdle = parser.ParseBoolean() }
        };

        /// <summary>
        /// Allows the use of TurretMoveStart and TurretMoveLoop within the UnitSpecificSounds 
        /// section of the object.
        /// </summary>
        public TurretAIData Turret { get; private set; }

        public bool AutoAcquireEnemiesWhenIdle { get; private set; }
    }
}
