using OpenSage.Content;
using OpenSage.Data.StreamFS;

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

        protected void SetNameAndInstanceId(Asset asset)
        {
            Name = asset.Name.Substring(asset.Name.IndexOf(':') + 1);
            FullName = asset.Name;
            InstanceId = asset.Header.InstanceId;
        }

        public int InternalId { get; internal set; }

        internal virtual BaseAsset DeepClone()
        {
            // TODO: Deep-clone asset so that it can be overridden.
            return (BaseAsset) MemberwiseClone();
        }
    }
}
