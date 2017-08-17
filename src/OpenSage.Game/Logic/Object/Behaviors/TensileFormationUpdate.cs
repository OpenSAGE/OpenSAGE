using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows object to share dynamic formation. If not enabled, then damage to this object will 
    /// affect all nearby objects that have this module, otherwise damage only affects this object.
    /// </summary>
    public sealed class TensileFormationUpdate : ObjectBehavior
    {
        internal static TensileFormationUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TensileFormationUpdate> FieldParseTable = new IniParseTable<TensileFormationUpdate>
        {
            { "Enabled", (parser, x) => x.Enabled = parser.ParseBoolean() },
            { "CrackSound", (parser, x) => x.CrackSound = parser.ParseAssetReference() }
        };

        public bool Enabled { get; private set; }
        public string CrackSound { get; private set; }
    }
}
