using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the object to take damage but not die. The object will only die from irresistable damage.
    /// </summary>
    public sealed class HighlanderBody : ObjectBody
    {
        internal static HighlanderBody Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HighlanderBody> FieldParseTable = new IniParseTable<HighlanderBody>()
            .Concat<HighlanderBody, ObjectBody>(BodyFieldParseTable);
    }
}
