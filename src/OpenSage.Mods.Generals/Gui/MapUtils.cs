using System.IO;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;
using OpenSage.Data;

namespace OpenSage.Mods.Generals.Gui
{
    public static class MapUtils
    {
        private static Vector2 GetRelativePosition(MapCache cache, int playerIndex)
        {
            var startPos = Vector3.Zero;

            switch (playerIndex)
            {
                case 0:
                    startPos = cache.Player1Start;
                    break;
                case 1:
                    startPos = cache.Player2Start;
                    break;
                case 2:
                    startPos = cache.Player3Start;
                    break;
                case 3:
                    startPos = cache.Player4Start;
                    break;
                case 4:
                    startPos = cache.Player5Start;
                    break;
                case 5:
                    startPos = cache.Player6Start;
                    break;
                case 6:
                    startPos = cache.Player7Start;
                    break;
                case 7:
                    startPos = cache.Player8Start;
                    break;
            }

            var relPos = startPos / cache.ExtentMax;

            return new Vector2(relPos.X, relPos.Y);
        }

        public static void SetMapPreview(MapCache mapCache, Control mapWindow, Game game)
        {
            var mapPath = FileSystem.NormalizeFilePath(mapCache.Name);
            var basePath = Path.GetDirectoryName(mapPath) + "/" + Path.GetFileNameWithoutExtension(mapPath);
            var thumbPath = basePath + ".tga";

            // Set thumbnail
            var thumbTexture = game.AssetStore.GuiTextures.GetByName(thumbPath).Texture;
            mapWindow.Data["MapPreview"] = thumbTexture;

            // Hide all start positions
            for (var i = 0; i < 8; ++i)
            {
                mapWindow.Controls[i].Hide();
            }

            // Set starting positions
            for (var i = 0; i < mapCache.NumPlayers; ++i)
            {
                var startPosCtrl = mapWindow.Controls[i];
                startPosCtrl.Show();

                var relPos = GetRelativePosition(mapCache, i);

                var newPos = new Point2D((int) (relPos.X * mapWindow.Width) - 8, (int) ((1.0 - relPos.Y) * mapWindow.Height) - 8);

                startPosCtrl.Bounds = new Rectangle(newPos, new Size(16));
            }
        }
    }
}
