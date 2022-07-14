using OpenSage.Content;

namespace OpenSage
{
    public abstract class BaseAsset : DisposableBase
    {
        public string FullName { get; private set; }
        public string Name { get; private set; }
        public uint InstanceId { get; private set; }

        protected void SetNameAndInstanceId(string typeName, string name)
        {
            Name = name;
            FullName = $"{typeName}:{name}";
            InstanceId = AssetHash.GetHash(name);
        }

        public int InternalId { get; internal set; }

        internal BaseAsset DeepClone()
        {
            // TODO: Deep-clone asset so that it can be overridden.
            return (BaseAsset) MemberwiseClone();
        }
    }
}
