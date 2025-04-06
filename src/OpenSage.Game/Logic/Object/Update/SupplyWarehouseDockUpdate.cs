using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public class SupplyWarehouseDockUpdate : DockUpdate
{
    private SupplyWarehouseDockUpdateModuleData _moduleData;
    private int _currentBoxes;

    internal SupplyWarehouseDockUpdate(GameObject gameObject, IGameEngine gameEngine, SupplyWarehouseDockUpdateModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
        _currentBoxes = _moduleData.StartingBoxes;
    }

    public bool HasBoxes() => _currentBoxes > 0;

    public bool GetBox()
    {
        if (_currentBoxes > 0)
        {
            _currentBoxes--;

            var boxPercentage = (float)_currentBoxes / _moduleData.StartingBoxes;
            GameObject.Drawable.SetSupplyBoxesRemaining(boxPercentage);

            return true;
        }
        return false;
    }

    public override UpdateSleepTime Update()
    {
        if (_currentBoxes <= 0 && _moduleData.DeleteWhenEmpty)
        {
            GameEngine.GameLogic.DestroyObject(GameObject);
        }

        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistInt32(ref _currentBoxes);
    }
}

public sealed class SupplyWarehouseDockUpdateModuleData : DockUpdateModuleData
{
    internal static SupplyWarehouseDockUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<SupplyWarehouseDockUpdateModuleData> FieldParseTable = DockUpdateModuleData.FieldParseTable
        .Concat(new IniParseTable<SupplyWarehouseDockUpdateModuleData>
        {
            { "StartingBoxes", (parser, x) => x.StartingBoxes = parser.ParseInteger() },
            { "DeleteWhenEmpty", (parser, x) => x.DeleteWhenEmpty = parser.ParseBoolean() }
        });

    /// <summary>
    /// Used to determine the visual representation of a full warehouse.
    /// </summary>
    public int StartingBoxes { get; private set; }

    /// <summary>
    /// True if warehouse should be deleted when depleted.
    /// </summary>
    public bool DeleteWhenEmpty { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new SupplyWarehouseDockUpdate(gameObject, gameEngine, this);
    }
}
