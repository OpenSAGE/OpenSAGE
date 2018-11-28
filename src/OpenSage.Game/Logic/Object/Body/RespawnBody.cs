using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class RespawnBodyModuleData : ActiveBodyModuleData
    {
        internal static new RespawnBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RespawnBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<RespawnBodyModuleData>
            {
                { "PermanentlyKilledByFilter", (parser, x) => x.PermanentlyKilledByFilter = ObjectFilter.Parse(parser) },
                { "HealingBuffFx", (parser, x) => x.HealingBuffFx = parser.ParseAssetReference() },
            });

        public ObjectFilter PermanentlyKilledByFilter { get; private set; }
        public string HealingBuffFx { get; private set; }
     }
}
