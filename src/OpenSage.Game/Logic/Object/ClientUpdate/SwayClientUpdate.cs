using System;
using System.Numerics;
using ImGuiNET;
using OpenSage.Client;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;
using OpenSage.Utilities;

namespace OpenSage.Logic.Object;

/// <summary>
/// The tree sway client update module.
/// </summary>
public sealed class SwayClientUpdate : ClientUpdateModule
{
    private readonly Drawable _drawable;
    private readonly GameEngine _gameEngine;

    private float _currentValue;
    private float _currentAngle;
    private float _currentDelta;
    private float _currentAngleLimit;
    private float _leanAngle;
    private short _currentVersion = -1; // So we don't match the first time
    private bool _swaying = true;

    internal SwayClientUpdate(Drawable drawable, GameEngine gameEngine)
    {
        _drawable = drawable;
        _gameEngine = gameEngine;
    }

    public override void ClientUpdate(in TimeInterval gameTime)
    {
        if (!_swaying)
        {
            return;
        }

        var draw = _drawable;

        // If breeze changes, always process the full update, even if not visible,
        // so that things offscreen won't "pop" when first viewed.
        ref readonly var breezeInfo = ref _gameEngine.Game.Scripting.BreezeInfo;
        if (breezeInfo.BreezeVersion != _currentVersion)
        {
            UpdateSway();
        }
        else if (!_drawable.IsVisible) // Otherwise, only update visible drawables.
        {
            return;
        }

        _currentValue += (float)(_currentDelta * gameTime.GetLogicFrameRelativeDeltaTime());
        if (_currentValue > MathUtility.TwoPi)
        {
            _currentValue -= MathUtility.TwoPi;
        }

        var cosine = MathF.Cos(_currentValue);

        var targetAngle = (cosine * _currentAngleLimit) + _leanAngle;
        var deltaAngle = targetAngle - _currentAngle;

        draw.InstanceMatrix =
            Matrix4x4.CreateRotationX(deltaAngle * breezeInfo.DirectionVector.X) *
            Matrix4x4.CreateRotationY(deltaAngle * breezeInfo.DirectionVector.Y) *
            _drawable.InstanceMatrix;

        _currentAngle = targetAngle;

        // Burned things don't sway.
        if (_drawable.GameObject.TestStatus(ObjectStatus.Burned))
        {
            StopSway();
        }
    }

    // TODO(Port): Actually call this.
    protected override void LoadPostProcess()
    {
        base.LoadPostProcess();

        UpdateSway();
    }

    public void StopSway()
    {
        _swaying = false;
    }

    /// <summary>
    /// Updates the sway parameters.
    /// </summary>
    private void UpdateSway()
    {
        ref readonly var breezeInfo = ref _gameEngine.Game.Scripting.BreezeInfo;

        if (breezeInfo.Randomness == 0.0f)
        {
            _currentValue = 0;
        }

        var delta = breezeInfo.Randomness * 0.5f;

        _currentAngleLimit = breezeInfo.Intensity * _gameEngine.GameClient.Random.NextSingle(1.0f - delta, 1.0f + delta);
        _currentDelta = 2.0f * MathF.PI / breezeInfo.BreezePeriod * _gameEngine.GameClient.Random.NextSingle(1.0f - delta, 1.0f + delta);
        _leanAngle = breezeInfo.Lean * _gameEngine.GameClient.Random.NextSingle(1.0f - delta, 1.0f + delta);
        _currentVersion = breezeInfo.BreezeVersion;
    }

    internal override void DrawInspector()
    {
        base.DrawInspector();

        ImGui.InputFloat("Current value", ref _currentValue);
        ImGui.InputFloat("Current angle", ref _currentAngle);
        ImGui.InputFloat("Current delta", ref _currentDelta);
        ImGui.InputFloat("Current angle limit", ref _currentAngleLimit);
        ImGui.InputFloat("Lean angle", ref _leanAngle);
        ImGui.LabelText("Current version", _currentVersion.ToString());
        ImGui.Checkbox("Swaying", ref _swaying);
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistSingle(ref _currentValue);
        reader.PersistSingle(ref _currentAngle);
        reader.PersistSingle(ref _currentDelta);
        reader.PersistSingle(ref _currentAngleLimit);
        reader.PersistSingle(ref _leanAngle);
        reader.PersistInt16(ref _currentVersion);
        reader.PersistBoolean(ref _swaying);
    }
}

/// <summary>
/// Allows the object to sway if enabled in GameData.INI or allowed by LOD/map specific settings.
/// </summary>
public sealed class SwayClientUpdateModuleData : ClientUpdateModuleData
{
    internal static SwayClientUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<SwayClientUpdateModuleData> FieldParseTable = new IniParseTable<SwayClientUpdateModuleData>();

    internal override ClientUpdateModule CreateModule(Drawable drawable, GameEngine gameEngine)
    {
        return new SwayClientUpdate(drawable, gameEngine);
    }
}
