using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Caliburn.Micro;
using OpenSage.Data;
using OpenSage.Data.W3d;
using OpenSage.DataViewer.Framework;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Cameras.Controllers;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class W3dFileContentViewModel : FileContentViewModel<W3dItemViewModelBase>, IGameViewModel
    {
        private readonly W3dFile _w3dFile;

        private readonly Entity _modelEntity;

        private readonly List<AnimationComponent> _animations;
        private readonly List<AnimationComponent> _externalAnimations;

        public Game Game { get; }

        public W3dFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _w3dFile = W3dFile.FromFileSystemEntry(file);

            Game = IoC.Get<GameService>().Game;

            var scene = new Scene();

            var cameraEntity = new Entity();
            scene.Entities.Add(cameraEntity);

            cameraEntity.Components.Add(new PerspectiveCameraComponent { FieldOfView = 70 });

            _modelEntity = Game.ContentManager.Load<Model>(File.FilePath, uploadBatch: null).CreateEntity();
            scene.Entities.Add(_modelEntity);

            var enclosingBoundingBox = _modelEntity.GetEnclosingBoundingBox();

            cameraEntity.Components.Add(new ArcballCameraController(
                enclosingBoundingBox.GetCenter(),
                Vector3.Distance(enclosingBoundingBox.Min, enclosingBoundingBox.Max) / 1.5f));

            Game.Scene = scene;

            _animations = new List<AnimationComponent>();
            _animations.AddRange(_modelEntity.Components.OfType<AnimationComponent>());

            // If this is a skin file, load "external" animations.
            _externalAnimations = new List<AnimationComponent>();
            if (_w3dFile.HLod != null && _w3dFile.HLod.Header.Name.EndsWith("_SKN"))
            {
                var namePrefix = _w3dFile.HLod.Header.Name.Substring(0, _w3dFile.HLod.Header.Name.LastIndexOf('_') + 1);
                var parentFolder = Path.GetDirectoryName(_w3dFile.FilePath);
                var pathPrefix = Path.Combine(parentFolder, namePrefix);
                foreach (var animationFileEntry in file.FileSystem.GetFiles(parentFolder))
                {
                    if (!animationFileEntry.FilePath.StartsWith(pathPrefix))
                    {
                        continue;
                    }

                    var animationModel = Game.ContentManager.Load<Model>(animationFileEntry.FilePath, uploadBatch: null);
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
        }

        protected override IReadOnlyList<W3dItemViewModelBase> CreateSubObjects()
        {
            var result = new List<W3dItemViewModelBase>();

            //if (_modelInstance.Model.HasHierarchy)
            {
                result.Add(new ModelViewModel());

                foreach (var animation in _animations)
                {
                    result.Add(new AnimationViewModel(animation, "Animations"));
                }

                foreach (var animation in _externalAnimations)
                {
                    result.Add(new AnimationViewModel(animation, "External Animations"));
                }
            }

            //foreach (var mesh in _modelInstance.Model.Meshes)
            //{
            //    result.Add(new ModelMeshViewModel(mesh));
            //}

            return result;
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
        //public abstract BoundingSphere BoundingSphere { get; }

        //public virtual void Update(GameTime gameTime) { }

        //public abstract void Draw(
        //    CommandEncoder commandEncoder,
        //    MeshEffect meshEffect,
        //    Camera camera,
        //    ref Matrix4x4 world,
        //    GameTime gameTime);
    }

    public sealed class ModelViewModel : W3dItemViewModelBase
    {
        //private readonly ModelInstance _modelInstance;

        public override string GroupName => string.Empty;

        public override string Name => "Hierarchy";

        //public override BoundingSphere BoundingSphere => _modelInstance.Model.BoundingSphere;
    }

    //public sealed class ModelMeshViewModel : W3dItemViewModelBase
    //{
    //    private readonly ModelMesh _mesh;

    //    public override string GroupName => "Meshes";

    //    public override string Name => _mesh.Name;

    //    public override BoundingSphere BoundingSphere => _mesh.BoundingSphere;

    //    public ModelMeshViewModel(ModelMesh mesh)
    //    {
    //        _mesh = mesh;
    //    }

    //    public override void Draw(
    //        CommandEncoder commandEncoder,
    //        MeshEffect meshEffect,
    //        Camera camera,
    //        ref Matrix4x4 world,
    //        GameTime gameTime)
    //    {
    //        _mesh.Draw(
    //            commandEncoder,
    //            meshEffect,
    //            camera,
    //            ref world,
    //            gameTime,
    //            false);

    //        _mesh.Draw(
    //            commandEncoder,
    //            meshEffect,
    //            camera,
    //            ref world,
    //            gameTime,
    //            true);
    //    }
    //}

    public sealed class AnimationViewModel : W3dItemViewModelBase
    {
        private readonly AnimationComponent _animationComponent;

        public override string GroupName { get; }

        public override string Name => _animationComponent.Animation.Name;

        //public override BoundingSphere BoundingSphere => _modelInstance.Model.BoundingSphere;

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

        //public override void Update(GameTime gameTime)
        //{
        //    _animationPlayer.Update(gameTime);
        //}

        //public override void Draw(
        //    CommandEncoder commandEncoder,
        //    MeshEffect meshEffect,
        //    Camera camera,
        //    ref Matrix4x4 world,
        //    GameTime gameTime)
        //{
        //    _modelInstance.Draw(
        //        commandEncoder,
        //        meshEffect,
        //        camera,
        //        ref world,
        //        gameTime);
        //}
    }
}
