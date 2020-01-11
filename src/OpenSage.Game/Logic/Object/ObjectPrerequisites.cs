using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;

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
            { "Object", (parser, x) => x.Objects.Add(ObjectPrerequisiteList.Parse(parser)) },
            { "Science", (parser, x) => x.Sciences.Add(SciencePrerequisiteList.Parse(parser)) }
        };

        public List<ObjectPrerequisiteList> Objects { get; } = new List<ObjectPrerequisiteList>();
        public List<SciencePrerequisiteList> Sciences { get; } = new List<SciencePrerequisiteList>();
    }

    /// <summary>
    /// Contains an OR'd list of prequisite objects.
    /// </summary>
    public sealed class ObjectPrerequisiteList : List<LazyAssetReference<ObjectDefinition>>
    {
        internal static ObjectPrerequisiteList Parse(IniParser parser)
        {
            var result = new ObjectPrerequisiteList();
            result.AddRange(parser.ParseObjectReferenceArray());
            return result;
        }
    }

    /// <summary>
    /// Contains an OR'd list of prequisite sciences.
    /// </summary>
    public sealed class SciencePrerequisiteList : List<string>
    {
        internal static SciencePrerequisiteList Parse(IniParser parser)
        {
            var result = new SciencePrerequisiteList();
            result.AddRange(parser.ParseAssetReferenceArray());
            return result;
        }
    }
}
