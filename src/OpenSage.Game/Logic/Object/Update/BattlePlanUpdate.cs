using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using FixedMath.NET;
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

        private LogicFrame _stateChangeCompleteFrame; // the frame at which the current state change will be completed

        private bool _bombardmentActive;
        private bool _holdTheLineActive;
        private bool _searchAndDestroyActive;
        private float _activeArmorDamageScalar = 1; // 0.9 when hold the line is active
        private float _activeSightRangeScalar = 1; // 1.2 when search and destroy is active

        private BitArray<ObjectKinds> _validMemberKindOf = new();
        private BitArray<ObjectKinds> _invalidMemberKindOf = new();

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
            _gameObject.Owner.InitializeStrategyData(_validMemberKindOf, _invalidMemberKindOf);
        }

        internal override void OnDie(BehaviorUpdateContext context, DeathType deathType, BitArray<ObjectStatus> status)
        {
            ClearActiveBattlePlan();
            // todo: remove unit bonuses (but don't disable units)
            base.OnDie(context, deathType, status);
        }

        private protected override void RunUpdate(BehaviorUpdateContext context)
        {
            SetDoorState();

            switch (_state)
            {
                case BattlePlanUpdateState.None:
                    if (_desired is not BattlePlanType.None && context.LogicFrame >= _stateChangeCompleteFrame)
                    {
                        // the state has been changed
                        _state = BattlePlanUpdateState.Activating;
                        _current = _desired;
                        PlayUnpackSound();
                        PlayAnnouncementSound();
                        SetStateChangeCompleteFrame(context.LogicFrame);
                    }
                    break;
                case BattlePlanUpdateState.Activating:
                    if (context.LogicFrame >= _stateChangeCompleteFrame)
                    {
                        _state = BattlePlanUpdateState.Active;
                        ActivateCurrentBattlePlan();
                        // todo: apply unit bonuses
                    }
                    break;
                case BattlePlanUpdateState.Active:
                    if (_current != _desired)
                    {
                        // if the desired state was changed while activating, we need to change again once activation is complete
                        _state = BattlePlanUpdateState.Deactivating;
                        // DisableAffectedUnits(); // todo: this currently causes an exception due to modifying the gameobject collection, but it's unclear why
                        PlayPackSound();
                        SetStateChangeCompleteFrame(context.LogicFrame);
                        ClearActiveBattlePlan();
                    }
                    break;
                case BattlePlanUpdateState.Deactivating:
                    if (context.LogicFrame >= _stateChangeCompleteFrame)
                    {
                        _state = BattlePlanUpdateState.None;
                        _current = BattlePlanType.None;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ClearActiveBattlePlan()
        {
            _bombardmentActive = false;
            _holdTheLineActive = false;
            _searchAndDestroyActive = false;
            _activeArmorDamageScalar = 1;
            _activeSightRangeScalar = 1;
            _gameObject.Owner.ClearBattlePlan();

            switch (_current)
            {
                case BattlePlanType.None:
                case BattlePlanType.Bombardment:
                    break;
                case BattlePlanType.HoldTheLine:
                    RemoveHoldTheLineMaxHealthScalar();
                    break;
                case BattlePlanType.SearchAndDestroy:
                    RemoveSearchAndDestroyVisionScalar();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ActivateCurrentBattlePlan()
        {
            _bombardmentActive = _current is BattlePlanType.Bombardment;
            _holdTheLineActive = _current is BattlePlanType.HoldTheLine;
            _searchAndDestroyActive = _current is BattlePlanType.SearchAndDestroy;
            _activeArmorDamageScalar = _current is BattlePlanType.HoldTheLine ? _moduleData.HoldTheLinePlanArmorDamageScalar : 1;
            _activeSightRangeScalar = _current is BattlePlanType.SearchAndDestroy ? _moduleData.SearchAndDestroyPlanSightRangeScalar : 1;
            _gameObject.Owner.SetActiveBattlePlan(_current, _activeArmorDamageScalar, _activeSightRangeScalar);

            switch (_current)
            {
                case BattlePlanType.None:
                case BattlePlanType.Bombardment:
                    break;
                case BattlePlanType.HoldTheLine:
                    AddHoldTheLineMaxHealthScalar();
                    break;
                case BattlePlanType.SearchAndDestroy:
                    AddSearchAndDestroyVisionScalar();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddHoldTheLineMaxHealthScalar()
        {
            var maxHealthScalar = (Fix64)_moduleData.StrategyCenterHoldTheLineMaxHealthScalar;
            var newMaxHealth = _gameObject.MaxHealth * maxHealthScalar;
            _gameObject.Health = _gameObject.HealthPercentage * newMaxHealth;
            _gameObject.MaxHealth = newMaxHealth;
        }

        private void RemoveHoldTheLineMaxHealthScalar()
        {
            var maxHealthScalar = (Fix64)_moduleData.StrategyCenterHoldTheLineMaxHealthScalar;
            var newMaxHealth = _gameObject.MaxHealth / maxHealthScalar;
            _gameObject.Health = _gameObject.HealthPercentage * newMaxHealth;
            _gameObject.MaxHealth = newMaxHealth;
        }

        private void AddSearchAndDestroyVisionScalar()
        {
            _gameObject.FindBehavior<StealthDetectorUpdate>().Active = _moduleData.StrategyCenterSearchAndDestroyDetectsStealth;
            _gameObject.ApplyVisionRangeScalar(_moduleData.StrategyCenterSearchAndDestroySightRangeScalar);
        }

        private void RemoveSearchAndDestroyVisionScalar()
        {
            _gameObject.FindBehavior<StealthDetectorUpdate>().Active = false;
            _gameObject.RemoveVisionRangeScalar(_moduleData.StrategyCenterSearchAndDestroySightRangeScalar);
        }

        private void DisableAffectedUnits()
        {
            var disabledUntilFrame = _context.GameLogic.CurrentFrame + _moduleData.BattlePlanChangeParalyzeTime;
            // if deactivating, set disabled_paralyzed to affected units based on kind, frames is current frame + property
            foreach (var gameObject in _context.GameLogic.Objects)
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

        private void SetStateChangeCompleteFrame(LogicFrame currentFrame)
        {
            var animationTime = _current switch
            {
                BattlePlanType.Bombardment => _moduleData.BombardmentPlanAnimationTime,
                BattlePlanType.HoldTheLine => _moduleData.HoldTheLinePlanAnimationTime,
                BattlePlanType.SearchAndDestroy => _moduleData.SearchAndDestroyPlanAnimationTime,
                _ => _moduleData.TransitionIdleTime,
            };
            _stateChangeCompleteFrame = currentFrame + animationTime;
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

            reader.PersistLogicFrame(ref _stateChangeCompleteFrame);

            reader.SkipUnknownBytes(2);

            reader.PersistSingle(ref _activeArmorDamageScalar); // 0.9 when hold the line is active

            // unsure of this part
            reader.PersistBoolean(ref _bombardmentActive); // this was 1 when bombardment was active
            reader.SkipUnknownBytes(3);
            reader.PersistBoolean(ref _searchAndDestroyActive); // this was 1 when search and destroy was enabled
            reader.SkipUnknownBytes(3);
            reader.PersistBoolean(ref _holdTheLineActive); // this was 1 when hold the line was active
            reader.SkipUnknownBytes(3);

            reader.PersistSingle(ref _activeSightRangeScalar);

            reader.PersistBitArray(ref _validMemberKindOf);
            reader.PersistBitArray(ref _invalidMemberKindOf);

            // unsure of this part
            reader.PersistInt32(ref _unknownInt);
            if (_unknownInt != 0 && _unknownInt != 11)
            {
                // 0 before any strategy had been selected, 11 after (even before it was fully active)
                throw new InvalidStateException();
            }
        }

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

            { "BombardmentPlanAnimationTime", (parser, x) => x.BombardmentPlanAnimationTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "HoldTheLinePlanAnimationTime", (parser, x) => x.HoldTheLinePlanAnimationTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "SearchAndDestroyPlanAnimationTime", (parser, x) => x.SearchAndDestroyPlanAnimationTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "TransitionIdleTime", (parser, x) => x.TransitionIdleTime = parser.ParseTimeMillisecondsToLogicFrames() },

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
            { "BattlePlanChangeParalyzeTime", (parser, x) => x.BattlePlanChangeParalyzeTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "HoldTheLinePlanArmorDamageScalar", (parser, x) => x.HoldTheLinePlanArmorDamageScalar = parser.ParseFloat() },
            { "SearchAndDestroyPlanSightRangeScalar", (parser, x) => x.SearchAndDestroyPlanSightRangeScalar = parser.ParseFloat() },

            { "StrategyCenterSearchAndDestroySightRangeScalar", (parser, x) => x.StrategyCenterSearchAndDestroySightRangeScalar = parser.ParseFloat() },
            { "StrategyCenterSearchAndDestroyDetectsStealth", (parser, x) => x.StrategyCenterSearchAndDestroyDetectsStealth = parser.ParseBoolean() },
            { "StrategyCenterHoldTheLineMaxHealthScalar", (parser, x) => x.StrategyCenterHoldTheLineMaxHealthScalar = parser.ParseFloat() },
            { "StrategyCenterHoldTheLineMaxHealthChangeType", (parser, x) => x.StrategyCenterHoldTheLineMaxHealthChangeType = parser.ParseEnum<MaxHealthChangeType>() },

            { "VisionObjectName", (parser, x) => x.VisionObjectName = parser.ParseObjectReference() }
        };

        public LazyAssetReference<SpecialPower> SpecialPowerTemplate { get; private set; }

        // Transition times
        public LogicFrameSpan BombardmentPlanAnimationTime { get; private set; }
        public LogicFrameSpan HoldTheLinePlanAnimationTime { get; private set; }
        public LogicFrameSpan SearchAndDestroyPlanAnimationTime { get; private set; }
        public LogicFrameSpan TransitionIdleTime { get; private set; }

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
        public LogicFrameSpan BattlePlanChangeParalyzeTime { get; private set; }
        public float HoldTheLinePlanArmorDamageScalar { get; private set; }
        public float SearchAndDestroyPlanSightRangeScalar { get; private set; }

        // Building bonuses
        public float StrategyCenterSearchAndDestroySightRangeScalar { get; private set; }
        public bool StrategyCenterSearchAndDestroyDetectsStealth { get; private set; }
        public float StrategyCenterHoldTheLineMaxHealthScalar { get; private set; }
        public MaxHealthChangeType StrategyCenterHoldTheLineMaxHealthChangeType { get; private set; }

        // Revealing
        public LazyAssetReference<ObjectDefinition> VisionObjectName { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new BattlePlanUpdate(gameObject, context, this);
        }
    }

    public enum BattlePlanType
    {
        None,
        Bombardment,
        HoldTheLine,
        SearchAndDestroy,
    }
}
