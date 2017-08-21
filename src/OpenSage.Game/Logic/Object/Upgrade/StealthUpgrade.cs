using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Eenables use of StealthUpdate module on this object. Requires InnateStealth = No defined in 
    /// the StealthUpdate module.
    /// </summary>
    public sealed class StealthUpgrade : ObjectBehavior
    {
        internal static StealthUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StealthUpgrade> FieldParseTable = new IniParseTable<StealthUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() }
        };

        public string TriggeredBy { get; private set; }
    }
}
