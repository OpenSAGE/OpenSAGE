using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class WanderAIUpdate : AIUpdate
    {
        internal WanderAIUpdate(GameObject gameObject, WanderAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
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
    /// Allows this object to move randomly about its point of origin using a SET_WANDER locomotor.
    /// </summary>
    public sealed class WanderAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static WanderAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<WanderAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<WanderAIUpdateModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new WanderAIUpdate(gameObject, this);
        }
    }
}
