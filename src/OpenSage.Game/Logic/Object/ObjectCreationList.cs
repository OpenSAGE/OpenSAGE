using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Gui.InGame;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    internal sealed class ObjectCreationListManager
    {
        public void Create(ObjectCreationList list, BehaviorUpdateContext context)
        {
            foreach (var item in list.Nuggets)
            {
                item.Execute(context);
            }
        }

        public void CreateAtPosition(ObjectCreationList list, BehaviorUpdateContext context, Vector3 position)
        {
            foreach (var item in list.Nuggets)
            {
                item.Execute(context, position);
            }
        }
    }

    public sealed class ObjectCreationList : BaseAsset
    {
        internal static ObjectCreationList Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("ObjectCreationList", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<ObjectCreationList> FieldParseTable = new IniParseTable<ObjectCreationList>
        {
            { "ApplyRandomForce", (parser, x) => x.Nuggets.Add(ApplyRandomForceOCNugget.Parse(parser)) },
            { "Attack", (parser, x) => x.Nuggets.Add(AttackOCNugget.Parse(parser)) },
            { "CreateDebris", (parser, x) => x.Nuggets.Add(CreateDebrisOCNugget.Parse(parser)) },
            { "CreateObject", (parser, x) => x.Nuggets.Add(CreateObjectOCNugget.Parse(parser)) },
            { "DeliverPayload", (parser, x) => x.Nuggets.Add(DeliverPayloadOCNugget.Parse(parser)) },
            { "FireWeapon", (parser, x) => x.Nuggets.Add(FireWeaponOCNugget.Parse(parser)) }
        };

        public List<OCNugget> Nuggets { get; } = new List<OCNugget>();
    }

    public abstract class OCNugget
    {
        internal abstract List<GameObject> Execute(BehaviorUpdateContext context, Vector3? position = null);
    }

    public sealed class CreateDebrisOCNugget : OCNugget
    {
        internal static CreateDebrisOCNugget Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<CreateDebrisOCNugget> FieldParseTable = new IniParseTable<CreateDebrisOCNugget>
        {
            { "ModelNames", (parser, x) => x.ModelNames = parser.ParseAssetReferenceArray() },
            { "AnimationSet", (parser, x) => x.AnimationSets.Add(parser.ParseAssetReferenceArray()) },
            { "FXFinal", (parser, x) => x.FXFinal = parser.ParseAssetReference() },
            { "Offset", (parser, x) => x.Offset = parser.ParseVector3() },
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
            { "ExtraFriction", (parser, x) => x.ExtraFriction = parser.ParseFloat() },
            { "YawRate", (parser, x) => x.YawRate = parser.ParseInteger() },
            { "PitchRate", (parser, x) => x.PitchRate = parser.ParseInteger() },
            { "RollRate", (parser, x) => x.RollRate = parser.ParseInteger() },
        };

        public string[] ModelNames { get; private set; }
        public List<string[]> AnimationSets { get; } = new List<string[]>();
        public string FXFinal { get; private set; }
        public Vector3 Offset { get; private set; }
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

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int YawRate { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int PitchRate { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int RollRate { get; private set; }

        internal override List<GameObject> Execute(BehaviorUpdateContext context, Vector3? position)
        {
            var debrisObject = context.GameContext.GameObjects.Add("GenericDebris", context.GameObject.Owner);

            var lifeTime = context.GameContext.Random.NextDouble() * (MaxLifetime - MinLifetime) + MinLifetime;
            debrisObject.LifeTime = context.Time.TotalTime + TimeSpan.FromMilliseconds(lifeTime);

            debrisObject.UpdateTransform(context.GameObject.Translation + Offset, context.GameObject.Rotation);

            // Model
            var w3dDebrisDraw = (W3dDebrisDraw) debrisObject.Drawable.DrawModules[0];
            // TODO
            //var modelName = ModelNames[context.GameContext.Random.Next(ModelNames.Length)];
            var modelName = ModelNames[0];
            w3dDebrisDraw.SetModelName(modelName);

            // Physics
            var physicsBehavior = debrisObject.FindBehavior<PhysicsBehavior>();
            physicsBehavior.Mass = Mass;

            if (Disposition.Get(ObjectDisposition.SendItFlying))
            {
                physicsBehavior.AddForce(
                    new Vector3(
                        ((float) context.GameContext.Random.NextDouble() - 0.5f) * DispositionIntensity * 200,
                        ((float) context.GameContext.Random.NextDouble() - 0.5f) * DispositionIntensity * 200,
                        DispositionIntensity * 200));
            }

            // TODO: Count, Disposition, DispositionIntensity

            return new List<GameObject> { debrisObject };
        }
    }

    public sealed class CreateObjectOCNugget : OCNugget
    {
        internal static CreateObjectOCNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CreateObjectOCNugget> FieldParseTable = new IniParseTable<CreateObjectOCNugget>
        {
            { "ObjectNames", (parser, x) => x.ObjectNames = parser.ParseObjectReferenceArray() },
            { "Offset", (parser, x) => x.Offset = parser.ParseVector3() },
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
            { "DiesOnBadLand", (parser, x) => x.DiesOnBadLand = parser.ParseBoolean() },
            { "VelocityScale", (parser, x) => x.VelocityScale = parser.ParseFloat() },
            { "UseJustBuiltFlag", (parser, x) => x.UseJustBuiltFlag = parser.ParseBoolean() },
            { "InheritAttributesFromSource", (parser, x) => x.InheritAttributesFromSource = parser.ParseBoolean() },
            { "IgnoreCommandPointLimit", (parser, x) => x.IgnoreCommandPointLimit = parser.ParseBoolean() },
            { "VeterancyLevel", (parser, x) => x.VeterancyLevel = parser.ParseInteger() },
            { "DispositionAngle", (parser, x) => x.DispositionAngle = parser.ParseFloat() },
            { "StartingBusyTime", (parser, x) => x.StartingBusyTime = parser.ParseInteger() },
            { "ParticleSystem", (parser, x) => x.ParticleSystem = parser.ParseIdentifier() },
            { "InheritScriptingName", (parser, x) => x.InheritScriptingName = parser.ParseBoolean() },
            { "IgnoreAllObjects", (parser, x) => x.IgnoreAllObjects = parser.ParseBoolean() },
            { "JustBuiltDuration", (parser, x) => x.JustBuiltDuration = parser.ParseInteger() },
            { "ForbiddenUpgrades", (parser, x) => x.ForbiddenUpgrades = parser.ParseAssetReferenceArray() },
            { "RequiredUpgrades", (parser, x) => x.RequiredUpgrades = parser.ParseAssetReferenceArray() },
            { "OrientInSecondaryDirection", (parser, x) => x.OrientInSecondaryDirection = parser.ParseBoolean() },
            { "OrientationOffset", (parser, x) => x.OrientationOffset = parser.ParseInteger() },
            { "ClearRemovables", (parser, x) => x.ClearRemovables = parser.ParseBoolean() },
            { "IssueMoveAfterCreation", (parser, x) => x.IssueMoveAfterCreation = parser.ParseBoolean() },
            { "MoveUsesStrafeUpdate", (parser, x) => x.MoveUsesStrafeUpdate = parser.ParseBoolean() },
            { "OrientInPrimaryDirection", (parser, x) => x.OrientInPrimaryDirection = parser.ParseBoolean() },
            { "OffsetInLocalSpace", (parser, x) => x.OffsetInLocalSpace = parser.ParseBoolean() },
            { "DestinationPlayer", (parser, x) => x.DestinationPlayer = parser.ParseAssetReference() },
            { "WaypointSpawnPoints", (parser, x) => x.WaypointSpawnPoints = parser.ParseAssetReference() },
        };

        public LazyAssetReference<ObjectDefinition>[] ObjectNames { get; private set; }
        public Vector3 Offset { get; private set; }
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

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool DiesOnBadLand { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float VelocityScale { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UseJustBuiltFlag { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool InheritAttributesFromSource { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IgnoreCommandPointLimit { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int VeterancyLevel { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float DispositionAngle { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int StartingBusyTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ParticleSystem { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool InheritScriptingName { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IgnoreAllObjects { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int JustBuiltDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] ForbiddenUpgrades { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] RequiredUpgrades { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool OrientInSecondaryDirection { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int OrientationOffset { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ClearRemovables { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool IssueMoveAfterCreation { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool MoveUsesStrafeUpdate { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool OrientInPrimaryDirection { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool OffsetInLocalSpace { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string DestinationPlayer { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string WaypointSpawnPoints { get; private set; }

        internal override List<GameObject> Execute(BehaviorUpdateContext context, Vector3? position)
        {
            var result = new List<GameObject>();
            // TODO

            foreach (var objectName in ObjectNames)
            {
                var newGameObject = context.GameContext.GameObjects.Add(objectName.Value, context.GameObject.Owner);
                // TODO: Count
                // TODO: Disposition
                // TODO: DispositionIntensity
                // TODO: IgnorePrimaryObstacle
                if (position.HasValue)
                {
                    newGameObject.UpdateTransform(position.Value);
                }
                else
                {
                    newGameObject.UpdateTransform(context.GameObject.Translation + Offset, context.GameObject.Rotation);
                }

                var lifetimeUpdate = newGameObject.FindBehavior<LifetimeUpdate>();
                if (lifetimeUpdate != null)
                {
                    var lifetime = context.GameContext.Random.NextDouble() * (MaxLifetime - MinLifetime) + MinLifetime;
                    lifetimeUpdate.Lifetime = context.Time.TotalTime + TimeSpan.FromMilliseconds(lifetime);
                }

                result.Add(newGameObject);
            }

            return result;
        }
    }

    public sealed class ApplyRandomForceOCNugget : OCNugget
    {
        internal static ApplyRandomForceOCNugget Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ApplyRandomForceOCNugget> FieldParseTable = new IniParseTable<ApplyRandomForceOCNugget>
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

        internal override List<GameObject> Execute(BehaviorUpdateContext context, Vector3? position)
        {
            // TODO
            return new List<GameObject>();
        }
    }

    public sealed class FireWeaponOCNugget : OCNugget
    {
        internal static FireWeaponOCNugget Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<FireWeaponOCNugget> FieldParseTable = new IniParseTable<FireWeaponOCNugget>
        {
            { "Weapon", (parser, x) => x.Weapon = parser.ParseAssetReference() }
        };

        public string Weapon { get; private set; }

        internal override List<GameObject> Execute(BehaviorUpdateContext context, Vector3? position)
        {
            // TODO
            return new List<GameObject>();
        }
    }

    public sealed class AttackOCNugget : OCNugget
    {
        internal static AttackOCNugget Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<AttackOCNugget> FieldParseTable = new IniParseTable<AttackOCNugget>
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

        internal override List<GameObject> Execute(BehaviorUpdateContext context, Vector3? position)
        {
            // TODO
            return new List<GameObject>();
        }
    }

    public sealed class DeliverPayloadOCNugget : OCNugget
    {
        internal static DeliverPayloadOCNugget Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<DeliverPayloadOCNugget> FieldParseTable = new IniParseTable<DeliverPayloadOCNugget>
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
            { "DropOffset", (parser, x) => x.DropOffset = parser.ParseVector3() },
            { "DropVariance", (parser, x) => x.DropVariance = parser.ParseVector3() },
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
        public Vector3 DropOffset { get; private set; }
        public Vector3 DropVariance { get; private set; }
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

        internal override List<GameObject> Execute(BehaviorUpdateContext context, Vector3? position)
        {
            // TODO
            return new List<GameObject>();
        }
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
        Floating,

        [IniEnum("BUILDING_CHUNKS")]
        BuildingChunks,

        [IniEnum("FORWARD_IMPACT")]
        ForwardImpact,

        [IniEnum("SPAWN_AROUND")]
        SpawnAround,

        [IniEnum("SET_ANGLE")]
        SetAngle,

        [IniEnum("FADE_AND_DIE_ORNAMENT")]
        FadeAndDieOrnament,

        [IniEnum("ANIMATED")]
        Animated,

        [IniEnum("RELATIVE_ANGLE"), AddedIn(SageGame.Bfme2)]
        RelativeAngle,

        [IniEnum("USE_WATER_SURFACE"), AddedIn(SageGame.Bfme2)]
        UseWaterSurface,

        [IniEnum("USE_CLIFF"), AddedIn(SageGame.Bfme2)]
        UseCliff,

        [IniEnum("ABSOLUTE_ANGLE"), AddedIn(SageGame.Bfme2)]
        AbsoluteAngle,
    }
}
