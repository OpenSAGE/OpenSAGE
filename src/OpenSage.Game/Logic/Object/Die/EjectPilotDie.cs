using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class EjectPilotDie : DieModule
{
    private readonly EjectPilotDieModuleData _moduleData;

    internal EjectPilotDie(GameObject gameObject, IGameEngine gameEngine, EjectPilotDieModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    protected override void Die(in DamageInfoInput damageInput)
    {
        var veterancy = (VeterancyLevel)GameObject.Rank;

        if (!_moduleData.VeterancyLevels.Get(veterancy))
        {
            return;
        }

        var isOnGround = true; // todo: determine if unit is airborne
        var creationList = isOnGround ? _moduleData.GroundCreationList : _moduleData.AirCreationList;
        foreach (var gameObject in GameEngine.ObjectCreationLists.Create(creationList.Value, new BehaviorUpdateContext(GameEngine, GameObject)))
        {
            gameObject.Rank = GameObject.Rank;
            GameEngine.AudioSystem.PlayAudioEvent(gameObject, GameObject.Definition.UnitSpecificSounds.VoiceEject?.Value);
        }
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

/// <summary>
/// Allows use of SoundEject and VoiceEject within UnitSpecificSounds section of the object.
/// </summary>
public sealed class EjectPilotDieModuleData : DieModuleData
{
    internal static EjectPilotDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<EjectPilotDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
        .Concat(new IniParseTable<EjectPilotDieModuleData>
        {
            { "GroundCreationList", (parser, x) => x.GroundCreationList = parser.ParseObjectCreationListReference() },
            { "AirCreationList", (parser, x) => x.AirCreationList = parser.ParseObjectCreationListReference() },
            { "VeterancyLevels", (parser, x) => x.VeterancyLevels = parser.ParseEnumBitArray<VeterancyLevel>() },
        });

    public LazyAssetReference<ObjectCreationList> GroundCreationList { get; private set; }
    public LazyAssetReference<ObjectCreationList> AirCreationList { get; private set; }
    public BitArray<VeterancyLevel> VeterancyLevels { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new EjectPilotDie(gameObject, gameEngine, this);
    }
}
