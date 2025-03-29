#nullable enable

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Client;

public sealed class Drawable : Entity, IPersistableObject
{
    private readonly Dictionary<string, ModuleBase> _tagToModuleLookup = new();

    private ModuleBase GetModuleByTag(string tag)
    {
        return _tagToModuleLookup[tag];
    }

    private readonly IGameEngine _gameEngine;

    private readonly List<string> _hiddenDrawModules;
    private readonly Dictionary<string, bool> _hiddenSubObjects;
    private readonly Dictionary<string, bool> _shownSubObjects;

    public override Transform Transform => GameObject?.Transform ?? base.Transform;

    public Dictionary<string, bool> ShownSubObjects => _shownSubObjects;
    public Dictionary<string, bool> HiddenSubObjects => _hiddenSubObjects;

    public readonly GameObject GameObject;
    public readonly ObjectDefinition Definition;

    public readonly IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates;

    public readonly BitArray<ModelConditionFlag> ModelConditionFlags;

    public ReadOnlySpan<DrawModule> DrawModules
    {
        get
        {
            if (ModelConditionFlags.BitsChanged)
            {
                foreach (var drawModule in _drawModules)
                {
                    drawModule.UpdateConditionState(ModelConditionFlags, _gameEngine.GameClient.Random);
                }
                ModelConditionFlags.BitsChanged = false;
            }
            return _drawModules;
        }
    }

    private readonly DrawModule[] _drawModules;

    private readonly ClientUpdateModule[] _clientUpdateModules;

    private uint _id;

    public uint ID
    {
        get => _id;
        internal set
        {
            var oldDrawableId = _id;

            _id = value;

            _gameEngine.GameClient.OnDrawableIdChanged(this, oldDrawableId);
        }
    }

    public ObjectId GameObjectID => GameObject?.Id ?? ObjectId.Invalid;

    /// <summary>
    /// For limiting tree sway, etc to visible objects.
    /// </summary>
    public bool IsVisible
    {
        get
        {
            foreach (var drawModule in _drawModules)
            {
                if (drawModule.IsVisible)
                {
                    return true;
                }
            }

            return false;
        }
    }

    // TODO(Port): Unify this, Drawable._transformMatrix, and GameObject.ModelTransform.
    public Matrix4x4 InstanceMatrix = Matrix4x4.Identity;

    public Vector3 UnitDirectionVector2D
    {
        get
        {
            // TODO: Move this to Entity base class and share with GameObject.
            var angle = Transform.Yaw;
            return new Vector3(
                MathF.Cos(angle),
                MathF.Sin(angle),
                0.0f);
        }
    }

    public Geometry DrawableGeometryInfo => GameObject?.Geometry ?? Definition.Geometry;

    // TODO(Port): Implement this.
    public bool ShadowsEnabled { get; set; }

    private Matrix4x3 _transformMatrix;

    private ColorFlashHelper? _selectionFlashHelper;
    private ColorFlashHelper? _scriptedFlashHelper;

    private ObjectDecalType _terrainDecalType;

    private float _explicitOpacity;
    private float _stealthOpacity;
    private float _effectiveStealthOpacity;
    private float _decalOpacityFadeTarget;
    private float _decalOpacityFadeRate;
    private float _decalOpacity;

    private DrawableStatus _status;
    private TintStatus _tintStatus;
    private TintStatus _previousTintStatus;
    private FadingMode _fadeMode;
    private uint _timeElapsedFade;
    private uint _timeToFade;

    private DrawableLocomotorInfo? _locomotorInfo;

    private StealthLookType _stealthLook;

    private uint _flashCount;
    private ColorRgba _flashColor;

    private bool _hidden;
    private bool _hiddenByStealth;

    private float _secondMaterialPassOpacity;

    private bool _instanceMatrixIsIdentity;
    private Matrix4x3 _instanceMatrix;
    private float _instanceScale;

    private readonly DrawableInfo _drawableInfo = new();

    private uint _expirationDate;

    private List<Animation> _animations = [];
    public IReadOnlyList<Animation> Animations => _animations;
    private readonly Dictionary<AnimationType, Animation> _animationMap = [];

    private bool _ambientSoundEnabled;
    private bool _ambientSoundEnabledFromScript;

    private Vector3? _selectionFlashColor;
    private Vector3 SelectionFlashColor => _selectionFlashColor ??=
        _gameEngine.AssetLoadContext.AssetStore.GameData.Current.SelectionFlashHouseColor
            ? GameObject.Owner.Color.ToVector3()
            : new Vector3( // the ini comments say "zero leaves color unaffected, 4.0 is purely saturated", however the value in the ini is 0.5 and value in sav files is 0.25, so either it's hardcoded or the comments are wrong and I choose configurability
                Math.Clamp(_gameEngine.AssetLoadContext.AssetStore.GameData.Current.SelectionFlashSaturationFactor / 2f, 0, 1),
                Math.Clamp(_gameEngine.AssetLoadContext.AssetStore.GameData.Current.SelectionFlashSaturationFactor / 2f, 0, 1),
                Math.Clamp(_gameEngine.AssetLoadContext.AssetStore.GameData.Current.SelectionFlashSaturationFactor / 2f, 0, 1));

    internal Drawable(ObjectDefinition objectDefinition, IGameEngine gameEngine, GameObject gameObject)
    {
        Definition = objectDefinition;
        _gameEngine = gameEngine;
        GameObject = gameObject;

        ModelConditionFlags = new BitArray<ModelConditionFlag>();

        // the casing on the object names doesn't always match
        _hiddenSubObjects = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
        _shownSubObjects = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

        var drawModules = new List<DrawModule>();
        foreach (var drawDataContainer in objectDefinition.Draws.Values)
        {
            var drawModuleData = (DrawModuleData)drawDataContainer.Data;
            var drawModule = AddDisposable(drawModuleData.CreateDrawModule(this, gameEngine));
            if (drawModule != null)
            {
                // TODO: This will never be null once we've implemented all the draw modules.
                drawModule.Tag = drawDataContainer.Tag;
                drawModules.Add(drawModule);
                _tagToModuleLookup.Add(drawDataContainer.Tag, drawModule);
                AddDisposable(drawModule);
            }
        }
        _drawModules = drawModules.ToArray();

        ModelConditionStates = _drawModules
            .SelectMany(x => x.ModelConditionStates)
            .Distinct()
            .OrderBy(x => x.NumBitsSet)
            .ToList();

        _hiddenDrawModules = new List<string>();

        var clientUpdateModules = new List<ClientUpdateModule>();
        foreach (var clientUpdateModuleDataContainer in objectDefinition.ClientUpdates.Values)
        {
            var clientUpdateModuleData = (ClientUpdateModuleData)clientUpdateModuleDataContainer.Data;
            var clientUpdateModule = AddDisposable(clientUpdateModuleData.CreateModule(this, gameEngine));
            if (clientUpdateModule != null)
            {
                // TODO: This will never be null once we've implemented all the draw modules.
                clientUpdateModule.Tag = clientUpdateModuleDataContainer.Tag;
                clientUpdateModules.Add(clientUpdateModule);
                _tagToModuleLookup.Add(clientUpdateModuleDataContainer.Tag, clientUpdateModule);
            }
        }
        _clientUpdateModules = clientUpdateModules.ToArray();
    }

    // as far as I can tell nothing like this is stored in the actual game, so I'm not sure how this was handled for network games where logic ticks weren't linked to fps (if at all - you can't save a network game, after all)
    private LogicFrame _lastSelectionFlashFrame;

    public void LogicTick(in TimeInterval gameTime)
    {
        var currentFrame = _gameEngine.GameLogic.CurrentFrame;
        if (currentFrame > _lastSelectionFlashFrame)
        {
            _lastSelectionFlashFrame = currentFrame;
            _selectionFlashHelper?.StepFrame();
        }

        foreach (var clientUpdateModule in _clientUpdateModules)
        {
            clientUpdateModule.ClientUpdate(gameTime);
        }
    }

    public void TriggerSelection()
    {
        _selectionFlashHelper ??= new ColorFlashHelper(); // this is dynamically created in generals - units don't get one until they have been selected
        _selectionFlashHelper.StartSelection(SelectionFlashColor);
    }

    public void AddAnimation(AnimationType animationName)
    {
        if (_animationMap.ContainsKey(animationName))
        {
            return;
        }

        var animationTemplate = _gameEngine.Game.AssetStore.Animations.GetByName(Animation.AnimationTypeToName(animationName));
        var animation = new Animation(animationTemplate, _gameEngine.LogicFramesPerSecond);

        _animations.Add(animation);
        _animationMap[animationName] = animation;
    }

    public void RemoveAnimation(AnimationType animationType)
    {
        if (_animationMap.Remove(animationType))
        {
            // todo: this can result in a lot of allocations
            _animations = _animations.Where(a => a.AnimationType != animationType).ToList();
        }
    }

    internal void CopyModelConditionFlags(BitArray<ModelConditionFlag> newFlags)
    {
        ModelConditionFlags.CopyFrom(newFlags);
    }

    // TODO: This probably shouldn't be here.
    public Matrix4x4? GetWeaponFireFXBoneTransform(WeaponSlot slot, int index)
    {
        foreach (var drawModule in DrawModules)
        {
            var fireFXBone = drawModule.GetWeaponFireFXBone(slot);
            if (fireFXBone != null)
            {
                var (modelInstance, bone) = drawModule.FindBone(fireFXBone + (index + 1).ToString("D2"));
                if (bone != null)
                {
                    return modelInstance.AbsoluteBoneTransforms[bone.Index];
                }
                break;
            }
        }

        return null;
    }

    // TODO: This probably shouldn't be here.
    public Matrix4x4? GetWeaponLaunchBoneTransform(WeaponSlot slot, int index)
    {
        foreach (var drawModule in DrawModules)
        {
            var fireFXBone = drawModule.GetWeaponLaunchBone(slot);
            if (fireFXBone != null)
            {
                var (modelInstance, bone) = drawModule.FindBone(fireFXBone + (index + 1).ToString("D2"));
                if (bone != null)
                {
                    return modelInstance.AbsoluteBoneTransforms[bone.Index];
                }
                break;
            }
        }

        return null;
    }

    public (ModelInstance? modelInstance, ModelBone? bone) FindBone(string boneName)
    {
        foreach (var drawModule in DrawModules)
        {
            var (modelInstance, bone) = drawModule.FindBone(boneName);
            if (bone != null)
            {
                return (modelInstance, bone);
            }
        }

        return (null, null);
    }

    // TODO: Cache this.
    public T? FindClientUpdateModule<T>()
        where T : ClientUpdateModule
    {
        foreach (var clientUpdateModule in _clientUpdateModules)
        {
            if (clientUpdateModule is T t)
            {
                return t;
            }
        }

        return null;
    }

