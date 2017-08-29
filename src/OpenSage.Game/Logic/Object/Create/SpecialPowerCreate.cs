using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Forces the object's SpecialPower to start charging upon creation of the object. Required 
    /// by special powers that have <see cref="SpecialPower.PublicTimer"/> set to <code>true</code>.
    /// </summary>
    public sealed class SpecialPowerCreateModuleData : CreateModuleData
    {
        internal static SpecialPowerCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpecialPowerCreateModuleData> FieldParseTable = new IniParseTable<SpecialPowerCreateModuleData>();
    }
}
