using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Logic.Object
{
    public sealed class W3dModelDraw : DrawModule
    {
        private readonly ContentManager _contentManager;
        private readonly W3dModelDrawModuleData _data;

        private readonly List<ModelConditionState> _conditionStates;
        private readonly ModelConditionState _defaultConditionState;

        private ModelConditionState _activeConditionState;

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
            }
        }

        internal override IEnumerable<AttachedParticleSystem> GetAllAttachedParticleSystems()
        {
            return (_activeModelDrawConditionState != null)
                ? _activeModelDrawConditionState.AttachedParticleSystems
                : Enumerable.Empty<AttachedParticleSystem>();
        }

        internal W3dModelDraw(ContentManager contentManager, W3dModelDrawModuleData data)
        {
            _contentManager = contentManager;
            _data = data;

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

        public override void UpdateConditionState(BitArray<ModelConditionFlag> flags)
        {
            ModelConditionState bestConditionState = null;
            var bestMatch = int.MinValue;

            // Find best matching ConditionState.
            foreach (var conditionState in _conditionStates)
            {
                var match = conditionState.ConditionFlags.And(flags).NumBitsSet;
                if (match > bestMatch)
                {
                    bestConditionState = conditionState;
                    bestMatch = match;
                }
            }

            if (bestConditionState == null || bestMatch == 0)
            {
                bestConditionState = _defaultConditionState;
            }

            SetActiveConditionState(bestConditionState);
        }

        private W3dModelDrawConditionState CreateModelDrawConditionStateInstance(ModelConditionState conditionState)
        {
            ModelInstance modelInstance = null;
            if (!string.Equals(conditionState.Model, "NONE", StringComparison.OrdinalIgnoreCase))
            {
                var w3dFilePath = Path.Combine("Art", "W3D", conditionState.Model + ".W3D");
                var model = _contentManager.Load<Model>(w3dFilePath);
                if (model != null)
                {
                    modelInstance = model.CreateInstance(_contentManager.GraphicsDevice);
                }
            }

            if (modelInstance != null)
            {
                // TODO: Multiple animations. Shouldn't play all of them. I think
                // we should randomly choose one of them?
                // And there is also IdleAnimation.
                var firstAnimation = conditionState.Animations
                    .Concat(conditionState.IdleAnimations)
                    .LastOrDefault();
                if (firstAnimation != null)
                {
                    var splitName = firstAnimation.Animation.Split('.');

                    var w3dFilePath = Path.Combine("Art", "W3D", splitName[0] + ".W3D");
                    var model = _contentManager.Load<Model>(w3dFilePath);

                    if (model.Animations.Length == 0)
                    {
                        // TODO: What is the actual algorithm here?
                        w3dFilePath = Path.Combine("Art", "W3D", splitName[1] + ".W3D");
                        model = _contentManager.Load<Model>(w3dFilePath);
                    }

                    if (model != null)
                    {
                        var animation = model.Animations.FirstOrDefault(x => string.Equals(x.Name, splitName[1], StringComparison.OrdinalIgnoreCase));
                        if (animation != null)
                        {
                            // TODO: Should this ever be null?

                            var animationInstance = new AnimationInstance(modelInstance, animation);

                            modelInstance.AnimationInstances.Add(animationInstance);

                            animationInstance.Play();
                        }
                    }
                }
            }

            var particleSystems = new List<ParticleSystem>();
            if (modelInstance != null)
            {
                foreach (var particleSysBone in conditionState.ParticleSysBones)
                {
                    var particleSystemDefinition = _contentManager.IniDataContext.ParticleSystems.First(x => x.Name == particleSysBone.ParticleSystem);
                    var bone = modelInstance.Model.Bones.FirstOrDefault(x => string.Equals(x.Name, particleSysBone.BoneName, StringComparison.OrdinalIgnoreCase));
                    if (bone == null)
                    {
                        // TODO: Should this ever happen?
                        continue;
                    }

                    particleSystems.Add(new ParticleSystem(
                        _contentManager,
                        particleSystemDefinition,
                        () => ref modelInstance.AbsoluteBoneTransforms[bone.Index]));
                }
            }

            return modelInstance != null
               ? new W3dModelDrawConditionState(modelInstance, particleSystems)
               : null;
        }

        internal override void Update(GameTime gameTime)
        {
            _activeModelDrawConditionState?.Update(gameTime);
        }

        internal override void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
            _activeModelDrawConditionState?.SetWorldMatrix(worldMatrix);
        }

        internal override void BuildRenderList(RenderList renderList, CameraComponent camera)
        {
            _activeModelDrawConditionState?.BuildRenderList(renderList, camera);
        }
    }

    internal sealed class W3dModelDrawConditionState : DisposableBase
    {
        private readonly ModelInstance _modelInstance;

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

        public void Update(GameTime gameTime)
        {
            _modelInstance.Update(gameTime);

            foreach (var attachedParticleSystem in AttachedParticleSystems)
            {
                attachedParticleSystem.ParticleSystem.Update(gameTime);
            }
        }

        public void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
            _modelInstance.SetWorldMatrix(worldMatrix);
        }

        public void BuildRenderList(RenderList renderList, CameraComponent camera)
        {
            _modelInstance.BuildRenderList(renderList, camera);
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
            { "InitialRecoilSpeed", (parser, x) => x.InitialRecoilSpeed = parser.ParseFloat() },
            { "MaxRecoilDistance", (parser, x) => x.MaxRecoilDistance = parser.ParseFloat() },
            { "RecoilSettleSpeed", (parser, x) => x.RecoilSettleSpeed = parser.ParseFloat() },
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

        public float InitialRecoilSpeed { get; private set; } = 2.0f;
        public float MaxRecoilDistance { get; private set; } = 3.0f;
        public float RecoilSettleSpeed { get; private set; } = 0.065f;

        private void ParseAliasConditionState(IniParser parser)
        {
            var lastConditionState = parser.Temp as ModelConditionState;
            if (lastConditionState == null)
            {
                throw new IniParseException("Cannot use AliasConditionState if there are no preceding ConditionStates", parser.CurrentPosition);
            }

            var conditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            var aliasedConditionState = lastConditionState.Clone(conditionFlags);

            ConditionStates.Add(aliasedConditionState);
        }

        internal override DrawModule CreateDrawModule(ContentManager contentManager)
        {
            return new W3dModelDraw(contentManager, this);
        }
    }

    public enum ModelLevelOfDetail
    {
        [IniEnum("MEDIUM")]
        Medium,
    }
}
