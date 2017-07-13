namespace OpenZH.Data.Map
{
    public enum BlendDirection : byte
    {
        BlendTowardsRight = 1,  // Or towards right, if BlendDescription.Flags contains Reversed
        BlendTowardsTop = 2,     // Or towards bottom, if BlendDescription.Flags contains Reversed
        BlendTowardsTopLeft = 4, // Or towards bottom right, if BlendDescription.Flags contains Reversed
        BlendTowardsTopRight = 8 // Or towards bottom left, if BlendDescription.Flags contains Reversed
    }
}
