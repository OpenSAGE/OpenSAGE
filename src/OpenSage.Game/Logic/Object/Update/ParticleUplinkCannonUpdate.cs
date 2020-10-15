using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class ParticleUplinkCannonUpdateModuleData : UpdateModuleData
    {
        internal static ParticleUplinkCannonUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ParticleUplinkCannonUpdateModuleData> FieldParseTable = new IniParseTable<ParticleUplinkCannonUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "BeginChargeTime", (parser, x) => x.BeginChargeTime = parser.ParseInteger() },
            { "RaiseAntennaTime", (parser, x) => x.RaiseAntennaTime = parser.ParseInteger() },
            { "ReadyDelayTime", (parser, x) => x.ReadyDelayTime = parser.ParseInteger() },

            { "TotalFiringTime", (parser, x) => x.TotalFiringTime = parser.ParseInteger() },

            { "DamagePerSecond", (parser, x) => x.DamagePerSecond = parser.ParseInteger() },
            { "TotalDamagePulses", (parser, x) => x.TotalDamagePulses = parser.ParseInteger() },
            { "WidthGrowTime", (parser, x) => x.WidthGrowTime = parser.ParseInteger() },
            { "BeamTravelTime", (parser, x) => x.BeamTravelTime = parser.ParseInteger() },
            { "DamageType", (parser, x) => x.DamageType = parser.ParseEnum<DamageType>() },
            { "DamageRadiusScalar", (parser, x) => x.DamageRadiusScalar = parser.ParseFloat() },
            { "RevealRange", (parser, x) => x.RevealRange = parser.ParseFloat() },

            { "OuterEffectBoneName", (parser, x) => x.OuterEffectBoneName = parser.ParseAssetReference() },
            { "OuterEffectNumBones", (parser, x) => x.OuterEffectNumBones = parser.ParseInteger() },
            { "ConnectorBoneName", (parser, x) => x.ConnectorBoneName = parser.ParseAssetReference() },
            { "FireBoneName", (parser, x) => x.FireBoneName = parser.ParseAssetReference() },

            { "OuterNodesLightFlareParticleSystem", (parser, x) => x.OuterNodesLightFlareParticleSystem = parser.ParseAssetReference() },
            { "OuterNodesMediumFlareParticleSystem", (parser, x) => x.OuterNodesMediumFlareParticleSystem = parser.ParseAssetReference() },
            { "OuterNodesIntenseFlareParticleSystem", (parser, x) => x.OuterNodesIntenseFlareParticleSystem = parser.ParseAssetReference() },

            { "ConnectorMediumLaserName", (parser, x) => x.ConnectorMediumLaserName = parser.ParseAssetReference() },
            { "ConnectorIntenseLaserName", (parser, x) => x.ConnectorIntenseLaserName = parser.ParseAssetReference() },

            { "LaserBaseLightFlareParticleSystemName", (parser, x) => x.LaserBaseLightFlareParticleSystemName = parser.ParseAssetReference() },
            { "ParticleBeamLaserName", (parser, x) => x.ParticleBeamLaserName = parser.ParseAssetReference() },
            { "GroundHitFX", (parser, x) => x.GroundHitFX = parser.ParseAssetReference() },

            { "BeamLaunchFX", (parser, x) => x.BeamLaunchFX = parser.ParseAssetReference() },
            { "DelayBetweenLaunchFX", (parser, x) => x.DelayBetweenLaunchFX = parser.ParseInteger() },

            { "TotalScorchMarks", (parser, x) => x.TotalScorchMarks = parser.ParseInteger() },
            { "ScorchMarkScalar", (parser, x) => x.ScorchMarkScalar = parser.ParseFloat() },

            { "SwathOfDeathDistance", (parser, x) => x.SwathOfDeathDistance = parser.ParseFloat() },
            { "SwathOfDeathAmplitude", (parser, x) => x.SwathOfDeathAmplitude = parser.ParseFloat() },

            { "ManualDrivingSpeed", (parser, x) => x.ManualDrivingSpeed = parser.ParseInteger() },
            { "ManualFastDrivingSpeed", (parser, x) => x.ManualFastDrivingSpeed = parser.ParseInteger() },
            { "DoubleClickToFastDriveDelay", (parser, x) => x.DoubleClickToFastDriveDelay = parser.ParseInteger() },

            { "PoweringUpSoundLoop", (parser, x) => x.PoweringUpSoundLoop = parser.ParseAssetReference() },
            { "UnpackToIdleSoundLoop", (parser, x) => x.UnpackToIdleSoundLoop = parser.ParseAssetReference() },
            { "FiringToPackSoundLoop", (parser, x) => x.FiringToPackSoundLoop = parser.ParseAssetReference() },
            { "GroundAnnihilationSoundLoop", (parser, x) => x.GroundAnnihilationSoundLoop = parser.ParseAssetReference() },

            { "DamagePulseRemnantObjectName", (parser, x) => x.DamagePulseRemnantObjectName = parser.ParseAssetReference() }
        };

        public string SpecialPowerTemplate { get; private set; }

        /// <summary>
        /// Duration of outer nodes beginning to charge.
        /// </summary>
        public int BeginChargeTime { get; private set; }

        /// <summary>
        /// Time to open the hatch and raise antenna.
        /// </summary>
        public int RaiseAntennaTime { get; private set; }

        /// <summary>
        /// Delay after antenna is raised, before being ready to fire.
        /// </summary>
        public int ReadyDelayTime { get; private set; }

        /// <summary>
        /// Total time that beam is in contact with the ground.
        /// </summary>
        public int TotalFiringTime { get; private set; }

        /// <summary>
        /// Amount of damage inflicted each second.
        /// </summary>
        public int DamagePerSecond { get; private set; }

        public int TotalDamagePulses { get; private set; }
        public int WidthGrowTime { get; private set; }
        public int BeamTravelTime { get; private set; }
        public DamageType DamageType { get; private set; }
        public float DamageRadiusScalar { get; private set; }
        public float RevealRange { get; private set; }

        public string OuterEffectBoneName { get; private set; }
        public int OuterEffectNumBones { get; private set; }
        public string ConnectorBoneName { get; private set; }
        public string FireBoneName { get; private set; }

        public string OuterNodesLightFlareParticleSystem { get; private set; }
        public string OuterNodesMediumFlareParticleSystem { get; private set; }
        public string OuterNodesIntenseFlareParticleSystem { get; private set; }

        public string ConnectorMediumLaserName { get; private set; }
        public string ConnectorIntenseLaserName { get; private set; }

        public string LaserBaseLightFlareParticleSystemName { get; private set; }
        public string ParticleBeamLaserName { get; private set; }
        public string GroundHitFX { get; private set; }

        public string BeamLaunchFX { get; private set; }
        public int DelayBetweenLaunchFX { get; private set; }

        public int TotalScorchMarks { get; private set; }
        public float ScorchMarkScalar { get; private set; }

        public float SwathOfDeathDistance { get; private set; }
        public float SwathOfDeathAmplitude { get; private set; }

        public int ManualDrivingSpeed { get; private set; }
        public int ManualFastDrivingSpeed { get; private set; }
        public int DoubleClickToFastDriveDelay { get; private set; }

        public string PoweringUpSoundLoop { get; private set; }
        public string UnpackToIdleSoundLoop { get; private set; }
        public string FiringToPackSoundLoop { get; private set; }
        public string GroundAnnihilationSoundLoop { get; private set; }

        public string DamagePulseRemnantObjectName { get; private set; }
    }
}
