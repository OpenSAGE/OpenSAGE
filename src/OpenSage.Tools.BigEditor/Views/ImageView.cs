using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using ImGuiNET;
using OpenSage.Mathematics;
using Veldrid;
using Veldrid.ImageSharp;

namespace OpenSage.Tools.BigEditor.Views
{
    class ImageView : View
    {
        private Texture _texture;
        private uint _mipLevel;
        private GraphicsDevice _graphicsDevice;
        private ImGuiRenderer _imguiRenderer;
        private readonly Dictionary<TextureViewDescription, TextureView> _textureViews;

        public ImageView(Stream stream, GraphicsDevice gd, ImGuiRenderer renderer)
        {
            _graphicsDevice = gd;
            _imguiRenderer = renderer;

            var imageSharpTex = new ImageSharpTexture(stream);
            _texture = imageSharpTex.CreateDeviceTexture(_graphicsDevice, _graphicsDevice.ResourceFactory);
            _textureViews = new Dictionary<TextureViewDescription, TextureView>();
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
                _textureViews.Add(textureViewDescription, textureView = AddDisposable(_graphicsDevice.ResourceFactory.CreateTextureView(ref textureViewDescription)));
            }

            var imagePointer = _imguiRenderer.GetOrCreateImGuiBinding(
                _graphicsDevice.ResourceFactory,
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
