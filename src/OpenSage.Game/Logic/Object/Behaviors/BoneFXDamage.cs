using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Enables use of BoneFXUpdate module on this object where an additional dynamic FX logic can 
    /// be used.
    /// </summary>
    public sealed class BoneFXDamage : ObjectBehavior
    {
        internal static BoneFXDamage Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BoneFXDamage> FieldParseTable = new IniParseTable<BoneFXDamage>();
    }
}
