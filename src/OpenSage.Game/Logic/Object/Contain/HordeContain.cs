using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class HordeContainModuleData : BehaviorModuleData
    {
        internal static HordeContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<HordeContainModuleData> FieldParseTable = new IniParseTable<HordeContainModuleData>
        {
            { "ObjectStatusOfContained", (parser, x) => x.ObjectStatusOfContained = parser.ParseEnumBitArray<ObjectStatus>() },
            { "InitialPayload", (parser, x) => x.InitialPayloads.Add(Payload.Parse(parser)) },
            { "Slots", (parser, x) => x.Slots = parser.ParseInteger() },
            { "PassengerFilter", (parser, x) => x.PassengerFilter = ObjectFilter.Parse(parser) },
            { "ShowPips", (parser, x) => x.ShowPips = parser.ParseBoolean() }, 
            { "ThisFormationIsTheMainFormation", (parser, x) => x.ThisFormationIsTheMainFormation = parser.ParseBoolean() },

            { "BannerCarriersAllowed", (parser, x) =>  x.BannerCarriersAllowed.AddRange(parser.ParseAssetReferenceArray())}
        };

        public BitArray<ObjectStatus> ObjectStatusOfContained { get; private set; }
        public List<Payload> InitialPayloads { get; private set; } = new List<Payload>();
        public int Slots { get; private set; }
        public ObjectFilter PassengerFilter { get; private set; }
        public bool ShowPips { get; private set; }
        public bool ThisFormationIsTheMainFormation { get; private set; }

        // NOTE: Despite the name, in BFME1 this always contains just 1 entry.
        public List<string> BannerCarriersAllowed { get; private set; } = new List<string>();
        public List<BannerCarrierPosition> BannerCarrierPositions { get; private set; } = new List<BannerCarrierPosition>();
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class Payload
    {
        internal static Payload Parse(IniParser parser)
        {
            return new Payload
            {
                UnitType = parser.ParseAssetReference(),
                Count = parser.ParseInteger()
            };
        }

        public string UnitType { get; private set; }
        public int Count { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class BannerCarrierPosition
    {
        internal static BannerCarrierPosition Parse(IniParser parser)
        {
            return new BannerCarrierPosition
            {
                UnitType = parser.ParseAttributeIdentifier("UnitType"),
                Position = parser.ParseAttributePoint2Df("Pos")
            };
        }

        public string UnitType { get; private set; }
        public Point2Df Position { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class RankInfo
    {
        public int RankNumber { get; private set; }
        public string UnitType { get; private set; }
        public Point2D Position { get; private set; }
    }
}
