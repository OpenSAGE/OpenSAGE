using System.Collections;
using System.Collections.Generic;
using OpenSage.Audio;
using OpenSage.Content.Loaders;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Graphics;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui.Wnd.Transitions;
using OpenSage.Logic.Object;
using OpenSage.Terrain;
using Veldrid;

namespace OpenSage.Content
{
    public sealed class OnDemandAssetLoadStrategy
    {
        private readonly bool _allowOnDemandLoading;
        private readonly IPathResolver _w3dPathResolver;
        private readonly IPathResolver _texturePathResolver;

        public static readonly OnDemandAssetLoadStrategy None = new OnDemandAssetLoadStrategy(false);

        public OnDemandAssetLoadStrategy(IPathResolver w3dPathResolver, IPathResolver texturePathResolver)
            : this(true)
        {
            _w3dPathResolver = w3dPathResolver;
            _texturePathResolver = texturePathResolver;
        }

        private OnDemandAssetLoadStrategy(bool allowOnDemandLoading)
        {
            _allowOnDemandLoading = allowOnDemandLoading;
        }

        public OnDemandAnimationLoader CreateAnimationLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandAnimationLoader(_w3dPathResolver)
                : null;
        }

        public OnDemandModelLoader CreateModelLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandModelLoader(_w3dPathResolver)
                : null;
        }

        public OnDemandModelBoneHierarchyLoader CreateModelBoneHierarchyLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandModelBoneHierarchyLoader(_w3dPathResolver)
                : null;
        }

        public OnDemandTextureLoader CreateGuiTextureLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandTextureLoader(false, _texturePathResolver)
                : null;
        }

        public OnDemandTextureLoader CreateTextureLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandTextureLoader(true, _texturePathResolver)
                : null;
        }
    }

    internal sealed class AssetStore
    {
        private readonly List<IScopedAssetCollection> _scopedAssetCollections;

        internal AssetLoadContext LoadContext { get; }

        public NamedScopedAssetCollection<AmbientStream> AmbientStreams { get; }
        public ScopedAnimationCollection Animations { get; }
        public NamedScopedAssetCollection<AudioEvent> AudioEvents { get; }
        public NamedScopedAssetCollection<AudioFile> AudioFiles { get; }
        public ScopedAudioLodCollection AudioLods { get; }
        public AudioSettings AudioSettings { get; internal set; }
        public NamedScopedAssetCollection<BridgeTemplate> BridgeTemplates { get; }

        [AddedIn(SageGame.Bfme2)]
        public NamedScopedAssetCollection<CrowdResponse> CrowdResponses { get; }

        public NamedScopedAssetCollection<DialogEvent> DialogEvents { get; }
        public NamedScopedAssetCollection<FXParticleSystemTemplate> FXParticleSystems { get; }
        public ScopedTextureCollection GuiTextures { get; }
        public NamedScopedAssetCollection<Locomotor> Locomotors { get; }
        public NamedScopedAssetCollection<MappedImage> MappedImages { get; }
        public ScopedModelCollection Models { get; }
        public ScopedModelBoneHierarchyCollection ModelBoneHierarchies { get; }
        public NamedScopedAssetCollection<MusicTrack> MusicTracks { get; }
        public ScopedObjectDefinitionCollection ObjectDefinitions { get; }
        public NamedScopedAssetCollection<ParticleSystemDefinition> ParticleSystems { get; }
        public ScopedPlayerTemplateCollection PlayerTemplates { get; }
        public NamedScopedAssetCollection<RoadTemplate> RoadTemplates { get; }
        public NamedScopedAssetCollection<StreamedSound> StreamedSounds { get; }
        public NamedScopedAssetCollection<TerrainTexture> TerrainTextures { get; }
        public ScopedTextureCollection Textures { get; }
        public ScopedWaterSetCollection WaterSets { get; }
        public NamedScopedAssetCollection<WindowTransition> WindowTransitions { get; }

        public AssetStore(
            FileSystem fileSystem,
            string language,
            GraphicsDevice graphicsDevice,
            StandardGraphicsResources standardGraphicsResources,
            ShaderResourceManager shaderResources,
            OnDemandAssetLoadStrategy loadStrategy)
        {
            LoadContext = new AssetLoadContext(
                fileSystem,
                language,
                graphicsDevice,
                standardGraphicsResources,
                shaderResources,
                this);

            _scopedAssetCollections = new List<IScopedAssetCollection>
            {
                (AmbientStreams = new NamedScopedAssetCollection<AmbientStream>(this)),
                (Animations = new ScopedAnimationCollection(this, loadStrategy.CreateAnimationLoader())),
                (AudioEvents = new NamedScopedAssetCollection<AudioEvent>(this)),
                (AudioFiles = new NamedScopedAssetCollection<AudioFile>(this)),
                (AudioLods = new ScopedAudioLodCollection(this)),
                (BridgeTemplates = new NamedScopedAssetCollection<BridgeTemplate>(this)),
                (CrowdResponses = new NamedScopedAssetCollection<CrowdResponse>(this)),
                (DialogEvents = new NamedScopedAssetCollection<DialogEvent>(this)),
                (FXParticleSystems = new NamedScopedAssetCollection<FXParticleSystemTemplate>(this)),
                (GuiTextures = new ScopedTextureCollection(this, loadStrategy.CreateGuiTextureLoader())),
                (Locomotors = new NamedScopedAssetCollection<Locomotor>(this)),
                (MappedImages = new NamedScopedAssetCollection<MappedImage>(this)),
                (Models = new ScopedModelCollection(this, loadStrategy.CreateModelLoader())),
                (ModelBoneHierarchies = new ScopedModelBoneHierarchyCollection(this, loadStrategy.CreateModelBoneHierarchyLoader())),
                (MusicTracks = new NamedScopedAssetCollection<MusicTrack>(this)),
                (ObjectDefinitions = new ScopedObjectDefinitionCollection(this)),
                (ParticleSystems = new NamedScopedAssetCollection<ParticleSystemDefinition>(this)),
                (PlayerTemplates = new ScopedPlayerTemplateCollection(this)),
                (RoadTemplates = new NamedScopedAssetCollection<RoadTemplate>(this)),
                (StreamedSounds = new NamedScopedAssetCollection<StreamedSound>(this)),
                (TerrainTextures = new NamedScopedAssetCollection<TerrainTexture>(this)),
                (Textures = new ScopedTextureCollection(this, loadStrategy.CreateTextureLoader())),
                (WaterSets = new ScopedWaterSetCollection(this)),
                (WindowTransitions = new NamedScopedAssetCollection<WindowTransition>(this)),
            };
        }

        internal void PushScope()
        {
            foreach (var scopedAssetCollection in _scopedAssetCollections)
            {
                scopedAssetCollection.PushScope();
            }
        }

        internal void PopScope()
        {
            foreach (var scopedAssetCollection in _scopedAssetCollections)
            {
                scopedAssetCollection.PopScope();
            }
        }
    }

    internal interface IScopedAssetCollection
    {
        void PushScope();
        void PopScope();
    }

    public abstract class ScopedAssetCollection<TKey, TValue> : IScopedAssetCollection, IEnumerable<TValue>
        where TValue : class
    {
        private readonly Stack<Dictionary<TKey, TValue>> _assetScopes = new Stack<Dictionary<TKey, TValue>>();
        private readonly AssetStore _assetStore;
        private readonly IOnDemandAssetLoader<TKey, TValue> _loader;

        private protected ScopedAssetCollection(AssetStore assetStore, IOnDemandAssetLoader<TKey, TValue> loader = null)
        {
            _assetStore = assetStore;
            _loader = loader;
        }

        void IScopedAssetCollection.PushScope()
        {
            _assetScopes.Push(new Dictionary<TKey, TValue>());
        }

        void IScopedAssetCollection.PopScope()
        {
            var assetScope = _assetScopes.Pop();
            foreach (var asset in assetScope.Values)
            {
                OnRemovingAsset(asset);
            }
        }

        protected virtual void OnAddingAsset(TValue asset) { }
        protected virtual void OnRemovingAsset(TValue asset) { }

        protected TValue GetByKey(TKey key)
        {
            var normalizedKey = NormalizeKey(key);

            // Find existing cached item.
            foreach (var assetScope in _assetScopes)
            {
                if (assetScope.TryGetValue(normalizedKey, out var result))
                {
                    return result;
                }
            }

            // Create new item and cache it.
            var newValue = default(TValue);
            if (_loader != null)
            {
                newValue = _loader.Load(normalizedKey, _assetStore.LoadContext);
            }
            _assetScopes.Peek().Add(normalizedKey, newValue);
            return newValue;
        }

        protected virtual TKey NormalizeKey(TKey key) => key;

        internal void Add(TValue asset)
        {
            // Existing entries take precedence
            var assetScope = _assetScopes.Peek();
            var key = GetKey(asset);
            if (!assetScope.ContainsKey(key))
            {
                assetScope.Add(key, asset);
                OnAddingAsset(asset);
            }
        }

        protected abstract TKey GetKey(TValue asset);

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            foreach (var assetScope in _assetScopes)
            {
                foreach (var asset in assetScope.Values)
                {
                    yield return asset;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TValue>)this).GetEnumerator();
    }

    public class ScopedWaterSetCollection : ScopedAssetCollection<TimeOfDay, WaterSet>
    {
        internal ScopedWaterSetCollection(AssetStore assetStore)
            : base(assetStore)
        {
        }

        public WaterSet GetByTimeOfDay(TimeOfDay timeOfDay) => GetByKey(timeOfDay);

        protected override TimeOfDay GetKey(WaterSet asset) => asset.TimeOfDay;
    }

    public class NamedScopedAssetCollection<T> : ScopedAssetCollection<string, T>
        where T : class, IHasName
    {
        internal NamedScopedAssetCollection(AssetStore assetStore)
            : base(assetStore)
        {
        }

        public T GetByName(string name) => GetByKey(name);

        protected override string GetKey(T asset) => asset.Name;
    }

    internal sealed class ScopedObjectDefinitionCollection : NamedScopedAssetCollection<ObjectDefinition>
    {
        private readonly Dictionary<int, ObjectDefinition> _byId;
        private int _nextId;

        public ScopedObjectDefinitionCollection(AssetStore assetStore)
            : base(assetStore)
        {
            _nextId = 1;
            _byId = new Dictionary<int, ObjectDefinition>();
        }

        public ObjectDefinition GetById(int id) => _byId[id];

        protected override void OnAddingAsset(ObjectDefinition asset)
        {
            asset.InternalId = _nextId;

            _byId.Add(_nextId, asset);

            _nextId++;
        }

        protected override void OnRemovingAsset(ObjectDefinition asset)
        {
            _byId.Remove(asset.InternalId);
        }

        protected override string GetKey(ObjectDefinition asset) => asset.Name;
    }

    public sealed class ScopedPlayerTemplateCollection : NamedScopedAssetCollection<PlayerTemplate>
    {
        private readonly List<PlayerTemplate> _list;
        private readonly Dictionary<string, PlayerTemplate> _bySide;
        private readonly List<PlayerTemplate> _playableSides;

        public IReadOnlyList<PlayerTemplate> PlayableSides => _playableSides;

        public int Count => _list.Count;

        internal ScopedPlayerTemplateCollection(AssetStore assetStore)
            : base(assetStore)
        {
            _list = new List<PlayerTemplate>();
            _bySide = new Dictionary<string, PlayerTemplate>();
            _playableSides = new List<PlayerTemplate>();
        }

        protected override void OnAddingAsset(PlayerTemplate asset)
        {
            // TODO: Not sure using Side as a key is correct because they're not unique within player templates.
            _bySide[asset.Side] = asset;

            if (asset.PlayableSide)
            {
                _playableSides.Add(asset);
            }
        }

        protected override void OnRemovingAsset(PlayerTemplate asset)
        {
            _bySide.Remove(asset.Side);
            _playableSides.Remove(asset);
        }

        public PlayerTemplate GetBySide(string side)
        {
            _bySide.TryGetValue(side, out var result);
            return result;
        }

        public PlayerTemplate GetByIndex(int index)
        {
            return _list[index];
        }
    }
}
