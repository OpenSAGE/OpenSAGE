using System.Collections.Generic;
using OpenSage.Audio;
using OpenSage.Content.Loaders;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.Graphics;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.Wnd.Transitions;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Terrain;
using Veldrid;

namespace OpenSage.Content
{
    public sealed class AssetStore
    {
        private readonly List<IScopedAssetCollection> _scopedAssetCollections;

        internal AssetLoadContext LoadContext { get; }

        // Singletons
        public AudioSettings AudioSettings { get; internal set; }

        // Collections
        public ScopedAssetCollection<string, AmbientStream> AmbientStreams { get; }
        public ScopedAssetCollection<string, Graphics.Animation.Animation> Animations { get; }
        public ScopedAssetCollection<string, AudioEvent> AudioEvents { get; }
        public ScopedAssetCollection<string, AudioFile> AudioFiles { get; }
        public ScopedAssetCollection<string, AudioLod> AudioLods { get; }
        public ScopedAssetCollection<string, BridgeTemplate> BridgeTemplates { get; }
        public ScopedAssetCollection<string, CampaignTemplate> CampaignTemplates { get; }
        public ScopedAssetCollection<string, CommandButton> CommandButtons { get; }
        public ScopedAssetCollection<string, CommandSet> CommandSets { get; }

        [AddedIn(SageGame.Bfme2)]
        public ScopedAssetCollection<string, CrowdResponse> CrowdResponses { get; }

        public ScopedAssetCollection<string, DialogEvent> DialogEvents { get; }
        public ScopedAssetCollection<string, FXParticleSystemTemplate> FXParticleSystemTemplates { get; }
        public ScopedAssetCollection<string, Texture> GuiTextures { get; }
        public ScopedAssetCollection<string, HeaderTemplate> HeaderTemplates { get; }
        public ScopedAssetCollection<string, Locomotor> Locomotors { get; }
        public ScopedAssetCollection<string, MappedImage> MappedImages { get; }
        public ScopedAssetCollection<string, Model> Models { get; }
        public ScopedAssetCollection<string, ModelBoneHierarchy> ModelBoneHierarchies { get; }
        public ScopedAssetCollection<string, MusicTrack> MusicTracks { get; }
        public ScopedAssetCollection<string, ObjectDefinition> ObjectDefinitions { get; }
        public ScopedAssetCollection<string, ParticleSystemTemplate> ParticleSystemTemplates { get; }
        public ScopedAssetCollection<string, PlayerTemplate> PlayerTemplates { get; }
        public ScopedAssetCollection<string, RoadTemplate> RoadTemplates { get; }
        public ScopedAssetCollection<string, StreamedSound> StreamedSounds { get; }
        public ScopedAssetCollection<string, TerrainTexture> TerrainTextures { get; }
        public ScopedAssetCollection<string, Texture> Textures { get; }
        public ScopedAssetCollection<string, Upgrade> Upgrades { get; }
        public ScopedAssetCollection<string, WaterSet> WaterSets { get; }
        public ScopedAssetCollection<string, WindowTransition> WindowTransitions { get; }

        internal AssetStore(
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
                (AmbientStreams = new ScopedAssetCollection<string, AmbientStream>(this, x => x.Name)),
                (Animations = new ScopedAssetCollection<string, Graphics.Animation.Animation>(this, x => x.Name, FileSystem.NormalizeFilePath, loadStrategy.CreateAnimationLoader())),
                (AudioEvents = new ScopedAssetCollection<string, AudioEvent>(this, x => x.Name)),
                (AudioFiles = new ScopedAssetCollection<string, AudioFile>(this, x => x.Name, loader: loadStrategy.CreateAudioFileLoader())),
                (AudioLods = new ScopedAssetCollection<string, AudioLod>(this, x => x.Level.ToString())),
                (BridgeTemplates = new ScopedAssetCollection<string, BridgeTemplate>(this, x => x.Name)),
                (CampaignTemplates = new ScopedAssetCollection<string, CampaignTemplate>(this, x => x.Name)),
                (CommandButtons = new ScopedAssetCollection<string, CommandButton>(this, x => x.Name)),
                (CommandSets = new ScopedAssetCollection<string, CommandSet>(this, x => x.Name)),
                (CrowdResponses = new ScopedAssetCollection<string, CrowdResponse>(this, x => x.Name)),
                (DialogEvents = new ScopedAssetCollection<string, DialogEvent>(this, x => x.Name)),
                (FXParticleSystemTemplates = new ScopedAssetCollection<string, FXParticleSystemTemplate>(this, x => x.Name)),
                (GuiTextures = new ScopedAssetCollection<string, Texture>(this, x => x.Name, FileSystem.NormalizeFilePath, loadStrategy.CreateGuiTextureLoader())),
                (HeaderTemplates = new ScopedAssetCollection<string, HeaderTemplate>(this, x => x.Name)),
                (Locomotors = new ScopedAssetCollection<string, Locomotor>(this, x => x.Name)),
                (MappedImages = new ScopedAssetCollection<string, MappedImage>(this, x => x.Name)),
                (Models = new ScopedAssetCollection<string, Model>(this, x => x.Name, FileSystem.NormalizeFilePath, loadStrategy.CreateModelLoader())),
                (ModelBoneHierarchies = new ScopedAssetCollection<string, ModelBoneHierarchy>(this, x => x.Name, FileSystem.NormalizeFilePath, loadStrategy.CreateModelBoneHierarchyLoader())),
                (MusicTracks = new ScopedAssetCollection<string, MusicTrack>(this, x => x.Name)),
                (ObjectDefinitions = new ScopedAssetCollection<string, ObjectDefinition>(this, x => x.Name)),
                (ParticleSystemTemplates = new ScopedAssetCollection<string, ParticleSystemTemplate>(this, x => x.Name)),
                (PlayerTemplates = new ScopedAssetCollection<string, PlayerTemplate>(this, x => x.Name)),
                (RoadTemplates = new ScopedAssetCollection<string, RoadTemplate>(this, x => x.Name)),
                (StreamedSounds = new ScopedAssetCollection<string, StreamedSound>(this, x => x.Name)),
                (TerrainTextures = new ScopedAssetCollection<string, TerrainTexture>(this, x => x.Name)),
                (Textures = new ScopedAssetCollection<string, Texture>(this, x => x.Name, FileSystem.NormalizeFilePath, loadStrategy.CreateTextureLoader())),
                (Upgrades = new ScopedAssetCollection<string, Upgrade>(this, x => x.Name)),
                (WaterSets = new ScopedAssetCollection<string, WaterSet>(this, x => x.TimeOfDay.ToString())),
                (WindowTransitions = new ScopedAssetCollection<string, WindowTransition>(this, x => x.Name)),
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

        internal IEnumerable<object> GetAllAssets()
        {
            foreach (var scopedAssetCollection in _scopedAssetCollections)
            {
                foreach (var asset in scopedAssetCollection)
                {
                    yield return asset;
                }
            }
        }
    }
}
