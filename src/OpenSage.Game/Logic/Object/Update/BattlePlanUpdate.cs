using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class BattlePlanUpdate : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly GameContext _context;
        private readonly BattlePlanUpdateModuleData _moduleData;

        private BattlePlanType _current; // the battle plan which the model currently represents
        private BattlePlanType _desired; // the battle plan we'd like the model to represent
        private BattlePlanType _active; // the battle plan active (different from current in that this is None while _state is not Active)
        private BattlePlanUpdateState _state; // the state of the strategy center

        private uint _stateChangeCompleteFrame; // the frame at which the current state change will be completed

        private uint _unknownUInt2;
        private uint _unknownUInt3;

        private bool _unknownBool1;
        private bool _unknownBool2;
        private ushort _unknownShort1;

        private uint _unknownUInt4;

        private List<byte> _unknownList1 = [];
        private List<byte> _unknownList2 = [];
        private List<byte> _unknownList3 = [];

        private ushort _unknownShort2;

        private List<byte> _unknownList4 = [];
        private List<byte> _unknownList5 = [];
        private List<byte> _unknownList6 = [];
        private List<byte> _unknownList7 = [];

        private int _unknownInt;

        internal BattlePlanUpdate(GameObject gameObject, GameContext context, BattlePlanUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _context = context;
            _moduleData = moduleData;
        }

        public void ChangeBattlePlan(BattlePlanType newPlan)
        {
            _desired = newPlan;
        }

        private protected override void RunUpdate(BehaviorUpdateContext context)
        {
            SetDoorState();

            switch (_state)
            {
                case BattlePlanUpdateState.None:
                    if (_desired is not BattlePlanType.None && context.LogicFrame.Value >= _stateChangeCompleteFrame)
                    {
                        // the state has been changed
                        _state = BattlePlanUpdateState.Activating;
                        _current = _desired;
                        PlayUnpackSound();
                        PlayAnnouncementSound();
                        SetStateChangeCompleteFrame(context.LogicFrame.Value);
                    }
                    break;
                case BattlePlanUpdateState.Activating:
                    if (context.LogicFrame.Value >= _stateChangeCompleteFrame)
                    {
                        _state = BattlePlanUpdateState.Active;
                        // todo: apply bonuses (unit + structure)
                    }
                    break;
                case BattlePlanUpdateState.Active:
                    if (_current != _desired)
                    {
                        // if the desired state was changed while activating, we need to change again once activation is complete
                        _state = BattlePlanUpdateState.Deactivating;
                        // DisableAffectedUnits(); // todo: this currently causes an exception due to modifying the gameobject collection, but it's unclear why
                        PlayPackSound();
                        SetStateChangeCompleteFrame(context.LogicFrame.Value);
                    }
                    break;
                case BattlePlanUpdateState.Deactivating:
                    if (context.LogicFrame.Value >= _stateChangeCompleteFrame)
                    {
                        _state = BattlePlanUpdateState.None;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DisableAffectedUnits()
        {
            var disabledUntilFrame = _context.GameLogic.CurrentFrame.Value + FramesForMs(_moduleData.BattlePlanChangeParalyzeTime);
            // if deactivating, set disabled_paralyzed to affected units based on kind, frames is current frame + property
            foreach (var gameObject in _context.GameObjects.Items)
            {
                if (gameObject.Owner == _gameObject.Owner && // we must own the object
                    gameObject.Definition.KindOf.Intersects(_moduleData.ValidMemberKindOf) && // and it should be one of these kinds
                    !gameObject.Definition.KindOf.Intersects(_moduleData.InvalidMemberKindOf)) // but not any of these
                {
                    gameObject.Disable(DisabledType.Paralyzed, disabledUntilFrame);
                    // todo: remove bonuses (unit + structure)
                }
            }
        }

        private void PlayUnpackSound()
        {
            var sound = _current switch
            {
                BattlePlanType.Bombardment => _moduleData.BombardmentPlanUnpackSound,
                BattlePlanType.HoldTheLine => _moduleData.HoldTheLinePlanUnpackSound,
                BattlePlanType.SearchAndDestroy => _moduleData.SearchAndDestroyPlanUnpackSound,
                _ => null,
            };
            _context.AudioSystem.PlayAudioEvent(sound?.Value);
        }

        private void PlayPackSound()
        {
            var sound = _current switch
            {
                BattlePlanType.Bombardment => _moduleData.BombardmentPlanPackSound,
                BattlePlanType.HoldTheLine => _moduleData.HoldTheLinePlanPackSound,
                BattlePlanType.SearchAndDestroy => _moduleData.SearchAndDestroyPlanPackSound,
                _ => null,
            };
            _context.AudioSystem.PlayAudioEvent(sound?.Value);
        }

        private void PlayAnnouncementSound()
        {
            var sound = _current switch
            {
                BattlePlanType.Bombardment => _moduleData.BombardmentAnnouncement,
                BattlePlanType.HoldTheLine => _moduleData.HoldTheLineAnnouncement,
                BattlePlanType.SearchAndDestroy => _moduleData.SearchAndDestroyAnnouncement,
                _ => null,
            };
            _context.AudioSystem.PlayAudioEvent(sound?.Value);
        }

        private void SetStateChangeCompleteFrame(uint currentFrame)
        {
            var animationTime = _current switch
            {
                BattlePlanType.Bombardment => _moduleData.BombardmentPlanAnimationTime,
                BattlePlanType.HoldTheLine => _moduleData.HoldTheLinePlanAnimationTime,
                BattlePlanType.SearchAndDestroy => _moduleData.SearchAndDestroyPlanAnimationTime,
                _ => _moduleData.TransitionIdleTime,
            };
            _stateChangeCompleteFrame = currentFrame + FramesForMs(animationTime);
        }

        private readonly FrozenDictionary<BattlePlanType, DoorState> _doorStates = new Dictionary<BattlePlanType, DoorState>
        {
            [BattlePlanType.None] = new(ModelConditionFlag.None, ModelConditionFlag.None, ModelConditionFlag.None),
            [BattlePlanType.Bombardment] = new(ModelConditionFlag.Door1Opening, ModelConditionFlag.Door1WaitingToClose, ModelConditionFlag.Door1Closing),
            [BattlePlanType.HoldTheLine] = new(ModelConditionFlag.Door2Opening, ModelConditionFlag.Door2WaitingToClose, ModelConditionFlag.Door2Closing),
            [BattlePlanType.SearchAndDestroy] = new(ModelConditionFlag.Door3Opening, ModelConditionFlag.Door3WaitingToClose, ModelConditionFlag.Door3Closing),
        }.ToFrozenDictionary();

        private void SetDoorState()
        {
            var states = _doorStates[_current];
            var doorFlag = _state switch
            {
                BattlePlanUpdateState.Activating => states.Activating,
                BattlePlanUpdateState.Active => states.Active,
                BattlePlanUpdateState.Deactivating => states.Deactivating,
                _ => ModelConditionFlag.None,
            };

            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Opening, doorFlag is ModelConditionFlag.Door1Opening);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1WaitingToClose, doorFlag is ModelConditionFlag.Door1WaitingToClose);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Closing, doorFlag is ModelConditionFlag.Door1Closing);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door2Opening, doorFlag is ModelConditionFlag.Door2Opening);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door2WaitingToClose, doorFlag is ModelConditionFlag.Door2WaitingToClose);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door2Closing, doorFlag is ModelConditionFlag.Door2Closing);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door3Opening, doorFlag is ModelConditionFlag.Door3Opening);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door3WaitingToClose, doorFlag is ModelConditionFlag.Door3WaitingToClose);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door3Closing, doorFlag is ModelConditionFlag.Door3Closing);
        }

        private readonly record struct DoorState(
            ModelConditionFlag Activating,
            ModelConditionFlag Active,
            ModelConditionFlag Deactivating);

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistEnum(ref _current);
            reader.PersistEnum(ref _desired);
            reader.PersistEnum(ref _active);
            reader.PersistEnum(ref _state);

            reader.PersistUInt32(ref _stateChangeCompleteFrame);

            // unsure of this part
            reader.PersistUInt32(ref _unknownUInt2);
            reader.PersistUInt32(ref _unknownUInt3);

            // unsure of this part
            reader.SkipUnknownBytes(2);
            reader.PersistBoolean(ref _unknownBool1); // this was 1 when search and destroy was enabled
            reader.SkipUnknownBytes(3);
            reader.PersistBoolean(ref _unknownBool2);
            reader.SkipUnknownBytes(3);
            reader.PersistUInt16(ref _unknownShort1);

            // unsure of this part
            reader.PersistUInt32(ref _unknownUInt4);
            reader.SkipUnknownBytes(3);

            reader.PersistListWithByteCount(_unknownList1, ByteListPersister);
            reader.PersistListWithByteCount(_unknownList2, ByteListPersister);
            reader.PersistListWithByteCount(_unknownList3, ByteListPersister);

            // unsure of this part
            reader.PersistUInt16(ref _unknownShort2);
            reader.SkipUnknownBytes(3);

            reader.PersistListWithByteCount(_unknownList4, ByteListPersister);
            reader.PersistListWithByteCount(_unknownList5, ByteListPersister);
            reader.PersistListWithByteCount(_unknownList6, ByteListPersister);
            reader.PersistListWithByteCount(_unknownList7, ByteListPersister);

            // unsure of this part
            reader.PersistInt32(ref _unknownInt);
        }

        private static void ByteListPersister(StatePersister persister, ref byte item) => persister.PersistByteValue(ref item);

        private enum BattlePlanUpdateState
        {
            None,
            Activating,
            Active,
            Deactivating,
        }
    }

    public sealed class BattlePlanUpdateModuleData : UpdateModuleData
    {
        internal static BattlePlanUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BattlePlanUpdateModuleData> FieldParseTable = new IniParseTable<BattlePlanUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseSpecialPowerReference() },

            { "BombardmentPlanAnimationTime", (parser, x) => x.BombardmentPlanAnimationTime = parser.ParseInteger() },
            { "HoldTheLinePlanAnimationTime", (parser, x) => x.HoldTheLinePlanAnimationTime = parser.ParseInteger() },
            { "SearchAndDestroyPlanAnimationTime", (parser, x) => x.SearchAndDestroyPlanAnimationTime = parser.ParseInteger() },
            { "TransitionIdleTime", (parser, x) => x.TransitionIdleTime = parser.ParseInteger() },

            { "BombardmentMessageLabel", (parser, x) => x.BombardmentMessageLabel = parser.ParseLocalizedStringKey() },
            { "HoldTheLineMessageLabel", (parser, x) => x.HoldTheLineMessageLabel = parser.ParseLocalizedStringKey() },
            { "SearchAndDestroyMessageLabel", (parser, x) => x.SearchAndDestroyMessageLabel = parser.ParseLocalizedStringKey() },

            { "BombardmentPlanUnpackSoundName", (parser, x) => x.BombardmentPlanUnpackSound = parser.ParseAudioEventReference() },
            { "BombardmentPlanPackSoundName", (parser, x) => x.BombardmentPlanPackSound = parser.ParseAudioEventReference() },
            { "BombardmentAnnouncementName", (parser, x) => x.BombardmentAnnouncement = parser.ParseAudioEventReference() },
            { "SearchAndDestroyPlanUnpackSoundName", (parser, x) => x.SearchAndDestroyPlanUnpackSound = parser.ParseAudioEventReference() },
            { "SearchAndDestroyPlanIdleLoopSoundName", (parser, x) => x.SearchAndDestroyPlanIdleLoopSound = parser.ParseAudioEventReference() },
            { "SearchAndDestroyPlanPackSoundName", (parser, x) => x.SearchAndDestroyPlanPackSound = parser.ParseAudioEventReference() },
            { "SearchAndDestroyAnnouncementName", (parser, x) => x.SearchAndDestroyAnnouncement = parser.ParseAudioEventReference() },
            { "HoldTheLinePlanUnpackSoundName", (parser, x) => x.HoldTheLinePlanUnpackSound = parser.ParseAudioEventReference() },
            { "HoldTheLinePlanPackSoundName", (parser, x) => x.HoldTheLinePlanPackSound = parser.ParseAudioEventReference() },
            { "HoldTheLineAnnouncementName", (parser, x) => x.HoldTheLineAnnouncement = parser.ParseAudioEventReference() },

            { "ValidMemberKindOf", (parser, x) => x.ValidMemberKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "InvalidMemberKindOf", (parser, x) => x.InvalidMemberKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "BattlePlanChangeParalyzeTime", (parser, x) => x.BattlePlanChangeParalyzeTime = parser.ParseInteger() },
            { "HoldTheLinePlanArmorDamageScalar", (parser, x) => x.HoldTheLinePlanArmorDamageScalar = parser.ParseFloat() },
            { "SearchAndDestroyPlanSightRangeScalar", (parser, x) => x.SearchAndDestroyPlanSightRangeScalar = parser.ParseFloat() },

            { "StrategyCenterSearchAndDestroySightRangeScalar", (parser, x) => x.StrategyCenterSearchAndDestroySightRangeScalar = parser.ParseFloat() },
            { "StrategyCenterSearchAndDestroyDetectsStealth", (parser, x) => x.StrategyCenterSearchAndDestroyDetectsStealth = parser.ParseBoolean() },
            { "StrategyCenterHoldTheLineMaxHealthScalar", (parser, x) => x.StrategyCenterHoldTheLineMaxHealthScalar = parser.ParseFloat() },
            { "StrategyCenterHoldTheLineMaxHealthChangeType", (parser, x) => x.StrategyCenterHoldTheLineMaxHealthChangeType = parser.ParseEnum<ChangeType>() },

            { "VisionObjectName", (parser, x) => x.VisionObjectName = parser.ParseAssetReference() }
        };

        public LazyAssetReference<SpecialPower> SpecialPowerTemplate { get; private set; }

        // Transition times
        public int BombardmentPlanAnimationTime { get; private set; }
        public int HoldTheLinePlanAnimationTime { get; private set; }
        public int SearchAndDestroyPlanAnimationTime { get; private set; }
        public int TransitionIdleTime { get; private set; }

        // Messages
        public string BombardmentMessageLabel { get; private set; }
        public string HoldTheLineMessageLabel { get; private set; }
        public string SearchAndDestroyMessageLabel { get; private set; }

        // Sounds
        public LazyAssetReference<BaseAudioEventInfo> BombardmentPlanUnpackSound { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> BombardmentPlanPackSound { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> BombardmentAnnouncement { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> SearchAndDestroyPlanUnpackSound { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> SearchAndDestroyPlanIdleLoopSound { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> SearchAndDestroyPlanPackSound { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> SearchAndDestroyAnnouncement { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> HoldTheLinePlanUnpackSound { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> HoldTheLinePlanPackSound { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> HoldTheLineAnnouncement { get; private set; }

        // Army bonuses
        public BitArray<ObjectKinds> ValidMemberKindOf { get; private set; } = new();
        public BitArray<ObjectKinds> InvalidMemberKindOf { get; private set; } = new();
        public int BattlePlanChangeParalyzeTime { get; private set; }
        public float HoldTheLinePlanArmorDamageScalar { get; private set; }
        public float SearchAndDestroyPlanSightRangeScalar { get; private set; }

        // Building bonuses
        public float StrategyCenterSearchAndDestroySightRangeScalar { get; private set; }
        public bool StrategyCenterSearchAndDestroyDetectsStealth { get; private set; }
        public float StrategyCenterHoldTheLineMaxHealthScalar { get; private set; }
        public ChangeType StrategyCenterHoldTheLineMaxHealthChangeType { get; private set; }

        // Revealing
        public string VisionObjectName { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new BattlePlanUpdate(gameObject, context, this);
        }
    }

    public enum ChangeType
    {
        [IniEnum("PRESERVE_RATIO")]
        PreserveRatio
    }

    public enum BattlePlanType
    {
        None,
        Bombardment,
        HoldTheLine,
        SearchAndDestroy,
    }
}
