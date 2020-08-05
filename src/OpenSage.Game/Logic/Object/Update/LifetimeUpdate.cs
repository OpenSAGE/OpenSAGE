using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class LifetimeUpdate : UpdateModule
    {
        // TODO
    }

    public sealed class LifetimeUpdateModuleData : UpdateModuleData
    {
        internal static LifetimeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LifetimeUpdateModuleData> FieldParseTable = new IniParseTable<LifetimeUpdateModuleData>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseTimeMilliseconds() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseTimeMilliseconds() },
            { "WaitForWakeUp", (parser, x) => x.WaitForWakeUp = parser.ParseBoolean() },
            { "DeathType", (parser, x) => x.DeathType = parser.ParseEnum<DeathType>() }
        };

        public TimeSpan MinLifetime { get; private set; }
        public TimeSpan MaxLifetime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool WaitForWakeUp { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public DeathType DeathType { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new LifetimeUpdate();
        }
    }
}
