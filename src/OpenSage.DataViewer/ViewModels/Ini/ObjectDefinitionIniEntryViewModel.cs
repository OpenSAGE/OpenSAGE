using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Cameras.Controllers;
using OpenSage.Logic.Object;

namespace OpenSage.DataViewer.ViewModels.Ini
{
    public sealed class ObjectDefinitionIniEntryViewModel : IniEntryViewModel
    {
        private readonly ObjectDefinition _definition;

        private ObjectComponent _objectComponent;

        public override string GroupName => "Object Definitions";

        public override string Name => _definition.Name;

        public IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; private set; }

        public BitArray<ModelConditionFlag> SelectedModelConditionState
        {
            get { return _objectComponent?.ModelConditionFlags; }
            set
            {
                _objectComponent.SetModelConditionFlags(value);
                NotifyOfPropertyChange();
            }
        }

        public ObjectDefinitionIniEntryViewModel(ObjectDefinition definition)
        {
            _definition = definition;
        }

        public override void Activate()
        {
            var scene = new Scene();

            var cameraEntity = new Entity();
            cameraEntity.Components.Add(new PerspectiveCameraComponent { FieldOfView = 70 });
            cameraEntity.Components.Add(new ArcballCameraController(Vector3.Zero, 300));
            scene.Entities.Add(cameraEntity);

            var objectEntity = Entity.FromObjectDefinition(_definition);
            _objectComponent = objectEntity.GetComponent<ObjectComponent>();
            scene.Entities.Add(objectEntity);

            Game.Scene = scene;

            Game.ResetElapsedTime();

            ModelConditionStates = _objectComponent.ModelConditionStates.ToList();
            NotifyOfPropertyChange(nameof(ModelConditionStates));
            SelectedModelConditionState = ModelConditionStates.FirstOrDefault();
        }
    }
}
