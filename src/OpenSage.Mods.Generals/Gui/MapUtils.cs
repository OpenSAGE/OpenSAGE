using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Mods.Generals.Gui
{
    public static class MapUtils
    {
        public static Vector2 GetRelativePosition(MapCache cache, int playerIndex)
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
    }
}
