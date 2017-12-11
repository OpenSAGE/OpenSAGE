using Eto.Forms;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class IniView : Panel
    {
    }

    //public sealed class IniFileContentViewModel : FileContentViewModel<IniEntryViewModel>
    //{
    //    private readonly IniDataContext _iniDataContext;

    //    public IniFileContentViewModel(FileSystemEntry file)
    //        : base(file)
    //    {
    //        _iniDataContext = new IniDataContext(file.FileSystem);
    //        _iniDataContext.LoadIniFile(file);

    //        foreach (var objectDefinition in _iniDataContext.Objects)
    //        {
    //            SubObjects.Add(new ObjectDefinitionIniEntryViewModel(objectDefinition));
    //        }

    //        foreach (var particleSystem in _iniDataContext.ParticleSystems)
    //        {
    //            SubObjects.Add(new ParticleSystemIniEntryViewModel(particleSystem));
    //        }
    //    }
    //}

    //public sealed class ParticleSystemIniEntryViewModel : IniEntryViewModel, IGameViewModel
    //{
    //    private readonly ParticleSystemDefinition _definition;

    //    public override string GroupName => "Particle Systems";

    //    public override string Name => _definition.Name;

    //    public ParticleSystemIniEntryViewModel(ParticleSystemDefinition definition)
    //    {
    //        _definition = definition;
    //    }

    //    void IGameViewModel.LoadScene(Game game)
    //    {
    //        var scene = new Scene();

    //        var particleSystemEntity = new Entity();
    //        particleSystemEntity.Components.Add(new ParticleSystem(_definition));
    //        scene.Entities.Add(particleSystemEntity);

    //        game.Scene = scene;
    //    }
    //}

    //public sealed class ObjectDefinitionIniEntryViewModel : IniEntryViewModel, IGameViewModel
    //{
    //    private readonly ObjectDefinition _definition;

    //    private ObjectComponent _objectComponent;

    //    public override string GroupName => "Object Definitions";

    //    public override string Name => _definition.Name;

    //    public IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; private set; }

    //    public BitArray<ModelConditionFlag> SelectedModelConditionState
    //    {
    //        get { return _objectComponent?.ModelConditionFlags; }
    //        set
    //        {
    //            _objectComponent.SetModelConditionFlags(value);
    //            NotifyOfPropertyChange();
    //        }
    //    }

    //    public ObjectDefinitionIniEntryViewModel(ObjectDefinition definition)
    //    {
    //        _definition = definition;
    //    }

    //    void IGameViewModel.LoadScene(Game game)
    //    {
    //        var scene = new Scene();

    //        var objectEntity = Entity.FromObjectDefinition(_definition);
    //        _objectComponent = objectEntity.GetComponent<ObjectComponent>();
    //        scene.Entities.Add(objectEntity);

    //        game.Scene = scene;

    //        ModelConditionStates = _objectComponent.ModelConditionStates.ToList();
    //        NotifyOfPropertyChange(nameof(ModelConditionStates));
    //        SelectedModelConditionState = ModelConditionStates.FirstOrDefault();
    //    }
    //}
}
