using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using LL.Graphics3D;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.Logic.Object
{
    public sealed class W3dModelDraw : DrawableComponent
    {
        private readonly W3dModelDrawModuleData _data;

        private readonly List<ModelConditionState> _conditionStates;
        private readonly ModelConditionState _defaultConditionState;

        private ModelConditionState _activeConditionState;

        public override IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates
        {
            get
            {
                if (_data.DefaultConditionState != null)
                {
                    yield return _data.DefaultConditionState.ConditionFlags;
                }

                foreach (var conditionState in _data.ConditionStates)
                {
                    yield return conditionState.ConditionFlags;
                }
            }
        }

        public W3dModelDraw(W3dModelDrawModuleData data)
        {
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
        }

        protected override void Start()
        {
            base.Start();

            SetActiveConditionState(_defaultConditionState);
        }

        private void SetActiveConditionState(ModelConditionState conditionState)
        {
            if (_activeConditionState == conditionState)
            {
                return;
            }

            Entity.Transform.Children.Clear();

            _activeConditionState = conditionState;

            Entity.AddChild(CreateModelDrawConditionStateEntity(conditionState));
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

        private Entity CreateModelDrawConditionStateEntity(ModelConditionState conditionState)
        {
            var result = new Entity();

            Entity modelEntity = null;
            if (!string.Equals(conditionState.Model, "NONE", StringComparison.OrdinalIgnoreCase))
            {
                var w3dFilePath = Path.Combine("Art", "W3D", conditionState.Model + ".W3D");
                var model = ContentManager.Load<Model>(w3dFilePath);
                if (model != null)
                {
                    result.AddChild(modelEntity = model.CreateEntity());
                }
            }

            if (modelEntity != null)
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
                    var model = ContentManager.Load<Model>(w3dFilePath);

                    if (model.Animations.Length == 0)
                    {
                        // TODO: What is the actual algorithm here?
                        w3dFilePath = Path.Combine("Art", "W3D", splitName[1] + ".W3D");
                        model = ContentManager.Load<Model>(w3dFilePath);
                    }

                    if (model != null)
                    {
                        var animation = model.Animations.FirstOrDefault(x => string.Equals(x.Name, splitName[1], StringComparison.OrdinalIgnoreCase));
                        if (animation != null)
                        {
                            // TODO: Should this ever be null?

                            var animationComponent = new AnimationComponent
                            {
                                Animation = animation
                            };

                            modelEntity.Components.Add(animationComponent);

                            animationComponent.Play();
                        }
                    }
                }
            }

            if (modelEntity != null)
            {
                foreach (var particleSysBone in conditionState.ParticleSysBones)
                {
                    var particleSystemDefinition = ContentManager.IniDataContext.ParticleSystems.First(x => x.Name == particleSysBone.ParticleSystem);
                    var bone = modelEntity.GetComponent<ModelComponent>().Bones.FirstOrDefault(x => string.Equals(x.Entity.Name, particleSysBone.BoneName, StringComparison.OrdinalIgnoreCase));
                    if (bone == null)
                    {
                        // TODO: Should this ever happen?
                        continue;
                    }

                    var particleSystem = new ParticleSystem(particleSystemDefinition);
                    bone.Entity.Components.Add(particleSystem);
                }
            }

            return result;
        }
    }

    public class W3dModelDrawModuleData : DrawModuleData
    {
        internal static W3dModelDrawModuleData ParseModel(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<W3dModelDrawModuleData> FieldParseTable = new IniParseTable<W3dModelDrawModuleData>
        {
            { "DefaultConditionState", (parser, x) => parser.Temp = x.DefaultConditionState = ModelConditionState.ParseDefault(parser) },
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
    }

    public enum ModelLevelOfDetail
    {
        [IniEnum("MEDIUM")]
        Medium,
    }
}
