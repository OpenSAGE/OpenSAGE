namespace OpenSage.Data.Map
{
    public enum BlendDirection : byte
    {
        BlendTowardsRight = 1,    // Or towards left, if BlendDescription.Flags contains Flipped
        BlendTowardsTop = 2,      // Or towards bottom, if BlendDescription.Flags contains Flipped
        BlendTowardsTopRight = 4, // Or towards bottom left, if BlendDescription.Flags contains Flipped
        BlendTowardsTopLeft = 8   // Or towards bottom right, if BlendDescription.Flags contains Flipped
    }
}
