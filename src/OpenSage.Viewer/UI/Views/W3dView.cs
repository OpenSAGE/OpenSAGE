﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Data.W3d;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Settings;
using OpenSage.Viewer.Framework;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class W3dView : GameView
    {
        private readonly List<W3dItem> _subObjects;
        private W3dItem _selectedItem;

        public W3dView(AssetViewContext context)
            : base(context)
        {
            var game = context.Game;

            var modelInstance = game.ContentManager
                .Load<Model>(context.Entry.FilePath)
                .CreateInstance(game.GraphicsDevice);

            void onUpdating(object sender, GameUpdatingEventArgs e) => modelInstance.Update(e.GameTime);

            game.Updating += onUpdating;
            AddDisposeAction(() => game.Updating -= onUpdating);

            void onBuildingRenderList(object sender, BuildingRenderListEventArgs e)
            {
                modelInstance.SetWorldMatrix(Matrix4x4.Identity);
                modelInstance.BuildRenderList(e.RenderList, e.Camera, true);
            }

            game.BuildingRenderList += onBuildingRenderList;
            AddDisposeAction(() => game.BuildingRenderList -= onBuildingRenderList);

            var enclosingBoundingBox = GetEnclosingBoundingBox(modelInstance);

            var cameraController = new ArcballCameraController(
                enclosingBoundingBox.GetCenter(),
                Vector3.Distance(enclosingBoundingBox.Min, enclosingBoundingBox.Max));

            game.Scene3D = new Scene3D(
                game,
                cameraController,
                null,
                null,
                Array.Empty<Terrain.WaterArea>(),
                Array.Empty<Terrain.Road>(),
                Array.Empty<Terrain.Bridge>(),
                null,
                new GameObjectCollection(game.ContentManager),
                new WaypointCollection(),
                new WaypointPathCollection(),
                WorldLighting.CreateDefault(),
                Array.Empty<Player>(),
                Array.Empty<Team>());

            var animations = new List<AnimationInstance>(modelInstance.AnimationInstances);

            var w3dFile = W3dFile.FromFileSystemEntry(context.Entry);
            var w3dHLod = w3dFile.GetHLod();

            // If this is a skin file, load "external" animations.
            var externalAnimations = new List<AnimationInstance>();
            if (w3dHLod != null && w3dHLod.Header.Name.EndsWith("_SKN", StringComparison.OrdinalIgnoreCase))
            {
                var namePrefix = w3dHLod.Header.Name.Substring(0, w3dHLod.Header.Name.LastIndexOf('_') + 1);
                var parentFolder = Path.GetDirectoryName(w3dFile.FilePath);
                var pathPrefix = Path.Combine(parentFolder, namePrefix);
                foreach (var animationFileEntry in context.Entry.FileSystem.GetFiles(parentFolder))
                {
                    if (!animationFileEntry.FilePath.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var w3dAnimationFile = W3dFile.FromFileSystemEntry(animationFileEntry);

                    var w3dAnimations = ModelLoader.LoadAnimations(w3dAnimationFile, game.ContentManager);
                    foreach (var w3dAnimation in w3dAnimations)
                    {
                        var externalAnimationInstance = new AnimationInstance(modelInstance, w3dAnimation);
                        modelInstance.AnimationInstances.Add(externalAnimationInstance);
                        externalAnimations.Add(externalAnimationInstance);
                    }
                }
            }

            _subObjects = new List<W3dItem>();

            _subObjects.Add(new W3dModelItem());

            foreach (var animation in animations)
            {
                _subObjects.Add(new W3dAnimationItem(animation, "Animation"));
            }

            foreach (var animation in externalAnimations)
            {
                _subObjects.Add(new W3dAnimationItem(animation, "External Animation"));
            }

            ActivateItem(_subObjects[0]);
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("w3d", new Vector2(250, 0), true, 0);

            foreach (var subObject in _subObjects)
            {
                if (ImGui.Selectable(subObject.Name, subObject == _selectedItem))
                {
                    ActivateItem(subObject);
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();

            base.Draw(ref isGameViewFocused);
        }

        private void ActivateItem(W3dItem item)
        {
            if (_selectedItem != null)
            {
                _selectedItem.Deactivate();
                _selectedItem = null;
            }

            if (item != null)
            {
                _selectedItem = item;
                _selectedItem.Activate();
            }
        }

        private abstract class W3dItem
        {
            public abstract string Name { get; }

            public virtual void Activate() { }
            public virtual void Deactivate() { }
        }

        private sealed class W3dModelItem : W3dItem
        {
            public override string Name { get; } = "Model";
        }

        private sealed class W3dAnimationItem : W3dItem
        {
            private readonly AnimationInstance _animationInstance;

            public override string Name { get; }

            public W3dAnimationItem(AnimationInstance animationInstance, string groupName)
            {
                _animationInstance = animationInstance;

                Name = $"{groupName} - {_animationInstance.Animation.Name}";
            }

            public override void Activate()
            {
                _animationInstance.Play();
            }

            public override void Deactivate()
            {
                _animationInstance.Stop();
            }
        }

        private static BoundingBox GetEnclosingBoundingBox(ModelInstance modelInstance)
        {
            var boundingBox = default(BoundingBox);

            var first = true;
            foreach (var subObject in modelInstance.Model.SubObjects)
            {
                var transformedBoundingBox = BoundingBox.Transform(subObject.RenderObject.BoundingBox,
                    modelInstance.ModelBoneInstances[subObject.Bone.Index].Matrix);

                if (first)
                {
                    boundingBox = transformedBoundingBox;
                    first = false;
                }
                else
                {
                    boundingBox = BoundingBox.CreateMerged(
                        boundingBox,
                        transformedBoundingBox);
                }
            }

            return boundingBox;
        }
    }
}
