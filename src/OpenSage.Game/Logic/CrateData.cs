#nullable enable

using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    public sealed class CrateData : BaseAsset
    {
        internal static CrateData Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("CrateData", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<CrateData> FieldParseTable = new IniParseTable<CrateData>
        {
            { "CreationChance", (parser, x) => x.CreationChance = parser.ParseFloat() },
            { "KilledByType", (parser, x) => x.KilledByType = parser.ParseEnum<ObjectKinds>() },
            { "KillerScience", (parser, x) => x.KillerScience = parser.ParseScienceReference() },
            { "VeterancyLevel", (parser, x) => x.VeterancyLevel = parser.ParseEnum<VeterancyLevel>() },
            { "OwnedByMaker", (parser, x) => x.OwnedByMaker = parser.ParseBoolean() },
            { "CrateObject", (parser, x) => x.CrateObjects.Add(CrateObject.Parse(parser)) },
        };

        /// <summary>
        /// Chance of a crate being created.
        /// </summary>
        public float CreationChance { get; private set; }

        /// <summary>
        /// <see cref="ObjectKinds"/> required by the killer in order to create the crate.
        /// </summary>
        public ObjectKinds? KilledByType { get; private set; }

        /// <summary>
        /// Science required by the killer in order to create the crate.
        /// </summary>
        public LazyAssetReference<Science>? KillerScience { get; private set; }

        /// <summary>
        /// The <b>victim</b> must have this veterancy level in order to generate the crate.
        /// </summary>
        public VeterancyLevel? VeterancyLevel { get; private set; }

        /// <summary>
        /// Used "to have the Crate assigned to the default team of the dead guy's player for scripting."
        /// </summary>
        public bool OwnedByMaker { get; private set; }

        /// <summary>
        /// Different crates which may be created if all conditions succeed.
        /// </summary>
        public List<CrateObject> CrateObjects { get; } = [];
    }

    public enum VeterancyLevel
    {
        [IniEnum("REGULAR")]
        Regular,

        [IniEnum("VETERAN")]
        Veteran,

        [IniEnum("ELITE")]
        Elite,

        [IniEnum("HEROIC")]
        Heroic,
    }

    public readonly record struct CrateObject(LazyAssetReference<ObjectDefinition>? Object, float Probability)
    {
        internal static CrateObject Parse(IniParser parser)
        {
            return new CrateObject(parser.ParseObjectReference(), parser.ParseFloat());
        }
    }
}
