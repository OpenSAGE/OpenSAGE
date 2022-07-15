using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class W3dModelDraw : DrawModule
    {
        private readonly W3dModelDrawModuleData _data;
        private readonly GameContext _context;

        private ModelConditionState _activeConditionState;
        protected AnimationState _activeAnimationState;
        private AnimationFlags _activeAnimationFlags;

        private W3dModelDrawConditionState _activeModelDrawConditionState;

        private readonly List<W3dModelDrawSomething>[] _unknownSomething;
        private bool _hasUnknownThing;
        private int _unknownInt;
        private float _unknownFloat;

        public AnimationState PreviousAnimationState { get; private set; }

        protected ModelInstance ActiveModelInstance => _activeModelDrawConditionState.Model;

        public override IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates
        {
            get
            {
                foreach (var conditionState in _data.ConditionStates)
                {
                    foreach (var conditionStateFlags in conditionState.ConditionFlags)
                    {
                        yield return conditionStateFlags;
                    }
                }

                foreach (var animationState in _data.AnimationStates)
                {
                    foreach (var animationStateFlags in animationState.ConditionFlags)
                    {
                        yield return animationStateFlags;
                    }
                }
            }
        }

        internal override string GetWeaponFireFXBone(WeaponSlot slot)
            => _activeConditionState?.WeaponFireFXBones.Find(x => x.WeaponSlot == slot)?.BoneName;

        internal override string GetWeaponLaunchBone(WeaponSlot slot)
            => _activeConditionState?.WeaponLaunchBones.Find(x => x.WeaponSlot == slot)?.BoneName;

        internal W3dModelDraw(
            W3dModelDrawModuleData data,
            Drawable drawable,
            GameContext context)
        {
            _data = data;
            Drawable = drawable;
            _context = context;

            UpdateConditionState(new BitArray<ModelConditionFlag>(), context.Random);

            _unknownSomething = new List<W3dModelDrawSomething>[3];
            for (var i = 0; i < _unknownSomething.Length; i++)
            {
                _unknownSomething[i] = new List<W3dModelDrawSomething>();
            }
        }

        private void SetActiveConditionState(ModelConditionState conditionState, Random random)
        {
            if (_activeConditionState == conditionState || ShouldWaitForRunningAnimationsToFinish())
            {
                return;
            }

            if (_activeModelDrawConditionState != null)
            {
                _activeModelDrawConditionState.Deactivate();
                RemoveAndDispose(ref _activeModelDrawConditionState);
            }

            var modelDrawConditionState = AddDisposable(CreateModelDrawConditionStateInstance(conditionState, random));

            _activeConditionState = conditionState;
            _activeModelDrawConditionState = modelDrawConditionState;

            NLog.LogManager.GetCurrentClassLogger().Info($"Set active condition state for {GameObject.Definition.Name}");
        }

        private bool ShouldWaitForRunningAnimationsToFinish()
        {
            return false;

            //return _activeConditionState != null;
                // TODO
                //&& animationState.WaitForStateToFinishIfPossible != null
                //&& _activeConditionState.TransitionKey == conditionState.WaitForStateToFinishIfPossible
                //&& (_activeModelDrawConditionState?.StillActive() ?? false);
        }

        protected virtual bool SetActiveAnimationState(AnimationState animationState, Random random)
        {
            if (animationState == _activeAnimationState && (_activeModelDrawConditionState?.StillActive() ?? false))
            {
                return false;
            }

            if (animationState == null
                || animationState.Animations.Count == 0
                || _activeModelDrawConditionState == null)
            {
                return true;
            }

            PreviousAnimationState = _activeAnimationState;

            if (_activeModelDrawConditionState?.Model != null)
            {
                foreach (var animationInstance in _activeModelDrawConditionState.Model.AnimationInstances)
                {
                    animationInstance.Stop();
                }
            }

            var modelInstance = _activeModelDrawConditionState.Model;
            modelInstance.AnimationInstances.Clear();
            _activeAnimationState = animationState;

            var animationBlock = animationState.Animations[random.Next(0, animationState.Animations.Count - 1)];
            if (animationBlock != null)
            {
                var anim = animationBlock.Animation.Value;
                //Check if the animation does really exist
                if (anim != null)
                {
                    var flags = animationState.Flags;
                    var mode = animationBlock.AnimationMode;
                    var animationInstance = new AnimationInstance(
                        modelInstance.ModelBoneInstances,
                        anim,
                        mode,
                        flags.HasFlag(AnimationFlags.StartFrameLast) || ((mode == AnimationMode.OnceBackwards || mode == AnimationMode.LoopBackwards) && !flags.HasFlag(AnimationFlags.StartFrameFirst)));
                    modelInstance.AnimationInstances.Add(animationInstance);
                    animationInstance.Play(animationBlock.AnimationSpeedFactorRange.GetValue(random));
                    ResetAnimationTimeStamps(animationInstance, anim, mode, flags);
                    _activeAnimationFlags = flags;
                }
            }

            if (animationState != null && animationState.Script != null)
            {
                _context.Scene3D.Game.Lua.ExecuteDrawModuleLuaCode(this, animationState.Script);
            }

            NLog.LogManager.GetCurrentClassLogger().Info($"Set active animation state for {GameObject.Definition.Name}");

            return true;
        }

        private void ResetAnimationTimeStamps(
            AnimationInstance animationInstance,
            W3DAnimation animation,
            AnimationMode mode,
            AnimationFlags flags)
        {
            if (flags == AnimationFlags.None ||
                flags.HasFlag(AnimationFlags.StartFrameFirst) ||
                flags.HasFlag(AnimationFlags.StartFrameLast))
            {
                if (mode == AnimationMode.OnceBackwards || mode == AnimationMode.LoopBackwards || flags.HasFlag(AnimationFlags.StartFrameLast))
                {
                    animationInstance.SetTime(animation.Clips.Max(c => c.Keyframes.LastOrDefault().Time));
                }
                else
                {
                    animationInstance.SetTime(TimeSpan.Zero);
                }
            }
            else if (flags.HasFlag(AnimationFlags.RandomStart))
            {
                animationInstance.SetTime(TimeSpan.FromMilliseconds(_context.Random.Next((int)animation.Duration.TotalMilliseconds)));
            }
            else
            {
                //TODO: implement other flags
                //throw new NotImplementedException();
            }
        }

        public void SetTransitionState(string state)
        {
            var transitionState = _data.TransitionStates.FirstOrDefault(x => x.StateName == state);
            SetActiveAnimationState(transitionState, _context.Random);
        }

        private static T FindBestFittingConditionState<T>(List<T> conditionStates, BitArray<ModelConditionFlag> flags)
            where T : IConditionState
        {
            T bestConditionState = default;
            var bestIntersections = 0;

            foreach (var conditionState in conditionStates)
            {
                foreach (var conditionStateFlags in conditionState.ConditionFlags)
                {
                    var numStateBits = conditionStateFlags.NumBitsSet;
                    var numIntersectionBits = conditionStateFlags.CountIntersectionBits(flags);

                    if (numIntersectionBits < bestIntersections
                        || numStateBits > numIntersectionBits)
                    {
                        continue;
                    }

                    bestConditionState = conditionState;
                    bestIntersections = numIntersectionBits;
                }
            }

            return bestConditionState;
        }

        public override void UpdateConditionState(BitArray<ModelConditionFlag> flags, Random random)
        {
            if (!flags.BitsChanged)
            {
                if (!(_activeModelDrawConditionState?.StillActive() ?? false)
                    && _activeAnimationState != null
                    && _activeAnimationState.IsIdleAnimation)
                {
                    SetActiveAnimationState(_activeAnimationState, random);
                }
                return;
            }

            var bestConditionState = FindBestFittingConditionState(_data.ConditionStates, flags);
            SetActiveConditionState(bestConditionState, random);

            foreach (var weaponMuzzleFlash in bestConditionState.WeaponMuzzleFlashes)
            {
                var visible = flags.Get(ModelConditionFlag.FiringA);
                for (var i = 0; i < _activeModelDrawConditionState.Model.ModelBoneInstances.Length; i++)
                {
                    var bone = _activeModelDrawConditionState.Model.ModelBoneInstances[i];
                    // StartsWith is a bit awkward here, but for instance AVCommance has WeaponMuzzleFlashes = { TurretFX }, and Bones = { TURRETFX01 }
                    if (bone.Name.StartsWith(weaponMuzzleFlash.BoneName, StringComparison.OrdinalIgnoreCase))
                    {
                        _activeModelDrawConditionState.Model.BoneVisibilities[i] = visible;
                    }
                }
            };

            var bestAnimationState = FindBestFittingConditionState(_data.AnimationStates, flags);
            SetActiveAnimationState(bestAnimationState, random);
        }

        private W3dModelDrawConditionState CreateModelDrawConditionStateInstance(
            ModelConditionState conditionState,
            Random random)
        {
            // Load model, fallback to default model.
            var model = conditionState.Model?.Value;
            var modelInstance = model?.CreateInstance(_context.AssetLoadContext.GraphicsDevice, _context.AssetLoadContext.StandardGraphicsResources, _context.AssetLoadContext.ShaderResources.Mesh) ?? null;

            if (modelInstance == null)
            {
                return null;
            }

            var particleSystems = new List<ParticleSystem>();

            foreach (var particleSysBone in conditionState.ParticleSysBones)
            {
                var particleSystemTemplate = particleSysBone.ParticleSystem.Value;
                if (particleSystemTemplate == null)
                {
                    throw new InvalidOperationException();
                }

                var bone = modelInstance.Model.BoneHierarchy.Bones.FirstOrDefault(x => string.Equals(x.Name, particleSysBone.BoneName, StringComparison.OrdinalIgnoreCase));
                if (bone == null)
                {
                    // TODO: Should this ever happen?
                    continue;
                }

                particleSystems.Add(_context.ParticleSystems.Create(
                    particleSystemTemplate,
                    () => ref modelInstance.AbsoluteBoneTransforms[bone.Index]));
            }

            return new W3dModelDrawConditionState(modelInstance, particleSystems, _context);
        }

        internal override (ModelInstance, ModelBone) FindBone(string boneName)
        {
            return (ActiveModelInstance, ActiveModelInstance.Model.BoneHierarchy.Bones.FirstOrDefault(x => string.Equals(x.Name, boneName, StringComparison.OrdinalIgnoreCase)));
        }

        internal ModelBoneInstance FindBoneInstance(string name)
        {
            foreach (var bone in ActiveModelInstance.Model.BoneHierarchy.Bones)
            {
                if (bone.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return ActiveModelInstance.ModelBoneInstances[bone.Index];
                }
            }

            return null;
        }

        internal override void Update(in TimeInterval gameTime)
        {
            if (_activeAnimationState?.Flags.HasFlag(AnimationFlags.AdjustHeightByConstructionPercent) ?? false)
            {
                var progress = GameObject.BuildProgress;
                GameObject.VerticalOffset = -((1.0f - progress) * GameObject.Geometry.MaxZ);
            }

            _activeModelDrawConditionState?.Update(gameTime);
        }

        internal override void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
            if (GameObject.VerticalOffset != 0)
            {
                var mat = worldMatrix * Matrix4x4.CreateTranslation(Vector3.UnitZ * GameObject.VerticalOffset);
                _activeModelDrawConditionState?.SetWorldMatrix(mat);
            }
            else
            {
                _activeModelDrawConditionState?.SetWorldMatrix(worldMatrix);
            }
        }

        internal override void BuildRenderList(
            RenderList renderList,
            Camera camera,
            bool castsShadow,
            MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS,
            Dictionary<string, bool> shownSubObjects = null,
            Dictionary<string, bool> hiddenSubObjects = null)
        {
            _activeModelDrawConditionState?.BuildRenderList(
                renderList,
                camera,
                castsShadow,
                renderItemConstantsPS,
                shownSubObjects,
                hiddenSubObjects);
        }

        internal override void DrawInspector()
        {
            foreach (var conditionFlags in _activeConditionState.ConditionFlags)
            {
                ImGui.LabelText("ConditionFlags", conditionFlags.DisplayName);
            }

            _activeModelDrawConditionState?.Model?.DrawInspector();
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistArray(
                _unknownSomething,
                static (StatePersister persister, ref List<W3dModelDrawSomething> item) =>
                {
                    persister.PersistListWithByteCountValue(item, static (StatePersister persister, ref W3dModelDrawSomething item) =>
                    {
                        persister.PersistObjectValue(ref item);
                    });
                });

            reader.SkipUnknownBytes(1);

            reader.PersistBoolean(ref _hasUnknownThing);
            if (_hasUnknownThing)
            {
                reader.PersistInt32(ref _unknownInt);
                reader.PersistSingle(ref _unknownFloat);
            }
        }

        public struct W3dModelDrawSomething : IPersistableObject
        {
            public uint UnknownInt;
            public float UnknownFloat1;
            public float UnknownFloat2;

            public void Persist(StatePersister persister)
            {
                persister.PersistUInt32(ref UnknownInt);
                persister.PersistSingle(ref UnknownFloat1);
                persister.PersistSingle(ref UnknownFloat2);
            }
        }
    }

    internal sealed class W3dModelDrawConditionState : DisposableBase
    {
        private readonly IEnumerable<ParticleSystem> _particleSystems;
        private readonly GameContext _context;

        public readonly ModelInstance Model;

        public W3dModelDrawConditionState(
            ModelInstance modelInstance,
            IEnumerable<ParticleSystem> particleSystems,
            GameContext context)
        {
            Model = AddDisposable(modelInstance);

            _particleSystems = particleSystems;
            _context = context;
        }

        public bool StillActive() => Model.AnimationInstances.Any(x => x.IsPlaying);

        public void Deactivate()
        {
            foreach (var particleSystem in _particleSystems)
            {
                particleSystem.Finish();
            }
        }

        public void Update(in TimeInterval gameTime)
        {
            Model.Update(gameTime);
        }

        public void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
            Model.SetWorldMatrix(worldMatrix);
        }

        public void BuildRenderList(
            RenderList renderList,
            Camera camera,
            bool castsShadow,
            MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS,
            Dictionary<string, bool> shownSubObjects = null,
            Dictionary<string, bool> hiddenSubObjects = null)
        {
            Model.BuildRenderList(
                renderList,
                camera,
                castsShadow,
                renderItemConstantsPS,
                shownSubObjects,
                hiddenSubObjects);
        }
    }

    public class W3dModelDrawModuleData : DrawModuleData, IParseCallbacks
    {
        internal static W3dModelDrawModuleData ParseModel(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<W3dModelDrawModuleData> FieldParseTable = new IniParseTable<W3dModelDrawModuleData>
        {
            { "DefaultConditionState", (parser, x) => x.ParseConditionStateGenerals(parser, ParseConditionStateType.Default) },
            { "ConditionState", (parser, x) => x.ParseConditionStateGenerals(parser, ParseConditionStateType.Normal) },

            { "DefaultModelConditionState", (parser, x) => x.ParseConditionState(parser, ParseConditionStateType.Default) },
            { "ModelConditionState", (parser, x) => x.ParseConditionState(parser, ParseConditionStateType.Normal) },

            { "IgnoreConditionStates", (parser, x) => x.IgnoreConditionStates = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "AliasConditionState", (parser, x) => x.ParseAliasConditionState(parser) },

            { "TransitionState", (parser, x) => x.ParseTransitionState(parser) },

            { "OkToChangeModelColor", (parser, x) => x.OkToChangeModelColor = parser.ParseBoolean() },
            { "ReceivesDynamicLights", (parser, x) => x.ReceivesDynamicLights = parser.ParseBoolean() },
            { "ProjectileBoneFeedbackEnabledSlots", (parser, x) => x.ProjectileBoneFeedbackEnabledSlots = parser.ParseEnumBitArray<WeaponSlot>() },
            { "AnimationsRequirePower", (parser, x) => x.AnimationsRequirePower = parser.ParseBoolean() },
            { "ParticlesAttachedToAnimatedBones", (parser, x) => x.ParticlesAttachedToAnimatedBones = parser.ParseBoolean() },
            { "MinLODRequired", (parser, x) => x.MinLodRequired = parser.ParseEnum<ModelLevelOfDetail>() },
            { "ExtraPublicBone", (parser, x) => x.ExtraPublicBones.Add(parser.ParseBoneName()) },
            { "AttachToBoneInAnotherModule", (parser, x) => x.AttachToBoneInAnotherModule = parser.ParseBoneName() },
            { "TrackMarks", (parser, x) => x.TrackMarks = parser.ParseTextureReference() },
            { "TrackMarksLeftBone", (parser, x) => x.TrackMarksLeftBone = parser.ParseAssetReference() },
            { "TrackMarksRightBone", (parser, x) => x.TrackMarksRightBone = parser.ParseAssetReference() },
            { "InitialRecoilSpeed", (parser, x) => x.InitialRecoilSpeed = parser.ParseFloat() },
            { "MaxRecoilDistance", (parser, x) => x.MaxRecoilDistance = parser.ParseFloat() },
            { "RecoilSettleSpeed", (parser, x) => x.RecoilSettleSpeed = parser.ParseFloat() },

            { "IdleAnimationState", (parser, x) => { var animationState = AnimationState.Parse(parser, ParseConditionStateType.Normal); animationState.IsIdleAnimation = true; x.AnimationStates.Add(animationState); } },
            { "AnimationState", (parser, x) => x.AnimationStates.Add(AnimationState.Parse(parser, ParseConditionStateType.Normal)) },
        };

        private readonly List<ModelConditionStateGenerals> _conditionStatesGenerals = new();
        private readonly List<TransitionStateGenerals> _transitionStatesGenerals = new();

        public BitArray<ModelConditionFlag> IgnoreConditionStates { get; private set; }
        public List<ModelConditionState> ConditionStates { get; } = new List<ModelConditionState>();
        public List<AnimationState> TransitionStates { get; } = new List<AnimationState>();

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

        public LazyAssetReference<TextureAsset> TrackMarks { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string TrackMarksLeftBone { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public string TrackMarksRightBone { get; private set; }

        public float InitialRecoilSpeed { get; private set; } = 2.0f;
        public float MaxRecoilDistance { get; private set; } = 3.0f;
        public float RecoilSettleSpeed { get; private set; } = 0.065f;

        public List<AnimationState> AnimationStates { get; } = new List<AnimationState>();

        private void ParseConditionStateGenerals(IniParser parser, ParseConditionStateType type)
        {
            ModelConditionStateGenerals modelConditionState;

            switch (type)
            {
                case ParseConditionStateType.Default:
                    modelConditionState = new ModelConditionStateGenerals();
                    modelConditionState.ConditionFlags.Add(new BitArray<ModelConditionFlag>());
                    break;

                case ParseConditionStateType.Normal:
                    modelConditionState = _conditionStatesGenerals.Count > 0
                        ? _conditionStatesGenerals[0].Clone() // TODO: This isn't right, it should only clone a Default state.
                        : new ModelConditionStateGenerals();
                    modelConditionState.ConditionFlags.Clear();
                    modelConditionState.ConditionFlags.Add(parser.ParseEnumBitArray<ModelConditionFlag>());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            ModelConditionStateGenerals.Parse(parser, modelConditionState);
            _conditionStatesGenerals.Add(modelConditionState);
            parser.Temp = modelConditionState;
        }

        private void ParseAliasConditionState(IniParser parser)
        {
            if (parser.Temp is not ModelConditionStateGenerals lastConditionState)
            {
                throw new IniParseException("Cannot use AliasConditionState if there are no preceding ConditionStates", parser.CurrentPosition);
            }

            var conditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            lastConditionState.ConditionFlags.Add(conditionFlags);
        }

        private void ParseConditionState(IniParser parser, ParseConditionStateType type)
        {
            ModelConditionState modelConditionState;

            switch (type)
            {
                case ParseConditionStateType.Default:
                    modelConditionState = new ModelConditionState();
                    modelConditionState.ConditionFlags.Add(new BitArray<ModelConditionFlag>());
                    break;

                case ParseConditionStateType.Normal:
                    modelConditionState = ConditionStates.Count > 0
                        ? ConditionStates[0].Clone() // TODO: This isn't right, it should only clone a Default state.
                        : new ModelConditionState();
                    modelConditionState.ConditionFlags.Clear();
                    modelConditionState.ConditionFlags.Add(parser.ParseEnumBitArray<ModelConditionFlag>());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            ModelConditionState.Parse(parser, modelConditionState);
            ConditionStates.Add(modelConditionState);
            parser.Temp = modelConditionState;
        }

        /// <summary>
        /// In Generals, transition states were defined like this:
        ///
        /// ```
        /// ConditionState = FIRING_A 
        ///         Animation         = NICNSC_SKL.NICNSC_ATA
        ///         AnimationMode = LOOP
        ///     TransitionKey     = TRANS_Firing
        /// End
        /// 
        /// ConditionState = FIRING_A REALLYDAMAGED
        ///     Animation         = NICNSC_SKL.NICNSC_ATC
        ///     AnimationMode = LOOP
        ///     TransitionKey     = TRANS_FiringDamaged
        /// End
        /// 
        /// TransitionState = TRANS_Firing TRANS_FiringDamaged
        ///     Animation       = NICNSC_SKL.NICNSC_AA2AC
        ///     AnimationMode = ONCE
        /// End
        /// ```
        ///
        /// but BFME and later games defined transition states like this:
        ///
        /// ```
        /// AnimationState = FIRING_A
        ///     StateName = FIRING_A
        ///     Animation = ATA
        ///         AnimationName = NICNSC_SKL.NICNSC_ATA
        ///         AnimationMode = LOOP
        ///     End
        /// End
        /// 
        /// AnimationState = FIRING_A REALLYDAMAGED
        ///     StateName = FIRING_A_REALLYDAMAGED
        ///     Animation = ATC
        ///         AnimationName = NICNSC_SKL.NICNSC_ATC
        ///         AnimationMode = LOOP
        ///     End
        ///     BeginScript
        ///         Prev = CurDrawablePrevAnimationState()
        ///         if Prev == "FIRING_A" then CurDrawableSetTransitionAnimState("TransitionFiringToFiringDamaged") end
        ///     EndScript
        /// End
        /// 
        /// TransitionState = TRANS_Firing_TRANS_FiringDamaged
        ///     Animation = AA2AC
        ///         AnimationName = NICNSC_SKL.NICNSC_AA2AC
        ///         AnimationMode = ONCE
        ///     End
        /// End
        /// ```
        ///
        /// We only want to deal with one way of doing it, so here we translate Generals style
        /// into BFME style.
        /// </summary>
        private void ParseTransitionState(IniParser parser)
        {
            if (parser.SageGame == SageGame.CncGenerals || parser.SageGame == SageGame.CncGeneralsZeroHour)
            {
                var transitionState = new TransitionStateGenerals
                {
                    From = parser.ParseIdentifier(),
                    To = parser.ParseIdentifier(),
                };

                ModelConditionStateGenerals.Parse(parser, transitionState);

                _transitionStatesGenerals.Add(transitionState);
            }
            else
            {
                TransitionStates.Add(AnimationState.Parse(parser, ParseConditionStateType.Transition));
            }
        }

        public void OnParsed()
        {
            foreach (var conditionState in _conditionStatesGenerals)
            {
                // TODO: If this ConditionState only has animation properties set,
                // skip creating a ModelConditionState for it.

                if (conditionState.NonAnimationPropertySet)
                {
                    var modelConditionState = new ModelConditionState();
                    conditionState.CopyTo(modelConditionState);
                    ConditionStates.Add(modelConditionState);
                }

                if (conditionState.AnimationPropertySet)
                {
                    var animationState = new AnimationState
                    {
                        StateName = conditionState.TransitionKey,
                        Script = $"Prev = CurDrawablePrevAnimationState{Environment.NewLine}",
                    };

                    conditionState.CopyTo(animationState);

                    if (conditionState.WaitForStateToFinishIfPossible != null)
                    {
                        animationState.Script += $"if Prev == \"{conditionState.WaitForStateToFinishIfPossible}\" then CurDrawableAllowToContinue() end{Environment.NewLine}";
                    }

                    foreach (var subObject in conditionState.HideSubObject)
                    {
                        animationState.Script += $"CurDrawableHideSubObject(\"{subObject}\"){Environment.NewLine}";
                    }

                    foreach (var subObject in conditionState.ShowSubObject)
                    {
                        animationState.Script += $"CurDrawableShowSubObject(\"{subObject}\"){Environment.NewLine}";
                    }

                    AnimationStates.Add(animationState);
                }
            }

            foreach (var transitionStateOld in _transitionStatesGenerals)
            {
                var transitionName = $"{transitionStateOld.From}_{transitionStateOld.To}";

                var transitionStateNew = new AnimationState
                {
                    StateName = transitionName,
                };

                transitionStateOld.CopyTo(transitionStateNew);

                TransitionStates.Add(transitionStateNew);

                foreach (var toAnimationState in AnimationStates.Where(x => x.StateName == transitionStateOld.To))
                {
                    toAnimationState.Script += $"if Prev == \"{transitionStateOld.From}\" then CurDrawableSetTransitionAnimState(\"{transitionName}\") end{Environment.NewLine}";
                }
            }

            _conditionStatesGenerals.Clear();
            _transitionStatesGenerals.Clear();
        }

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return new W3dModelDraw(this, drawable, context);
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
