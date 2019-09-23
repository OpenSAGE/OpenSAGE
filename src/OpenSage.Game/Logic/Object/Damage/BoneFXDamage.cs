using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Enables use of BoneFXUpdate module on this object where an additional dynamic FX logic can 
    /// be used.
    /// </summary>
    public sealed class BoneFXDamageModuleData : DamageModuleData
    {
        internal static BoneFXDamageModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BoneFXDamageModuleData> FieldParseTable = new IniParseTable<BoneFXDamageModuleData>();
    }
}
