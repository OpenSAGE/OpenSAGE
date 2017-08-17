using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Forces the object's SpecialPower to start charging upon creation of the object. Required 
    /// by special powers that have <see cref="SpecialPower.PublicTimer"/> set to <code>true</code>.
    /// </summary>
    public sealed class SpecialPowerCreate : ObjectBehavior
    {
        internal static SpecialPowerCreate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpecialPowerCreate> FieldParseTable = new IniParseTable<SpecialPowerCreate>();
    }
}
