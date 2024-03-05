using System.Linq;
using System.Numerics;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class ParticleUplinkCannonUpdate : UpdateModule
    {
        private ParticleCannonStatus _status;

        private DownlinkBeamStatus _downlinkBeamStatus;

        private readonly uint[] _flareFxSystemIds = new uint[16];

        private readonly uint[] _connectorLaserDrawableIds = new uint[16];

        private uint _upwardOrbitalLaserDrawableId; // the laser which leaves the particle cannon and fires into the sky
        private uint _downwardOrbitalLaserDrawableId; // the laser which descends from the sky and the player controls to do damage
        private uint _readyToFireFxSystemId;

        private readonly Vector3[] _flareFxLocations = new Vector3[16];
        private readonly Matrix4x3[] _flareFxTransforms = Enumerable.Repeat(Matrix4x3.Identity, 16).ToArray();

        private Vector3 _unknownPosition1; // this seems to be just like the ready to fire fx location, but lower
        private Vector3 _readyToFireFxLocationMaybe; // this seems to be the location of the ready to fire fx, but is present even when the effect is not
        private Vector3 _targetPosition; // the position the particle cannon should move towards

        private bool _unknownBool1;
        private bool _unknownBool2;

        private Vector3 _initialLaunchPosition;
        private Vector3 _currentPosition;

        private uint _scorchMarkCounter;

        private LogicFrame _unknownFrame1;
        private LogicFrame _unknownFrame2;

        private uint _damagePulseCounter;

        private LogicFrame _unknownFrame3;
        private LogicFrame _firingStartFrame; // the frame where the user selected a target for the particle cannon
        private LogicFrame _firingUpStartShrinkingFrame; // _firingStartFrame + _moduleData.TotalFiringTime: the time when the upward beam should start shrinking

        private LogicFrame _mostRecentClickFrame; // the frame of the most recent click
        private LogicFrame _secondMostRecentClickFrame; // the frame of the second-most recent click (most recent click moves here with each click)

        private readonly GameObject _gameObject;
        private readonly GameContext _context;
        private readonly ParticleUplinkCannonUpdateModuleData _moduleData;

        internal ParticleUplinkCannonUpdate(GameObject gameObject, GameContext context, ParticleUplinkCannonUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _context = context;
            _moduleData = moduleData;
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistEnum(ref _status);

            reader.PersistEnum(ref _downlinkBeamStatus);
            reader.SkipUnknownBytes(4);

            reader.PersistArray(_flareFxSystemIds, UIntArrayPersister);
            reader.PersistArray(_connectorLaserDrawableIds, UIntArrayPersister);

            reader.PersistUInt32(ref _upwardOrbitalLaserDrawableId);
            reader.PersistUInt32(ref _downwardOrbitalLaserDrawableId);
            reader.SkipUnknownBytes(4);
            reader.PersistUInt32(ref _readyToFireFxSystemId);

            reader.PersistArray(_flareFxLocations, Vector3ArrayPersister);
            reader.PersistArray(_flareFxTransforms, Matrix4x3ArrayPersister);

            reader.PersistVector3(ref _unknownPosition1);
            reader.PersistVector3(ref _readyToFireFxLocationMaybe);
            reader.PersistVector3(ref _targetPosition);

            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);

            reader.SkipUnknownBytes(1);

            reader.PersistVector3(ref _initialLaunchPosition);
            reader.PersistVector3(ref _currentPosition);

            reader.PersistUInt32(ref _scorchMarkCounter);

            reader.PersistLogicFrame(ref _unknownFrame1); // present after beam came down - potentially next scorch mark frame?
            reader.PersistLogicFrame(ref _unknownFrame2); // related to DelayBetweenLaunchFX? increased by 30 while firing

            reader.PersistUInt32(ref _damagePulseCounter);
            reader.PersistLogicFrame(ref _unknownFrame3); // present after beam came down - potentially next damage pulse frame?

            reader.PersistLogicFrame(ref _firingStartFrame);
            reader.PersistLogicFrame(ref _firingUpStartShrinkingFrame);

            reader.PersistLogicFrame(ref _mostRecentClickFrame);
            reader.PersistLogicFrame(ref _secondMostRecentClickFrame);
        }

        private static void UIntArrayPersister(StatePersister persister, ref uint item) => persister.PersistUInt32Value(ref item);
        private static void Vector3ArrayPersister(StatePersister persister, ref Vector3 item) => persister.PersistVector3Value(ref item);
        private static void Matrix4x3ArrayPersister(StatePersister persister, ref Matrix4x3 item) => persister.PersistMatrix4x3Value(ref item, readVersion: false);

        private enum DownlinkBeamStatus
        {
            None,
            GrowingOrPresent, // the same status seems to be used whether the beam is growing or full-size
            Shrinking,
            Disappeared,
        }

        private enum ParticleCannonStatus
        {
            Reloading,
            // todo: the naming on these may be wrong
            Charging, // UNPACKING
            RaisingAntenna, // UNPACKING switch to outernodemediumflare from here on out (why do the system ids keep changing after?)
            ReadyDelay, // DEPLOYED
            Ready, // DEPLOYED
            Unknown1, // what triggers this?
            Firing, // DEPLOYED intense laser
            Waning, // DEPLOYED uplink beam starting to shrink
            Packing, // PACKING - OG generals remains in packing forever after firing?
        }
    }

    public sealed class ParticleUplinkCannonUpdateModuleData : UpdateModuleData
    {
        internal static ParticleUplinkCannonUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ParticleUplinkCannonUpdateModuleData> FieldParseTable = new IniParseTable<ParticleUplinkCannonUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseSpecialPowerReference() },
            { "BeginChargeTime", (parser, x) => x.BeginChargeTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "RaiseAntennaTime", (parser, x) => x.RaiseAntennaTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "ReadyDelayTime", (parser, x) => x.ReadyDelayTime = parser.ParseTimeMillisecondsToLogicFrames() },

            { "TotalFiringTime", (parser, x) => x.TotalFiringTime = parser.ParseTimeMillisecondsToLogicFrames() },

            { "DamagePerSecond", (parser, x) => x.DamagePerSecond = parser.ParseInteger() },
            { "TotalDamagePulses", (parser, x) => x.TotalDamagePulses = parser.ParseInteger() },
            { "WidthGrowTime", (parser, x) => x.WidthGrowTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "BeamTravelTime", (parser, x) => x.BeamTravelTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "DamageType", (parser, x) => x.DamageType = parser.ParseEnum<DamageType>() },
            { "DamageRadiusScalar", (parser, x) => x.DamageRadiusScalar = parser.ParseFloat() },
            { "RevealRange", (parser, x) => x.RevealRange = parser.ParseFloat() },

            { "OuterEffectBoneName", (parser, x) => x.OuterEffectBoneName = parser.ParseAssetReference() },
            { "OuterEffectNumBones", (parser, x) => x.OuterEffectNumBones = parser.ParseInteger() },
            { "ConnectorBoneName", (parser, x) => x.ConnectorBoneName = parser.ParseAssetReference() },
            { "FireBoneName", (parser, x) => x.FireBoneName = parser.ParseAssetReference() },

            { "OuterNodesLightFlareParticleSystem", (parser, x) => x.OuterNodesLightFlareParticleSystem = parser.ParseFXParticleSystemTemplateReference() },
            { "OuterNodesMediumFlareParticleSystem", (parser, x) => x.OuterNodesMediumFlareParticleSystem = parser.ParseFXParticleSystemTemplateReference() },
            { "OuterNodesIntenseFlareParticleSystem", (parser, x) => x.OuterNodesIntenseFlareParticleSystem = parser.ParseFXParticleSystemTemplateReference() },

            { "ConnectorMediumLaserName", (parser, x) => x.ConnectorMediumLaserName = parser.ParseObjectReference() },
            { "ConnectorIntenseLaserName", (parser, x) => x.ConnectorIntenseLaserName = parser.ParseObjectReference() },

            { "LaserBaseLightFlareParticleSystemName", (parser, x) => x.LaserBaseLightFlareParticleSystemName = parser.ParseFXParticleSystemTemplateReference() },
            { "ParticleBeamLaserName", (parser, x) => x.ParticleBeamLaserName = parser.ParseObjectReference() },
            { "GroundHitFX", (parser, x) => x.GroundHitFX = parser.ParseFXListReference() },

            { "BeamLaunchFX", (parser, x) => x.BeamLaunchFX = parser.ParseFXListReference() },
            { "DelayBetweenLaunchFX", (parser, x) => x.DelayBetweenLaunchFX = parser.ParseTimeMillisecondsToLogicFrames() },

            { "TotalScorchMarks", (parser, x) => x.TotalScorchMarks = parser.ParseInteger() },
            { "ScorchMarkScalar", (parser, x) => x.ScorchMarkScalar = parser.ParseFloat() },

            { "SwathOfDeathDistance", (parser, x) => x.SwathOfDeathDistance = parser.ParseFloat() },
            { "SwathOfDeathAmplitude", (parser, x) => x.SwathOfDeathAmplitude = parser.ParseFloat() },

            { "ManualDrivingSpeed", (parser, x) => x.ManualDrivingSpeed = parser.ParseInteger() },
            { "ManualFastDrivingSpeed", (parser, x) => x.ManualFastDrivingSpeed = parser.ParseInteger() },
            { "DoubleClickToFastDriveDelay", (parser, x) => x.DoubleClickToFastDriveDelay = parser.ParseTimeMillisecondsToLogicFrames() },

            { "PoweringUpSoundLoop", (parser, x) => x.PoweringUpSoundLoop = parser.ParseAudioEventReference() },
            { "UnpackToIdleSoundLoop", (parser, x) => x.UnpackToIdleSoundLoop = parser.ParseAudioEventReference() },
            { "FiringToPackSoundLoop", (parser, x) => x.FiringToPackSoundLoop = parser.ParseAudioEventReference() },
            { "GroundAnnihilationSoundLoop", (parser, x) => x.GroundAnnihilationSoundLoop = parser.ParseAudioEventReference() },

            { "DamagePulseRemnantObjectName", (parser, x) => x.DamagePulseRemnantObjectName = parser.ParseObjectReference() }
        };

        public LazyAssetReference<SpecialPower> SpecialPowerTemplate { get; private set; }

        /// <summary>
        /// Duration of outer nodes beginning to charge.
        /// </summary>
        public LogicFrameSpan BeginChargeTime { get; private set; }

        /// <summary>
        /// Time to open the hatch and raise antenna.
        /// </summary>
        public LogicFrameSpan RaiseAntennaTime { get; private set; }

        /// <summary>
        /// Delay after antenna is raised, before being ready to fire.
        /// </summary>
        public LogicFrameSpan ReadyDelayTime { get; private set; }

        /// <summary>
        /// Total time that beam is in contact with the ground.
        /// </summary>
        public LogicFrameSpan TotalFiringTime { get; private set; }

        /// <summary>
        /// Amount of damage inflicted each second.
        /// </summary>
        public int DamagePerSecond { get; private set; }

        public int TotalDamagePulses { get; private set; }
        public LogicFrameSpan WidthGrowTime { get; private set; }
        public LogicFrameSpan BeamTravelTime { get; private set; }
        public DamageType DamageType { get; private set; }
        public float DamageRadiusScalar { get; private set; }
        public float RevealRange { get; private set; }

        public string OuterEffectBoneName { get; private set; }
        public int OuterEffectNumBones { get; private set; }
        public string ConnectorBoneName { get; private set; }
        public string FireBoneName { get; private set; }

        public LazyAssetReference<FXParticleSystemTemplate> OuterNodesLightFlareParticleSystem { get; private set; }
        public LazyAssetReference<FXParticleSystemTemplate> OuterNodesMediumFlareParticleSystem { get; private set; }
        public LazyAssetReference<FXParticleSystemTemplate> OuterNodesIntenseFlareParticleSystem { get; private set; }

        public LazyAssetReference<ObjectDefinition> ConnectorMediumLaserName { get; private set; }
        public LazyAssetReference<ObjectDefinition> ConnectorIntenseLaserName { get; private set; }

        public LazyAssetReference<FXParticleSystemTemplate> LaserBaseLightFlareParticleSystemName { get; private set; }
        public LazyAssetReference<ObjectDefinition> ParticleBeamLaserName { get; private set; }
        public LazyAssetReference<FXList> GroundHitFX { get; private set; }

        public LazyAssetReference<FXList> BeamLaunchFX { get; private set; }
        public LogicFrameSpan DelayBetweenLaunchFX { get; private set; }

        public int TotalScorchMarks { get; private set; }
        public float ScorchMarkScalar { get; private set; }

        public float SwathOfDeathDistance { get; private set; }
        public float SwathOfDeathAmplitude { get; private set; }

        public int ManualDrivingSpeed { get; private set; }
        public int ManualFastDrivingSpeed { get; private set; }
        public LogicFrameSpan DoubleClickToFastDriveDelay { get; private set; }

        public LazyAssetReference<BaseAudioEventInfo> PoweringUpSoundLoop { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> UnpackToIdleSoundLoop { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> FiringToPackSoundLoop { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> GroundAnnihilationSoundLoop { get; private set; }

        public LazyAssetReference<ObjectDefinition> DamagePulseRemnantObjectName { get; private set; }

        internal override ParticleUplinkCannonUpdate CreateModule(GameObject gameObject, GameContext context)
        {
            return new ParticleUplinkCannonUpdate(gameObject, context, this);
        }
    }
}
