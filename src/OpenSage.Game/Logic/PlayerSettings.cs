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
        public PlayerTemplate Template { get; set; }
        public PlayerOwner Owner { get; set; }
        public string Name { get; set; }

        public PlayerSetting(int? startPosition, PlayerTemplate template, ColorRgb color, PlayerOwner owner = PlayerOwner.None, string name = "")
        {
            StartPosition = startPosition;
            Template = template;
            Color = color;
            Owner = owner;
            Name = name;
        }
    }
}
