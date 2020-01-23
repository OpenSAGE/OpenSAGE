using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class ReplaceSelfUpgradeModuleData : UpgradeModuleData
    {
        internal static ReplaceSelfUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ReplaceSelfUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<ReplaceSelfUpgradeModuleData>
            {
                { "ReplaceWith", (parser, x) => x.ReplaceWith = parser.ParseAssetReference() },
                { "AndThenAddA", (parser, x) => x.AndThenAddAs.Add(parser.ParseAssetReference()) }
            });

        public string ReplaceWith { get; private set; }
        public List<string> AndThenAddAs { get; } = new List<string>();
    }
}
