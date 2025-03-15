using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Diagnostics.Util;
using OpenSage.FX;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class ToppleUpdate : UpdateModule, ICollideModule
{
    /// <summary>
    /// This is our "bounce" limit. Slightly less than 90 degrees, to account for slop.
    /// </summary>
    private const float AngularLimit = MathUtility.PiOver2 - (MathF.PI / 64.0f);

    private readonly ToppleUpdateModuleData _moduleData;

    /// <summary>
    /// Stage this module is in.
    /// </summary>
    private ToppleState _toppleState;

    /// <summary>
    /// Amount by which _angularVelocity is increasing each frame.
    /// </summary>
    private float _angularAcceleration;

    /// <summary>
    /// Velocity in degrees per frame (or is it radians per frame?).
    /// </summary>
    private float _angularVelocity;

    /// <summary>
    /// Z-less direction we are toppling.
    /// </summary>
    private Vector3 _toppleDirection;

    /// <summary>
    /// How much have I rotated so I know when to bounce.
    /// </summary>
    private float _angularAccumulation;

    /// <summary>
    /// How much to modify x each frame.
    /// </summary>
    private float _angleDeltaX;

    /// <summary>
    /// How many frames to tweak x angle.
    /// </summary>
    private int _numAngleDeltaX;

    /// <summary>
    /// Do the bounce FX if we do bounce.
    /// </summary>
    private bool _doBounceFX;

    /// <summary>
    /// Topple options.
    /// </summary>
    private ToppleOptions _options;

    /// <summary>
    /// Stump generated, if any.
    /// </summary>
    private uint _stumpId;

    internal bool IsAbleToBeToppled => _toppleState == ToppleState.Upright;

    internal ToppleUpdate(GameObject gameObject, GameContext context, ToppleUpdateModuleData moduleData)
        : base(gameObject, context)
    {
        _moduleData = moduleData;

        _toppleState = ToppleState.Upright;

        SetWakeFrame(UpdateSleepTime.Forever);
    }

    /// <summary>
    /// Keeps track of rotational fall distance and bounce, and stops when needed.
    /// </summary>
    public override UpdateSleepTime Update()
    {
        Debug.Assert(_toppleState != ToppleState.Upright, "Hmm, we should we sleeping here");
        if (_toppleState == ToppleState.Upright || _toppleState == ToppleState.Down)
        {
            return UpdateSleepTime.Forever;
        }

        // If the velocity after a bounce will be this or lower, just stop at zero.
        const float velocityBounceLimit = 0.01f;

        // And if this is low, then skip the bounce sound.
        const float velocityBounceSoundLimit = 0.03f;

        var obj = GameObject;
        if (_numAngleDeltaX > 0)
        {
            obj.SetTransformMatrix(Matrix4x4.CreateRotationZ(_angleDeltaX) * obj.TransformMatrix);
            _numAngleDeltaX--;
        }

        var curVelToUse = _angularVelocity;
        if (_angularAccumulation + curVelToUse > AngularLimit)
        {
            curVelToUse = AngularLimit - _angularAccumulation;
        }

        obj.SetTransformMatrix(
            Matrix4x4.CreateRotationX(-curVelToUse * _toppleDirection.Y) *
            Matrix4x4.CreateRotationY(curVelToUse * _toppleDirection.X) *
            obj.TransformMatrix);

        _angularAccumulation += curVelToUse;
        if (_angularAccumulation >= AngularLimit && _angularVelocity > 0)
        {
            // Hit, so either bounce or stop if too little remaining velocity.
            _angularVelocity *= -_moduleData.BounceVelocityPercent;

            if ((_options & ToppleOptions.NoBounce) != 0 ||
                MathF.Abs(_angularVelocity) < velocityBounceLimit)
            {
                // Too slow, just stop.
                _angularVelocity = 0;
                _toppleState = ToppleState.Down;

                if (_moduleData.KillWhenFinishedToppling)
                {
                    DeathByToppling(obj);
                    if (_moduleData.ReorientToppledRubble)
                    {
                        // We have a separate rubble state that needs to be
                        // upright, and centered on the new "center" pos...
                        var pos = Vector3.Transform(
                            new Vector3(0, 0, obj.Geometry.MaxZ),
                            obj.TransformMatrix);
                        obj.SetTranslation(pos);

                        // This relies on the fact that SetOrientation always
                        // forces us straight up in the Z axis!
                        obj.SetOrientation(obj.Yaw);

                    }
                }

                if (_moduleData.KillStumpWhenToppled)
                {
                    var stump = Context.GameLogic.GetObjectById(_stumpId);
                    if (stump != null)
                    {
                        DeathByToppling(stump);
                    }
                }
            }
            else if (MathF.Abs(_angularVelocity) >= velocityBounceSoundLimit)
            {
                // Fast enough bounce to warrant the bounce fx?
                if ((_options & ToppleOptions.NoFX) == 0)
                {
                    _moduleData.BounceFX?.Value.Execute(
                        new FXListExecutionContext(
                            GameObject.Rotation,
                            GameObject.Translation,
                            Context));
                }
            }
        }
        else
        {
            _angularVelocity += _angularAcceleration;
        }

        GameObject.Drawable.ShadowsEnabled = false;

        return UpdateSleepTime.None;
    }

    private static void DeathByToppling(GameObject obj)
    {
        // Use a special "toppling" damage type here so that toppled stuff can
        // have different damage/die modules for toppled-death vs other-death.
        var damageInfo = new DamageData();
        damageInfo.Request.DamageType = DamageType.Unresistable;
        damageInfo.Request.DeathType = DeathType.Toppled;
        damageInfo.Request.DamageDealer = 0;
        damageInfo.Request.DamageToDeal = DamageConstants.HugeDamageAmount;
        obj.AttemptDamage(ref damageInfo);
    }

    public void OnCollide(GameObject other, in Vector3 location, in Vector3 normal)
    {
        // If we've already started toppling, don't do anything.
        if (_toppleState != ToppleState.Upright)
        {
            return;
        }

        // Note that other == null means "collide with ground"
        if (other == null)
        {
            return;
        }

        // TODO from the original game:
        // @todo JohnA -- Should you get around to adding trees to avoidance pathfinding, then you'll
        // want to change this code:
        // if (other.CrusherLevel > GameObject.CrushableLevel) // <----proper tree method
        if (other.CrusherLevel > 1)
        {
            // Give a vector with direction to thing and my speed.
            var toppleVector = GameObject.Translation;
            toppleVector.X -= other.Translation.X;
            toppleVector.Y -= other.Translation.Y;
            toppleVector.Z = 0;

            var velocity = other.Physics?.Velocity ?? Vector3.Zero;

            // This will call back into our ApplyTopplingForce method.
            GameObject.Topple(toppleVector, velocity.Length(), ToppleOptions.None);
        }
    }

    internal void ApplyTopplingForce(in Vector3 toppleDirection, float toppleSpeed, ToppleOptions options)
    {
        if (GameObject.IsEffectivelyDead)
        {
            return;
        }

        SetWakeFrame(UpdateSleepTime.None);

        if (_moduleData.KillWhenStartToppling)
        {
            SetWakeFrame(UpdateSleepTime.Forever);
            GameObject.Kill();
            return;
        }

        _toppleDirection = Vector3.Normalize(toppleDirection);

        Context.Game.Scripting.AdjustToppleDirection(GameObject, _toppleDirection);

        _angularVelocity = toppleSpeed * _moduleData.InitialVelocityPercent;
        _angularAcceleration = toppleSpeed * _moduleData.InitialAccelPercent;
        _toppleState = ToppleState.Falling;
        _options = options;

        // Tell the drawable to stop swaying.
        GameObject.Drawable.FindClientUpdateModule<SwayClientUpdate>()?.StopSway();

        // Rotate around the z-axis so that our x-axis is perpendicular to the topple direction.
        // This is really a trick to ensure that relatively planar things (like street lights)
        // fall parallel to the ground, so that they don't end up sticking through the ground.
        // Yeah, it assumes the models are constructed appropriately, but is a cheap way of
        // minimizing the problem.
        var currentAngleX = MathUtility.NormalizeAngle(GameObject.Yaw);
        var toppleAngle = MathUtility.NormalizeAngle(MathF.Atan2(_toppleDirection.Y, _toppleDirection.X));
        if (_moduleData.ToppleLeftOrRightOnly)
        {
            // It's a fence or such, and can only topple left or right, so pick the closest.
            toppleAngle = AngleClosestTo(
                currentAngleX + MathUtility.PiOver2,
                currentAngleX - MathUtility.PiOver2,
                toppleAngle);
            _toppleDirection.X = MathF.Cos(toppleAngle);
            _toppleDirection.Y = MathF.Sin(toppleAngle);

            // Go ahead and remove it from the pathfinder now, rather than
            // waiting for the topple to finish... since we might be in a
            // slightly different position when toppled, which can confuse
            // the pathfinder and not de-obstacle everything correctly.
            Context.AI.Pathfinder.RemoveObjectFromPathfindMap(GameObject);
        }

        // Desired angle is toppleAngle +/- PI/2, whichever is cloesr to currentAngle.
        var desiredAngleX = AngleClosestTo(
            toppleAngle + MathUtility.PiOver2,
            toppleAngle - MathUtility.PiOver2,
            currentAngleX);
        _numAngleDeltaX = MathUtility.FloorToInt(AngularLimit / (_angularVelocity * 2));
        if (_numAngleDeltaX < 1)
        {
            _numAngleDeltaX = 1;
        }
        _angleDeltaX = (desiredAngleX - currentAngleX) / _numAngleDeltaX;

        GameObject.SetModelConditionState(ModelConditionFlag.Toppled);

        _moduleData.ToppleFX?.Value.Execute(
            new FXListExecutionContext(
                GameObject.Rotation,
                GameObject.Translation,
                Context));

        // If this is a tree, create a stump.
        if (_moduleData.StumpName != null)
        {
            var stump = Context.GameLogic.CreateObject(_moduleData.StumpName.Value, null);
            stump.UpdateTransform(GameObject.Translation, GameObject.Rotation);
            _stumpId = stump.ID;

            // If we are "burned", then we will burn our stump.
            if (GameObject.ModelConditionFlags.Get(ModelConditionFlag.Burned))
            {
                stump.SetModelConditionState(ModelConditionFlag.Burned);
            }
        }
    }

    private static float AngleClosestTo(float a1, float a2, float desired)
    {
        a1 = MathUtility.NormalizeAngle(a1);
        a2 = MathUtility.NormalizeAngle(a2);

        var deltaA1 = MathF.Abs(MathUtility.CalculateAngleDelta(desired, a1));
        var deltaA2 = MathF.Abs(MathUtility.CalculateAngleDelta(desired, a2));

        return deltaA1 < deltaA2
            ? a1
            : a2;
    }

    internal override void DrawInspector()
    {
        base.DrawInspector();

        ImGui.InputFloat("Angular velocity", ref _angularVelocity);
        ImGui.InputFloat("Angular acceleration", ref _angularAcceleration);
        ImGui.InputFloat3("Topple direction", ref _toppleDirection);
        ImGuiUtility.ComboEnum("Topple state", ref _toppleState);
        ImGui.InputFloat("Angular accumulation", ref _angularAccumulation);
        ImGui.InputFloat("Angle delta X", ref _angleDeltaX);
        ImGui.InputInt("Num angle delta X", ref _numAngleDeltaX);
        ImGui.Checkbox("Do bounce FX", ref _doBounceFX);
        ImGuiUtility.ComboEnum("Options", ref _options);
        ImGui.LabelText("Stump", _stumpId.ToString());
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistSingle(ref _angularVelocity);
        reader.PersistSingle(ref _angularAcceleration);
        reader.PersistVector3(ref _toppleDirection);
        reader.PersistEnum(ref _toppleState);
        reader.PersistSingle(ref _angularAccumulation);
        reader.PersistSingle(ref _angleDeltaX);
        reader.PersistInt32(ref _numAngleDeltaX);
        reader.PersistBoolean(ref _doBounceFX);
        reader.PersistEnumFlags(ref _options);
        reader.PersistUInt32(ref _stumpId);
    }

    private enum ToppleState
    {
        Upright = 0,
        Falling = 1,
        Down = 2,
    }
}

