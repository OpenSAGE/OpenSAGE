namespace OpenSage.Data.Map
{
    public enum AssetPropertyType : byte
    {
        Boolean = 0,
        Integer = 1,
        RealNumber = 2,
        AsciiString = 3,
        UnicodeString = 4,

        [AddedIn(SageGame.Cnc3)]
        Unknown = 5,
    }
}
