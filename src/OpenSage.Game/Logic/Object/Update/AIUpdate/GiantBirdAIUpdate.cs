using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class GiantBirdAIUpdateModuleData : AIUpdateModuleData
    {
        internal static new GiantBirdAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<GiantBirdAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<GiantBirdAIUpdateModuleData>
            {
                { "FollowThroughDistance", (parser, x) => x.FollowThroughDistance = parser.ParseInteger() },
                { "FollowThroughCheckStep", (parser, x) => x.FollowThroughCheckStep = parser.ParseInteger() },
                { "FollowThroughGradient", (parser, x) => x.FollowThroughGradient = parser.ParseFloat() },
                { "GrabTossTimeTrigger", (parser, x) => x.GrabTossTimeTrigger = parser.ParseFloat() },
                { "GrabTossHeightTrigger", (parser, x) => x.GrabTossHeightTrigger = parser.ParseFloat() },
                { "TossFX", (parser, x) => x.TossFX = parser.ParseAssetReference() },
            });

        public int FollowThroughDistance { get; private set; }
        public int FollowThroughCheckStep { get; private set; }
        public float FollowThroughGradient { get; private set; }
        public float GrabTossTimeTrigger { get; private set; }
        public float GrabTossHeightTrigger { get; private set; }
        public string TossFX { get; private set; }
    }
}
