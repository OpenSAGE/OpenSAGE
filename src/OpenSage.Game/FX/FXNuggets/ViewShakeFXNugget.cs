using System;
using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    public sealed class ViewShakeFXNugget : FXNugget
    {
        internal static ViewShakeFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ViewShakeFXNugget> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<ViewShakeFXNugget>
        {
            { "Type", (parser, x) => x.Type = parser.ParseEnum<ViewShakeType>() }
        });

        public ViewShakeType Type { get; private set; }

        internal override void Execute(FXListExecutionContext context)
        {
            var gameData = context.GameContext.AssetLoadContext.AssetStore.GameData.Current;

            var intensity = Math.Min(GetShakeIntensity(gameData), gameData.MaxShakeIntensity);

            // TODO: What is MaxShakeRange - is it the maximum distance a unit can be
            // offscreen while still causing the camera to shake?

            // TODO: Implement this.
        }

        private float GetShakeIntensity(GameData gameData) => Type switch
        {
            ViewShakeType.Subtle => gameData.ShakeSubtleIntensity,
            ViewShakeType.Normal => gameData.ShakeNormalIntensity,
            ViewShakeType.Strong => gameData.ShakeStrongIntensity,
            ViewShakeType.Severe => gameData.ShakeSevereIntensity,
            ViewShakeType.CineExtreme => gameData.ShakeCineExtremeIntensity,
            ViewShakeType.CineInsane => gameData.ShakeCineInsaneIntensity,
            _ => throw new InvalidOperationException(),
        };
    }
}
