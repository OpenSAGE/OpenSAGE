using System;
using System.IO;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class FireSpreadUpdate : UpdateModule
    {
        // TODO

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

    public sealed class FireSpreadUpdateModuleData : UpdateModuleData
    {
        internal static FireSpreadUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireSpreadUpdateModuleData> FieldParseTable = new IniParseTable<FireSpreadUpdateModuleData>
        {
            { "OCLEmbers", (parser, x) => x.OCLEmbers = parser.ParseObjectCreationListReference() },
            { "MinSpreadDelay", (parser, x) => x.MinSpreadDelay = parser.ParseTimeMilliseconds() },
            { "MaxSpreadDelay", (parser, x) => x.MaxSpreadDelay = parser.ParseTimeMilliseconds() },
            { "SpreadTryRange", (parser, x) => x.SpreadTryRange = parser.ParseInteger() }
        };

        public LazyAssetReference<ObjectCreationList> OCLEmbers { get; private set; }
        public TimeSpan MinSpreadDelay { get; private set; }
        public TimeSpan MaxSpreadDelay { get; private set; }
        public int SpreadTryRange { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new FireSpreadUpdate();
        }
    }
}
