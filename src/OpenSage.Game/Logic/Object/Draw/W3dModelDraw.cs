using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;

namespace OpenSage.Logic.Object
{
    public sealed class W3dModelDraw : Drawable
    {
        private readonly List<W3dModelDrawConditionState> _conditionStates;
        private readonly W3dModelDrawConditionState _defaultConditionState;

        private W3dModelDrawConditionState _activeConditionState;

        public W3dModelDraw(
            W3dModelDrawModuleData data,
            FileSystem fileSystem,
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch)
        {
            _conditionStates = new List<W3dModelDrawConditionState>();

            if (data.DefaultConditionState != null)
            {
                var defaultConditionState = AddDisposable(new W3dModelDrawConditionState(contentManager, uploadBatch, data.DefaultConditionState));
                _conditionStates.Add(defaultConditionState);
            }

            foreach (var conditionState in data.ConditionStates)
            {
                // TODO
                if (!conditionState.ConditionFlags.AnyBitSet)
                {
                    _conditionStates.Add(AddDisposable(new W3dModelDrawConditionState(contentManager, uploadBatch, conditionState)));
                }
            }

            _defaultConditionState = _conditionStates.Find(x => !x.Flags.AnyBitSet);
            _activeConditionState = _defaultConditionState;
        }

        public override void OnModelConditionStateChanged(BitArray<ModelConditionFlag> state)
        {
            _activeConditionState = _conditionStates.Find(x => x.Flags.Equals(state)) ?? _defaultConditionState;
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
        private readonly ModelInstance _modelInstance;
        private readonly List<AnimationPlayer> _animationPlayers;

        public BitArray<ModelConditionFlag> Flags { get; }

        public W3dModelDrawConditionState(ContentManager contentManager, ResourceUploadBatch uploadBatch, ModelConditionState data)
        {
            Flags = data.ConditionFlags;

            if (!string.Equals(data.Model, "NONE", System.StringComparison.OrdinalIgnoreCase))
            {
                var w3dFilePath = Path.Combine("Art", "W3D", data.Model + ".W3D");
                var model = contentManager.Load<Model>(w3dFilePath, uploadBatch);
                if (model != null)
                {
                    _modelInstance = AddDisposable(new ModelInstance(model, contentManager.GraphicsDevice));
                }
            }

            _animationPlayers = new List<AnimationPlayer>();
            if (_modelInstance != null)
            {
                // TODO: Multiple animations. Shouldn't play all of them. I think
                // we should randomly choose one of them?
                // And there is also IdleAnimation.
                foreach (var objectConditionAnimation in data.Animations)
                {
                    var splitName = objectConditionAnimation.Animation.Split('.');

                    var w3dFilePath = Path.Combine("Art", "W3D", splitName[0] + ".W3D");
                    var model = contentManager.Load<Model>(w3dFilePath, uploadBatch);

                    var animation = model.Animations.First(x => string.Equals(x.Name, splitName[1], StringComparison.OrdinalIgnoreCase));

                    var animationPlayer = new AnimationPlayer(animation, _modelInstance);

                    _animationPlayers.Add(animationPlayer);

                    animationPlayer.Start();
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var animationPlayer in _animationPlayers)
            {
                animationPlayer.Update(gameTime);
            }
        }

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
