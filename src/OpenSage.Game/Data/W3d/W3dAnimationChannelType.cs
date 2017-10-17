namespace OpenSage.Data.W3d
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

        TimeCodedTranslationX,
        TimeCodedTranslationY,
        TimeCodedTranslationZ,
        TimeCodedQuaternion,

        AdaptiveDeltaTranslationX,
        AdaptiveDeltaTranslationY,
        AdaptiveDeltaTranslationZ,
        AdaptiveDeltaQuaternion,

        UnknownBfme
    }
}
