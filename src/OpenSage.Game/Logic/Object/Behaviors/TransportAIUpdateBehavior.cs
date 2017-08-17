using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Used on TRANSPORT KindOfs that contain other objects.
    /// </summary>
    public sealed class TransportAIUpdateBehavior : AIUpdateInterfaceBehavior
    {
        internal static new TransportAIUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TransportAIUpdateBehavior> FieldParseTable = new IniParseTable<TransportAIUpdateBehavior>()
            .Concat<TransportAIUpdateBehavior, AIUpdateInterfaceBehavior>(BaseFieldParseTable);
    }
}
