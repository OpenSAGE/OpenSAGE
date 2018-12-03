using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class DestroyEnvironmentUpdateModuleData : UpdateModuleData
    {
        internal static DestroyEnvironmentUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DestroyEnvironmentUpdateModuleData> FieldParseTable = new IniParseTable<DestroyEnvironmentUpdateModuleData>
            {
                { "StartTime", (parser, x) => x.StartTime = parser.ParseInteger() },
                { "DestructionTime", (parser, x) => x.DestructionTime = parser.ParseInteger() },
             
            };

        public int StartTime { get; private set; }
        public int DestructionTime { get; private set; }
    }
}
