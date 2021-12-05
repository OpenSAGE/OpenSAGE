using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class GarrisonContain : OpenContainModule
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknown2 = reader.ReadUInt32();

            var unknownBool = reader.ReadBoolean();
            if (unknownBool)
            {
                throw new InvalidStateException();
            }

            var unknown1 = reader.ReadUInt16();
            if (unknown1 != 40)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < 800; i++)
            {
                var unknown = reader.ReadByte();
                if (unknown != 0)
                {
                    throw new InvalidStateException();
                }
            }

            var unknown3 = reader.ReadUInt32();
            if (unknown3 != 0)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < 120; i++)
            {
                var unknown = reader.ReadVector3();
            }

            var unknown4 = reader.ReadBoolean();

            for (var i = 0; i < 13; i++)
            {
                var unknown = reader.ReadByte();
                if (unknown != 0)
                {
                    throw new InvalidStateException();
                }
            }
        }
    }

    /// <summary>
    /// Hardcoded to use the GarrisonGun object definition for the weapons pointing from the object 
    /// when occupants are firing and these are drawn at bones named FIREPOINT. Also, it Allows use 
    /// of the GARRISONED Model ModelConditionState.
    /// </summary>
    public class GarrisonContainModuleData : OpenContainModuleData
    {
        internal static GarrisonContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<GarrisonContainModuleData> FieldParseTable = OpenContainModuleData.FieldParseTable
            .Concat(new IniParseTable<GarrisonContainModuleData>
            {
                { "MobileGarrison", (parser, x) => x.MobileGarrison = parser.ParseBoolean() },
                { "InitialRoster", (parser, x) => x.InitialRoster = InitialRoster.Parse(parser) },
                { "ImmuneToClearBuildingAttacks", (parser, x) => x.ImmuneToClearBuildingAttacks = parser.ParseBoolean() },
                { "IsEnclosingContainer", (parser, x) => x.IsEnclosingContainer = parser.ParseBoolean() },
                { "ObjectStatusOfContained", (parser, x) => x.ObjectStatusOfContained = parser.ParseEnumBitArray<ObjectStatus>() },
                { "PassengerFilter", (parser, x) => x.PassengerFilter = ObjectFilter.Parse(parser) }
            });
        
        public bool MobileGarrison { get; private set; }
        public InitialRoster InitialRoster { get; private set; }
        public bool ImmuneToClearBuildingAttacks { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool IsEnclosingContainer { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public BitArray<ObjectStatus> ObjectStatusOfContained { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectFilter PassengerFilter { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new GarrisonContain();
        }
    }

    public sealed class InitialRoster
    {
        internal static InitialRoster Parse(IniParser parser)
        {
            return new InitialRoster
            {
                TemplateId = parser.ParseAssetReference(),
                Count = parser.ParseInteger()
            };
        }

        public string TemplateId { get; private set; }
        public int Count { get; private set; }
    }
}
