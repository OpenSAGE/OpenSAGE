using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Data;
using OpenSage.Data.W3d;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Cameras.Controllers;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class W3dFileContentViewModel : FileContentViewModel<W3dItemViewModelBase>, IGameViewModel
    {
        private readonly W3dFile _w3dFile;

        private Entity _modelEntity;

        private List<AnimationComponent> _animations;
        private List<AnimationComponent> _externalAnimations;

        public W3dFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _w3dFile = W3dFile.FromFileSystemEntry(file);
        }

        void IGameViewModel.LoadScene(Game game)
        {
            var scene = new Scene();

            _modelEntity = game.ContentManager.Load<Model>(File.FilePath).CreateEntity();
            scene.Entities.Add(_modelEntity);

            var enclosingBoundingBox = _modelEntity.GetEnclosingBoundingBox();
            scene.CameraController.CanPlayerInputChangePitch = true;
            scene.CameraController.TerrainPosition = enclosingBoundingBox.GetCenter();
            scene.CameraController.Zoom = Vector3.Distance(enclosingBoundingBox.Min, enclosingBoundingBox.Max) / 400f;

            game.Scene = scene;

            _animations = new List<AnimationComponent>();
            _animations.AddRange(_modelEntity.Components.OfType<AnimationComponent>());

            // If this is a skin file, load "external" animations.
            _externalAnimations = new List<AnimationComponent>();
            if (_w3dFile.HLod != null && _w3dFile.HLod.Header.Name.EndsWith("_SKN", System.StringComparison.OrdinalIgnoreCase))
            {
                var namePrefix = _w3dFile.HLod.Header.Name.Substring(0, _w3dFile.HLod.Header.Name.LastIndexOf('_') + 1);
                var parentFolder = Path.GetDirectoryName(_w3dFile.FilePath);
                var pathPrefix = Path.Combine(parentFolder, namePrefix);
                foreach (var animationFileEntry in File.FileSystem.GetFiles(parentFolder))
                {
                    if (!animationFileEntry.FilePath.StartsWith(pathPrefix, System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var animationModel = game.ContentManager.Load<Model>(animationFileEntry.FilePath);
                    foreach (var animation in animationModel.Animations)
                    {
                        var externalAnimationComponent = new AnimationComponent
                        {
                            Animation = animation
                        };
                        _modelEntity.Components.Add(externalAnimationComponent);
                        _externalAnimations.Add(externalAnimationComponent);
                    }
                }
            }

            SubObjects.Add(new ModelViewModel());

            foreach (var animation in _animations)
            {
                SubObjects.Add(new AnimationViewModel(animation, "Animations"));
            }

            foreach (var animation in _externalAnimations)
            {
                SubObjects.Add(new AnimationViewModel(animation, "External Animations"));
            }

            SelectedSubObject = SubObjects[0];
        }

        protected override void OnSelectedSubObjectChanged(W3dItemViewModelBase subObject)
        {
            if (subObject != null)
            {
                //CameraController.Reset(
                //    subObject.BoundingSphere.Center,
                //    subObject.BoundingSphere.Radius * 1.6f);

                //CameraController.Reset(
                //    Vector3.Zero,
                //    200);
            }
        }
    }

    public abstract class W3dItemViewModelBase : FileSubObjectViewModel
    {
        
    }

    public sealed class ModelViewModel : W3dItemViewModelBase
    {
        public override string GroupName => string.Empty;

        public override string Name => "Hierarchy";
    }

    public sealed class AnimationViewModel : W3dItemViewModelBase
    {
        private readonly AnimationComponent _animationComponent;

        public override string GroupName { get; }

        public override string Name => _animationComponent.Animation.Name;

        public AnimationViewModel(AnimationComponent animation, string groupName)
        {
            _animationComponent = animation;

            GroupName = groupName;
        }

        public override void Activate()
        {
            _animationComponent.Play();
        }

        public override void Deactivate()
        {
            _animationComponent.Stop();
        }
    }
}
