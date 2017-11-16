using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class ObjectPrerequisites
    {
        internal static ObjectPrerequisites Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ObjectPrerequisites> FieldParseTable = new IniParseTable<ObjectPrerequisites>
        {
            { "Object", (parser, x) => x.Objects.Add(PrerequisiteList.Parse(parser)) },
            { "Science", (parser, x) => x.Sciences.Add(PrerequisiteList.Parse(parser)) }
        };

        public List<PrerequisiteList> Objects { get; } = new List<PrerequisiteList>();
        public List<PrerequisiteList> Sciences { get; } = new List<PrerequisiteList>();
    }

    /// <summary>
    /// Contains an OR'd list of prequisite objects or sciences.
    /// </summary>
    public sealed class PrerequisiteList : List<string>
    {
        internal static PrerequisiteList Parse(IniParser parser)
        {
            var result = new PrerequisiteList();
            result.AddRange(parser.ParseAssetReferenceArray());
            return result;
        }
    }
}
