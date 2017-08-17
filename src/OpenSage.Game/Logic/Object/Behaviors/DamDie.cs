using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows object to continue to exist as an obstacle but allowing water terrain to move 
    /// through. The module must be applied after any other death modules.
    /// </summary>
    public sealed class DamDie : ObjectBehavior
    {
        internal static DamDie Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DamDie> FieldParseTable = new IniParseTable<DamDie>();
    }
}
