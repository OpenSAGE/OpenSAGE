using System.Numerics;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;
using OpenSage.Graphics;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class W3dView : Splitter
    {
        public W3dView(FileSystemEntry entry, Game game)
        {
            var scene = new Scene();

            var modelEntity = game.ContentManager.Load<Model>(entry.FilePath).CreateEntity();
            scene.Entities.Add(modelEntity);

            var enclosingBoundingBox = modelEntity.GetEnclosingBoundingBox();
            scene.CameraController.CanPlayerInputChangePitch = true;
            scene.CameraController.TerrainPosition = enclosingBoundingBox.GetCenter();
            scene.CameraController.Zoom = Vector3.Distance(enclosingBoundingBox.Min, enclosingBoundingBox.Max) / 400f;

            game.Scene = scene;

            //_animations = new List<AnimationComponent>();
            //_animations.AddRange(_modelEntity.Components.OfType<AnimationComponent>());

            //// If this is a skin file, load "external" animations.
            //_externalAnimations = new List<AnimationComponent>();
            //if (_w3dFile.HLod != null && _w3dFile.HLod.Header.Name.EndsWith("_SKN", System.StringComparison.OrdinalIgnoreCase))
            //{
            //    var namePrefix = _w3dFile.HLod.Header.Name.Substring(0, _w3dFile.HLod.Header.Name.LastIndexOf('_') + 1);
            //    var parentFolder = Path.GetDirectoryName(_w3dFile.FilePath);
            //    var pathPrefix = Path.Combine(parentFolder, namePrefix);
            //    foreach (var animationFileEntry in File.FileSystem.GetFiles(parentFolder))
            //    {
            //        if (!animationFileEntry.FilePath.StartsWith(pathPrefix, System.StringComparison.OrdinalIgnoreCase))
            //        {
            //            continue;
            //        }

            //        var animationModel = game.ContentManager.Load<Model>(animationFileEntry.FilePath);
            //        foreach (var animation in animationModel.Animations)
            //        {
            //            var externalAnimationComponent = new AnimationComponent
            //            {
            //                Animation = animation
            //            };
            //            _modelEntity.Components.Add(externalAnimationComponent);
            //            _externalAnimations.Add(externalAnimationComponent);
            //        }
            //    }
            //}

            //SubObjects.Add(new ModelViewModel());

            //foreach (var animation in _animations)
            //{
            //    SubObjects.Add(new AnimationViewModel(animation, "Animations"));
            //}

            //foreach (var animation in _externalAnimations)
            //{
            //    SubObjects.Add(new AnimationViewModel(animation, "External Animations"));
            //}

            //SelectedSubObject = SubObjects[0];

            Panel2 = new GameControl
            {
                Game = game
            };
        }
    }

    //public sealed class AnimationViewModel : W3dItemViewModelBase
    //{
    //    private readonly AnimationComponent _animationComponent;

    //    public override string GroupName { get; }

    //    public override string Name => _animationComponent.Animation.Name;

    //    public AnimationViewModel(AnimationComponent animation, string groupName)
    //    {
    //        _animationComponent = animation;

    //        GroupName = groupName;
    //    }

    //    public override void Activate()
    //    {
    //        _animationComponent.Play();
    //    }

    //    public override void Deactivate()
    //    {
    //        _animationComponent.Stop();
    //    }
    //}
}
