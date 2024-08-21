#nullable enable

using System.Numerics;
using OpenSage.Audio;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class GarrisonContain : OpenContainModule
    {
        private uint _originalTeamId;
        private readonly Vector3[] _positions = new Vector3[120];
        private bool _originalTeamSet;

        internal GarrisonContain(GameObject gameObject, GameContext gameContext, OpenContainModuleData moduleData) : base(gameObject, gameContext, moduleData)
        {
        }

        private protected override void UpdateModuleSpecific(BehaviorUpdateContext context)
        {
            var isGarrisoned = ContainedObjectIds.Count > 0;

            if (!_originalTeamSet && isGarrisoned)
            {
                // store the team this object should return to when no occupying force is present
                var originalTeam = GameObjectForId(ContainedObjectIds[0]).Owner.DefaultTeam?.Id;
                if (originalTeam.HasValue)
                {
                    _originalTeamId = originalTeam.Value;
                }
                else
                {
                    // todo: DefaultTeam is not currently set on player object - this if statement can be removed once we're actually setting this
                    _originalTeamId = uint.MaxValue;
                }
                _originalTeamSet = true;
            }

            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Garrisoned, isGarrisoned);
            if (_originalTeamSet)
            {
                if (isGarrisoned)
                {
                    GameObject.Owner = GameObjectForId(ContainedObjectIds[0]).Owner;
                }
                else
                {
                    var owner = GameContext.Game.TeamFactory.FindTeamById(_originalTeamId)?.Template.Owner;
                    owner ??= GameContext.Game.PlayerManager.GetCivilianPlayer(); // todo: this behavior can be removed when DefaultTeam is set properly

                    GameObject.Owner = owner;
                }
            }
        }

        protected override bool HealthTooLowToHoldUnits()
        {
            return GameObject.IsKindOf(ObjectKinds.GarrisonableUntilDestroyed)
                ? base.HealthTooLowToHoldUnits()
                : GameObject.ModelConditionFlags.Get(ModelConditionFlag.ReallyDamaged);
        }

        protected override BaseAudioEventInfo? GetEnterVoiceLine(UnitSpecificSounds sounds)
        {
            return sounds.VoiceGarrison?.Value;
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistUInt32(ref _originalTeamId);

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

            reader.PersistBoolean(ref _originalTeamSet);

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

        /// <summary>
        /// AllowInsideKindOf is never explicitly set for GarrisonContain, but it seems to only be for infantry.
        /// </summary>
        public override BitArray<ObjectKinds> AllowInsideKindOf { get; protected set; } = new(ObjectKinds.Infantry);

        public bool MobileGarrison { get; private set; }
        public InitialRoster? InitialRoster { get; private set; }
        public bool ImmuneToClearBuildingAttacks { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool IsEnclosingContainer { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public BitArray<ObjectStatus> ObjectStatusOfContained { get; private set; } = new();

        [AddedIn(SageGame.Bfme)]
        public ObjectFilter PassengerFilter { get; private set; } = new();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new GarrisonContain(gameObject, context, this);
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

        public required string TemplateId { get; init; }
        public int Count { get; private set; }
    }
}
