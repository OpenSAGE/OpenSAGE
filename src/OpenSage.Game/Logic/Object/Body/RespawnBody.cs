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
                { "CanRespawn", (parser, x) => x.CanRespawn = parser.ParseBoolean() }
            });

        public ObjectFilter PermanentlyKilledByFilter { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool CanRespawn { get; private set; }
     }
}
