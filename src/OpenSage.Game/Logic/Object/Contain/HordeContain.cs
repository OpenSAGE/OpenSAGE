using System.Collections.Generic;
using System.Numerics;
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
            { "RandomOffset", (parser, x ) => x.RandomOffset = parser.ParsePoint() },

            { "BannerCarriersAllowed", (parser, x) => x.BannerCarriersAllowed.AddRange(parser.ParseAssetReferenceArray()) },
            { "BannerCarrierPosition", (parser, x) => x.BannerCarrierPositions.Add(BannerCarrierPosition.Parse(parser)) },

            { "RankInfo", (parser, x) => x.RankInfos.Add(RankInfo.Parse(parser)) },

            { "RanksToReleaseWhenAttacking", (parser, x) => x.RanksToReleaseWhenAttacking = parser.ParseIntegerArray() },

            { "ComboHordes", (parser, x) => x.ComboHordes.Add(ComboHorde.Parse(parser)) },

            { "UseSlowHordeMovement", (parser, x) => x.UseSlowHordeMovement = parser.ParseBoolean() },

            { "MeleeAttackLeashDistance", (parser, x) =>  x.MeleeAttackLeashDistance = parser.ParseInteger() },
            { "MachineAllowed", (parser, x) => x.MachineAllowed = parser.ParseBoolean() },
            { "MachineType", (parser, x) => x.MachineType = parser.ParseAssetReference() },

            { "AlternateFormation", (parser, x) => x.AlternateFormation = parser.ParseAssetReference() }
        };

        public BitArray<ObjectStatus> ObjectStatusOfContained { get; private set; }
        public List<Payload> InitialPayloads { get; private set; } = new List<Payload>();
        public int Slots { get; private set; }
        public ObjectFilter PassengerFilter { get; private set; }
        public bool ShowPips { get; private set; }
        public bool ThisFormationIsTheMainFormation { get; private set; }
        public Point2D RandomOffset { get; private set; }

        // NOTE: Despite the name, in BFME1 this always contains just 1 entry.
        public List<string> BannerCarriersAllowed { get; private set; } = new List<string>();
        public List<BannerCarrierPosition> BannerCarrierPositions { get; private set; } = new List<BannerCarrierPosition>();

        public List<RankInfo> RankInfos { get; private set; } = new List<RankInfo>();

        public int[] RanksToReleaseWhenAttacking { get; private set; }

        public List<ComboHorde> ComboHordes { get; private set; } = new List<ComboHorde>();

        public bool UseSlowHordeMovement { get; private set; }

        public int MeleeAttackLeashDistance { get; private set; }
        public bool MachineAllowed { get; private set; }
        public string MachineType { get; private set; }

        public string AlternateFormation { get; private set; }
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
                Position = parser.ParseAttributeVector2("Pos")
            };
        }

        public string UnitType { get; private set; }
        public Vector2 Position { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class RankInfo
    {
        internal static RankInfo Parse(IniParser parser)
        {
            var rankNumber = parser.ParseAttributeInteger("RankNumber");
            var unitType = parser.ParseAttributeIdentifier("UnitType");
            var positions = new List<Point2D>();

            while (parser.ParseAttributeOptional("Position", parser.ParsePoint, out var position))
            {
                positions.Add(position);
            }

            return new RankInfo
            {
                RankNumber = rankNumber,
                UnitType = unitType,
                Positions = positions
            };
        }

        public int RankNumber { get; private set; }
        public string UnitType { get; private set; }
        public List<Point2D> Positions { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class ComboHorde
    {
        internal static ComboHorde Parse(IniParser parser)
        {
            return new ComboHorde
            {
                Target = parser.ParseAttributeIdentifier("Target"),
                Result = parser.ParseAttributeIdentifier("Result"),
                InitiateVoice = parser.ParseAttributeIdentifier("InitiateVoice")
            };
        }

        public string Target { get; private set; }
        public string Result { get; private set; }
        public string InitiateVoice { get; private set; }

    }
}
