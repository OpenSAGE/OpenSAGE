using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class AIUpdateInterfaceBehavior : ObjectBehavior
    {
        internal static AIUpdateInterfaceBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AIUpdateInterfaceBehavior> FieldParseTable = new IniParseTable<AIUpdateInterfaceBehavior>
        {
            { "Turret", (parser, x) => x.Turret = TurretAIData.Parse(parser) }
        };

        /// <summary>
        /// Allows the use of TurretMoveStart and TurretMoveLoop within the UnitSpecificSounds 
        /// section of the object.
        /// </summary>
        public TurretAIData Turret { get; private set; }
    }
}
