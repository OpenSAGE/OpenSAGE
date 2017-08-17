using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class OCLSpecialPowerBehavior : ObjectBehavior
    {
        internal static OCLSpecialPowerBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<OCLSpecialPowerBehavior> FieldParseTable = new IniParseTable<OCLSpecialPowerBehavior>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "OCL", (parser, x) => x.OCL = parser.ParseAssetReference() },
            { "UpgradeOCL", (parser, x) => x.UpgradeOCLs.Add(UpgradeOCL.Parse(parser)) },
            { "CreateLocation", (parser, x) => x.CreateLocation = parser.ParseEnum<OCLCreationPoint>() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public string OCL { get; private set; }
        public List<UpgradeOCL> UpgradeOCLs { get; } = new List<UpgradeOCL>();
        public OCLCreationPoint CreateLocation { get; private set; }
    }

    public sealed class UpgradeOCL
    {
        internal static UpgradeOCL Parse(IniParser parser)
        {
            return new UpgradeOCL
            {
                Science = parser.ParseAssetReference(),
                OCL = parser.ParseAssetReference()
            };
        }

        public string Science { get; private set; }
        public string OCL { get; private set; }
    }

    public enum OCLCreationPoint
    {
        [IniEnum("USE_OWNER_OBJECT")]
        UseOwnerObject,

        [IniEnum("CREATE_AT_EDGE_NEAR_SOURCE")]
        CreateAtEdgeNearSource,

        [IniEnum("CREATE_AT_EDGE_FARTHEST_FROM_TARGET")]
        CreateAtEdgeFarthestFromTarget,

        [IniEnum("CREATE_ABOVE_LOCATION")]
        CreateAboveLocation,

        [IniEnum("CREATE_AT_LOCATION")]
        CreateAtLocation,
    }
}
