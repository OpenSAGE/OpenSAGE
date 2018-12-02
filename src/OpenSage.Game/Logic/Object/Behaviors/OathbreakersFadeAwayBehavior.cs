using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class OathbreakersFadeAwayBehaviorModuleData : BehaviorModuleData
    {
        internal static OathbreakersFadeAwayBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<OathbreakersFadeAwayBehaviorModuleData> FieldParseTable = new IniParseTable<OathbreakersFadeAwayBehaviorModuleData>
        {
            { "FadeOutTime", (parser, x) => x.FadeOutTime = parser.ParseInteger() },
        };

        public int FadeOutTime { get; private set; }
    }
}
