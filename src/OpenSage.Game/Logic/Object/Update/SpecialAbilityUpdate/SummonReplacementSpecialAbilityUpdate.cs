using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2Rotwk)]
    public sealed class SummonReplacementSpecialAbilityUpdateModuleData : SpecialAbilityUpdateModuleData
    {
        internal new static SummonReplacementSpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<SummonReplacementSpecialAbilityUpdateModuleData> FieldParseTable = SpecialAbilityUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<SummonReplacementSpecialAbilityUpdateModuleData>
            {
                { "MountedTemplate", (parser, x) => x.MountedTemplate = parser.ParseAssetReference() },
                { "MustFinishAbility", (parser, x) => x.MustFinishAbility = parser.ParseBoolean() }
            });

        public string MountedTemplate { get; private set; }
        public bool MustFinishAbility { get; private set; }
    }
}
