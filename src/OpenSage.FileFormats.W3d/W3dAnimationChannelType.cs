namespace OpenSage.FileFormats.W3d
{
    public enum W3dAnimationChannelType : ushort
    {
        TranslationX,
        TranslationY,
        TranslationZ,
        XR,
        YR,
        ZR,
        Quaternion,

        UnknownBfme = 15, //probably Opacity
    }
}
