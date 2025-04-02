#nullable enable

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

/// <summary>
/// An active body specifically for structures that are built,
/// and/or interactable with the player.
/// </summary>
public sealed class StructureBody : ActiveBody
{
    /// <summary>
    /// Object that built this structure.
    /// </summary>
    private ObjectId _constructorObjectID;

    internal StructureBody(GameObject gameObject, IGameEngine gameEngine, StructureBodyModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
    }

    // This method is in the original code, but isn't actually used anywhere.
    public void SetConstructorObject(GameObject obj)
    {
        _constructorObjectID = obj?.Id ?? ObjectId.Invalid;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistObjectId(ref _constructorObjectID);
    }
}

/// <summary>
/// Used by objects with STRUCTURE and IMMOBILE KindOfs defined.
/// </summary>
public sealed class StructureBodyModuleData : ActiveBodyModuleData
{
    internal static new StructureBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<StructureBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
        .Concat(new IniParseTable<StructureBodyModuleData>());

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new StructureBody(gameObject, gameEngine, this);
    }
}
