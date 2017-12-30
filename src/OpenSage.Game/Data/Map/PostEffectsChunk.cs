using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.BattleForMiddleEarthII)]
    public sealed class PostEffectsChunk : Asset
    {
        public const string AssetName = "PostEffectsChunk";

        public PostEffect[] PostEffects { get; private set; }

        internal static PostEffectsChunk Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numPostEffects = reader.ReadByte();
                var postEffects = new PostEffect[numPostEffects];

                for (var i = 0; i < numPostEffects; i++)
                {
                    postEffects[i] = PostEffect.Parse(reader);
                }

                return new PostEffectsChunk
                {
                    PostEffects = postEffects
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((byte) PostEffects.Length);

                foreach (var postEffect in PostEffects)
                {
                    postEffect.WriteTo(writer);
                }
            });
        }
    }
}
