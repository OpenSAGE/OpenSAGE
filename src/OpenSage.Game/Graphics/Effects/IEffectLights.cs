namespace OpenSage.Graphics.Effects
{
    public interface IEffectLights
    {
        LightingType LightingType { get; }
    }

    public enum LightingType
    {
        Terrain,
        Object
    }
}
