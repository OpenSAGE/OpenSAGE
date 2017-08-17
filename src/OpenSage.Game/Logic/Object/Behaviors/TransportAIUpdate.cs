using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Used on TRANSPORT KindOfs that contain other objects.
    /// </summary>
    public sealed class TransportAIUpdate : AIUpdateInterface
    {
        internal static new TransportAIUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TransportAIUpdate> FieldParseTable = new IniParseTable<TransportAIUpdate>()
            .Concat<TransportAIUpdate, AIUpdateInterface>(BaseFieldParseTable);
    }
}
