using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows this object to move randomly about its point of origin using a SET_WANDER locomotor.
    /// </summary>
    public sealed class WanderAIUpdateModuleData : AIUpdateModuleData
    {
        internal static new WanderAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<WanderAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<WanderAIUpdateModuleData>());
    }
}