[Flags]
public enum ToppleOptions
{
    None = 0,

    /// <summary>
    /// Do not bounce when hit the ground.
    /// </summary>
    NoBounce = 0x1,

    /// <summary>
    /// Do not play any FX when hit the ground.
    /// </summary>
    NoFX = 0x2,
}

public sealed class ToppleUpdateModuleData : UpdateModuleData
{
    private const float StartVelocityPercent = 0.2f;
    private const float StartAccelerationPercent = 0.01f;

    /// <summary>
    /// Multiply the velocity by this when you bounce.
    /// </summary>
    private const float VelocityBouncePercent = 0.3f;

    internal static ToppleUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<ToppleUpdateModuleData> FieldParseTable = new IniParseTable<ToppleUpdateModuleData>
    {
        { "ToppleFX", (parser, x) => x.ToppleFX = parser.ParseFXListReference() },
        { "BounceFX", (parser, x) => x.BounceFX = parser.ParseFXListReference() },
        { "KillWhenStartToppling", (parser, x) => x.KillWhenStartToppling = parser.ParseBoolean() },
        { "KillWhenFinishedToppling", (parser, x) => x.KillWhenFinishedToppling = parser.ParseBoolean() },
        { "KillStumpWhenToppled", (parser, x) => x.KillStumpWhenToppled = parser.ParseBoolean() },
        { "ToppleLeftOrRightOnly", (parser, x) => x.ToppleLeftOrRightOnly = parser.ParseBoolean() },
        { "ReorientToppledRubble", (parser, x) => x.ReorientToppledRubble = parser.ParseBoolean() },
        { "BounceVelocityPercent", (parser, x) => x.BounceVelocityPercent = parser.ParsePercentage() },
        { "InitialAccelPercent", (parser, x) => x.InitialAccelPercent = parser.ParsePercentage() },
        { "StumpName", (parser, x) => x.StumpName = parser.ParseObjectReference() },
    };

    public LazyAssetReference<FXList> ToppleFX { get; private set; }
    public LazyAssetReference<FXList> BounceFX { get; private set; }
    public bool KillWhenStartToppling { get; private set; }
    public bool KillWhenFinishedToppling { get; private set; }
    public bool KillStumpWhenToppled { get; private set; }

    /// <summary>
    /// Constrained to topple to my left or right only.
    /// </summary>
    public bool ToppleLeftOrRightOnly { get; private set; }

    public bool ReorientToppledRubble { get; private set; }
    public Percentage BounceVelocityPercent { get; private set; } = new Percentage(VelocityBouncePercent);
    public Percentage InitialVelocityPercent { get; private set; } = new Percentage(StartVelocityPercent);
    public Percentage InitialAccelPercent { get; private set; } = new Percentage(StartAccelerationPercent);
    public LazyAssetReference<ObjectDefinition> StumpName { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
    {
        return new ToppleUpdate(gameObject, context, this);
    }
}
