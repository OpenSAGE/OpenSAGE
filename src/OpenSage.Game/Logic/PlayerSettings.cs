using System.Diagnostics;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    public enum PlayerOwner
    {
        Player,
        EasyAi,
        MediumAi,
        HardAi,
        None
    }


    [DebuggerDisplay("[Player:{Name}]")]
    public struct PlayerSetting
    {
        public ColorRgb Color { get; set; }
        public int? StartPosition { get; }
        public string Side { get; set; }
        public PlayerOwner Owner { get; set; }
        public string Name { get; private set; }

        public PlayerSetting(int? startPosition, string side, ColorRgb color, PlayerOwner owner = PlayerOwner.None, string name = "")
        {
            StartPosition = startPosition;
            Side = side;
            Color = color;
            Owner = owner;
            Name = name;
        }
    }
}
