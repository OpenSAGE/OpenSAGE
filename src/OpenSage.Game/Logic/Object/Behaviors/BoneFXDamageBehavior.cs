using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Enables use of BoneFXUpdate module on this object where an additional dynamic FX logic can 
    /// be used.
    /// </summary>
    public sealed class BoneFXDamageBehavior : ObjectBehavior
    {
        internal static BoneFXDamageBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BoneFXDamageBehavior> FieldParseTable = new IniParseTable<BoneFXDamageBehavior>();
    }
}
