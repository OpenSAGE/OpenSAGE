using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class LoadSubsystem
    {
        internal static LoadSubsystem Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LoadSubsystem> FieldParseTable = new IniParseTable<LoadSubsystem>
        {
            { "Loader", (parser, x) => x.Loader = parser.ParseEnum<SubsystemLoader>() },
            { "InitFile", (parser, x) => x.InitFiles.Add(parser.ParseFileName()) },
            { "InitFileDebug", (parser, x) => x.InitFilesDebug.Add(parser.ParseFileName()) },
            { "InitPath", (parser, x) => x.InitPaths.Add(parser.ParseFileName()) },
        };

        public string Name { get; private set; }

        public SubsystemLoader Loader { get; private set; }
        public List<string> InitFiles { get; } = new List<string>();
        public List<string> InitFilesDebug { get; } = new List<string>();
        public List<string> InitPaths { get; } = new List<string>();
    }

    public enum SubsystemLoader
    {
        [IniEnum("INI")]
        Ini
    }
}
