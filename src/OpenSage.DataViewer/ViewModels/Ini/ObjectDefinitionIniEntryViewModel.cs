using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Cameras.Controllers;
using OpenSage.Logic.Object;

namespace OpenSage.DataViewer.ViewModels.Ini
{
    public sealed class ObjectDefinitionIniEntryViewModel : IniEntryViewModel, IGameViewModel
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

        void IGameViewModel.LoadScene(Game game)
        {
            var scene = new Scene();

            var objectEntity = Entity.FromObjectDefinition(_definition);
            _objectComponent = objectEntity.GetComponent<ObjectComponent>();
            scene.Entities.Add(objectEntity);

            game.Scene = scene;

            ModelConditionStates = _objectComponent.ModelConditionStates.ToList();
            NotifyOfPropertyChange(nameof(ModelConditionStates));
            SelectedModelConditionState = ModelConditionStates.FirstOrDefault();
        }
    }
}
