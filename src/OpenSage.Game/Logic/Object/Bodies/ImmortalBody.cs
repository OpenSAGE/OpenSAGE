using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Prevents the object from dying or taking damage.
    /// </summary>
    public sealed class ImmortalBody : ObjectBody
    {
        internal static ImmortalBody Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ImmortalBody> FieldParseTable = new IniParseTable<ImmortalBody>()
            .Concat<ImmortalBody, ObjectBody>(BodyFieldParseTable);
    }
}
