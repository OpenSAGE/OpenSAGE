using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.BattleForMiddleEarthII)]
    public sealed class TriggerAreas : Asset
    {
        public const string AssetName = "TriggerAreas";

        public TriggerArea[] Areas { get; private set; }

        internal static TriggerAreas Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numTriggers = reader.ReadUInt32();
                var triggers = new TriggerArea[numTriggers];

                for (var i = 0; i < numTriggers; i++)
                {
                    triggers[i] = TriggerArea.Parse(reader);
                }

                return new TriggerAreas
                {
                    Areas = triggers
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Areas.Length);

                foreach (var trigger in Areas)
                {
                    trigger.WriteTo(writer);
                }
            });
        }
    }
}
