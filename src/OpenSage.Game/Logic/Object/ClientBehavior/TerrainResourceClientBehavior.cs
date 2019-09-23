using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class TerrainResourceClientBehaviorData : ClientBehaviorModuleData
    {
        internal static TerrainResourceClientBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<TerrainResourceClientBehaviorData> FieldParseTable = new IniParseTable<TerrainResourceClientBehaviorData>
        {
        };
    }
}
