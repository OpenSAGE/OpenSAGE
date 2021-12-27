using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class OverlordContain : TransportContain
    {
        internal OverlordContain(OverlordContainModuleData moduleData)
            : base(moduleData)
        {
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

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
            return new OverlordContain(this);
        }
    }
}