    internal void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime, in Matrix4x4 worldMatrix, in MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS)
    {
        var castsShadow = false;
        switch (Definition.Shadow)
        {
            case ObjectShadowType.ShadowVolume:
            case ObjectShadowType.ShadowVolumeNew:
                castsShadow = true;
                break;
        }

        // Update all draw modules
        foreach (var drawModule in DrawModules)
        {
            if (_hiddenDrawModules.Contains(drawModule.Tag))
            {
                continue;
            }

            var pitchShift = renderItemConstantsPS;
            if (_selectionFlashHelper?.IsActive == true)
            {
                pitchShift = pitchShift with
                {
                    TintColor = pitchShift.TintColor + _selectionFlashHelper.CurrentColor,
                };
            }

            drawModule.Update(gameTime);

            var finalMatrix = InstanceMatrix * worldMatrix;
            ApplyPhysicsTransform(ref finalMatrix);
            drawModule.SetWorldMatrix(finalMatrix);

            drawModule.BuildRenderList(
                renderList,
                camera,
                castsShadow,
                pitchShift,
                _shownSubObjects,
                _hiddenSubObjects);
        }
    }

    private void ApplyPhysicsTransform(ref Matrix4x4 matrix)
    {
        var obj = GameObject;

        if (obj == null || obj.IsDisabledByType(DisabledType.Held))
        {
            return;
        }

        // TODO(Port): Port more checks.
        var frozen = // _gameEngine.Scene3D.TacticalView.IsTimeFrozen ||
            !_gameEngine.Scene3D.TacticalView.IsCameraMovementFinished;
        //_gameEngine.Game.Scripting.IsTimeFrozenDebug ||
        //_gameEngine.Game.Scripting.IsTimeFrozenScript;

        if (frozen)
        {
            return;
        }

        if (CalculatePhysicsTransform(out var info))
        {
            matrix = Matrix4x4.CreateRotationZ(info.TotalYaw)
                * Matrix4x4.CreateRotationX(-info.TotalRoll)
                * Matrix4x4.CreateRotationY(info.TotalPitch)
                * Matrix4x4.CreateTranslation(0, 0, info.TotalZ)
                * matrix;
        }
    }

    private bool CalculatePhysicsTransform(out PhysicsTransformInfo info)
    {
        var locomotor = GameObject.AIUpdate?.CurrentLocomotor;

        var hasPhysicsTransform = false;
        info = new PhysicsTransformInfo();

        if (locomotor != null)
        {
            switch (locomotor.Appearance)
            {
                case LocomotorAppearance.FourWheels:
                    CalculatePhysicsTransformWheels(locomotor, ref info);
                    hasPhysicsTransform = true;
                    break;

                case LocomotorAppearance.Motorcycle:
                    CalculatePhysicsTransformMotorcycle(locomotor, ref info);
                    hasPhysicsTransform = true;
                    break;

                case LocomotorAppearance.Treads:
                    CalculatePhysicsTransformTreads(locomotor, ref info);
                    hasPhysicsTransform = true;
                    break;

                case LocomotorAppearance.Hover:
                case LocomotorAppearance.Wings:
                    CalculatePhysicsTransformHoverOrWings(locomotor, ref info);
                    hasPhysicsTransform = true;
                    break;

                case LocomotorAppearance.Thrust:
                    CalculatePhysicsTransformThrust(locomotor, ref info);
                    hasPhysicsTransform = true;
                    break;
            }
        }

        if (hasPhysicsTransform)
        {
            // Original comment:
            // HOTFIX: Ensure that we are not passing denormalized values back to caller
            // @todo remove hotfix
            if (info.TotalPitch > -1e-20f && info.TotalPitch < 1e-20f)
            {
                info.TotalPitch = 0.0f;
            }

            if (info.TotalRoll > -1e-20f && info.TotalRoll < 1e-20f)
            {
                info.TotalRoll = 0.0f;
            }

            if (info.TotalYaw > -1e-20f && info.TotalYaw < 1e-20f)
            {
                info.TotalYaw = 0.0f;
            }

            if (info.TotalZ > -1e-20f && info.TotalZ < 1e-20f)
            {
                info.TotalZ = 0.0f;
            }
        }

        return hasPhysicsTransform;
    }

    private void CalculatePhysicsTransformWheels(Locomotor locomotor, ref PhysicsTransformInfo info)
    {
        EnsureDrawableLocomotorInfo();

        var ACCEL_PITCH_LIMIT = locomotor.LocomotorTemplate.AccelerationPitchLimit;
        var DECEL_PITCH_LIMIT = locomotor.LocomotorTemplate.DecelerationPitchLimit;
        var BOUNCE_ANGLE_KICK = locomotor.LocomotorTemplate.BounceAmount;
        var PITCH_STIFFNESS = locomotor.LocomotorTemplate.PitchStiffness;
        var ROLL_STIFFNESS = locomotor.LocomotorTemplate.RollStiffness;
        var PITCH_DAMPING = locomotor.LocomotorTemplate.PitchDamping;
        var ROLL_DAMPING = locomotor.LocomotorTemplate.RollDamping;
        var FORWARD_ACCEL_COEFF = locomotor.LocomotorTemplate.ForwardAccelerationPitchFactor;
        var LATERAL_ACCEL_COEFF = locomotor.LocomotorTemplate.LateralAccelerationRollFactor;
        var UNIFORM_AXIAL_DAMPING = locomotor.LocomotorTemplate.UniformAxialDamping;

        var MAX_SUSPENSION_EXTENSION = locomotor.LocomotorTemplate.MaximumWheelExtension; //-2.3f;
        //const Real MAX_SUSPENSION_COMPRESSION = locomotor->getMaxWheelCompression(); //1.4f;
        var WHEEL_ANGLE = locomotor.LocomotorTemplate.FrontWheelTurnAngle;

        var DO_WHEELS = locomotor.LocomotorTemplate.HasSuspension;

        // get object from logic
        var obj = GameObject;
        if (obj == null)
        {
            return;
        }

        var ai = obj.AIUpdate;
        if (ai == null)
        {
            return;
        }

        // get object physics state
        var physics = obj.Physics;
        if (physics == null)
        {
            return;
        }

        // get our position and direction vector
        var pos = Translation;
        var dir = UnitDirectionVector2D;
        var accel = physics.Acceleration;

        // compute perpendicular (2d)
        var perp = new Vector3(
            -dir.Y,
            dir.X,
            0.0f);

        // find pitch and roll of terrain under chassis
        var hheight = _gameEngine.Game.TerrainLogic.GetLayerHeight(pos.X, pos.Y, obj.Layer, out var normal);

        var dot = normal.X * dir.X + normal.Y * dir.Y;
        var groundPitch = dot * (MathF.PI / 2.0f);

        dot = normal.X * perp.X + normal.Y * perp.Y;
        var groundRoll = dot * (MathF.PI / 2.0f);

        var airborne = obj.IsSignificantlyAboveTerrain;

        if (airborne)
        {
            if (DO_WHEELS)
            {
                // Wheels extend when airborne.
                _locomotorInfo.WheelInfo.FramesAirborne = 0;
                _locomotorInfo.WheelInfo.FramesAirborneCounter++;
                if (pos.Z - hheight > -MAX_SUSPENSION_EXTENSION)
                {
                    _locomotorInfo.WheelInfo.RearLeftHeightOffset += (MAX_SUSPENSION_EXTENSION - _locomotorInfo.WheelInfo.RearLeftHeightOffset) / 2.0f;
                    _locomotorInfo.WheelInfo.RearRightHeightOffset += (MAX_SUSPENSION_EXTENSION - _locomotorInfo.WheelInfo.RearRightHeightOffset) / 2.0f;
                }
                else
                {
                    _locomotorInfo.WheelInfo.RearLeftHeightOffset += (0 - _locomotorInfo.WheelInfo.RearLeftHeightOffset) / 2.0f;
                    _locomotorInfo.WheelInfo.RearRightHeightOffset += (0 - _locomotorInfo.WheelInfo.RearRightHeightOffset) / 2.0f;
                }
            }
            // Calculate suspension info.
            var length2 = obj.Geometry.MajorRadius;
            var width2 = obj.Geometry.MinorRadius;
            var pitchHeight2 = length2 * MathF.Sin(_locomotorInfo.Pitch + _locomotorInfo.AccelerationPitch - groundPitch);
            var rollHeight2 = width2 * MathF.Sin(_locomotorInfo.Roll + _locomotorInfo.AccelerationRoll - groundRoll);
            info.TotalZ = MathF.Abs(pitchHeight2) / 4 + MathF.Abs(rollHeight2) / 4;
            return; // maintain the same orientation while we fly through the air.
        }

        // Bouncy.
        var curSpeed = physics.VelocityMagnitude;

	    var maxSpeed = ai.CurrentLocomotorSpeed;
        if (!airborne && curSpeed > maxSpeed / 10)
        {
            var factor = curSpeed / maxSpeed;
            if (MathF.Abs(_locomotorInfo.PitchRate) < factor * BOUNCE_ANGLE_KICK / 4 && MathF.Abs(_locomotorInfo.RollRate) < factor * BOUNCE_ANGLE_KICK / 8)
            {
                // do the bouncy. 
                switch (_gameEngine.GameClient.Random.Next(0, 3))
                {
                    case 0:
                        _locomotorInfo.PitchRate -= BOUNCE_ANGLE_KICK * factor;
                        _locomotorInfo.RollRate -= BOUNCE_ANGLE_KICK * factor / 2;
                        break;
                    case 1:
                        _locomotorInfo.PitchRate += BOUNCE_ANGLE_KICK * factor;
                        _locomotorInfo.RollRate -= BOUNCE_ANGLE_KICK * factor / 2;
                        break;
                    case 2:
                        _locomotorInfo.PitchRate -= BOUNCE_ANGLE_KICK * factor;
                        _locomotorInfo.RollRate += BOUNCE_ANGLE_KICK * factor / 2;
                        break;
                    case 3:
                        _locomotorInfo.PitchRate += BOUNCE_ANGLE_KICK * factor;
                        _locomotorInfo.RollRate += BOUNCE_ANGLE_KICK * factor / 2;
                        break;
                }
            }
        }

        // process chassis suspension dynamics - damp back towards groundPitch

        // the ground can only push back if we're touching it
        if (!airborne)
        {
            _locomotorInfo.PitchRate += ((-PITCH_STIFFNESS * (_locomotorInfo.Pitch - groundPitch)) + (-PITCH_DAMPING * _locomotorInfo.PitchRate));     // spring/damper
            if (_locomotorInfo.PitchRate > 0.0f)
                _locomotorInfo.PitchRate *= 0.5f;

            _locomotorInfo.RollRate += ((-ROLL_STIFFNESS * (_locomotorInfo.Roll - groundRoll)) + (-ROLL_DAMPING * _locomotorInfo.RollRate));       // spring/damper
        }

        _locomotorInfo.Pitch += _locomotorInfo.PitchRate * UNIFORM_AXIAL_DAMPING;
        _locomotorInfo.Roll += _locomotorInfo.RollRate * UNIFORM_AXIAL_DAMPING;

        // process chassis acceleration dynamics - damp back towards zero

        _locomotorInfo.AccelerationPitchRate += ((-PITCH_STIFFNESS * (_locomotorInfo.AccelerationPitch)) + (-PITCH_DAMPING * _locomotorInfo.AccelerationPitchRate));       // spring/damper
        _locomotorInfo.AccelerationPitch += _locomotorInfo.AccelerationPitchRate;

        _locomotorInfo.AccelerationRollRate += ((-ROLL_STIFFNESS * _locomotorInfo.AccelerationRoll) + (-ROLL_DAMPING * _locomotorInfo.AccelerationRollRate));      // spring/damper
        _locomotorInfo.AccelerationRoll += _locomotorInfo.AccelerationRollRate;

        // compute total pitch and roll of tank
        info.TotalPitch = _locomotorInfo.Pitch + _locomotorInfo.AccelerationPitch;
        info.TotalRoll = _locomotorInfo.Roll + _locomotorInfo.AccelerationRoll;

        if (physics.IsMotive)
        {
            // cause the chassis to pitch & roll in reaction to acceleration/deceleration
            var forwardAccel = dir.X * accel.X + dir.Y * accel.Y;
            _locomotorInfo.AccelerationPitchRate += -(FORWARD_ACCEL_COEFF * forwardAccel);

            var lateralAccel = -dir.Y * accel.X + dir.X * accel.Y;
            _locomotorInfo.AccelerationRollRate += -(LATERAL_ACCEL_COEFF * lateralAccel);
        }

        // limit acceleration pitch and roll

        if (_locomotorInfo.AccelerationPitch > DECEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationPitch = DECEL_PITCH_LIMIT;
        else if (_locomotorInfo.AccelerationPitch < -ACCEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationPitch = -ACCEL_PITCH_LIMIT;

        if (_locomotorInfo.AccelerationRoll > DECEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationRoll = DECEL_PITCH_LIMIT;
        else if (_locomotorInfo.AccelerationRoll < -ACCEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationRoll = -ACCEL_PITCH_LIMIT;

        info.TotalZ = 0;

        // Calculate suspension info.
        var length = obj.Geometry.MajorRadius;
        var width = obj.Geometry.MinorRadius;
        var pitchHeight = length * MathF.Sin(info.TotalPitch - groundPitch);
        var rollHeight = width * MathF.Sin(info.TotalRoll - groundRoll);
        if (DO_WHEELS)
        {
            // calculate each wheel position
            _locomotorInfo.WheelInfo.FramesAirborne = _locomotorInfo.WheelInfo.FramesAirborneCounter;
            _locomotorInfo.WheelInfo.FramesAirborneCounter = 0;
            var newInfo = _locomotorInfo.WheelInfo;
            PhysicsTurningType rotation = physics.Turning;
            if (rotation == PhysicsTurningType.Negative)
            {
                newInfo.WheelAngle = -WHEEL_ANGLE;
            }
            else if (rotation == PhysicsTurningType.Positive)
            {
                newInfo.WheelAngle = WHEEL_ANGLE;
            }
            else
            {
                newInfo.WheelAngle = 0;
            }
            if (physics.GetForwardSpeed2D() < 0.0f)
            {
                // if we're moving backwards, the wheels rotate in the opposite direction.
                newInfo.WheelAngle = -newInfo.WheelAngle;
            }

            //
            ///@todo Steven/John ... please review this and make sure it makes sense (CBD)
            // we're going to add the angle to the current wheel rotation ... but we're going to 
            // divide that number to add small angles.  This allows for "smoother" wheel turning
            // transitions ... and when the AI has things move in a straight line, since it's
            // constantly telling the object to go left, go straight, go right, go straight,
            // etc, this smaller angle we'll be adding covers the constant wheel shifting 
            // left and right when moving in a relatively straight line
            //
            const float WHEEL_SMOOTHNESS = 10.0f; // higher numbers add smaller angles, make it more "smooth"
            _locomotorInfo.WheelInfo.WheelAngle += (newInfo.WheelAngle - _locomotorInfo.WheelInfo.WheelAngle) / WHEEL_SMOOTHNESS;

            const float SPRING_FACTOR = 0.9f;
            if (pitchHeight < 0)
            {   // Front raising up
                newInfo.FrontLeftHeightOffset = SPRING_FACTOR * (pitchHeight / 3 + pitchHeight / 2);
                newInfo.FrontRightHeightOffset = SPRING_FACTOR * (pitchHeight / 3 + pitchHeight / 2);
                newInfo.RearLeftHeightOffset = -pitchHeight / 2 + pitchHeight / 4;
                newInfo.RearRightHeightOffset = -pitchHeight / 2 + pitchHeight / 4;
            }
            else
            {   // Back rasing up.
                newInfo.FrontLeftHeightOffset = (-pitchHeight / 4 + pitchHeight / 2);
                newInfo.FrontRightHeightOffset = (-pitchHeight / 4 + pitchHeight / 2);
                newInfo.RearLeftHeightOffset = SPRING_FACTOR * (-pitchHeight / 2 + -pitchHeight / 3);
                newInfo.RearRightHeightOffset = SPRING_FACTOR * (-pitchHeight / 2 + -pitchHeight / 3);
            }
            if (rollHeight > 0)
            {   // Right raising up
                newInfo.FrontRightHeightOffset += -SPRING_FACTOR * (rollHeight / 3 + rollHeight / 2);
                newInfo.RearRightHeightOffset += -SPRING_FACTOR * (rollHeight / 3 + rollHeight / 2);
                newInfo.RearLeftHeightOffset += rollHeight / 2 - rollHeight / 4;
                newInfo.FrontLeftHeightOffset += rollHeight / 2 - rollHeight / 4;
            }
            else
            {   // Left rasing up.
                newInfo.FrontRightHeightOffset += -rollHeight / 2 + rollHeight / 4;
                newInfo.RearRightHeightOffset += -rollHeight / 2 + rollHeight / 4;
                newInfo.RearLeftHeightOffset += SPRING_FACTOR * (rollHeight / 3 + rollHeight / 2);
                newInfo.FrontLeftHeightOffset += SPRING_FACTOR * (rollHeight / 3 + rollHeight / 2);
            }
            if (newInfo.FrontLeftHeightOffset < _locomotorInfo.WheelInfo.FrontLeftHeightOffset)
            {
                // If it's going down, dampen the movement a bit
                _locomotorInfo.WheelInfo.FrontLeftHeightOffset += (newInfo.FrontLeftHeightOffset - _locomotorInfo.WheelInfo.FrontLeftHeightOffset) / 2.0f;
            }
            else
            {
                _locomotorInfo.WheelInfo.FrontLeftHeightOffset = newInfo.FrontLeftHeightOffset;
            }
            if (newInfo.FrontRightHeightOffset < _locomotorInfo.WheelInfo.FrontRightHeightOffset)
            {
                // If it's going down, dampen the movement a bit
                _locomotorInfo.WheelInfo.FrontRightHeightOffset += (newInfo.FrontRightHeightOffset - _locomotorInfo.WheelInfo.FrontRightHeightOffset) / 2.0f;
            }
            else
            {
                _locomotorInfo.WheelInfo.FrontRightHeightOffset = newInfo.FrontRightHeightOffset;
            }
            if (newInfo.RearLeftHeightOffset < _locomotorInfo.WheelInfo.RearLeftHeightOffset)
            {
                // If it's going down, dampen the movement a bit
                _locomotorInfo.WheelInfo.RearLeftHeightOffset += (newInfo.RearLeftHeightOffset - _locomotorInfo.WheelInfo.RearLeftHeightOffset) / 2.0f;
            }
            else
            {
                _locomotorInfo.WheelInfo.RearLeftHeightOffset = newInfo.RearLeftHeightOffset;
            }
            if (newInfo.RearRightHeightOffset < _locomotorInfo.WheelInfo.RearRightHeightOffset)
            {
                // If it's going down, dampen the movement a bit
                _locomotorInfo.WheelInfo.RearRightHeightOffset += (newInfo.RearRightHeightOffset - _locomotorInfo.WheelInfo.RearRightHeightOffset) / 2.0f;
            }
            else
            {
                _locomotorInfo.WheelInfo.RearRightHeightOffset = newInfo.RearRightHeightOffset;
            }
            //_locomotorInfo.WheelInfo = newInfo;
            if (_locomotorInfo.WheelInfo.FrontLeftHeightOffset < MAX_SUSPENSION_EXTENSION)
            {
                _locomotorInfo.WheelInfo.FrontLeftHeightOffset = MAX_SUSPENSION_EXTENSION;
            }
            if (_locomotorInfo.WheelInfo.FrontRightHeightOffset < MAX_SUSPENSION_EXTENSION)
            {
                _locomotorInfo.WheelInfo.FrontRightHeightOffset = MAX_SUSPENSION_EXTENSION;
            }
            if (_locomotorInfo.WheelInfo.RearLeftHeightOffset < MAX_SUSPENSION_EXTENSION)
            {
                _locomotorInfo.WheelInfo.RearLeftHeightOffset = MAX_SUSPENSION_EXTENSION;
            }
            if (_locomotorInfo.WheelInfo.RearRightHeightOffset < MAX_SUSPENSION_EXTENSION)
            {
                _locomotorInfo.WheelInfo.RearRightHeightOffset = MAX_SUSPENSION_EXTENSION;
            }

            // Original has commented out code to clamp to max compression values.
            /*
            if (_locomotorInfo.WheelInfo.FrontLeftHeightOffset>MAX_SUSPENSION_COMPRESSION) {
                _locomotorInfo.WheelInfo.FrontLeftHeightOffset=MAX_SUSPENSION_COMPRESSION;
            }
            if (_locomotorInfo.WheelInfo.FrontRightHeightOffset>MAX_SUSPENSION_COMPRESSION) {
                _locomotorInfo.WheelInfo.FrontRightHeightOffset=MAX_SUSPENSION_COMPRESSION;
            }
            if (_locomotorInfo.WheelInfo.RearLeftHeightOffset>MAX_SUSPENSION_COMPRESSION) {
                _locomotorInfo.WheelInfo.RearLeftHeightOffset=MAX_SUSPENSION_COMPRESSION;
            }
            if (_locomotorInfo.WheelInfo.RearRightHeightOffset>MAX_SUSPENSION_COMPRESSION) {
                _locomotorInfo.WheelInfo.RearRightHeightOffset=MAX_SUSPENSION_COMPRESSION;
            }	
            */
        }
        // If we are > 22 degrees, need to raise height;
        var divisor = 4.0f;
        var pitch = MathF.Abs(info.TotalPitch - groundPitch);

        if (pitch > MathF.PI / 8)
        {
            divisor = ((4 * MathF.PI / 8) + (1 * (pitch - MathF.PI / 8))) / pitch;
        }
        info.TotalZ += MathF.Abs(pitchHeight) / divisor;
        info.TotalZ += MathF.Abs(rollHeight) / divisor;
    }

    private void CalculatePhysicsTransformMotorcycle(Locomotor locomotor, ref PhysicsTransformInfo info)
    {
        EnsureDrawableLocomotorInfo();

        var ACCEL_PITCH_LIMIT = locomotor.LocomotorTemplate.AccelerationPitchLimit;
        var DECEL_PITCH_LIMIT = locomotor.LocomotorTemplate.DecelerationPitchLimit;
        var BOUNCE_ANGLE_KICK = locomotor.LocomotorTemplate.BounceAmount;
        var PITCH_STIFFNESS = locomotor.LocomotorTemplate.PitchStiffness;
        var ROLL_STIFFNESS = locomotor.LocomotorTemplate.RollStiffness;
        var PITCH_DAMPING = locomotor.LocomotorTemplate.PitchDamping;
        var ROLL_DAMPING = locomotor.LocomotorTemplate.RollDamping;
        var FORWARD_ACCEL_COEFF = locomotor.LocomotorTemplate.ForwardAccelerationPitchFactor;
        var LATERAL_ACCEL_COEFF = locomotor.LocomotorTemplate.LateralAccelerationRollFactor;
        var UNIFORM_AXIAL_DAMPING = locomotor.LocomotorTemplate.UniformAxialDamping;

        var MAX_SUSPENSION_EXTENSION = locomotor.LocomotorTemplate.MaximumWheelExtension; //-2.3f;
        //	const Real MAX_SUSPENSION_COMPRESSION = locomotor->getMaxWheelCompression(); //1.4f;
        var WHEEL_ANGLE = locomotor.LocomotorTemplate.FrontWheelTurnAngle; //PI/8;

        var DO_WHEELS = locomotor.LocomotorTemplate.HasSuspension;

        // get object from logic
        var obj = GameObject;

        var ai = obj.AIUpdate;
        if (ai == null)
        {
            return;
        }

        // get object physics state
        var physics = obj.Physics;
        if (physics == null)
        {
            return;
        }

        // get our position and direction vector
        var pos = Translation;
        var dir = UnitDirectionVector2D;
        var accel = physics.Acceleration;

        // compute perpendicular (2d)
        var perp = new Vector3(
            -dir.Y,
            dir.X,
            0.0f);

        // find pitch and roll of terrain under chassis
        var hheight = _gameEngine.Game.TerrainLogic.GetLayerHeight(pos.X, pos.Y, obj.Layer, out var normal);

        var dot = normal.X * dir.X + normal.Y * dir.Y;
        var groundPitch = dot * (MathF.PI / 2.0f);

        dot = normal.X * perp.X + normal.Y * perp.Y;
        var groundRoll = dot * (MathF.PI / 2.0f);

        var airborne = obj.IsSignificantlyAboveTerrain;

        if (airborne)
        {
            if (DO_WHEELS)
            {
                // Wheels extend when airborne.
                _locomotorInfo.WheelInfo.FramesAirborne = 0;
                _locomotorInfo.WheelInfo.FramesAirborneCounter++;
                if (pos.Z - hheight > -MAX_SUSPENSION_EXTENSION)
                {
                    _locomotorInfo.WheelInfo.RearLeftHeightOffset += (MAX_SUSPENSION_EXTENSION - _locomotorInfo.WheelInfo.RearLeftHeightOffset) / 2.0f;
                    _locomotorInfo.WheelInfo.RearRightHeightOffset = _locomotorInfo.WheelInfo.RearLeftHeightOffset;
                }
                else
                {
                    _locomotorInfo.WheelInfo.RearLeftHeightOffset += (0 - _locomotorInfo.WheelInfo.RearLeftHeightOffset) / 2.0f;
                    _locomotorInfo.WheelInfo.RearRightHeightOffset = _locomotorInfo.WheelInfo.RearLeftHeightOffset;
                }
            }
            // Calculate suspension info.
            var length2 = obj.Geometry.MajorRadius;
            //Real width = obj->getGeometryInfo().getMinorRadius();
            var pitchHeight2 = length2 * MathF.Sin(_locomotorInfo.Pitch + _locomotorInfo.AccelerationPitch - groundPitch);
            //Real rollHeight = width*Sin(_locomotorInfo.Roll + _locomotorInfo.AccelerationRoll - groundRoll);
            info.TotalZ = MathF.Abs(pitchHeight2) / 4;// + MathF.Abs(rollHeight)/4;
            //return; // maintain the same orientation while we fly through the air.
        }

        // Bouncy.
        var curSpeed = physics.VelocityMagnitude;
        var maxSpeed = ai.CurrentLocomotorSpeed;
        if (!airborne && curSpeed > maxSpeed / 10)
        {
            var factor = curSpeed / maxSpeed;
            if (MathF.Abs(_locomotorInfo.PitchRate) < factor * BOUNCE_ANGLE_KICK / 4 && MathF.Abs(_locomotorInfo.RollRate) < factor * BOUNCE_ANGLE_KICK / 8)
            {
                // do the bouncy. 
                switch (_gameEngine.GameClient.Random.Next(0, 3))
                {
                    case 0:
                        _locomotorInfo.PitchRate -= BOUNCE_ANGLE_KICK * factor;
                        _locomotorInfo.RollRate -= BOUNCE_ANGLE_KICK * factor / 2;
                        break;
                    case 1:
                        _locomotorInfo.PitchRate += BOUNCE_ANGLE_KICK * factor;
                        _locomotorInfo.RollRate -= BOUNCE_ANGLE_KICK * factor / 2;
                        break;
                    case 2:
                        _locomotorInfo.PitchRate -= BOUNCE_ANGLE_KICK * factor;
                        _locomotorInfo.RollRate += BOUNCE_ANGLE_KICK * factor / 2;
                        break;
                    case 3:
                        _locomotorInfo.PitchRate += BOUNCE_ANGLE_KICK * factor;
                        _locomotorInfo.RollRate += BOUNCE_ANGLE_KICK * factor / 2;
                        break;
                }
            }
        }

        // process chassis suspension dynamics - damp back towards groundPitch

        // the ground can only push back if we're touching it
        if (!airborne)
        {
            _locomotorInfo.PitchRate += ((-PITCH_STIFFNESS * (_locomotorInfo.Pitch - groundPitch)) + (-PITCH_DAMPING * _locomotorInfo.PitchRate));     // spring/damper
            _locomotorInfo.RollRate += ((-ROLL_STIFFNESS * (_locomotorInfo.Roll - groundRoll)) + (-ROLL_DAMPING * _locomotorInfo.RollRate));       // spring/damper
        }
        else
        {
            //Autolevel
            _locomotorInfo.PitchRate += ((-PITCH_STIFFNESS * _locomotorInfo.Pitch) + (-PITCH_DAMPING * _locomotorInfo.PitchRate));     // spring/damper
            _locomotorInfo.RollRate += ((-ROLL_STIFFNESS * _locomotorInfo.Roll) + (-ROLL_DAMPING * _locomotorInfo.RollRate));      // spring/damper
        }

        _locomotorInfo.Pitch += _locomotorInfo.PitchRate * UNIFORM_AXIAL_DAMPING;
        _locomotorInfo.Roll += _locomotorInfo.RollRate * UNIFORM_AXIAL_DAMPING;

        // process chassis acceleration dynamics - damp back towards zero

        _locomotorInfo.AccelerationPitchRate += ((-PITCH_STIFFNESS * (_locomotorInfo.AccelerationPitch)) + (-PITCH_DAMPING * _locomotorInfo.AccelerationPitchRate));       // spring/damper
        _locomotorInfo.AccelerationPitch += _locomotorInfo.AccelerationPitchRate;

        _locomotorInfo.AccelerationRollRate += ((-ROLL_STIFFNESS * _locomotorInfo.AccelerationRoll) + (-ROLL_DAMPING * _locomotorInfo.AccelerationRollRate));      // spring/damper
        _locomotorInfo.AccelerationRoll += _locomotorInfo.AccelerationRollRate;

        // compute total pitch and roll of tank
        info.TotalPitch = _locomotorInfo.Pitch + _locomotorInfo.AccelerationPitch;


        // THis logic had recently been added to Drawable::applyPhysicsXform(), which was naughty, since it clamped the roll in every drawable in the game
        // Now only motorcycles enjoy this constraint
        var unclampedRoll = _locomotorInfo.Roll + _locomotorInfo.AccelerationRoll;
        info.TotalRoll = (unclampedRoll > 0.5f && unclampedRoll < -0.5f ? unclampedRoll : 0.0f);

        if (airborne)
        {
        }

        if (physics.IsMotive)
        {
            // cause the chassis to pitch & roll in reaction to acceleration/deceleration
            var forwardAccel = dir.X * accel.X + dir.Y * accel.Y;
            _locomotorInfo.AccelerationPitchRate += -(FORWARD_ACCEL_COEFF * forwardAccel);

            var lateralAccel = -dir.Y * accel.X + dir.X * accel.Y;
            _locomotorInfo.AccelerationRollRate += -(LATERAL_ACCEL_COEFF * lateralAccel);
        }

        // limit acceleration pitch and roll

        if (_locomotorInfo.AccelerationPitch > DECEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationPitch = DECEL_PITCH_LIMIT;
        else if (_locomotorInfo.AccelerationPitch < -ACCEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationPitch = -ACCEL_PITCH_LIMIT;

        if (_locomotorInfo.AccelerationRoll > DECEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationRoll = DECEL_PITCH_LIMIT;
        else if (_locomotorInfo.AccelerationRoll < -ACCEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationRoll = -ACCEL_PITCH_LIMIT;

        info.TotalZ = 0;

        // Calculate suspension info.
        var length = obj.Geometry.MajorRadius;
        var width = obj.Geometry.MinorRadius;
        var pitchHeight = length * MathF.Sin(info.TotalPitch - groundPitch);
        var rollHeight = width * MathF.Sin(info.TotalRoll - groundRoll);
        if (DO_WHEELS)
        {
            // calculate each wheel position
            _locomotorInfo.WheelInfo.FramesAirborne = _locomotorInfo.WheelInfo.FramesAirborneCounter;
            _locomotorInfo.WheelInfo.FramesAirborneCounter = 0;
            var newInfo = _locomotorInfo.WheelInfo;
            PhysicsTurningType rotation = physics.Turning;
            if (rotation == PhysicsTurningType.Negative)
            {
                newInfo.WheelAngle = -WHEEL_ANGLE;
            }
            else if (rotation == PhysicsTurningType.Positive)
            {
                newInfo.WheelAngle = WHEEL_ANGLE;
            }
            else
            {
                newInfo.WheelAngle = 0;
            }
            if (physics.GetForwardSpeed2D() < 0.0f)
            {
                // if we're moving backwards, the wheels rotate in the opposite direction.
                newInfo.WheelAngle = -newInfo.WheelAngle;
            }

            //
            ///@todo Steven/John ... please review this and make sure it makes sense (CBD)
            // we're going to add the angle to the current wheel rotation ... but we're going to 
            // divide that number to add small angles.  This allows for "smoother" wheel turning
            // transitions ... and when the AI has things move in a straight line, since it's
            // constantly telling the object to go left, go straight, go right, go straight,
            // etc, this smaller angle we'll be adding covers the constant wheel shifting 
            // left and right when moving in a relatively straight line
            //
            const float WHEEL_SMOOTHNESS = 10.0f; // higher numbers add smaller angles, make it more "smooth"
            _locomotorInfo.WheelInfo.WheelAngle += (newInfo.WheelAngle - _locomotorInfo.WheelInfo.WheelAngle) / WHEEL_SMOOTHNESS;

            const float SPRING_FACTOR = 0.9f;
            if (pitchHeight < 0)
            {
                // Front raising up
                newInfo.FrontLeftHeightOffset = SPRING_FACTOR * (pitchHeight / 3 + pitchHeight / 2);
                newInfo.RearLeftHeightOffset = -pitchHeight / 2 + pitchHeight / 4;
                newInfo.FrontRightHeightOffset = newInfo.FrontLeftHeightOffset;
                newInfo.RearRightHeightOffset = newInfo.RearLeftHeightOffset;
            }
            else
            {
                // Back raising up.
                newInfo.FrontLeftHeightOffset = (-pitchHeight / 4 + pitchHeight / 2);
                newInfo.RearLeftHeightOffset = SPRING_FACTOR * (-pitchHeight / 2 + -pitchHeight / 3);
                newInfo.FrontRightHeightOffset = newInfo.FrontLeftHeightOffset;
                newInfo.RearRightHeightOffset = newInfo.RearLeftHeightOffset;
            }
            /*
            if (rollHeight>0) {	// Right raising up
                newInfo.FrontRightHeightOffset += -SPRING_FACTOR*(rollHeight/3+rollHeight/2);
                newInfo.RearLeftHeightOffset += rollHeight/2 - rollHeight/4;
            }	else {	// Left raising up.
                newInfo.FrontRightHeightOffset += -rollHeight/2 + rollHeight/4;
                newInfo.RearLeftHeightOffset += SPRING_FACTOR*(rollHeight/3+rollHeight/2);
            }
            */
            if (newInfo.FrontLeftHeightOffset < _locomotorInfo.WheelInfo.FrontLeftHeightOffset)
            {
                // If it's going down, dampen the movement a bit
                _locomotorInfo.WheelInfo.FrontLeftHeightOffset += (newInfo.FrontLeftHeightOffset - _locomotorInfo.WheelInfo.FrontLeftHeightOffset) / 2.0f;
                _locomotorInfo.WheelInfo.FrontRightHeightOffset = _locomotorInfo.WheelInfo.FrontLeftHeightOffset;
            }
            else
            {
                _locomotorInfo.WheelInfo.FrontLeftHeightOffset = newInfo.FrontLeftHeightOffset;
                _locomotorInfo.WheelInfo.FrontRightHeightOffset = newInfo.FrontLeftHeightOffset;
            }
            if (newInfo.RearLeftHeightOffset < _locomotorInfo.WheelInfo.RearLeftHeightOffset)
            {
                // If it's going down, dampen the movement a bit
                _locomotorInfo.WheelInfo.RearLeftHeightOffset += (newInfo.RearLeftHeightOffset - _locomotorInfo.WheelInfo.RearLeftHeightOffset) / 2.0f;
                _locomotorInfo.WheelInfo.RearRightHeightOffset = _locomotorInfo.WheelInfo.RearLeftHeightOffset;
            }
            else
            {
                _locomotorInfo.WheelInfo.RearLeftHeightOffset = newInfo.RearLeftHeightOffset;
                _locomotorInfo.WheelInfo.RearRightHeightOffset = newInfo.RearLeftHeightOffset;
            }
            //_locomotorInfo.WheelInfo = newInfo;
            if (_locomotorInfo.WheelInfo.FrontLeftHeightOffset < MAX_SUSPENSION_EXTENSION)
            {
                _locomotorInfo.WheelInfo.FrontLeftHeightOffset = MAX_SUSPENSION_EXTENSION;
                _locomotorInfo.WheelInfo.FrontRightHeightOffset = MAX_SUSPENSION_EXTENSION;
            }
            if (_locomotorInfo.WheelInfo.RearLeftHeightOffset < MAX_SUSPENSION_EXTENSION)
            {
                _locomotorInfo.WheelInfo.RearLeftHeightOffset = MAX_SUSPENSION_EXTENSION;
                _locomotorInfo.WheelInfo.RearRightHeightOffset = MAX_SUSPENSION_EXTENSION;
            }
        }
        // If we are > 22 degrees, need to raise height;
        var divisor = 4.0f;
        var pitch = MathF.Abs(info.TotalPitch - groundPitch);

        if (pitch > MathF.PI / 8)
        {
            divisor = ((4 * MathF.PI / 8) + (1 * (pitch - MathF.PI / 8))) / pitch;
        }

        if (!airborne)
        {
            info.TotalZ += MathF.Abs(pitchHeight) / divisor;
            info.TotalZ += MathF.Abs(rollHeight) / divisor;
        }
    }

    private void CalculatePhysicsTransformTreads(Locomotor locomotor, ref PhysicsTransformInfo info)
    {
        EnsureDrawableLocomotorInfo();

        var OVERLAP_SHRINK_FACTOR = 0.8f;
        var FLATTENED_OBJECT_HEIGHT = 0.5f;
        var LEAVE_OVERLAP_PITCH_KICK = MathF.PI / 128;
        var OVERLAP_ROUGH_VIBRATION_FACTOR = 5.0f;
        var MAX_ROUGH_VIBRATION = 0.5f;
        var ACCEL_PITCH_LIMIT = locomotor.LocomotorTemplate.AccelerationPitchLimit;
        var DECEL_PITCH_LIMIT = locomotor.LocomotorTemplate.DecelerationPitchLimit;
        var PITCH_STIFFNESS = locomotor.LocomotorTemplate.PitchStiffness;
        var ROLL_STIFFNESS = locomotor.LocomotorTemplate.RollStiffness;
        var PITCH_DAMPING = locomotor.LocomotorTemplate.PitchDamping;
        var ROLL_DAMPING = locomotor.LocomotorTemplate.RollDamping;
        var FORWARD_ACCEL_COEFF = locomotor.LocomotorTemplate.ForwardAccelerationPitchFactor;
        var LATERAL_ACCEL_COEFF = locomotor.LocomotorTemplate.LateralAccelerationRollFactor;
        var UNIFORM_AXIAL_DAMPING = locomotor.LocomotorTemplate.UniformAxialDamping;

        // get object from logic
        var obj = GameObject;
        if (obj == null)
        {
            return;
        }

        var ai = obj.AIUpdate;
        if (ai == null)
        {
            return;
        }

        // get object physics state
        var physics = obj.Physics;
        if (physics == null)
        {
            return;
        }

        // get our position and direction vector
        var pos = Translation;
        var dir = UnitDirectionVector2D;
        var accel = physics.Acceleration;
        var vel = physics.Velocity;

        // compute perpendicular (2d)
        var perp = new Vector3(
            -dir.Y,
            dir.X,
            0.0f);

        // find pitch and roll of terrain under chassis
        /*	Real hheight = */
        _gameEngine.Game.TerrainLogic.GetLayerHeight(pos.X, pos.Y, obj.Layer, out var normal);

        // override surface normal if we are overlapping another object - crushing it
        var overlapZ = 0.0f;

        // get object we are currently overlapping, if any
        var overlapped = _gameEngine.GameLogic.GetObjectById(physics.CurrentOverlap);
        if (overlapped != null && overlapped.IsKindOf(ObjectKinds.Shrubbery))
        {
            overlapped = null; // We just smash through shrubbery.  jba.
        }

        if (overlapped != null)
        {
            var overPos = overlapped.Translation;
            var dx = overPos.X - pos.X;
            var dy = overPos.Y - pos.Y;
            var centerDistSqr = MathUtility.Square(dx) + MathUtility.Square(dy);

            // compute maximum distance between objects, if their edges just touched
            var ourSize = DrawableGeometryInfo.BoundingCircleRadius;
            var otherSize = overlapped.Geometry.BoundingCircleRadius;
            var maxCenterDist = otherSize + ourSize;

            // shrink the overlap distance a bit to avoid floating
            maxCenterDist *= OVERLAP_SHRINK_FACTOR;
            if (centerDistSqr < MathUtility.Square(maxCenterDist))
            {
                var centerDist = MathF.Sqrt(centerDistSqr);
                var amount = 1.0f - centerDist / maxCenterDist;
                if (amount < 0.0f)
                    amount = 0.0f;
                else if (amount > 1.0f)
                    amount = 1.0f;

                // rough vibrations proportional to speed when we drive over something
                var rough = (vel.X * vel.X + vel.Y * vel.Y) * OVERLAP_ROUGH_VIBRATION_FACTOR;
                if (rough > MAX_ROUGH_VIBRATION)
                    rough = MAX_ROUGH_VIBRATION;

                var height = overlapped.Geometry.MaxZ;

                // do not "go up" flattened crushed things
                var flat = false;
                if (overlapped.IsKindOf(ObjectKinds.LowOverlappable)
                    || overlapped.IsKindOf(ObjectKinds.Infantry)
                    || (overlapped.BodyModule.FrontCrushed && overlapped.BodyModule.BackCrushed))
                {
                    flat = true;
                    height = FLATTENED_OBJECT_HEIGHT;
                }

                if (amount < FLATTENED_OBJECT_HEIGHT && flat == false)
                {
                    overlapZ = height * 2.0f * amount;

                    // compute vector along "surface"
                    // not proportional to actual geometry to avoid overlay steep inclines, etc
                    var v = new Vector3(
                        dx / centerDist,
                        dy / centerDist,
                        0.2f); // 0.25

                    var up = new Vector3(
                        _gameEngine.GameClient.Random.NextSingle(-rough, rough),
                        _gameEngine.GameClient.Random.NextSingle(-rough, rough),
                        1.0f);
                    up = Vector3.Normalize(up);

                    // TODO: Check these arguments are the right way round.
                    var prp = Vector3.Cross(v, up);
                    normal = Vector3.Cross(prp, v);

                    // compute unit normal
                    normal = Vector3.Normalize(normal);
                }
                else
                {
                    // sitting on top of object
                    overlapZ = height;

                    normal = new Vector3(
                        _gameEngine.GameClient.Random.NextSingle(-rough, rough),
                        _gameEngine.GameClient.Random.NextSingle(-rough, rough),
                        1.0f);
                    normal = Vector3.Normalize(normal);
                }
            }
        }
        else    // no overlap this frame
        {
            // if we had an overlap last frame, and we're now in the air, give a
            // kick to the pitch for effect
            if (physics.PreviousOverlap.IsValid && _locomotorInfo.OverlapZ > 0.0f)
            {
                _locomotorInfo.PitchRate += LEAVE_OVERLAP_PITCH_KICK;
            }
        }

        var dot = normal.X * dir.X + normal.Y * dir.Y;
        var groundPitch = dot * (MathF.PI / 2.0f);

        dot = normal.X * perp.X + normal.Y * perp.Y;
        var groundRoll = dot * (MathF.PI / 2.0f);

        // process chassis suspension dynamics - damp back towards groundPitch

        // the ground can only push back if we're touching it
        if (overlapped != null || _locomotorInfo.OverlapZ <= 0.0f)
        {
            _locomotorInfo.PitchRate += ((-PITCH_STIFFNESS * (_locomotorInfo.Pitch - groundPitch)) + (-PITCH_DAMPING * _locomotorInfo.PitchRate));     // spring/damper
            if (_locomotorInfo.PitchRate > 0.0f)
                _locomotorInfo.PitchRate *= 0.5f;

            _locomotorInfo.RollRate += ((-ROLL_STIFFNESS * (_locomotorInfo.Roll - groundRoll)) + (-ROLL_DAMPING * _locomotorInfo.RollRate));       // spring/damper
        }

        _locomotorInfo.Pitch += _locomotorInfo.PitchRate * UNIFORM_AXIAL_DAMPING;
        _locomotorInfo.Roll += _locomotorInfo.RollRate * UNIFORM_AXIAL_DAMPING;

        // process chassis recoil dynamics - damp back towards zero

        _locomotorInfo.AccelerationPitchRate += ((-PITCH_STIFFNESS * (_locomotorInfo.AccelerationPitch)) + (-PITCH_DAMPING * _locomotorInfo.AccelerationPitchRate));       // spring/damper
        _locomotorInfo.AccelerationPitch += _locomotorInfo.AccelerationPitchRate;

        _locomotorInfo.AccelerationRollRate += ((-ROLL_STIFFNESS * _locomotorInfo.AccelerationRoll) + (-ROLL_DAMPING * _locomotorInfo.AccelerationRollRate));      // spring/damper
        _locomotorInfo.AccelerationRoll += _locomotorInfo.AccelerationRollRate;

        // compute total pitch and roll of tank
        info.TotalPitch = _locomotorInfo.Pitch + _locomotorInfo.AccelerationPitch;
        info.TotalRoll = _locomotorInfo.Roll + _locomotorInfo.AccelerationRoll;

        if (physics.IsMotive)
        {
            // cause the chassis to pitch & roll in reaction to acceleration/deceleration
            var forwardAccel = dir.X * accel.X + dir.Y * accel.Y;
            _locomotorInfo.AccelerationPitchRate += -(FORWARD_ACCEL_COEFF * forwardAccel);

            var lateralAccel = -dir.Y * accel.X + dir.X * accel.Y;
            _locomotorInfo.AccelerationRollRate += -(LATERAL_ACCEL_COEFF * lateralAccel);
        }

        // There's a section of #ifdef'd-out code in the original, for recoiling from being damaged.

        // limit recoil pitch and roll

        if (_locomotorInfo.AccelerationPitch > DECEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationPitch = DECEL_PITCH_LIMIT;
        else if (_locomotorInfo.AccelerationPitch < -ACCEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationPitch = -ACCEL_PITCH_LIMIT;

        if (_locomotorInfo.AccelerationRoll > DECEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationRoll = DECEL_PITCH_LIMIT;
        else if (_locomotorInfo.AccelerationRoll < -ACCEL_PITCH_LIMIT)
            _locomotorInfo.AccelerationRoll = -ACCEL_PITCH_LIMIT;

        // adjust z
        if (overlapZ > _locomotorInfo.OverlapZ)
        {
            _locomotorInfo.OverlapZ = overlapZ;
            /// @todo Z needs to accelerate/decelerate, not be directly set (MSB)
            // _locomotorInfo.OverlapZ += 0.4f;
            _locomotorInfo.OverlapZVelocity = 0.0f;
        }

        var ztmp = _locomotorInfo.OverlapZ / 2.0f;

        // do fake Z physics
        if (_locomotorInfo.OverlapZ > 0.0f)
        {
            _locomotorInfo.OverlapZVelocity -= 0.2f;
            _locomotorInfo.OverlapZ += _locomotorInfo.OverlapZVelocity;
        }

        if (_locomotorInfo.OverlapZ <= 0.0f)
        {
            _locomotorInfo.OverlapZ = 0.0f;
            _locomotorInfo.OverlapZVelocity = 0.0f;
        }
        info.TotalZ = ztmp;
    }

    private void CalculatePhysicsTransformHoverOrWings(Locomotor locomotor, ref PhysicsTransformInfo info)
    {
        EnsureDrawableLocomotorInfo();

        var accelerationPitchLimit = locomotor.LocomotorTemplate.AccelerationPitchLimit;
        var decelerationPitchLimit = locomotor.LocomotorTemplate.DecelerationPitchLimit;
        var pitchStiffness = locomotor.LocomotorTemplate.PitchStiffness;
        var rollStiffness = locomotor.LocomotorTemplate.RollStiffness;
        var pitchDamping = locomotor.LocomotorTemplate.PitchDamping;
        var rollDamping = locomotor.LocomotorTemplate.RollDamping;
        var zVelocityPitchCoefficient = locomotor.LocomotorTemplate.PitchInDirectionOfZVelFactor;
        var forwardVelocityCoefficient = locomotor.LocomotorTemplate.ForwardVelocityPitchFactor;
        var lateralVelocityCoefficient = locomotor.LocomotorTemplate.LateralVelocityRollFactor;
        var forwardAccelerationCoefficient = locomotor.LocomotorTemplate.ForwardAccelerationPitchFactor;
        var lateralAccelerationCoefficient = locomotor.LocomotorTemplate.LateralAccelerationRollFactor;
        var uniformAxialDamping = locomotor.LocomotorTemplate.UniformAxialDamping;

        // get object from logic
        var physics = GameObject?.Physics;
        if (physics == null)
        {
            return;
        }

        var dir = UnitDirectionVector2D;
        var accel = physics.Acceleration;
        var vel = physics.Velocity;

        _locomotorInfo.PitchRate += ((-pitchStiffness * _locomotorInfo.Pitch) + (-pitchDamping * _locomotorInfo.PitchRate)); // Spring/damper
        _locomotorInfo.Pitch += _locomotorInfo.PitchRate * uniformAxialDamping;

        _locomotorInfo.RollRate += ((-rollStiffness * _locomotorInfo.Roll) + (-rollDamping * _locomotorInfo.RollRate)); // Spring/damper
        _locomotorInfo.Roll += _locomotorInfo.RollRate * uniformAxialDamping;

        // Process chassis acceleration dynamics - damp back towards zero.

        _locomotorInfo.AccelerationPitchRate += ((-pitchStiffness * (_locomotorInfo.AccelerationPitch)) + (-pitchDamping * _locomotorInfo.AccelerationPitchRate)); // Spring/damper
        _locomotorInfo.AccelerationPitch += _locomotorInfo.AccelerationPitchRate;

        _locomotorInfo.AccelerationRollRate += ((-rollStiffness * _locomotorInfo.AccelerationRoll) + (-rollDamping * _locomotorInfo.AccelerationRollRate)); // Spring/damper
        _locomotorInfo.AccelerationRoll += _locomotorInfo.AccelerationRollRate;

        // Compute total pitch and roll of tank.
        info.TotalPitch = _locomotorInfo.Pitch + _locomotorInfo.AccelerationPitch;
        info.TotalRoll = _locomotorInfo.Roll + _locomotorInfo.AccelerationRoll;

        if (physics.IsMotive)
        {
            if (zVelocityPitchCoefficient != 0.0f)
            {
                const float tinyDeltaZ = 0.001f;
                if (MathF.Abs(vel.Z) > tinyDeltaZ)
                {
                    var pitch = MathF.Atan2(vel.Z, MathF.Sqrt(MathUtility.Square(vel.X) + MathUtility.Square(vel.Y)));
                    _locomotorInfo.Pitch -= zVelocityPitchCoefficient * pitch;
                }
            }

            // Cause the chassis to pitch & roll in reaction to current speed.
            var forwardVel = dir.X * vel.X + dir.Y * vel.Y;
            _locomotorInfo.Pitch += -(forwardVelocityCoefficient * forwardVel);

            var lateralVel = -dir.Y * vel.X + dir.X * vel.Y;
            _locomotorInfo.Roll += -(lateralVelocityCoefficient * lateralVel);

            // Cause the chassis to pitch & roll in reaction to acceleration/deceleration.
            var forwardAccel = dir.X * accel.X + dir.Y * accel.Y;
            _locomotorInfo.AccelerationPitchRate += -(forwardAccelerationCoefficient * forwardAccel);

            var lateralAccel = -dir.Y * accel.X + dir.X * accel.Y;
            _locomotorInfo.AccelerationRollRate += -(lateralAccelerationCoefficient * lateralAccel);
        }

        // limit acceleration pitch and roll

        if (_locomotorInfo.AccelerationPitch > decelerationPitchLimit)
        {
            _locomotorInfo.AccelerationPitch = decelerationPitchLimit;
        }
        else if (_locomotorInfo.AccelerationPitch < -accelerationPitchLimit)
        {
            _locomotorInfo.AccelerationPitch = -accelerationPitchLimit;
        }

        if (_locomotorInfo.AccelerationRoll > decelerationPitchLimit)
        {
            _locomotorInfo.AccelerationRoll = decelerationPitchLimit;
        }
        else if (_locomotorInfo.AccelerationRoll < -accelerationPitchLimit)
        {
            _locomotorInfo.AccelerationRoll = -accelerationPitchLimit;
        }

        var rudderCorrectionDegree = locomotor.LocomotorTemplate.RudderCorrectionDegree;
        var rudderCorrectionRate = locomotor.LocomotorTemplate.RudderCorrectionRate;
        var elevatorCorrectionDegree = locomotor.LocomotorTemplate.ElevatorCorrectionDegree;
        var elevatorCorrectionRate = locomotor.LocomotorTemplate.ElevatorCorrectionRate;

        info.TotalYaw = rudderCorrectionDegree * MathF.Sin(_locomotorInfo.YawModulator += rudderCorrectionRate);
        info.TotalPitch += elevatorCorrectionDegree * MathF.Cos(_locomotorInfo.PitchModulator += elevatorCorrectionRate);

        info.TotalZ = 0.0f;
    }

    private void CalculatePhysicsTransformThrust(Locomotor locomotor, ref PhysicsTransformInfo info)
    {
        EnsureDrawableLocomotorInfo();

        // TODO(Port): These values should be scaled by deltaTime.
        var thrustRoll = locomotor.LocomotorTemplate.ThrustRoll;
        var wobbleRate = locomotor.LocomotorTemplate.ThrustWobbleRate;
        var maxWobble = locomotor.LocomotorTemplate.ThrustMaxWobble;
        var minWobble = locomotor.LocomotorTemplate.ThrustMinWobble;

        // Original comment:
        // this is a kind of quick thrust implementation cause we need scud missiles to wobble *now*,
        // we deal with just adjusting pitch, yaw, and roll just a little bit

        if (wobbleRate != 0)
        {
            // Wobble is either 0 or 1.
            // If it's 0, we're wobbling in one direction.
            // If it's 1, we're wobbling in the other direction.
            if (_locomotorInfo.Wobble >= 1.0f)
            {
                // Near centre, increase pitch and yaw faster, then when we get near max wobble,
                // slow down the rate of increase.
                if (_locomotorInfo.Pitch < maxWobble - wobbleRate * 2)
                {
                    _locomotorInfo.Pitch += wobbleRate;
                    _locomotorInfo.Yaw += wobbleRate;
                }
                else
                {
                    _locomotorInfo.Pitch += (wobbleRate / 2.0f);
                    _locomotorInfo.Yaw += (wobbleRate / 2.0f);
                }
                if (_locomotorInfo.Pitch >= maxWobble)
                {
                    _locomotorInfo.Wobble = -1.0f;
                }
            }
            else
            {
                if (_locomotorInfo.Pitch >= minWobble + wobbleRate * 2.0f)
                {
                    _locomotorInfo.Pitch -= wobbleRate;
                    _locomotorInfo.Yaw -= wobbleRate;
                }
                else
                {
                    _locomotorInfo.Pitch -= (wobbleRate / 2.0f);
                    _locomotorInfo.Yaw -= (wobbleRate / 2.0f);
                }
                if (_locomotorInfo.Pitch <= minWobble)
                {
                    _locomotorInfo.Wobble = 1.0f;
                }
            }

            info.TotalPitch = _locomotorInfo.Pitch;
            info.TotalYaw = _locomotorInfo.Yaw;
        }

        if (thrustRoll != 0)
        {
            _locomotorInfo.Roll += thrustRoll;
            info.TotalRoll = _locomotorInfo.Roll;
        }
    }

    [MemberNotNull(nameof(_locomotorInfo))]
    private void EnsureDrawableLocomotorInfo()
    {
        if (_locomotorInfo == null)
        {
            _locomotorInfo = new DrawableLocomotorInfo();
        }
    }

    public void HideDrawModule(string module)
    {
        if (!_hiddenDrawModules.Contains(module))
        {
            _hiddenDrawModules.Add(module);
        }
    }

    public void ShowDrawModule(string module)
    {
        _hiddenDrawModules.Remove(module);
    }

    public void HideSubObject(string subObject)
    {
        if (subObject == null) return;

        if (!_hiddenSubObjects.ContainsKey(subObject))
        {
            _hiddenSubObjects.Add(subObject, false);
        }
        _shownSubObjects.Remove(subObject);
    }

    public void HideSubObjectPermanently(string subObject)
    {
        if (subObject == null) return;

        if (!_hiddenSubObjects.ContainsKey(subObject))
        {
            _hiddenSubObjects.Add(subObject, true);
        }
        else
        {
            _hiddenSubObjects[subObject] = true;
        }
        _shownSubObjects.Remove(subObject);
    }

    public void ShowSubObject(string subObject)
    {
        if (subObject == null) return;

        if (!_shownSubObjects.ContainsKey(subObject))
        {
            _shownSubObjects.Add(subObject, false);
        }
        _hiddenSubObjects.Remove(subObject);
    }

    public void ShowSubObjectPermanently(string subObject)
    {
        if (subObject == null) return;

        if (!_shownSubObjects.ContainsKey(subObject))
        {
            _shownSubObjects.Add(subObject, true);
        }
        else
        {
            _shownSubObjects[subObject] = true;
        }
        _hiddenSubObjects.Remove(subObject);
    }

    public void SetAnimationDuration(LogicFrameSpan frames)
    {
        foreach (var drawModule in DrawModules)
        {
            drawModule.SetAnimationDuration(frames);
        }
    }

    public void SetSupplyBoxesRemaining(float boxPercentage)
    {
        foreach (var drawModule in DrawModules)
        {
            drawModule.SetSupplyBoxesRemaining(boxPercentage);
        }
    }

    public void SetTerrainDecal(ObjectDecalType decalType)
    {
        if (_terrainDecalType == decalType)
        {
            return;
        }

        _terrainDecalType = decalType;

        foreach (var drawModule in DrawModules)
        {
            // Only the first draw module gets a decal to prevent stacking.
            // Should be okay as long as we keep the primary object in the
            // first module.
            drawModule.SetTerrainDecal(decalType);
            break;
        }
    }

    // C++: Drawable::clientOnly_getFirstRenderObjInfo
    // This doesn't return the position separately, because Transform already has that.
    public bool GetFirstRenderObjInfo(
        [MaybeNullWhen(false)] out float boundingSphereRadius,
        [MaybeNullWhen(false)] out Transform transform)
    {
        var drawModule = _drawModules.FirstOrDefault();

        if (drawModule == null || drawModule.BoundingSphere == null)
        {
            boundingSphereRadius = 0.0f;
            transform = null;
            return false;
        }

        boundingSphereRadius = drawModule.BoundingSphere.Value.Radius;
        // TODO: Is this correct? Generals seems to have a separate Transform field for each draw module,
        // not just the Drawable as a whole.
        transform = Transform;
        return true;
    }

    private static readonly FrozenDictionary<BodyDamageType, ModelConditionFlag> DamageTypeLookup = new Dictionary<BodyDamageType, ModelConditionFlag>()
    {
        { BodyDamageType.Damaged, ModelConditionFlag.Damaged },
        { BodyDamageType.ReallyDamaged, ModelConditionFlag.ReallyDamaged },
        { BodyDamageType.Rubble, ModelConditionFlag.Rubble },
    }.ToFrozenDictionary();

    public void ReactToBodyDamageStateChange(BodyDamageType newState)
    {
        ModelConditionFlags.Set(ModelConditionFlag.Damaged, false);
        ModelConditionFlags.Set(ModelConditionFlag.ReallyDamaged, false);
        ModelConditionFlags.Set(ModelConditionFlag.Rubble, false);

        if (DamageTypeLookup.TryGetValue(newState, out var flag))
        {
            ModelConditionFlags.Set(flag, true);
        }

        // TODO(Port): Port this.
        // When loading map, ambient sound starting is handled by onLevelStart(), so that we can
        // correctly react to customizations
        //if (!TheGameLogic->isLoadingMap())
        //    startAmbientSound(newState, TheGlobalData->m_timeOfDay);
    }

    internal void Destroy()
    {
        foreach (var drawModule in DrawModules)
        {
            drawModule.Dispose();
        }
    }

    public void Persist(StatePersister reader)
    {
        var version = reader.PersistVersion(7);

        var id = ID;
        reader.PersistUInt32(ref id);
        if (reader.Mode == StatePersistMode.Read)
        {
            ID = id;
        }

        var modelConditionFlags = ModelConditionFlags.Clone();
        reader.PersistBitArray(ref modelConditionFlags);
        if (reader.Mode == StatePersistMode.Read)
        {
            CopyModelConditionFlags(modelConditionFlags);
        }

        reader.PersistMatrix4x3(ref _transformMatrix);

        var hasSelectionFlashHelper = _selectionFlashHelper != null;
        reader.PersistBoolean(ref hasSelectionFlashHelper);
        if (hasSelectionFlashHelper)
        {
            _selectionFlashHelper ??= new ColorFlashHelper();
            reader.PersistObject(_selectionFlashHelper);
        }

        var hasScriptedFlashHelper = _scriptedFlashHelper != null;
        reader.PersistBoolean(ref hasScriptedFlashHelper);
        if (hasScriptedFlashHelper)
        {
            _scriptedFlashHelper ??= new ColorFlashHelper();
            reader.PersistObject(_scriptedFlashHelper);
        }

        var decalType = _terrainDecalType;
        reader.PersistEnum(ref decalType);
        if (reader.SageGame == SageGame.CncGenerals && (int)decalType == 6)
        {
            // The existing enum value for "None" was changed between Generals and ZH.
            // In Generals, "None" == 6.
            decalType = ObjectDecalType.None;
        }
        if (reader.Mode == StatePersistMode.Read)
        {
            SetTerrainDecal(decalType);
        }

        reader.PersistSingle(ref _explicitOpacity);
        reader.PersistSingle(ref _stealthOpacity);
        reader.PersistSingle(ref _effectiveStealthOpacity);
        reader.PersistSingle(ref _decalOpacityFadeTarget);
        reader.PersistSingle(ref _decalOpacityFadeRate);
        reader.PersistSingle(ref _decalOpacity);

        var objectId = GameObjectID;
        reader.PersistObjectId(ref objectId);
        if (objectId != GameObjectID)
        {
            throw new InvalidStateException();
        }

        reader.PersistEnumFlags(ref _status);
        reader.PersistEnumFlags(ref _tintStatus);
        reader.PersistEnumFlags(ref _previousTintStatus);
        reader.PersistEnum(ref _fadeMode);
        reader.PersistUInt32(ref _timeElapsedFade);
        reader.PersistUInt32(ref _timeToFade);

        var hasLocomotorInfo = _locomotorInfo != null;
        reader.PersistBoolean(ref hasLocomotorInfo);
        if (hasLocomotorInfo)
        {
            if (reader.Mode == StatePersistMode.Read && _locomotorInfo == null)
            {
                _locomotorInfo = new DrawableLocomotorInfo();
            }

            reader.PersistObject(_locomotorInfo);
        }

        PersistModules(reader);

        reader.PersistEnum(ref _stealthLook);

        reader.PersistUInt32(ref _flashCount);
        reader.PersistColorRgba(ref _flashColor);

        reader.PersistBoolean(ref _hidden);
        reader.PersistBoolean(ref _hiddenByStealth);

        reader.PersistSingle(ref _secondMaterialPassOpacity);

        reader.PersistBoolean(ref _instanceMatrixIsIdentity);
        reader.PersistMatrix4x3(ref _instanceMatrix, false);
        reader.PersistSingle(ref _instanceScale);

        reader.PersistObjectId(ref _drawableInfo.ShroudStatusObjectId);

        reader.PersistUInt32(ref _expirationDate);

        var animation2DCount = (byte)_animations.Count;
        reader.PersistByte(ref animation2DCount);

        reader.BeginArray("Animations");

        if (reader.Mode == StatePersistMode.Read)
        {
            _animations = [];
        }

        for (var i = 0; i < animation2DCount; i++)
        {
            reader.BeginObject();
            var animation = reader.Mode == StatePersistMode.Read ? null : _animations[i];
            var animation2DName = animation?.Template.Name;
            reader.PersistAsciiString(ref animation2DName);

            var keepTillFrame = 0;
            reader.PersistInt32(ref keepTillFrame);

            var animation2DName2 = animation2DName;
            reader.PersistAsciiString(ref animation2DName2);
            if (animation2DName2 != animation2DName)
            {
                throw new InvalidStateException();
            }

            if (reader.Mode == StatePersistMode.Read)
            {
                var animationTemplate = reader.AssetStore.Animations.GetByName(animation2DName);
                animation = new Animation(animationTemplate, _gameEngine.LogicFramesPerSecond);
                _animations.Add(animation);
            }

            reader.PersistObject(animation);

            reader.EndObject();
        }

        reader.EndArray();

        reader.PersistBoolean(ref _ambientSoundEnabled);

        if (version >= 6)
        {
            reader.PersistBoolean(ref _ambientSoundEnabledFromScript);
        }

        if (version >= 7)
        {
            var customized = false;
            reader.PersistBoolean(ref customized);

            if (customized)
            {
                throw new NotImplementedException();
            }
        }
    }

    private void PersistModules(StatePersister reader)
    {
        reader.BeginObject("ModuleGroups");

        reader.PersistVersion(1);

        ushort numModuleGroups = 2;
        reader.PersistUInt16(ref numModuleGroups);

        if (numModuleGroups != 2)
        {
            throw new InvalidStateException();
        }

        PersistModuleGroup(reader, "DrawModules", DrawModules);
        PersistModuleGroup(reader, "ClientUpdateModules", new ReadOnlySpan<ClientUpdateModule>(_clientUpdateModules));

        reader.EndObject();
    }

    private void PersistModuleGroup<T>(StatePersister reader, string groupName, ReadOnlySpan<T> modules)
        where T : ModuleBase
    {
        reader.BeginObject(groupName);

        var numModules = (ushort)modules.Length;
        reader.PersistUInt16(ref numModules);

        reader.BeginArray("Modules");
        for (var moduleIndex = 0; moduleIndex < numModules; moduleIndex++)
        {
            reader.BeginObject();

            ModuleBase module;
            if (reader.Mode == StatePersistMode.Read)
            {
                var moduleTag = "";
                reader.PersistAsciiString(ref moduleTag);
                module = GetModuleByTag(moduleTag);
            }
            else
            {
                module = modules[moduleIndex];
                var moduleTag = module.Tag;
                reader.PersistAsciiString(ref moduleTag);
            }

            reader.BeginSegment($"{module.GetType().Name} module in game object {Definition.Name}");

            reader.PersistObject(module);

            reader.EndSegment();

            reader.EndObject();
        }
        reader.EndArray();

        reader.EndObject();
    }

    [Flags]
    private enum DrawableStatus
    {
        /// <summary>
        /// No status.
        /// </summary>
        None = 0,

        /// <summary>
        /// Drawable can reflect.
        /// </summary>
        DrawsInMirror = 0x1,

        /// <summary>
        /// Use SetShadowsEnabled access method.
        /// </summary>
        Shadows = 0x2,

        /// <summary>
        /// Drawable tint color is "locked" and won't fade to normal.
        /// </summary>
        TintColorLocked = 0x4,

        /// <summary>
        /// Do _not_ auto-create particle systems based on model condition.
        /// </summary>
        NoStateParticles = 0x8,

        /// <summary>
        /// Do _not_ save this drawable (UI fluff only).
        /// Ignored (error, actually) if attached to an object.
        /// </summary>
        NoSave = 0x10,
    }

    [Flags]
    private enum TintStatus
    {
        None = 0,

        /// <summary>
        /// Drawable tint color is deathly dark grey.
        /// </summary>
        Disabled = 0x1,

        /// <summary>
        /// Drawable tint color is sickly green.
        /// </summary>
        Irradiated = 0x2,

        /// <summary>
        /// Drawable tint color is open-sore red.
        /// </summary>
        Poisoned = 0x4,

        /// <summary>
        /// When gaining subdual damage, we tint SUBDUAL_DAMAGE_COLOR.
        /// </summary>
        GainingSubdualDamage = 0x8,

        /// <summary>
        /// When frenzied, we tint FRENZY_COLOR.
        /// </summary>
        Frenzy = 0x10,
    }

    private enum FadingMode
    {
        None = 0,
        In = 1,
        Out = 2,
    }

    private class DrawableLocomotorInfo : IPersistableObject
    {
        /// <summary>
        /// Pitch of the entire drawable.
        /// </summary>
        public float Pitch = 0.0f;

        /// <summary>
        /// Rate of change of pitch.
        /// </summary>
        public float PitchRate = 0.0f;

        /// <summary>
        /// Roll of the entire drawable.
        /// </summary>
        public float Roll = 0.0f;

        /// <summary>
        /// Rate of change of roll.
        /// </summary>
        public float RollRate = 0.0f;

        /// <summary>
        /// Yaw for the entire drawable.
        /// </summary>
        public float Yaw = 0.0f;

        /// <summary>
        /// Pitch of the drawable due to impact/acceleration.
        /// </summary>
        public float AccelerationPitch = 0.0f;

        /// <summary>
        /// Rate of change of pitch.
        /// </summary>
        public float AccelerationPitchRate = 0.0f;

        /// <summary>
        /// Roll of the entire drawable.
        /// </summary>
        public float AccelerationRoll = 0.0f;

        /// <summary>
        /// Rate of change of roll.
        /// </summary>
        public float AccelerationRollRate = 0.0f;

        /// <summary>
        /// Fake Z velocity.
        /// </summary>
        public float OverlapZVelocity = 0.0f;

        /// <summary>
        /// Current height (additional).
        /// </summary>
        public float OverlapZ = 0.0f;

        /// <summary>
        /// For wobbling.
        /// </summary>
        public float Wobble = 1.0f;

        /// <summary>
        /// For the swimmy soft hover of a helicopter.
        /// </summary>
        public float YawModulator = 0.0f;

        /// <summary>
        /// For the swimmy soft hover of a helicopter.
        /// </summary>
        public float PitchModulator = 0.0f;

        /// <summary>
        /// Wheel offset and angle info for a wheeled type locomotor.
        /// </summary>
        public readonly WheelInfo WheelInfo = new();

        public void Persist(StatePersister persister)
        {
            persister.PersistSingle(ref Pitch);
            persister.PersistSingle(ref PitchRate);
            persister.PersistSingle(ref Roll);
            persister.PersistSingle(ref RollRate);
            persister.PersistSingle(ref Yaw);
            persister.PersistSingle(ref AccelerationPitch);
            persister.PersistSingle(ref AccelerationPitchRate);
            persister.PersistSingle(ref AccelerationRoll);
            persister.PersistSingle(ref AccelerationRollRate);
            persister.PersistSingle(ref OverlapZVelocity);
            persister.PersistSingle(ref OverlapZ);
            persister.PersistSingle(ref Wobble);

            persister.PersistObject(WheelInfo);
        }
    }

    private class WheelInfo : IPersistableObject
    {
        // Height offsets for tires due to suspension sway.
        public float FrontLeftHeightOffset = 0.0f;
        public float FrontRightHeightOffset = 0.0f;
        public float RearLeftHeightOffset = 0.0f;
        public float RearRightHeightOffset = 0.0f;

        /// <summary>
        /// Wheel angle. 0 = straight, > 0 = left, < 0 = right.
        /// </summary>
        public float WheelAngle = 0.0f;

        public int FramesAirborneCounter = 0;

        /// <summary>
        /// How many frames it was in the ir.
        /// </summary>
        public int FramesAirborne;

        public void Persist(StatePersister persister)
        {
            persister.PersistSingle(ref FrontLeftHeightOffset);
            persister.PersistSingle(ref FrontRightHeightOffset);
            persister.PersistSingle(ref RearLeftHeightOffset);
            persister.PersistSingle(ref RearRightHeightOffset);
            persister.PersistSingle(ref WheelAngle);
            persister.PersistInt32(ref FramesAirborneCounter);
            persister.PersistInt32(ref FramesAirborne);
        }
    }

    private struct PhysicsTransformInfo
    {
        public float TotalPitch;
        public float TotalRoll;
        public float TotalYaw;
        public float TotalZ;
    }
}

public enum ObjectDecalType
{
    HordeInfantry = 1, // exhorde.dds
    NationalismInfantry = 2, // exhorde_up.dds
    HordeVehicle = 3, // exhordeb.dds
    NationalismVehicle = 4, // exhordeb_up.dds
    Crate = 5, // exjunkcrate.dds
    HordeWithFanaticismUpgrade = 6,
    ChemSuit = 7,
    None = 8,
    ShadowTexture = 9,
}

public enum StealthLookType
{
    /// <summary>
    /// Unit is not stealthed at all.
    /// </summary>
    None = 0,

    /// <summary>
    /// Unit is stealthed-but-visible due to friendly status.
    /// </summary>
    VisibleFriendly = 1,

    /// <summary>
    /// We can have units that are disguised (instead of invisible).
    /// </summary>
    DisguisedEnemy = 2,

    /// <summary>
    /// Unit is stealthed and invisible, but a second material pass
    /// is added to reveal the invisible unit as with heat vision.
    /// </summary>
    VisibleDetected = 3,

    /// <summary>
    /// Unit is stealthed-but-visible due to being detected,
    /// and rendered in heatvision effect second material pass.
    /// </summary>
    VisibleFriendlyDetected = 4,

    /// <summary>
    /// Unit is stealthed-and-invisible.
    /// </summary>
    Invisible = 5,
}

/// <summary>
/// Simple structure used to bind W3D render objects to our own Drawables.
/// </summary>
public class DrawableInfo
{
    [Flags]
    public enum ExtraRenderFlags
    {
        None = 0,
        IsOccluded = 0x1,
        PotentialOccluder = 0x2,
        PotentialOccludee = 0x4,
        IsTranslucent = 0x8,
        IsNonOccluderOrOccludee = 0x10,
        DelayedRender = IsTranslucent | PotentialOccludee,
    }

    /// <summary>
    /// Since we sometimes have drawables without objects, this points to a
    /// parent object from which we pull shroud status.
    /// </summary>
    public ObjectId ShroudStatusObjectId;

    /// <summary>
    /// Pointer back to drawable containing this <see cref="DrawableInfo"/>.
    /// </summary>
    public Drawable? Drawable;

    /// <summary>
    /// Pointer to ghost object for this drawable used for fogged versions.
    /// </summary>
    public GhostObject? GhostObject;

    /// <summary>
    /// Extra render settings flags that are tied to render objects with drawables.
    /// </summary>
    public ExtraRenderFlags Flags;
}
