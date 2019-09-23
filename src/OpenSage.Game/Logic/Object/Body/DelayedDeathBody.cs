using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class DelayedDeathBodyModuleData : ActiveBodyModuleData
    {
        internal static new DelayedDeathBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DelayedDeathBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<DelayedDeathBodyModuleData>
            {
                { "DelayedDeathTime", (parser, x) => x.DelayedDeathTime = parser.ParseInteger() },
                { "CanRespawn", (parser, x) => x.CanRespawn = parser.ParseBoolean() },
                { "DoHealthCheck", (parser, x) => x.DoHealthCheck = parser.ParseBoolean() },
                { "ImmortalUntilDeathTime", (parser, x) => x.ImmortalUntilDeathTime = parser.ParseBoolean() },
                { "DelayedDeathPrerequisiteUpgrade", (parser, x) => x.DelayedDeathPrerequisiteUpgrade = parser.ParseAssetReference() },
                { "InvulnerableFX", (parser, x) => x.InvulnerableFX = parser.ParseAssetReference() },
                { "PermanentlyKilledByFilter", (parser, x) => x.PermanentlyKilledByFilter = ObjectFilter.Parse(parser) }
            });

        public int DelayedDeathTime { get; private set; }
        public bool CanRespawn { get; private set; }
        public bool DoHealthCheck { get; private set; }
        public bool ImmortalUntilDeathTime { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string DelayedDeathPrerequisiteUpgrade { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string InvulnerableFX { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public ObjectFilter PermanentlyKilledByFilter { get; private set; }
    }
}
