using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Like Transport, but when full, passes transport queries along to first passenger 
    /// (redirects like tunnel). Basically works like the Overlord Contain.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class HelixContainModuleData : TransportContainModuleData
    {
        internal static new HelixContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<HelixContainModuleData> FieldParseTable = TransportContainModuleData.FieldParseTable
            .Concat(new IniParseTable<HelixContainModuleData>());
    }
}
