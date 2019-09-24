namespace OpenSage
{
    public abstract class BaseSingletonAsset : BaseAsset
    {
        protected BaseSingletonAsset()
        {
            SetNameAndInstanceId(GetType().Name, GetType().Name);
        }
    }
}
