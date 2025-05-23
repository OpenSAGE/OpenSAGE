﻿using System;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;
using SharpAudio;

namespace OpenSage.Logic.Object;

[AddedIn(SageGame.Bfme)]
public class GateOpenAndCloseBehavior : UpdateModule
{
    private GateOpenAndCloseBehaviorModuleData _moduleData;
    private bool _open;
    private bool _playedFinishedSound = false;
    private bool _toggledColliders = false;
    private string _closedGeometry = "Closed";
    private string[] _openGeometries = { "OpenLeft", "OpenRight" };

    private DoorState _state;

    private LogicFrame _toggleFinishedTime;
    private LogicFrame _pathingToggleTime;
    private LogicFrame _finishedSoundTime;

    private AudioSource _openingSoundLoop;
    private AudioSource _closingSoundLoop;

    internal GateOpenAndCloseBehavior(GameObject gameObject, IGameEngine gameEngine, GateOpenAndCloseBehaviorModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;

        _open = false; // _moduleData.OpenByDefault;
        _state = DoorState.Idle;
    }

    public void Toggle()
    {
        switch (_state)
        {
            case DoorState.Idle:
                _state = _open ? DoorState.StartClosing : DoorState.StartOpening;
                break;
            case DoorState.StartOpening:
            case DoorState.Opening:
                _state = DoorState.StartClosing;
                break;
            case DoorState.StartClosing:
            case DoorState.Closing:
                _state = DoorState.StartOpening;
                break;
        }
    }

    public override UpdateSleepTime Update()
    {
        var audioSystem = GameEngine.AudioSystem;

        var currentFrame = GameEngine.GameLogic.CurrentFrame;

        switch (_state)
        {
            case DoorState.Idle:
                // TODO(Port): Use correct value.
                return UpdateSleepTime.None;

            case DoorState.StartOpening:
                audioSystem.DisposeSource(_closingSoundLoop);
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Closing, false);
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Opening, true);
                _toggleFinishedTime = currentFrame + _moduleData.ResetTime;
                _pathingToggleTime = currentFrame + (_moduleData.ResetTime * _moduleData.PercentOpenForPathing);

                _openingSoundLoop = audioSystem.PlayAudioEvent(GameObject, _moduleData.SoundOpeningGateLoop?.Value, true);
                _finishedSoundTime = currentFrame + _moduleData.TimeBeforePlayingOpenSound;

                _state = DoorState.Opening;
                _toggledColliders = false;
                _playedFinishedSound = false;
                break;

            case DoorState.Opening:
                if (!_toggledColliders && currentFrame >= _pathingToggleTime)
                {
                    GameObject.HideCollider(_closedGeometry);
                    foreach (var openCollider in _openGeometries)
                    {
                        GameObject.ShowCollider(openCollider);
                    }
                    _toggledColliders = true;
                }
                if (!_playedFinishedSound && currentFrame >= _finishedSoundTime)
                {
                    audioSystem.DisposeSource(_openingSoundLoop);
                    audioSystem.PlayAudioEvent(GameObject, _moduleData.SoundFinishedOpeningGate?.Value);
                    _playedFinishedSound = true;
                }
                if (currentFrame >= _toggleFinishedTime)
                {
                    _open = true;
                    _state = DoorState.Idle;
                }
                break;

            case DoorState.StartClosing:
                audioSystem.DisposeSource(_openingSoundLoop);
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Opening, false);
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Closing, true);
                _toggleFinishedTime = currentFrame + _moduleData.ResetTime;
                _pathingToggleTime = currentFrame + (_moduleData.ResetTime * _moduleData.PercentOpenForPathing);

                _closingSoundLoop = audioSystem.PlayAudioEvent(GameObject, _moduleData.SoundClosingGateLoop?.Value, true);
                _finishedSoundTime = currentFrame + _moduleData.TimeBeforePlayingClosedSound;

                _state = DoorState.Closing;
                _toggledColliders = false;
                _playedFinishedSound = false;
                break;

            case DoorState.Closing:
                if (!_toggledColliders && currentFrame >= _pathingToggleTime)
                {
                    GameObject.ShowCollider(_closedGeometry);
                    foreach (var openCollider in _openGeometries)
                    {
                        GameObject.HideCollider(openCollider);
                    }
                    _toggledColliders = true;
                }
                if (!_playedFinishedSound && currentFrame >= _finishedSoundTime)
                {
                    audioSystem.DisposeSource(_closingSoundLoop);
                    audioSystem.PlayAudioEvent(GameObject, _moduleData.SoundFinishedClosingGate?.Value);
                    _playedFinishedSound = true;
                }
                if (currentFrame >= _toggleFinishedTime)
                {
                    _open = false;
                    _state = DoorState.Idle;
                }
                break;
        }

        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }
}

enum DoorState
{
    Idle,
    StartOpening,
    Opening,
    StartClosing,
    Closing
}

public sealed class GateOpenAndCloseBehaviorModuleData : BehaviorModuleData
{
    internal static GateOpenAndCloseBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<GateOpenAndCloseBehaviorModuleData> FieldParseTable = new IniParseTable<GateOpenAndCloseBehaviorModuleData>
    {
        { "ResetTimeInMilliseconds", (parser, x) => x.ResetTime = parser.ParseTimeMillisecondsToLogicFrames() },
        { "OpenByDefault", (parser, x) => x.OpenByDefault = parser.ParseBoolean() },
        { "PercentOpenForPathing", (parser, x) => x.PercentOpenForPathing = parser.ParsePercentage() },
        { "SoundOpeningGateLoop", (parser, x) => x.SoundOpeningGateLoop = parser.ParseAudioEventReference() },
        { "SoundClosingGateLoop", (parser, x) => x.SoundClosingGateLoop = parser.ParseAudioEventReference() },
        { "SoundFinishedOpeningGate", (parser, x) => x.SoundFinishedOpeningGate = parser.ParseAudioEventReference() },
        { "SoundFinishedClosingGate", (parser, x) => x.SoundFinishedClosingGate = parser.ParseAudioEventReference() },
        { "TimeBeforePlayingOpenSound", (parser, x) => x.TimeBeforePlayingOpenSound = parser.ParseTimeMillisecondsToLogicFrames() },
        { "TimeBeforePlayingClosedSound", (parser, x) => x.TimeBeforePlayingClosedSound = parser.ParseTimeMillisecondsToLogicFrames() },
        { "Proxy", (parser, x) => x.Proxy = parser.ParseAssetReference() },
        { "RepelCollidingUnits", (parser, x) => x.RepelCollidingUnits = parser.ParseBoolean() }
    };

    public LogicFrameSpan ResetTime { get; private set; }
    public bool OpenByDefault { get; private set; }
    public Percentage PercentOpenForPathing { get; private set; }
    public LazyAssetReference<BaseAudioEventInfo> SoundOpeningGateLoop { get; private set; }
    public LazyAssetReference<BaseAudioEventInfo> SoundClosingGateLoop { get; private set; }
    public LazyAssetReference<BaseAudioEventInfo> SoundFinishedOpeningGate { get; private set; }
    public LazyAssetReference<BaseAudioEventInfo> SoundFinishedClosingGate { get; private set; }
    public LogicFrameSpan TimeBeforePlayingOpenSound { get; private set; }
    public LogicFrameSpan TimeBeforePlayingClosedSound { get; private set; }
    public string Proxy { get; private set; } // what is this?

    [AddedIn(SageGame.Bfme2)]
    public bool RepelCollidingUnits { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new GateOpenAndCloseBehavior(gameObject, gameEngine, this);
    }
}
