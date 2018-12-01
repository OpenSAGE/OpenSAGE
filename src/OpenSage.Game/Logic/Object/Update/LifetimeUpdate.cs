using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class LifetimeUpdateModuleData : UpdateModuleData
    {
        internal static LifetimeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LifetimeUpdateModuleData> FieldParseTable = new IniParseTable<LifetimeUpdateModuleData>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseFloat() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseFloat() },
            { "WaitForWakeUp", (parser, x) => x.WaitForWakeUp = parser.ParseBoolean() },
            { "DeathType", (parser, x) => x.DeathType = parser.ParseEnum<DeathType>() }
        };

        public float MinLifetime { get; private set; }
        public float MaxLifetime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool WaitForWakeUp { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public DeathType DeathType { get; private set; }
    }
}
