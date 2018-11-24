using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LargeGroupAudioUpdateModuleData : UpdateModuleData
    {
        internal static LargeGroupAudioUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LargeGroupAudioUpdateModuleData> FieldParseTable = new IniParseTable<LargeGroupAudioUpdateModuleData>
        {
           { "Key", (parser, x) => x.Keys.AddRange(parser.ParseAssetReferenceArray()) },
           { "UnitWeight", (parser, x) => x.UnitWeight = parser.ParseInteger() },
        };

       public List<string> Keys { get; } = new List<string>();
       public int UnitWeight { get; private set; }
    }
}
