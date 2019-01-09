using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Diagnostics.AssetViews
{
    internal sealed class TextureView : AssetView
    {
        private readonly Texture _texture;
        private readonly Dictionary<TextureViewDescription, Veldrid.TextureView> _textureViews;

        private uint _mipLevel;

        public TextureView(DiagnosticViewContext context, Texture texture)
            : base(context)
        {
            _texture = texture;
            _textureViews = new Dictionary<TextureViewDescription, Veldrid.TextureView>();
        }

        public override void Draw()
        {
            ImGui.BeginChild("mip level", new Vector2(150, 0), true, 0);

            for (var i = 0u; i < _texture.MipLevels; i++)
            {
                if (ImGui.Selectable($"MipMap {i}", i == _mipLevel))
                {
                    _mipLevel = i;
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();

            var textureViewDescription = new TextureViewDescription(_texture, _mipLevel, 1, 0, 1);

            if (!_textureViews.TryGetValue(textureViewDescription, out var textureView))
            {
                _textureViews.Add(textureViewDescription, textureView = AddDisposable(Context.Game.GraphicsDevice.ResourceFactory.CreateTextureView(ref textureViewDescription)));
            }

            var imagePointer = Context.ImGuiRenderer.GetOrCreateImGuiBinding(
                Context.Game.GraphicsDevice.ResourceFactory,
                textureView);

            var availableSize = ImGui.GetContentRegionAvail();

            var size = SizeF.CalculateSizeFittingAspectRatio(
                new SizeF(_texture.Width, _texture.Height),
                new Size((int) availableSize.X, (int) availableSize.Y));

            ImGui.Image(
                imagePointer,
                new Vector2(size.Width, size.Height),
                Vector2.Zero,
                Vector2.One,
                Vector4.One,
                Vector4.Zero);
        }
    }
}
