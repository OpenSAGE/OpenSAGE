﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object
{
    public sealed class GameObject : DisposableBase
    {
        public ObjectDefinition Definition { get; }

        public Transform Transform { get; }

        public IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; }

        public BitArray<ModelConditionFlag> ModelConditionFlags { get; private set; }

        public IReadOnlyList<DrawModule> DrawModules { get; }

        public Collider Collider { get; }

        // TODO: This could use a smaller fixed point type.
        public decimal Health { get; set; }

        public Player Owner { get; set; }

        public Team Team { get; set; }

        public bool IsSelectable { get; set; }

        public bool IsSelected { get; set; }

        public Vector3 RallyPoint { get; set; }

        private Locomotor CurrentLocomotor { get; set; }

        private IniDataContext Context { get; set; }

        private Vector3? TargetPoint { get; set; }

        private TimeSpan ConstructionStart { get; set; }

        public GameObject(ObjectDefinition objectDefinition, ContentManager contentManager, Player owner)
        {
            Definition = objectDefinition;
            Context = contentManager.IniDataContext;
            Owner = owner;

            SetLocomotor();
            Transform = Transform.CreateIdentity();

            var drawModules = new List<DrawModule>();
            foreach (var drawData in objectDefinition.Draws)
            {
                var drawModule = AddDisposable(drawData.CreateDrawModule(contentManager));
                if (drawModule != null)
                {
                    // TODO: This will never be null once we've implemented all the draw modules.
                    drawModules.Add(drawModule);
                }
            }
            DrawModules = drawModules;

            Collider = Collider.Create(objectDefinition, Transform);

            ModelConditionStates = drawModules
                .SelectMany(x => x.ModelConditionStates)
                .Distinct(new BitArrayEqualityComparer<ModelConditionFlag>())
                .OrderBy(x => x.NumBitsSet)
                .ToList();

            SetModelConditionFlags(new BitArray<ModelConditionFlag>());

            IsSelectable = Definition.KindOf?.Get(ObjectKinds.Selectable) ?? false;
        }

        internal IEnumerable<AttachedParticleSystem> GetAllAttachedParticleSystems()
        {
            foreach (var drawModule in DrawModules)
            {
                foreach (var attachedParticleSystem in drawModule.GetAllAttachedParticleSystems())
                {
                    yield return attachedParticleSystem;
                }
            }
        }

        internal void LogicTick(ulong frame)
        {
            // TODO: Update modules.
        }

        internal void MoveTo(Vector3 targetPos)
        {
            if (Definition.KindOf == null) return;

            if (Definition.KindOf.Get(ObjectKinds.Infantry)
                || Definition.KindOf.Get(ObjectKinds.Vehicle))
            {
                TargetPoint = targetPos;

                //TODO: calculate angle
            }

            var flags = new BitArray<ModelConditionFlag>();
            flags.Set(ModelConditionFlag.Moving, true);
            SetModelConditionFlags(flags);
        }

        internal void StartConstruction(in TimeInterval gameTime)
        {
            if (Definition.KindOf == null) return;

            if (Definition.KindOf.Get(ObjectKinds.Structure))
            {
                var flags = new BitArray<ModelConditionFlag>();
                flags.Set(ModelConditionFlag.ActivelyBeingConstructed, true);
                flags.Set(ModelConditionFlag.AwaitingConstruction, true);
                flags.Set(ModelConditionFlag.PartiallyConstructed, true);
                SetModelConditionFlags(flags);
                ConstructionStart = gameTime.TotalTime;
                //ConstructionTick = TimeSpan.FromSeconds(Definition.BuildTime) / 100.0f;
            }
        }

        internal void LocalLogicTick(in TimeInterval gameTime, float tickT, HeightMap heightMap)
        {
            var flags = new BitArray<ModelConditionFlag>();

            // Check if the unit is currently moving
            flags.Set(ModelConditionFlag.Moving, true);
            if (ModelConditionFlags.And(flags).AnyBitSet && TargetPoint.HasValue)
            {
                // This locomotor speed is distance/second
                var distance = CurrentLocomotor.Speed * (gameTime.DeltaTime.Milliseconds / 1000.0f);

                var direction = Vector3.Normalize(TargetPoint.Value - Transform.Translation);
                Transform.Translation += direction * distance;

                var x = Transform.Translation.X;
                var y = Transform.Translation.Y;
                var z = heightMap.GetHeight(x, y);

                Transform.Translation = new Vector3(x, y, z);

                if (Vector3.Distance(Transform.Translation, TargetPoint.Value) < 0.5f)
                {
                    TargetPoint = null;
                    SetModelConditionFlags(new BitArray<ModelConditionFlag>());
                }
            }

            // Check if the unit is being constructed
            flags.SetAll(false);
            flags.Set(ModelConditionFlag.ActivelyBeingConstructed, true);
            flags.Set(ModelConditionFlag.AwaitingConstruction, true);
            flags.Set(ModelConditionFlag.PartiallyConstructed, true);
            if (ModelConditionFlags.And(flags).AnyBitSet)
            {
                if (gameTime.TotalTime > (ConstructionStart + TimeSpan.FromSeconds(Definition.BuildTime)))
                {
                    SetModelConditionFlags(new BitArray<ModelConditionFlag>());
                }
            }

            // Update all draw modules
            foreach (var drawModule in DrawModules)
            {
                drawModule.Update(gameTime);
            }

            // TODO: Make sure we've processed everything that might update
            // this object's transform before updating draw modules' position.
            var worldMatrix = Transform.Matrix;
            foreach (var drawModule in DrawModules)
            {
                drawModule.SetWorldMatrix(worldMatrix);
            }
        }

        internal void BuildRenderList(RenderList renderList, Camera camera)
        {
            var castsShadow = false;
            switch (Definition.Shadow)
            {
                case ObjectShadowType.ShadowVolume:
                case ObjectShadowType.ShadowVolumeNew:
                    castsShadow = true;
                    break;
            }

            foreach (var drawModule in DrawModules)
            {
                drawModule.BuildRenderList(
                    renderList,
                    camera,
                    castsShadow,
                    Owner);
            }
        }

        public void SetModelConditionFlags(BitArray<ModelConditionFlag> flags)
        {
            ModelConditionFlags = flags;

            // TODO: Let each drawable use the appropriate TransitionState between ConditionStates.

            foreach (var drawModule in DrawModules)
            {
                drawModule.UpdateConditionState(flags);
            }
        }

        public void OnLocalSelect(AudioSystem gameAudio)
        {
            if (Definition.VoiceSelect != null)
            {
                gameAudio.PlayAudioEvent(Definition.VoiceSelect);
            }
        }

        public void OnLocalMove(AudioSystem gameAudio)
        {
            if (Definition.VoiceMove != null)
            {
                gameAudio.PlayAudioEvent(Definition.VoiceMove);
            }
        }

        private void SetLocomotor()
        {
            var locoDefs = Definition.Locomotors;
            if (locoDefs.Count > 0)
            {
                var name = locoDefs.First().Value[0];
                if (Context.Locomotors.ContainsKey(name))
                {
                    CurrentLocomotor = Context.Locomotors[name];
                }
            }
        }
    }
}
