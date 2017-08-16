using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Prevents normal interaction with other objects.
    /// </summary>
    public sealed class InactiveBody : ObjectBody
    {
        internal static InactiveBody Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InactiveBody> FieldParseTable = new IniParseTable<InactiveBody>();
    }
}
