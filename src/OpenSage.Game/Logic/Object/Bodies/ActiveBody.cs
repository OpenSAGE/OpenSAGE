using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class ActiveBody : ObjectBody
    {
        internal static ActiveBody Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ActiveBody> FieldParseTable = new IniParseTable<ActiveBody>()
            .Concat<ActiveBody, ObjectBody>(BodyFieldParseTable);
    }
}
