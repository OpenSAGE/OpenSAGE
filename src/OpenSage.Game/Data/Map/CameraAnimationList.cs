using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.BattleForMiddleEarthII)]
    public sealed class CameraAnimationList : Asset
    {
        public const string AssetName = "CameraAnimationList";

        public CameraAnimation[] Animations { get; private set; }

        internal static CameraAnimationList Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numAnimations = reader.ReadUInt32();
                var animations = new CameraAnimation[numAnimations];

                for (var i = 0; i < numAnimations; i++)
                {
                    animations[i] = CameraAnimation.Parse(reader);
                }

                return new CameraAnimationList
                {
                    Animations = animations
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Animations.Length);

                foreach (var animation in Animations)
                {
                    animation.WriteTo(writer);
                }
            });
        }
    }
}
