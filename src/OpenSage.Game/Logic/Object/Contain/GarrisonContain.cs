using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class GarrisonContain : OpenContainModule
    {
        private uint _unknown1;
        private readonly Vector3[] _positions = new Vector3[120];
        private bool _unknown3;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistUInt32(ref _unknown1);

            reader.SkipUnknownBytes(1);

            ushort unknown2 = 40;
            reader.PersistUInt16(ref unknown2);
            if (unknown2 != 40)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(804);

            reader.PersistArray(
                _positions,
                static (StatePersister persister, ref Vector3 item) =>
                {
                    persister.PersistVector3Value(ref item);
                });

            reader.PersistBoolean(ref _unknown3);

            reader.SkipUnknownBytes(13);
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
