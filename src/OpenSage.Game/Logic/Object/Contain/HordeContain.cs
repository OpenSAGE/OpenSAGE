using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class HordeContainBehavior : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly HordeContainModuleData _moduleData;

        private Dictionary<int, List<HordeMemberPosition>> _formation;
        private List<GameObject> _payload;

        private ProductionUpdate _productionUpdate;
        private int _pendingRegistrations;

        public HordeContainBehavior(GameObject gameObject, HordeContainModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;

            _payload = new List<GameObject>();
            _formation = CreateFormationOffsets();
        }

        public List<GameObject> SelectAll(bool value)
        {
            _gameObject.IsSelected = value;
            foreach (var obj in _payload)
            {
                obj.IsSelected = value;
            }
            return _payload;
        }

        private Dictionary<int, List<HordeMemberPosition>> CreateFormationOffsets()
        {
            var result = new Dictionary<int, List<HordeMemberPosition>>();
            var i = 0;
            foreach (var rankInfo in _moduleData.RankInfos)
            {
                result.Add(i, new List<HordeMemberPosition>());
                foreach (var pos in rankInfo.Positions)
                {
                    result[i].Add(new HordeMemberPosition
                    {
                        Definition = rankInfo.UnitType.Value,
                        Object = null,
                        Position = new Vector3(pos, 0)
                    });
                }
                i++;
            }
            return result;
        }

        public void Unpack()
        {
            foreach (var rank in _formation.Values)
            {
                foreach (var position in rank)
                {
                    var createdObject = _gameObject.Parent.Add(position.Definition, _gameObject.Owner);
                    createdObject.ParentHorde = _gameObject;
                    position.Object = createdObject;
                    _payload.Add(createdObject);

                    createdObject.UpdateTransform(_gameObject.Translation + position.Position, _gameObject.Rotation);
                }
            }
        }

        public void SetTargetPoints(Vector3 targetPosition, Vector3 targetDirection)
        {
            var targetYaw = MathUtility.GetYawFromDirection(targetDirection.Vector2XY());

            foreach (var rank in _formation.Values)
            {
                foreach (var position in rank)
                {
                    var offset = Vector3.Transform(position.Position, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, targetYaw));

                    position.Object?.AIUpdate?.TargetPoints.Clear();
                    position.Object?.AIUpdate?.AppendPathToTargetPoint(targetPosition + offset);
                    position.Object?.AIUpdate?.SetTargetDirection(targetDirection);
                }
            }
        }

        public void Register(GameObject obj)
        {
            foreach (var rank in _formation.Values)
            {
                foreach (var position in rank)
                {
                    if (position.Object != null)
                    {
                        continue;
                    }

                    if (position.Definition == obj.Definition
                        || position.Definition.ObjectIsMemberOfBuildVariations(obj.Definition))
                    {
                        position.Object = obj;
                        _payload.Add(obj);

                        _pendingRegistrations--;
                        if (_pendingRegistrations == 0)
                        {
                            _productionUpdate.ParentHorde = null;
                            _productionUpdate.CloseDoor();
                            _productionUpdate = null;
                        }
                        return;
                    }
                }
            }
        }

        public Vector3 GetFormationOffset(GameObject obj)
        {
            var hordeYaw = _gameObject.Yaw;
            foreach (var rank in _formation.Values)
            {
                foreach (var position in rank)
                {
                    if (position.Object == obj)
                    {
                        _payload.Add(obj);
                        return Vector3.Transform(position.Position, Quaternion.CreateFromYawPitchRoll(hordeYaw, 0, 0));
                    }
                }
            }
            return Vector3.Zero;
        }

        public void EnqueuePayload(ProductionUpdate productionUpdate, int delay)
        {
            var delay_s = delay / 1000.0f;
            _productionUpdate = productionUpdate;

            foreach (var rank in _formation.Values)
            {
                foreach (var position in rank)
                {
                    _productionUpdate.SpawnPayload(position.Definition, delay_s);
                    _pendingRegistrations++;
                }
            }
        }

        internal override void Update(BehaviorUpdateContext context)
        {

        }
    }

    [AddedIn(SageGame.Bfme)]
    public class HordeContainModuleData : BehaviorModuleData
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
            { "ComboHorde", (parser, x) => x.ComboHordes.Add(ComboHorde.Parse(parser)) },

            { "UseSlowHordeMovement", (parser, x) => x.UseSlowHordeMovement = parser.ParseBoolean() },

            { "MeleeAttackLeashDistance", (parser, x) =>  x.MeleeAttackLeashDistance = parser.ParseInteger() },
            { "MachineAllowed", (parser, x) => x.MachineAllowed = parser.ParseBoolean() },
            { "MachineType", (parser, x) => x.MachineType = parser.ParseAssetReference() },

            { "AlternateFormation", (parser, x) => x.AlternateFormation = parser.ParseAssetReference() },

            { "BackUpMinDelayTime", (parser, x) => x.BackUpMinDelayTime = parser.ParseInteger() },
            { "BackUpMaxDelayTime", (parser, x) => x.BackUpMaxDelayTime = parser.ParseInteger() },
            { "BackUpMinDistance", (parser, x) => x.BackUpMinDistance = parser.ParseInteger() },
            { "BackUpMaxDistance", (parser, x) => x.BackUpMaxDistance = parser.ParseInteger() },
            { "BackupPercentage", (parser, x) => x.BackupPercentage = parser.ParseFloat() },
            { "AttributeModifiers", (parser, x) => x.AttributeModifiers = parser.ParseAssetReferenceArray() },
            { "RanksThatStopAdvance", (parser, x) => x.RanksThatStopAdvance = parser.ParseInteger() },
            { "RanksToJustFreeWhenAttacking", (parser, x) => x.RanksToJustFreeWhenAttacking = parser.ParseInteger() },
            { "NotComboFormation", (parser, x) => x.NotComboFormation = parser.ParseBoolean() },
            { "UsePorcupineBody", (parser, x) => x.UsePorcupineBody = parser.ParseBoolean() },
            { "SplitHorde", (parser, x) => x.SplitHordes.Add(SplitHorde.Parse(parser)) },
            { "UseMarchingAnims", (parser, x) => x.UseMarchingAnims = parser.ParseBoolean() },
            { "ForcedLocomotorSet", (parser, x) => x.ForcedLocomotorSet = parser.ParseEnum<LocomotorSetType>() },
            { "UpdateWeaponSetFlagsOnHordeToo", (parser, x) => x.UpdateWeaponSetFlagsOnHordeToo = parser.ParseBoolean() },
            { "RankSplit", (parser, x) => x.RankSplit = parser.ParseBoolean() },
            { "SplitHordeNumber", (parser, x) => x.SplitHordeNumber = parser.ParseInteger() },
            { "FrontAngle", (parser, x) => x.FrontAngle = parser.ParseFloat() },
            { "FlankedDelay", (parser, x) => x.FlankedDelay = parser.ParseInteger() },
            { "MeleeBehavior", (parser, x) => x.MeleeBehavior = MeleeBehavior.Parse(parser) },
            { "IsPorcupineFormation", (parser, x) => x.IsPorcupineFormation = parser.ParseBoolean() },
            { "MinimumHordeSize", (parser, x) => x.MinimumHordeSize = parser.ParseInteger() },
            { "VisionRearOverride", (parser, x) => x.VisionRearOverride = parser.ParsePercentage() },
            { "VisionSideOverride", (parser, x) => x.VisionSideOverride = parser.ParsePercentage() },
            { "BannerCarrierMinLevel", (parser, x) => x.BannerCarrierMinLevel = parser.ParseInteger() },
            { "BannerCarrierDestroyHordeOnDeath", (parser, x) => x.BannerCarrierDestroyHordeOnDeath = parser.ParseBoolean() },
            { "BannerCarrierHordeDeathType", (parser, x) => x.BannerCarrierHordeDeathType = parser.ParseEnumBitArray<DeathType>() },
            { "LivingWorldOverloadTemplate", (parser, x) => x.LivingWorldOverloadTemplate = parser.ParseAssetReference() }
        };

        public BitArray<ObjectStatus> ObjectStatusOfContained { get; private set; }
        public List<Payload> InitialPayloads { get; } = new List<Payload>();
        public int Slots { get; private set; }
        public ObjectFilter PassengerFilter { get; private set; }
        public bool ShowPips { get; private set; }
        public bool ThisFormationIsTheMainFormation { get; private set; }
        public Point2D RandomOffset { get; private set; }

        // NOTE: Despite the name, in BFME1 this always contains just 1 entry.
        public List<string> BannerCarriersAllowed { get; } = new List<string>();
        public List<BannerCarrierPosition> BannerCarrierPositions { get; } = new List<BannerCarrierPosition>();

        public List<RankInfo> RankInfos { get; private set; } = new List<RankInfo>();

        public int[] RanksToReleaseWhenAttacking { get; private set; }

        public List<ComboHorde> ComboHordes { get; } = new List<ComboHorde>();

        public bool UseSlowHordeMovement { get; private set; }

        public int MeleeAttackLeashDistance { get; private set; }
        public bool MachineAllowed { get; private set; }
        public string MachineType { get; private set; }

        public string AlternateFormation { get; private set; }

        public int BackUpMinDelayTime { get; private set; }
        public int BackUpMaxDelayTime { get; private set; }
        public int BackUpMinDistance { get; private set; }
        public int BackUpMaxDistance { get; private set; }
        public float BackupPercentage { get; private set; }
        public string[] AttributeModifiers { get; private set; }
        public int RanksThatStopAdvance { get; private set; }
        public int RanksToJustFreeWhenAttacking { get; private set; }
        public bool NotComboFormation { get; private set; }
        public bool UsePorcupineBody { get; private set; }
        public List<SplitHorde> SplitHordes { get; } = new List<SplitHorde>();
        public bool UseMarchingAnims { get; private set; }
        public LocomotorSetType ForcedLocomotorSet { get; private set; }
        public bool UpdateWeaponSetFlagsOnHordeToo { get; private set; }
        public bool RankSplit { get; private set; }
        public int SplitHordeNumber { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float FrontAngle { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int FlankedDelay { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public MeleeBehavior MeleeBehavior { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool IsPorcupineFormation { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MinimumHordeSize { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage VisionRearOverride { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage VisionSideOverride { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int BannerCarrierMinLevel { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool BannerCarrierDestroyHordeOnDeath { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public BitArray<DeathType> BannerCarrierHordeDeathType { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string LivingWorldOverloadTemplate { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new HordeContainBehavior(gameObject, this);
        }
    }

    class HordeMemberPosition
    {
        public ObjectDefinition Definition;
        public GameObject Object { get; set; }
        public Vector3 Position;
        public bool Initialized { get; set; } = false;
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class Payload
    {
        internal static Payload Parse(IniParser parser)
        {
            var payload = new Payload
            {
                Object = parser.ParseObjectReference()
            };

            payload.Count = parser.GetIntegerOptional();
            return payload;
        }

        public LazyAssetReference<ObjectDefinition> Object { get; private set; }
        public int Count { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class BannerCarrierPosition
    {
        internal static BannerCarrierPosition Parse(IniParser parser)
        {
            return new BannerCarrierPosition
            {
                UnitType = parser.ParseAttributeObjectReference("UnitType"),
                Position = parser.ParseAttributeVector2("Pos")
            };
        }

        public LazyAssetReference<ObjectDefinition> UnitType { get; private set; }
        public Vector2 Position { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class RankInfo
    {
        internal static RankInfo Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<RankInfo> FieldParseTable = new IniParseTable<RankInfo>
        {
            { "RankNumber", (parser, x) => x.RankNumber = parser.ParseInteger() },
            { "UnitType", (parser, x) => x.UnitType = parser.ParseObjectReference() },
            { "Position", (parser, x) => x.Positions.Add(parser.ParseVector2()) },
            { "RevokedWeaponCondition", (parser, x) => x.RevokedWeaponCondition = parser.ParseEnum<WeaponSetConditions>() },
            { "GrantedWeaponCondition", (parser, x) => x.GrantedWeaponCondition = parser.ParseEnum<WeaponSetConditions>() },
            { "Leader", (parser, x) => x.Leaders.Add(Leader.Parse(parser)) },
        };

        public int RankNumber { get; private set; }
        public LazyAssetReference<ObjectDefinition> UnitType { get; private set; }
        public List<Vector2> Positions { get; } = new List<Vector2>();
        public WeaponSetConditions RevokedWeaponCondition { get; private set; }
        public WeaponSetConditions GrantedWeaponCondition { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<Leader> Leaders { get; } = new List<Leader>();
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class ComboHorde
    {
        internal static ComboHorde Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<ComboHorde> FieldParseTable = new IniParseTable<ComboHorde>
        {
            { "Target", (parser, x) => x.Target = parser.ParseIdentifier() },
            { "Result", (parser, x) => x.Result = parser.ParseIdentifier() },
            { "InitiateVoice", (parser, x) => x.InitiateVoice = parser.ParseAssetReference() },
        };

        public string Target { get; private set; }
        public string Result { get; private set; }
        public string InitiateVoice { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class SplitHorde
    {
        internal static SplitHorde Parse(IniParser parser)
        {
            return new SplitHorde
            {
                SplitResult = parser.ParseAttributeIdentifier("SplitResult"),
                UnitType = parser.ParseAttributeIdentifier("UnitType")
            };
        }

        public string SplitResult { get; private set; }
        public string UnitType { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class Leader
    {
        internal static Leader Parse(IniParser parser)
        {
            return new Leader
            {
                X = parser.ParseInteger(),
                Y = parser.ParseInteger(),
            };
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class MeleeBehavior
    {
        internal static MeleeBehavior Parse(IniParser parser)
        {
            return parser.ParseNamedBlock((x, name) => x.Name = name, FieldParseTable);
        }

        internal static readonly IniParseTable<MeleeBehavior> FieldParseTable = new IniParseTable<MeleeBehavior>
        {
            { "FacingBonus", (parser, x) => x.FacingBonus = parser.ParseFloat() },
            { "AngleLimitCos", (parser, x) => x.AngleLimitCos = parser.ParseFloat() },
            { "InnerRange", (parser, x) => x.InnerRange = parser.ParseInteger() },
            { "OuterRange", (parser, x) => x.OuterRange = parser.ParseInteger() },
            { "OuterRangeBuildings", (parser, x) => x.OuterRangeBuildings = parser.ParseInteger() },
        };

        public string Name { get; private set; }

        public float FacingBonus { get; private set; }
        public float AngleLimitCos { get; private set; }
        public int InnerRange { get; private set; }
        public int OuterRange { get; private set; }
        public int OuterRangeBuildings { get; private set; }
    }
}
