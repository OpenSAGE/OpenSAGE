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

    public struct PlayerSetting
    {
        public ColorRgb Color { get; set; }
        public string Side { get; set; } //TODO: use the playertemplate directly
        public PlayerOwner Owner { get; set; }

        public PlayerSetting(string side, ColorRgb color, PlayerOwner owner = PlayerOwner.None)
        {
            Side = side;
            Color = color;
            Owner = owner;
        }
    }
}
