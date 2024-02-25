using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class OverlordContain : TransportContain
    {
        private readonly OverlordContainModuleData _moduleData;

        internal OverlordContain(GameObject gameObject, GameContext gameContext, OverlordContainModuleData moduleData)
            : base(gameObject, gameContext, moduleData)
        {
            _moduleData = moduleData;
        }

        private protected override void UpdateModuleSpecific(BehaviorUpdateContext context)
        {
            // todo: ExperienceSinkForRider
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.SkipUnknownBytes(1);
        }
    }

    /// <summary>
    /// Like Transport, but when full, passes transport queries along to first passenger
    /// (redirects like tunnel).
    /// </summary>
    public sealed class OverlordContainModuleData : TransportContainModuleData
    {
        internal static new OverlordContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<OverlordContainModuleData> FieldParseTable = TransportContainModuleData.FieldParseTable
            .Concat(new IniParseTable<OverlordContainModuleData>
            {
                { "PayloadTemplateName", (parser, x) => x.PayloadTemplateName = parser.ParseAssetReference() },
                { "ExperienceSinkForRider", (parser, x) => x.ExperienceSinkForRider = parser.ParseBoolean() }
            });

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string PayloadTemplateName { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ExperienceSinkForRider { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new OverlordContain(gameObject, context, this);
        }
    }
}
