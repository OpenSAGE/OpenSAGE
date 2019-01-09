using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ImGuiNET;
using OpenSage.Diagnostics.AssetViews;

namespace OpenSage.Diagnostics
{
    internal sealed class AssetListView : DiagnosticView
    {
        private readonly List<string> _audioFilenames;

        private object _currentAsset;
        private AssetView _currentAssetView;

        public override string DisplayName { get; } = "Asset List";

        public override Vector2 DefaultSize { get; } = new Vector2(600, 400);

        public AssetListView(DiagnosticViewContext context)
            : base(context)
        {
            // TODO: This actually needs to use assets that have already been loaded.
            // And update when assets are loaded or unloaded.

            // TODO: Remove this.
            _audioFilenames = new List<string>();
            foreach (var entry in context.Game.ContentManager.FileSystem.Files)
            {
                switch (Path.GetExtension(entry.FilePath).ToLowerInvariant())
                {
                    case ".mp3":
                    case ".wav":
                        _audioFilenames.Add(entry.FilePath);
                        break;
                }
            }
        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("asset list sidebar", new Vector2(200, 0), true, 0);

            void DrawAssetItem(object asset, string assetName, Func<AssetView> createAssetView)
            {
                if (ImGui.Selectable(assetName, asset == _currentAsset))
                {
                    _currentAsset = asset;

                    RemoveAndDispose(ref _currentAssetView);

                    _currentAssetView = AddDisposable(createAssetView());
                }
                ImGuiUtility.DisplayTooltipOnHover(assetName);
            }

            foreach (var asset in Game.ContentManager.CachedObjects)
            {
                var assetName = AssetView.GetAssetName(asset);
                if (assetName == null)
                {
                    continue;
                }

                DrawAssetItem(asset, assetName, () => AssetView.CreateAssetView(Context, _currentAsset));
            }

            // TODO: Remove this, once audio assets are handled the same as other assets.
            foreach (var audioFilename in _audioFilenames)
            {
                DrawAssetItem(audioFilename, $"Audio:{audioFilename}", () => new SoundView(Context, audioFilename));
            }

            foreach (var particleSystemDefinition in Context.Game.ContentManager.IniDataContext.ParticleSystems)
            {
                DrawAssetItem(particleSystemDefinition, $"ParticleSystem:{particleSystemDefinition.Name}", () => new ParticleSystemView(Context, particleSystemDefinition.ToFXParticleSystemTemplate()));
            }

            foreach (var particleSystemTemplate in Context.Game.ContentManager.IniDataContext.FXParticleSystems)
            {
                DrawAssetItem(particleSystemTemplate, $"FXParticleSystem:{particleSystemTemplate.Name}", () => new ParticleSystemView(Context, particleSystemTemplate));
            }

            ImGui.EndChild();

            ImGui.SameLine();

            if (_currentAsset != null)
            {
                ImGui.BeginChild("asset view");
                _currentAssetView.Draw();
                ImGui.EndChild();
            }
            else
            {
                ImGui.Text("Select a previewable asset.");
            }
        }
    }
}
