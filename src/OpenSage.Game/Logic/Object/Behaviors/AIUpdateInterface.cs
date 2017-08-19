using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public class AIUpdateInterface : ObjectBehavior
    {
        internal static AIUpdateInterface Parse(IniParser parser) => parser.ParseBlock(BaseFieldParseTable);

        internal static readonly IniParseTable<AIUpdateInterface> BaseFieldParseTable = new IniParseTable<AIUpdateInterface>
        {
            { "Turret", (parser, x) => x.Turret = TurretAIData.Parse(parser) },
            { "AutoAcquireEnemiesWhenIdle", (parser, x) => x.AutoAcquireEnemiesWhenIdle = AutoAcquireEnemies.Parse(parser) },
            { "MoodAttackCheckRate", (parser, x) => x.MoodAttackCheckRate = parser.ParseInteger() }
        };

        /// <summary>
        /// Allows the use of TurretMoveStart and TurretMoveLoop within the UnitSpecificSounds 
        /// section of the object.
        /// </summary>
        public TurretAIData Turret { get; private set; }

        public AutoAcquireEnemies AutoAcquireEnemiesWhenIdle { get; private set; }
        public int MoodAttackCheckRate { get; private set; }
    }

    public struct AutoAcquireEnemies
    {
        internal static AutoAcquireEnemies Parse(IniParser parser)
        {
            var result = new AutoAcquireEnemies
            {
                AutoAttackUnits = parser.ParseBoolean()
            };

            if (parser.CurrentTokenType == IniTokenType.Identifier)
            {
                result.Type = parser.ParseEnum<AutoAcquireEnemiesType>();
            }

            return result;
        }

        public bool AutoAttackUnits { get; private set; }
        public AutoAcquireEnemiesType Type { get; private set; }
    }

    public enum AutoAcquireEnemiesType
    {
        /// <summary>
        /// Attack buildings in addition to units.
        /// </summary>
        [IniEnum("ATTACK_BUILDINGS")]
        AttackBuildings,

        /// <summary>
        /// Don't counter-attack.
        /// </summary>
        [IniEnum("NotWhileAttacking")]
        NotWhileAttacking,
    }
}
