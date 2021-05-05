using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class GrantUpgradeCreate : CreateModule
    {
        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);
        }
    }

    public sealed class GrantUpgradeCreateModuleData : CreateModuleData
    {
        internal static GrantUpgradeCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GrantUpgradeCreateModuleData> FieldParseTable = new IniParseTable<GrantUpgradeCreateModuleData>
        {
            { "UpgradeToGrant", (parser, x) => x.UpgradeToGrant = parser.ParseAssetReference() },
            { "ExemptStatus", (parser, x) => x.ExemptStatus = parser.ParseEnum<ObjectStatus>() },
            { "GiveOnBuildComplete", (parser, x) => x.GiveOnBuildComplete = parser.ParseBoolean() }
        };

        public string UpgradeToGrant { get; private set; }
        public ObjectStatus ExemptStatus { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool GiveOnBuildComplete { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new GrantUpgradeCreate();
        }
    }
}
