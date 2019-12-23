using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class W3dModelDraw : DrawModule
    {
        private readonly W3dModelDrawModuleData _data;
        private readonly AssetLoadContext _loadContext;

        private readonly List<ModelConditionState> _conditionStates;
        private readonly ModelConditionState _defaultConditionState;

        private readonly List<AnimationState> _animationStates;
        private readonly AnimationState _idleAnimationState;

        private ModelConditionState _activeConditionState;
        private AnimationState _activeAnimationState;

        private W3dModelDrawConditionState _activeModelDrawConditionState;

        public override IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates
        {
            get
            {
                yield return _defaultConditionState.ConditionFlags;

                foreach (var conditionState in _conditionStates)
                {
                    yield return conditionState.ConditionFlags;
                }

                foreach (var animationState in _animationStates)
                {
                    yield return animationState.TypeFlags;
                }
            }
        }

        internal override IEnumerable<AttachedParticleSystem> GetAllAttachedParticleSystems()
        {
            return (_activeModelDrawConditionState != null)
                ? _activeModelDrawConditionState.AttachedParticleSystems
                : Enumerable.Empty<AttachedParticleSystem>();
        }

        internal W3dModelDraw(W3dModelDrawModuleData data, AssetLoadContext loadContext)
        {
            _data = data;
            _loadContext = loadContext;

            _conditionStates = new List<ModelConditionState>();

            if (data.DefaultConditionState != null)
            {
                _defaultConditionState = data.DefaultConditionState;
            }

            foreach (var conditionState in data.ConditionStates)
            {
                _conditionStates.Add(conditionState);
            }

            if (_defaultConditionState == null)
            {
                _defaultConditionState = _conditionStates.Find(x => !x.ConditionFlags.AnyBitSet);

                if (_defaultConditionState != null)
                {
                    _conditionStates.Remove(_defaultConditionState);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            SetActiveConditionState(_defaultConditionState);

            _animationStates = new List<AnimationState>();

            if (data.IdleAnimationState != null)
            {
                _idleAnimationState = data.IdleAnimationState;
            }

            foreach (var animationState in data.AnimationStates)
            {
                _animationStates.Add(animationState);
            }
        }

        private void SetActiveConditionState(ModelConditionState conditionState)
        {
            if (_activeConditionState == conditionState)
            {
                return;
            }

            RemoveAndDispose(ref _activeModelDrawConditionState);

            _activeConditionState = conditionState;

            _activeModelDrawConditionState = AddDisposable(
                CreateModelDrawConditionStateInstance(
                    conditionState));
        }

        private void SetActiveAnimationState(AnimationState animationState)
        {
            if (_activeAnimationState == animationState
             || _activeModelDrawConditionState == null)
            {
                return;
            }

            _activeAnimationState = animationState;

            var modelInstance = _activeModelDrawConditionState.Model;

            var firstAnimationBlock = animationState.Animations.FirstOrDefault();
            if (firstAnimationBlock != null)
            {
                foreach(var animation in firstAnimationBlock.Animations)
                {
                    var anim = animation.Value;
                    //Check if the animation does really exist
                    if(anim != null)
                    {
                        var animationInstance = new AnimationInstance(modelInstance, anim);
                        modelInstance.AnimationInstances.Add(animationInstance);
                        animationInstance.Play();
                        break;
                    }
                }
            }
        }

        public override void UpdateConditionState(BitArray<ModelConditionFlag> flags)
        {
            ModelConditionState bestConditionState = null;
            var bestMatch = int.MinValue;

            // Find best matching ModelConditionState.
            foreach (var conditionState in _conditionStates)
            {
                var numStateBits = conditionState.ConditionFlags.NumBitsSet;
                var numIntersectionBits = conditionState.ConditionFlags.CountIntersectionBits(flags);

                // If there's no intersection never select this.
                if (numIntersectionBits != numStateBits)
                {
                    continue;
                }

                if (numIntersectionBits > bestMatch)
                {
                    bestConditionState = conditionState;
                    bestMatch = numIntersectionBits;
                }
            }

            if (bestConditionState == null || bestMatch == 0)
            {
                bestConditionState = _defaultConditionState;
            }

            SetActiveConditionState(bestConditionState);

            AnimationState bestAnimationState = null;
            bestMatch = int.MinValue;

            // Find best matching ModelConditionState.
            foreach (var animationState in _animationStates)
            {
                var numStateBits = animationState.TypeFlags.NumBitsSet;
                var numIntersectionBits = animationState.TypeFlags.CountIntersectionBits(flags);

                // If there's no intersection never select this.
                if (numIntersectionBits != numStateBits)
                {
                    continue;
                }

                if (numIntersectionBits > bestMatch)
                {
                    bestAnimationState = animationState;
                    bestMatch = numIntersectionBits;
                }
            }

            if (bestAnimationState == null || bestMatch == 0)
            {
                bestAnimationState = _idleAnimationState;
            }

            SetActiveAnimationState(bestAnimationState);
        }

        private W3dModelDrawConditionState CreateModelDrawConditionStateInstance(ModelConditionState conditionState)
        {
            // Load model, fallback to default model.
            var model = conditionState.Model?.Value ?? _defaultConditionState.Model?.Value;

            ModelInstance modelInstance = null;
            if (model != null)
            {
                modelInstance = model.CreateInstance(_loadContext);
            }

            if (modelInstance != null)
            {
                // TODO: Multiple animations. Shouldn't play all of them. I think
                // we should randomly choose one of them?
                // And there is also IdleAnimation.
                var firstAnimation = conditionState.ConditionAnimations
                    .Concat(conditionState.IdleAnimations)
                    .LastOrDefault();
                if (firstAnimation != null)
                {
                    var animation = firstAnimation.Animation.Value;

                    if (animation != null)
                    {
                        var animationInstance = new AnimationInstance(modelInstance, animation);
                        modelInstance.AnimationInstances.Add(animationInstance);
                        animationInstance.Play();
                    }
                }
            }

            var particleSystems = new List<ParticleSystem>();
            if (modelInstance != null)
            {
                foreach (var particleSysBone in conditionState.ParticleSysBones)
                {
                    var particleSystemTemplate = _loadContext.AssetStore.FXParticleSystemTemplates.GetByName(particleSysBone.ParticleSystem);
                    if (particleSystemTemplate == null)
                    {
                        particleSystemTemplate = _loadContext.AssetStore.ParticleSystemTemplates.GetByName(particleSysBone.ParticleSystem)?.ToFXParticleSystemTemplate();

                        if (particleSystemTemplate == null)
                        {
                            throw new InvalidOperationException("Missing referenced particle system: " + particleSysBone.ParticleSystem);
                        }
                    }

                    var bone = modelInstance.Model.BoneHierarchy.Bones.FirstOrDefault(x => string.Equals(x.Name, particleSysBone.BoneName, StringComparison.OrdinalIgnoreCase));
                    if (bone == null)
                    {
                        // TODO: Should this ever happen?
                        continue;
                    }

                    particleSystems.Add(new ParticleSystem(
                        particleSystemTemplate,
                        _loadContext,
                        () => ref modelInstance.AbsoluteBoneTransforms[bone.Index]));
                }
            }

            return modelInstance != null
               ? new W3dModelDrawConditionState(modelInstance, particleSystems)
               : null;
        }

        internal override void Update(in TimeInterval gameTime)
        {
            _activeModelDrawConditionState?.Update(gameTime);
        }

        internal override void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
            _activeModelDrawConditionState?.SetWorldMatrix(worldMatrix);
        }

        internal override void BuildRenderList(
            RenderList renderList,
            Camera camera,
            bool castsShadow,
            Player owner)
        {
            _activeModelDrawConditionState?.BuildRenderList(renderList, camera, castsShadow, owner);
        }
    }

    internal sealed class W3dModelDrawConditionState : DisposableBase
    {
        private readonly ModelInstance _modelInstance;

        public ModelInstance Model => _modelInstance;

        public IReadOnlyList<AttachedParticleSystem> AttachedParticleSystems { get; }

        public W3dModelDrawConditionState(ModelInstance modelInstance, IEnumerable<ParticleSystem> particleSystems)
        {
            _modelInstance = AddDisposable(modelInstance);

            var attachedParticleSystems = new List<AttachedParticleSystem>();
            foreach (var particleSystem in particleSystems)
            {
                AddDisposable(particleSystem);

                attachedParticleSystems.Add(new AttachedParticleSystem(
                    particleSystem,
                    x =>
                    {
                        attachedParticleSystems.Remove(x);
                        RemoveToDispose(particleSystem);
                        particleSystem.Dispose();
                    }));
            }
            AttachedParticleSystems = attachedParticleSystems;
        }

        public void Update(in TimeInterval gameTime)
        {
            _modelInstance.Update(gameTime);
        }

        public void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
            _modelInstance.SetWorldMatrix(worldMatrix);
        }

        public void BuildRenderList(
            RenderList renderList,
            Camera camera,
            bool castsShadow,
            Player owner)
        {
            _modelInstance.BuildRenderList(renderList, camera, castsShadow, owner);
        }
    }

    public class W3dModelDrawModuleData : DrawModuleData
    {
        internal static W3dModelDrawModuleData ParseModel(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<W3dModelDrawModuleData> FieldParseTable = new IniParseTable<W3dModelDrawModuleData>
        {
            { "DefaultConditionState", (parser, x) => parser.Temp = x.DefaultConditionState = ModelConditionState.ParseDefault(parser) },
            { "DefaultModelConditionState", (parser, x) => parser.Temp = x.DefaultConditionState = ModelConditionState.ParseDefault(parser) },

            {
                "ConditionState",
                (parser, x) =>
                {
                    var conditionState = ModelConditionState.Parse(parser);
                    x.ConditionStates.Add(conditionState);
                    parser.Temp = conditionState;
                }
            },
            {
                "ModelConditionState",
                (parser, x) =>
                {
                    var conditionState = ModelConditionState.Parse(parser);
                    x.ConditionStates.Add(conditionState);
                    parser.Temp = conditionState;
                }
            },

            { "IgnoreConditionStates", (parser, x) => x.IgnoreConditionStates = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "AliasConditionState", (parser, x) => x.ParseAliasConditionState(parser) },
            { "TransitionState", (parser, x) => x.TransitionStates.Add(TransitionState.Parse(parser)) },
            { "OkToChangeModelColor", (parser, x) => x.OkToChangeModelColor = parser.ParseBoolean() },
            { "ReceivesDynamicLights", (parser, x) => x.ReceivesDynamicLights = parser.ParseBoolean() },
            { "ProjectileBoneFeedbackEnabledSlots", (parser, x) => x.ProjectileBoneFeedbackEnabledSlots = parser.ParseEnumBitArray<WeaponSlot>() },
            { "AnimationsRequirePower", (parser, x) => x.AnimationsRequirePower = parser.ParseBoolean() },
            { "ParticlesAttachedToAnimatedBones", (parser, x) => x.ParticlesAttachedToAnimatedBones = parser.ParseBoolean() },
            { "MinLODRequired", (parser, x) => x.MinLodRequired = parser.ParseEnum<ModelLevelOfDetail>() },
            { "ExtraPublicBone", (parser, x) => x.ExtraPublicBones.Add(parser.ParseBoneName()) },
            { "AttachToBoneInAnotherModule", (parser, x) => x.AttachToBoneInAnotherModule = parser.ParseBoneName() },
            { "TrackMarks", (parser, x) => x.TrackMarks = parser.ParseFileName() },
            { "TrackMarksLeftBone", (parser, x) => x.TrackMarksLeftBone = parser.ParseAssetReference() },
            { "TrackMarksRightBone", (parser, x) => x.TrackMarksRightBone = parser.ParseAssetReference() },
            { "InitialRecoilSpeed", (parser, x) => x.InitialRecoilSpeed = parser.ParseFloat() },
            { "MaxRecoilDistance", (parser, x) => x.MaxRecoilDistance = parser.ParseFloat() },
            { "RecoilSettleSpeed", (parser, x) => x.RecoilSettleSpeed = parser.ParseFloat() },

            { "IdleAnimationState", (parser, x) => x.IdleAnimationState = AnimationState.Parse(parser) },
            { "AnimationState", (parser, x) => x.AnimationStates.Add(AnimationState.Parse(parser)) },
        };

        public BitArray<ModelConditionFlag> IgnoreConditionStates { get; private set; }
        public ModelConditionState DefaultConditionState { get; private set; }
        public List<ModelConditionState> ConditionStates { get; } = new List<ModelConditionState>();
        public List<TransitionState> TransitionStates { get; } = new List<TransitionState>();

        public bool OkToChangeModelColor { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ReceivesDynamicLights { get; private set; }

        public BitArray<WeaponSlot> ProjectileBoneFeedbackEnabledSlots { get; private set; }
        public bool AnimationsRequirePower { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ParticlesAttachedToAnimatedBones { get; private set; }

        /// <summary>
        /// Minimum level of detail required before this object appears in the game.
        /// </summary>
        public ModelLevelOfDetail MinLodRequired { get; private set; }

        public List<string> ExtraPublicBones { get; } = new List<string>();
        public string AttachToBoneInAnotherModule { get; private set; }

        public string TrackMarks { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string TrackMarksLeftBone { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public string TrackMarksRightBone { get; private set; }

        public float InitialRecoilSpeed { get; private set; } = 2.0f;
        public float MaxRecoilDistance { get; private set; } = 3.0f;
        public float RecoilSettleSpeed { get; private set; } = 0.065f;

        public AnimationState IdleAnimationState { get; private set; }
        public List<AnimationState> AnimationStates { get; } = new List<AnimationState>();

        private void ParseAliasConditionState(IniParser parser)
        {
            if (!(parser.Temp is ModelConditionState lastConditionState))
            {
                throw new IniParseException("Cannot use AliasConditionState if there are no preceding ConditionStates", parser.CurrentPosition);
            }

            var conditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            var aliasedConditionState = lastConditionState.Clone(conditionFlags);

            ConditionStates.Add(aliasedConditionState);
        }

        internal override DrawModule CreateDrawModule(AssetLoadContext loadContext)
        {
            return new W3dModelDraw(this, loadContext);
        }
    }

    public enum ModelLevelOfDetail
    {
        [IniEnum("LOW")]
        Low,

        [IniEnum("MEDIUM")]
        Medium,

        [IniEnum("HIGH")]
        High,
    }
}
