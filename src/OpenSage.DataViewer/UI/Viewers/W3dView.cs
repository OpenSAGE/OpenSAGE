using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.Data.W3d;
using OpenSage.DataViewer.Controls;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Cameras.Controllers;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Settings;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class W3dView : Splitter
    {
        private readonly ListBox _listBox;
        private W3dItem _selectedItem;

        public W3dView(FileSystemEntry entry, Func<IntPtr, Game> createGame)
        {
            _listBox = new ListBox
            {
                Width = 250,
                ItemTextBinding = Binding.Property((W3dItem v) => v.Name)
            };
            _listBox.SelectedValueChanged += OnSelectedValueChanged;
            Panel1 = _listBox;

            _listBox.SelectedIndex = 0;

            Panel2 = new GameControl
            {
                CreateGame = h =>
                {
                    var game = createGame(h);

                    var modelInstance = game.ContentManager
                        .Load<Model>(entry.FilePath)
                        .CreateInstance(game.GraphicsDevice);

                    game.Updating += (sender, e) =>
                    {
                        modelInstance.Update(e.GameTime);
                    };

                    game.BuildingRenderList += (sender, e) =>
                    {
                        modelInstance.SetWorldMatrix(Matrix4x4.Identity);
                        modelInstance.BuildRenderList(e.RenderList, e.Camera);
                    };

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
                        WorldLighting.CreateDefault(),
                        Array.Empty<Player>(),
                        Array.Empty<Team>());

                    var animations = new List<AnimationInstance>(modelInstance.AnimationInstances);

                    var w3dFile = W3dFile.FromFileSystemEntry(entry);

                    // If this is a skin file, load "external" animations.
                    var externalAnimations = new List<AnimationInstance>();
                    if (w3dFile.HLod != null && w3dFile.HLod.Header.Name.EndsWith("_SKN", StringComparison.OrdinalIgnoreCase))
                    {
                        var namePrefix = w3dFile.HLod.Header.Name.Substring(0, w3dFile.HLod.Header.Name.LastIndexOf('_') + 1);
                        var parentFolder = Path.GetDirectoryName(w3dFile.FilePath);
                        var pathPrefix = Path.Combine(parentFolder, namePrefix);
                        foreach (var animationFileEntry in entry.FileSystem.GetFiles(parentFolder))
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

                    var subObjects = new List<W3dItem>();

                    subObjects.Add(new W3dModelItem());

                    foreach (var animation in animations)
                    {
                        subObjects.Add(new W3dAnimationItem(animation, "Animation"));
                    }

                    foreach (var animation in externalAnimations)
                    {
                        subObjects.Add(new W3dAnimationItem(animation, "External Animation"));
                    }

                    _listBox.DataStore = subObjects;

                    return game;
                }
            };
        }

        private void OnSelectedValueChanged(object sender, EventArgs e)
        {
            if (_selectedItem != null)
            {
                _selectedItem.Deactivate();
                _selectedItem = null;
            }

            if (_listBox.SelectedValue != null)
            {
                _selectedItem = (W3dItem) _listBox.SelectedValue;
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
