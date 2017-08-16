using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows object to continue to exist as an obstacle but allowing water terrain to move 
    /// through. The module must be applied after any other death modules.
    /// </summary>
    public sealed class DamDieBehavior : ObjectBehavior
    {
        internal static DamDieBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DamDieBehavior> FieldParseTable = new IniParseTable<DamDieBehavior>();
    }
}
