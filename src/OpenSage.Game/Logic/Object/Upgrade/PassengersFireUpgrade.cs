using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Contain modules should have the "PassengersAllowedToFire" parameter set to "No" in order 
    /// for this module to work.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class PassengersFireUpgrade : ObjectBehavior
    {
        internal static PassengersFireUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PassengersFireUpgrade> FieldParseTable = new IniParseTable<PassengersFireUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() }
        };

        public string TriggeredBy { get; private set; }
    }
}
