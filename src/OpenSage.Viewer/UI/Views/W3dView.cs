using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Data.W3d;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Settings;
using OpenSage.Viewer.Framework;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class W3dView : GameView
    {
        public W3dView(AssetViewContext context)
            : base(context)
        {
            var game = context.Game;

            var modelInstance = game.ContentManager
                .Load<Model>(context.Entry)
                .CreateInstance(game.GraphicsDevice);

            void onUpdating(object sender, GameUpdatingEventArgs e) => modelInstance.Update(e.GameTime);

            game.Updating += onUpdating;
            AddDisposeAction(() => game.Updating -= onUpdating);

            void onBuildingRenderList(object sender, BuildingRenderListEventArgs e)
            {
                modelInstance.SetWorldMatrix(Matrix4x4.Identity);
                modelInstance.BuildRenderList(e.RenderList, e.Camera);
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
                null,
                new GameObjectCollection(game.ContentManager),
                new WaypointCollection(),
                new WaypointPathCollection(),
                WorldLighting.CreateDefault());

            var animations = new List<AnimationInstance>(modelInstance.AnimationInstances);

            var w3dFile = W3dFile.FromFileSystemEntry(context.Entry);

            // If this is a skin file, load "external" animations.
            var externalAnimations = new List<AnimationInstance>();
            if (w3dFile.HLod != null && w3dFile.HLod.Header.Name.EndsWith("_SKN", StringComparison.OrdinalIgnoreCase))
            {
                var namePrefix = w3dFile.HLod.Header.Name.Substring(0, w3dFile.HLod.Header.Name.LastIndexOf('_') + 1);
                var parentFolder = Path.GetDirectoryName(w3dFile.FilePath);
                var pathPrefix = Path.Combine(parentFolder, namePrefix);
                foreach (var animationFileEntry in context.Entry.FileSystem.GetFiles(parentFolder))
                {
                    if (!animationFileEntry.FilePath.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var animationModel = game.ContentManager.Load<Model>(animationFileEntry.FilePath);
                    foreach (var animation in animationModel.Animations)
                    {
                        var externalAnimationInstance = new AnimationInstance(modelInstance, animation);
                        modelInstance.AnimationInstances.Add(externalAnimationInstance);
                        externalAnimations.Add(externalAnimationInstance);
                    }
                }
            }
        }

        private static BoundingBox GetEnclosingBoundingBox(ModelInstance modelInstance)
        {
            var boundingBox = default(BoundingBox);

            var first = true;
            foreach (var mesh in modelInstance.Model.Meshes)
            {
                var transformedBoundingBox = mesh.BoundingBox.Transform(
                    modelInstance.ModelBoneInstances[mesh.ParentBone.Index].Matrix);

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
