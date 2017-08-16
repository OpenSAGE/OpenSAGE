using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class ObjectCreationList
    {
        internal static ObjectCreationList Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<ObjectCreationList> FieldParseTable = new IniParseTable<ObjectCreationList>
        {
            { "ApplyRandomForce", (parser, x) => x.Items.Add(ApplyRandomForceObjectCreationListItem.Parse(parser)) },
            { "Attack", (parser, x) => x.Items.Add(AttackObjectCreationListItem.Parse(parser)) },
            { "CreateDebris", (parser, x) => x.Items.Add(CreateDebrisObjectCreationListItem.Parse(parser)) },
            { "CreateObject", (parser, x) => x.Items.Add(CreateObjectObjectCreationListItem.Parse(parser)) },
            { "DeliverPayload", (parser, x) => x.Items.Add(DeliverPayloadObjectCreationListItem.Parse(parser)) },
            { "FireWeapon", (parser, x) => x.Items.Add(FireWeaponObjectCreationListItem.Parse(parser)) }
        };

        public string Name { get; private set; }

        public List<ObjectCreationListItem> Items { get; } = new List<ObjectCreationListItem>();
    }

    public abstract class ObjectCreationListItem
    {

    }

    public sealed class CreateDebrisObjectCreationListItem : ObjectCreationListItem
    {
        internal static CreateDebrisObjectCreationListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<CreateDebrisObjectCreationListItem> FieldParseTable = new IniParseTable<CreateDebrisObjectCreationListItem>
        {
            { "ModelNames", (parser, x) => x.ModelNames = parser.ParseAssetReferenceArray() },
            { "AnimationSet", (parser, x) => x.AnimationSets.Add(parser.ParseAssetReferenceArray()) },
            { "FXFinal", (parser, x) => x.FXFinal = parser.ParseAssetReference() },
            { "Offset", (parser, x) => x.Offset = Coord3D.Parse(parser) },
            { "Mass", (parser, x) => x.Mass = parser.ParseFloat() },
            { "Count", (parser, x) => x.Count = parser.ParseInteger() },
            { "Disposition", (parser, x) => x.Disposition = parser.ParseEnumBitArray<ObjectDisposition>() },
            { "DispositionIntensity", (parser, x) => x.DispositionIntensity = parser.ParseFloat() },
            { "MinForceMagnitude", (parser, x) => x.MinForceMagnitude = parser.ParseInteger() },
            { "MaxForceMagnitude", (parser, x) => x.MaxForceMagnitude = parser.ParseInteger() },
            { "MinForcePitch", (parser, x) => x.MinForcePitch = parser.ParseFloat() },
            { "MaxForcePitch", (parser, x) => x.MaxForcePitch = parser.ParseFloat() },
            { "SpinRate", (parser, x) => x.SpinRate = parser.ParseFloat() },
            { "ParticleSystem", (parser, x) => x.ParticleSystem = parser.ParseAssetReference() },
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseInteger() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseInteger() },
            { "BounceSound", (parser, x) => x.BounceSound = parser.ParseAssetReference() },
            { "OkToChangeModelColor", (parser, x) => x.OkToChangeModelColor = parser.ParseBoolean() },
            { "IgnorePrimaryObstacle", (parser, x) => x.IgnorePrimaryObstacle = parser.ParseBoolean() },
            { "OrientInForceDirection", (parser, x) => x.OrientInForceDirection = parser.ParseBoolean() },
            { "ExtraBounciness", (parser, x) => x.ExtraBounciness = parser.ParseFloat() },
            { "ExtraFriction", (parser, x) => x.ExtraFriction = parser.ParseFloat() }
        };

        public string[] ModelNames { get; private set; }
        public List<string[]> AnimationSets { get; } = new List<string[]>();
        public string FXFinal { get; private set; }
        public Coord3D Offset { get; private set; }
        public float Mass { get; private set; }
        public int Count { get; private set; }
        public BitArray<ObjectDisposition> Disposition { get; private set; }
        public float DispositionIntensity { get; private set; }
        public int MinForceMagnitude { get; private set; }
        public int MaxForceMagnitude { get; private set; }
        public float MinForcePitch { get; private set; }
        public float MaxForcePitch { get; private set; }
        public float SpinRate { get; private set; }
        public string ParticleSystem { get; private set; }
        public int MinLifetime { get; private set; }
        public int MaxLifetime { get; private set; }
        public string BounceSound { get; private set; }
        public bool OkToChangeModelColor { get; private set; }
        public bool IgnorePrimaryObstacle { get; private set; }
        public bool OrientInForceDirection { get; private set; }
        public float ExtraBounciness { get; private set; }
        public float ExtraFriction { get; private set; }
    }

    public sealed class CreateObjectObjectCreationListItem : ObjectCreationListItem
    {
        internal static CreateObjectObjectCreationListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<CreateObjectObjectCreationListItem> FieldParseTable = new IniParseTable<CreateObjectObjectCreationListItem>
        {
            { "ObjectNames", (parser, x) => x.ObjectNames = parser.ParseAssetReference() },
            { "Offset", (parser, x) => x.Offset = Coord3D.Parse(parser) },
            { "Count", (parser, x) => x.Count = parser.ParseInteger() },
            { "SpreadFormation", (parser, x) => x.SpreadFormation = parser.ParseBoolean() },
            { "MinDistanceAFormation", (parser, x) => x.MinDistanceAFormation = parser.ParseFloat() },
            { "MinDistanceBFormation", (parser, x) => x.MinDistanceBFormation = parser.ParseFloat() },
            { "MaxDistanceFormation", (parser, x) => x.MaxDistanceFormation = parser.ParseFloat() },
            { "FadeIn", (parser, x) => x.FadeIn = parser.ParseBoolean() },
            { "FadeTime", (parser, x) => x.FadeTime = parser.ParseInteger() },
            { "FadeSound", (parser, x) => x.FadeSound = parser.ParseAssetReference() },
            { "PutInContainer", (parser, x) => x.PutInContainer = parser.ParseAssetReference() },
            { "IgnorePrimaryObstacle", (parser, x) => x.IgnorePrimaryObstacle = parser.ParseBoolean() },
            { "InheritsVeterancy", (parser, x) => x.InheritsVeterancy = parser.ParseBoolean() },
            { "Disposition", (parser, x) => x.Disposition = parser.ParseEnumBitArray<ObjectDisposition>() },
            { "DispositionIntensity", (parser, x) => x.DispositionIntensity = parser.ParseFloat() },
            { "PreserveLayer", (parser, x) => x.PreserveLayer = parser.ParseBoolean() },
            { "MinForceMagnitude", (parser, x) => x.MinForceMagnitude = parser.ParseInteger() },
            { "MaxForceMagnitude", (parser, x) => x.MaxForceMagnitude = parser.ParseInteger() },
            { "MinForcePitch", (parser, x) => x.MinForcePitch = parser.ParseFloat() },
            { "MaxForcePitch", (parser, x) => x.MaxForcePitch = parser.ParseFloat() },
            { "SpinRate", (parser, x) => x.SpinRate = parser.ParseFloat() },
            { "RollRate", (parser, x) => x.RollRate = parser.ParseFloat() },
            { "PitchRate", (parser, x) => x.PitchRate = parser.ParseFloat() },
            { "YawRate", (parser, x) => x.YawRate = parser.ParseFloat() },
            { "InvulnerableTime", (parser, x) => x.InvulnerableTime = parser.ParseInteger() },
            { "RequiresLivePlayer", (parser, x) => x.RequiresLivePlayer = parser.ParseBoolean() },
            { "ExtraBounciness", (parser, x) => x.ExtraBounciness = parser.ParseFloat() },
            { "ExtraFriction", (parser, x) => x.ExtraFriction = parser.ParseFloat() },
            { "ContainInsideSourceObject", (parser, x) => x.ContainInsideSourceObject = parser.ParseBoolean() },
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseInteger() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseInteger() },
            { "SkipIfSignificantlyAirborne", (parser, x) => x.SkipIfSignificantlyAirborne = parser.ParseBoolean() },
        };

        public string ObjectNames { get; private set; }
        public Coord3D Offset { get; private set; }
        public int Count { get; private set; }
        public bool SpreadFormation { get; private set; }
        public float MinDistanceAFormation { get; private set; }
        public float MinDistanceBFormation { get; private set; }
        public float MaxDistanceFormation { get; private set; }
        public bool FadeIn { get; private set; }
        public int FadeTime { get; private set; }
        public string FadeSound { get; private set; }
        public string PutInContainer { get; private set; }
        public bool IgnorePrimaryObstacle { get; private set; }
        public bool InheritsVeterancy { get; private set; }
        public BitArray<ObjectDisposition> Disposition { get; private set; }
        public float DispositionIntensity { get; private set; }
        public bool PreserveLayer { get; private set; }
        public int MinForceMagnitude { get; private set; }
        public int MaxForceMagnitude { get; private set; }
        public float MinForcePitch { get; private set; }
        public float MaxForcePitch { get; private set; }
        public float SpinRate { get; private set; }
        public float RollRate { get; private set; }
        public float PitchRate { get; private set; }
        public float YawRate { get; private set; }
        public int InvulnerableTime { get; private set; }
        public bool RequiresLivePlayer { get; private set; }
        public float ExtraBounciness { get; private set; }
        public float ExtraFriction { get; private set; }
        public bool ContainInsideSourceObject { get; private set; }
        public int MinLifetime { get; private set; }
        public int MaxLifetime { get; private set; }
        public bool SkipIfSignificantlyAirborne { get; private set; }
    }

    public sealed class ApplyRandomForceObjectCreationListItem : ObjectCreationListItem
    {
        internal static ApplyRandomForceObjectCreationListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ApplyRandomForceObjectCreationListItem> FieldParseTable = new IniParseTable<ApplyRandomForceObjectCreationListItem>
        {
            { "MinForceMagnitude", (parser, x) => x.MinForceMagnitude = parser.ParseInteger() },
            { "MaxForceMagnitude", (parser, x) => x.MaxForceMagnitude = parser.ParseInteger() },
            { "MinForcePitch", (parser, x) => x.MinForcePitch = parser.ParseFloat() },
            { "MaxForcePitch", (parser, x) => x.MaxForcePitch = parser.ParseFloat() },
            { "SpinRate", (parser, x) => x.SpinRate = parser.ParseFloat() }
        };

        public int MinForceMagnitude { get; private set; }
        public int MaxForceMagnitude { get; private set; }
        public float MinForcePitch { get; private set; }
        public float MaxForcePitch { get; private set; }
        public float SpinRate { get; private set; }
    }

    public sealed class FireWeaponObjectCreationListItem : ObjectCreationListItem
    {
        internal static FireWeaponObjectCreationListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<FireWeaponObjectCreationListItem> FieldParseTable = new IniParseTable<FireWeaponObjectCreationListItem>
        {
            { "Weapon", (parser, x) => x.Weapon = parser.ParseAssetReference() }
        };

        public string Weapon { get; private set; }
    }

    public sealed class AttackObjectCreationListItem : ObjectCreationListItem
    {
        internal static AttackObjectCreationListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<AttackObjectCreationListItem> FieldParseTable = new IniParseTable<AttackObjectCreationListItem>
        {
            { "WeaponSlot", (parser, x) => x.WeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "NumberOfShots", (parser, x) => x.NumberOfShots = parser.ParseInteger() },
            { "DeliveryDecalRadius", (parser, x) => x.DeliveryDecalRadius = parser.ParseInteger() },
            { "DeliveryDecal", (parser, x) => x.DeliveryDecal = RadiusDecalTemplate.Parse(parser) }
        };

        public WeaponSlot WeaponSlot { get; private set; }
        public int NumberOfShots { get; private set; }
        public int DeliveryDecalRadius { get; private set; }
        public RadiusDecalTemplate DeliveryDecal { get; private set; }
    }

    public sealed class DeliverPayloadObjectCreationListItem : ObjectCreationListItem
    {
        internal static DeliverPayloadObjectCreationListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<DeliverPayloadObjectCreationListItem> FieldParseTable = new IniParseTable<DeliverPayloadObjectCreationListItem>
        {
            { "Transport", (parser, x) => x.Transport = parser.ParseAssetReference() },
            { "FormationSize", (parser, x) => x.FormationSize = parser.ParseInteger() },
            { "FormationSpacing", (parser, x) => x.FormationSpacing = parser.ParseFloat() },
            { "StartAtPreferredHeight", (parser, x) => x.StartAtPreferredHeight = parser.ParseBoolean() },
            { "StartAtMaxSpeed", (parser, x) => x.StartAtMaxSpeed = parser.ParseBoolean() },
            { "MaxAttempts", (parser, x) => x.MaxAttempts = parser.ParseInteger() },
            { "DropDelay", (parser, x) => x.DropDelay = parser.ParseInteger() },
            { "ParachuteDirectly", (parser, x) => x.ParachuteDirectly = parser.ParseBoolean() },
            { "PutInContainer", (parser, x) => x.PutInContainer = parser.ParseAssetReference() },
            { "DropOffset", (parser, x) => x.DropOffset = Coord3D.Parse(parser) },
            { "DropVariance", (parser, x) => x.DropVariance = Coord3D.Parse(parser) },
            { "Payload", (parser, x) => x.Payload = Payload.Parse(parser) },
            { "FireWeapon", (parser, x) => x.FireWeapon = parser.ParseBoolean() },
            { "DeliveryDistance", (parser, x) => x.DeliveryDistance = parser.ParseInteger() },
            { "PreOpenDistance", (parser, x) => x.PreOpenDistance = parser.ParseInteger() },
            { "WeaponErrorRadius", (parser, x) => x.WeaponErrorRadius = parser.ParseInteger() },
            { "DelayDeliveryMax", (parser, x) => x.DelayDeliveryMax = parser.ParseInteger() },
            { "VisibleItemsDroppedPerInterval", (parser, x) => x.VisibleItemsDroppedPerInterval = parser.ParseInteger() },
            { "VisibleDropBoneBaseName", (parser, x) => x.VisibleDropBoneBaseName = parser.ParseBoneName() },
            { "VisibleSubObjectBaseName", (parser, x) => x.VisibleSubObjectBaseName = parser.ParseAssetReference() },
            { "VisibleNumBones", (parser, x) => x.VisibleNumBones = parser.ParseInteger() },
            { "VisiblePayloadTemplateName", (parser, x) => x.VisiblePayloadTemplateName = parser.ParseAssetReference() },
            { "VisiblePayloadWeaponTemplate", (parser, x) => x.VisiblePayloadWeaponTemplate = parser.ParseAssetReference() },
            { "InheritTransportVelocity", (parser, x) => x.InheritTransportVelocity = parser.ParseBoolean() },
            { "ExitPitchRate", (parser, x) => x.ExitPitchRate = parser.ParseInteger() },
            { "DiveStartDistance", (parser, x) => x.DiveStartDistance = parser.ParseInteger() },
            { "DiveEndDistance", (parser, x) => x.DiveEndDistance = parser.ParseInteger() },
            { "StrafingWeaponSlot", (parser, x) => x.StrafingWeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "StrafeLength", (parser, x) => x.StrafeLength = parser.ParseInteger() },
            { "StrafeWeaponFX", (parser, x) => x.StrafeWeaponFX = parser.ParseAssetReference() },
            { "SelfDestructObject", (parser, x) => x.SelfDestructObject = parser.ParseBoolean() },
            { "WeaponConvergenceFactor", (parser, x) => x.WeaponConvergenceFactor = parser.ParseFloat() },
            { "DeliveryDecalRadius", (parser, x) => x.DeliveryDecalRadius = parser.ParseInteger() },
            { "DeliveryDecal", (parser, x) => x.DeliveryDecal = RadiusDecalTemplate.Parse(parser) },
        };

        public string Transport { get; private set; }
        public int FormationSize { get; private set; }
        public float FormationSpacing { get; private set; }
        public bool StartAtPreferredHeight { get; private set; }
        public bool StartAtMaxSpeed { get; private set; }
        public int MaxAttempts { get; private set; }
        public int DropDelay { get; private set; }
        public bool ParachuteDirectly { get; private set; }
        public string PutInContainer { get; private set; }
        public Coord3D DropOffset { get; private set; }
        public Coord3D DropVariance { get; private set; }
        public Payload Payload { get; private set; }
        public bool FireWeapon { get; private set; }
        public int DeliveryDistance { get; private set; }
        public int PreOpenDistance { get; private set; }
        public int WeaponErrorRadius { get; private set; }
        public int DelayDeliveryMax { get; private set; }
        public int VisibleItemsDroppedPerInterval { get; private set; }
        public string VisibleDropBoneBaseName { get; private set; }
        public string VisibleSubObjectBaseName { get; private set; }
        public int VisibleNumBones { get; private set; }
        public string VisiblePayloadTemplateName { get; private set; }
        public string VisiblePayloadWeaponTemplate { get; private set; }
        public bool InheritTransportVelocity { get; private set; }
        public int ExitPitchRate { get; private set; }
        public int DiveStartDistance { get; private set; }
        public int DiveEndDistance { get; private set; }
        public WeaponSlot StrafingWeaponSlot { get; private set; }
        public int StrafeLength { get; private set; }
        public string StrafeWeaponFX { get; private set; }
        public bool SelfDestructObject { get; private set; }
        public float WeaponConvergenceFactor { get; private set; }
        public int DeliveryDecalRadius { get; private set; }
        public RadiusDecalTemplate DeliveryDecal { get; private set; }
    }

    public struct Payload
    {
        internal static Payload Parse(IniParser parser)
        {
            return new Payload
            {
                Name = parser.ParseAssetReference(),
                Quantity = parser.NextTokenIf(IniTokenType.IntegerLiteral)?.IntegerValue ?? 1
            };
        }

        public string Name { get; private set; }
        public int Quantity { get; private set; }
    }

    public enum ObjectDisposition
    {
        [IniEnum("RANDOM_FORCE")]
        RandomForce,

        [IniEnum("LIKE_EXISTING")]
        LikeExisting,

        [IniEnum("INHERIT_VELOCITY")]
        InheritVelocity,

        [IniEnum("ON_GROUND_ALIGNED")]
        OnGroundAligned,

        [IniEnum("SEND_IT_FLYING")]
        SendItFlying,

        [IniEnum("SEND_IT_OUT")]
        SendItOut,

        [IniEnum("SEND_IT_UP")]
        SendItUp,

        [IniEnum("FLOATING")]
        Floating
    }
}
