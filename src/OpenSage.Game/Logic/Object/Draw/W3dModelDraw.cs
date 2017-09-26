using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using LLGfx;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.Logic.Object
{
    public sealed class W3dModelDraw : Drawable
    {
        private readonly List<W3dModelDrawConditionState> _conditionStates;
        private readonly W3dModelDrawConditionState _defaultConditionState;

        private W3dModelDrawConditionState _activeConditionState;

        public override IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates
        {
            get
            {
                if (_defaultConditionState != null)
                {
                    yield return _defaultConditionState.Flags;
                }

                foreach (var conditionState in _conditionStates)
                {
                    yield return conditionState.Flags;
                }
            }
        }

        public W3dModelDraw(
            W3dModelDrawModuleData data,
            GameContext gameContext,
            ResourceUploadBatch uploadBatch)
        {
            _conditionStates = new List<W3dModelDrawConditionState>();

            if (data.DefaultConditionState != null)
            {
                _defaultConditionState = AddDisposable(new W3dModelDrawConditionState(gameContext, data.DefaultConditionState));
            }

            foreach (var conditionState in data.ConditionStates)
            {
                _conditionStates.Add(AddDisposable(new W3dModelDrawConditionState(gameContext, conditionState)));
            }

            if (_defaultConditionState == null)
            {
                _defaultConditionState = _conditionStates.Find(x => !x.Flags.AnyBitSet);

                if (_defaultConditionState == null)
                {
                    throw new InvalidOperationException();
                }
            }

            SetActiveConditionState(_defaultConditionState);
        }

        private void SetActiveConditionState(W3dModelDrawConditionState conditionState)
        {
            if (_activeConditionState == conditionState)
            {
                return;
            }

            _activeConditionState?.Unload();

            _activeConditionState = conditionState;

            _activeConditionState.Load();
        }

        public override void UpdateConditionState(BitArray<ModelConditionFlag> flags)
        {
            W3dModelDrawConditionState bestConditionState = null;
            var bestMatch = int.MinValue;

            // Find best matching ConditionState.
            foreach (var conditionState in _conditionStates)
            {
                var match = conditionState.Flags.And(flags).NumBitsSet;
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

        public override void Update(GameTime gameTime)
        {
            _activeConditionState.Update(gameTime);
        }

        public override void Draw(
            CommandEncoder commandEncoder,
            MeshEffect meshEffect,
            Camera camera,
            ref Matrix4x4 world)
        {
            _activeConditionState.Draw(
                commandEncoder,
                meshEffect,
                camera,
                ref world);
        }
    }

    internal sealed class W3dModelDrawConditionState : GraphicsObject
    {
        private readonly GameContext _gameContext;
        private readonly ModelConditionState _data;

        private readonly List<AnimationPlayer> _animationPlayers;
        private ModelInstance _modelInstance;

        private readonly List<ParticleSystem> _particleSystems;

        private bool _loaded;

        public BitArray<ModelConditionFlag> Flags { get; }

        public W3dModelDrawConditionState(
            GameContext gameContext, 
            ModelConditionState data)
        {
            _gameContext = gameContext;
            _data = data;

            Flags = data.ConditionFlags;

            _animationPlayers = new List<AnimationPlayer>();
            _particleSystems = new List<ParticleSystem>();
        }

        public void Load()
        {
            if (_loaded)
            {
                foreach (var particleSystem in _particleSystems)
                {
                    _gameContext.ParticleSystemManager.Add(particleSystem);
                }
                return;
            }

            var uploadBatch = new ResourceUploadBatch(_gameContext.GraphicsDevice);
            uploadBatch.Begin();

            if (!string.Equals(_data.Model, "NONE", System.StringComparison.OrdinalIgnoreCase))
            {
                var w3dFilePath = Path.Combine("Art", "W3D", _data.Model + ".W3D");
                var model = _gameContext.ContentManager.Load<Model>(w3dFilePath, uploadBatch);
                if (model != null)
                {
                    _modelInstance = AddDisposable(new ModelInstance(model, _gameContext.GraphicsDevice));
                }
            }

            if (_modelInstance != null)
            {
                // TODO: Multiple animations. Shouldn't play all of them. I think
                // we should randomly choose one of them?
                // And there is also IdleAnimation.
                if (_data.Animations.Count > 0)
                {
                    var objectConditionAnimation = _data.Animations[0];

                    var splitName = objectConditionAnimation.Animation.Split('.');

                    var w3dFilePath = Path.Combine("Art", "W3D", splitName[0] + ".W3D");
                    var model = _gameContext.ContentManager.Load<Model>(w3dFilePath, uploadBatch);

                    var animation = model.Animations.FirstOrDefault(x => string.Equals(x.Name, splitName[1], StringComparison.OrdinalIgnoreCase));
                    if (animation != null)
                    {
                        // TODO: Should this ever be null?

                        var animationPlayer = new AnimationPlayer(animation, _modelInstance);

                        _animationPlayers.Add(animationPlayer);

                        animationPlayer.Start();
                    }
                }
            }

            if (_modelInstance != null)
            {
                foreach (var particleSysBone in _data.ParticleSysBones)
                {
                    var particleSystemDefinition = _gameContext.IniDataContext.ParticleSystems.First(x => x.Name == particleSysBone.ParticleSystem);
                    var bone = _modelInstance.Model.Bones.FirstOrDefault(x => string.Equals(x.Name, particleSysBone.BoneName, StringComparison.OrdinalIgnoreCase));
                    if (bone == null)
                    {
                        // TODO: Should this ever happen?
                        continue;
                    }

                    var particleSystem = AddDisposable(new ParticleSystem(
                        particleSystemDefinition,
                        _gameContext.ContentManager,
                        () => _modelInstance.AbsoluteBoneTransforms[bone.Index] * _savedWorld));

                    _particleSystems.Add(particleSystem);

                    _gameContext.ParticleSystemManager.Add(particleSystem);

                    AddDisposeAction(() => _gameContext.ParticleSystemManager.Remove(particleSystem));
                }
            }

            uploadBatch.End();

            _loaded = true;
        }

        public void Unload()
        {
            foreach (var particleSystem in _particleSystems)
            {
                _gameContext.ParticleSystemManager.Remove(particleSystem);
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var animationPlayer in _animationPlayers)
            {
                animationPlayer.Update(gameTime);
            }
        }

        // TODO: Don't do this.
        private Matrix4x4 _savedWorld;

        public void Draw(
            CommandEncoder commandEncoder,
            MeshEffect meshEffect,
            Camera camera,
            ref Matrix4x4 world)
        {
            if (_modelInstance == null)
            {
                return;
            }

            _savedWorld = world;

            _modelInstance.Draw(
                commandEncoder,
                meshEffect,
                camera,
                ref world);
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
                    if (parser.CurrentTokenType == IniTokenType.Identifier)
                    {
                        var conditionState = ModelConditionState.Parse(parser);
                        x.ConditionStates.Add(conditionState);
                        parser.Temp = conditionState;
                    }
                    else
                    {
                        // ODDITY: ZH ChemicalGeneral.ini:10694 uses ConditionState with no flag,
                        // instead of DefaultConditionState.
                        parser.Temp = x.DefaultConditionState = ModelConditionState.ParseDefault(parser);
                    }
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
