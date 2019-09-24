using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Ini;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;

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

        internal static CrowdResponse ParseAsset(BinaryReader reader, Asset asset, AssetImportCollection imports)
        {
            var result = new CrowdResponse
            {
                Weight = reader.ReadInt32(),
                Thresholds = reader.ReadListAtOffset(() => Threshold.ParseAsset(reader, imports))
            };
            result.SetNameAndInstanceId(asset);
            return result;
        }

        public int Weight { get; private set; }
        public List<Threshold> Thresholds { get; private set; } = new List<Threshold>();
    }
}
