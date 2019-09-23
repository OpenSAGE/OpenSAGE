using OpenSage.Data.Ini;
using OpenSage.Logic.Object.Damage;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class DamageFilteredCreateObjectDieModuleData : DieModuleData
    {
        internal static DamageFilteredCreateObjectDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DamageFilteredCreateObjectDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<DamageFilteredCreateObjectDieModuleData>
            {
                { "DamageTypeTriggersInstantly", (parser, x) => x.DamageTypeTriggersInstantly = parser.ParseEnum<DamageType>() },
                { "DamageTypeTriggersForDuration", (parser, x) => x.DamageTypeTriggersForDuration = parser.ParseEnum<DamageType>() },
                { "PostFilterTriggeredDuration", (parser, x) => x.PostFilterTriggeredDuration = parser.ParseInteger() },
                { "CreationList", (parser, x) => x.CreationList = parser.ParseAssetReference() },
            });
        public DamageType DamageTypeTriggersInstantly { get; private set; }
        public DamageType DamageTypeTriggersForDuration { get; private set; }
        public int PostFilterTriggeredDuration { get; private set; }
        public string CreationList { get; private set; }
    }
}
