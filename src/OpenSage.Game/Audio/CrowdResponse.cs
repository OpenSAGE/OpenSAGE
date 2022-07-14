using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Audio
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class CrowdResponse : BaseAsset
    {
        internal static CrowdResponse Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("CrowdResponse", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<CrowdResponse> FieldParseTable = new IniParseTable<CrowdResponse>
        {
            { "Threshold", (parser, x) => x.Thresholds.Add(Threshold.Parse(parser)) },
            { "Weight", (parser, x) => x.Weight = parser.ParseInteger() },
        };

        public int Weight { get; private set; }
        public List<Threshold> Thresholds { get; private set; } = new List<Threshold>();
    }
}
