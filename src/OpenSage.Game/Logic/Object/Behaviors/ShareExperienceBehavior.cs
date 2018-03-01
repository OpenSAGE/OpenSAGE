using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ShareExperienceBehaviorModuleData : UpdateModuleData
    {
        internal static ShareExperienceBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ShareExperienceBehaviorModuleData> FieldParseTable = new IniParseTable<ShareExperienceBehaviorModuleData>
        {
            { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) },
            { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
            { "DropOff", (parser, x) => x.DropOff = parser.ParseFloat() }
        };

        public ObjectFilter ObjectFilter { get; private set; }
        public float Radius { get; private set; }
        public float DropOff { get; private set; }
    }
}
