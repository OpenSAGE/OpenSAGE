using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LLGfx;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Cameras.Controllers;
using OpenSage.Graphics.Effects;
using OpenSage.Logic.Object;
using OpenSage.Terrain;

namespace OpenSage.DataViewer.ViewModels.Ini
{
    public sealed class ObjectDefinitionIniEntryViewModel : FileSubObjectViewModel, IGameViewModel
    {
        private readonly ObjectDefinition _definition;

        //private Thing _thing;

        public Game Game { get; }

        public override string GroupName => "Object Definitions";

        public override string Name => _definition.Name;

        public IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; private set; }

        //public BitArray<ModelConditionFlag> SelectedModelConditionState
        //{
        //    get { return _thing?.ModelCondition; }
        //    set
        //    {
        //        _thing.ModelCondition = value;
        //        NotifyOfPropertyChange();
        //    }
        //}

        public ObjectDefinitionIniEntryViewModel(
            ObjectDefinition definition,
            Game game)
        {
            _definition = definition;

            Game = game;
        }

        public override void Activate()
        {
            var scene = new Scene();

            var cameraEntity = new Entity();
            scene.Entities.Add(cameraEntity);

            cameraEntity.Components.Add(new PerspectiveCameraComponent
            {
                FieldOfView = 70
            });

            var cameraController = new ArcballCameraController();
            cameraEntity.Components.Add(cameraController);
            cameraController.Reset(Vector3.Zero, 300);

            var gameEntity = Entity.FromObjectDefinition(_definition);
            scene.Entities.Add(gameEntity);

            Game.Scene = scene;

            Game.ResetElapsedTime();

            //ModelConditionStates = _thing.ModelConditionStates.ToList();
            //NotifyOfPropertyChange(nameof(ModelConditionStates));
            //SelectedModelConditionState = ModelConditionStates.FirstOrDefault();
        }

        public override void Deactivate()
        {
            Game.Scene = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (Game.Scene != null)
            {
                Deactivate();
            }

            base.Dispose(disposing);
        }
    }
}
