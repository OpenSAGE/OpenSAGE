using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage
{
    public abstract class LoadSubsystemEntry
    {
        public string Value { get; }

        protected LoadSubsystemEntry(string value)
        {
            Value = value;
        }
    }

    public sealed class InitFile : LoadSubsystemEntry
    {
        public InitFile(string value) : base(value)
        {
        }
    }

    public sealed class InitFileDebug : LoadSubsystemEntry
    {
        public InitFileDebug(string value) : base(value)
        {
        }
    }

    public sealed class InitPath : LoadSubsystemEntry
    {
        public InitPath(string value) : base(value)
        {
        }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class IncludePathCinematics : LoadSubsystemEntry
    {
        public IncludePathCinematics(string value) : base(value)
        {
        }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LoadSubsystem
    {
        internal static LoadSubsystem Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LoadSubsystem> FieldParseTable = new IniParseTable<LoadSubsystem>
        {
            { "Loader", (parser, x) => x.Loader = parser.ParseEnum<SubsystemLoader>() },
            { "InitFile", (parser, x) => x.Entries.Add(new InitFile(parser.ParseFileName())) },
            { "InitFileDebug", (parser, x) => x.Entries.Add(new InitFileDebug(parser.ParseFileName())) },
            { "InitPath", (parser, x) => x.Entries.Add(new InitPath(parser.ParseFileName())) },
            { "IncludePathCinematics", (parser, x) => x.Entries.Add(new IncludePathCinematics(parser.ParseFileName())) },
            { "ExcludePath", (parser, x) => x.ExcludePath.Add(parser.ParseFileName()) },
        };

        public string Name { get; private set; }

        public SubsystemLoader Loader { get; private set; }
        public List<LoadSubsystemEntry> Entries { get; } = new List<LoadSubsystemEntry>();
        [AddedIn(SageGame.Bfme2)]
        public HashSet<string> ExcludePath { get; } = new HashSet<string>();
    }

    public enum SubsystemLoader
    {
        [IniEnum("INI")]
        Ini
    }
}
