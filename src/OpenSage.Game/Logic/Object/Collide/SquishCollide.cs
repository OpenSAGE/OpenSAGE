using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class SquishCollide : CollideModule
    {
        // TODO

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    public sealed class SquishCollideModuleData : CollideModuleData
    {
        internal static SquishCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SquishCollideModuleData> FieldParseTable = new IniParseTable<SquishCollideModuleData>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new SquishCollide();
        }
    }
}
