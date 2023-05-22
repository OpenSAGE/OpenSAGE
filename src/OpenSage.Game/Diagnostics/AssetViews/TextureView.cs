using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Diagnostics.AssetViews
{
    [AssetView(typeof(Texture))]
    internal sealed class TextureView : AssetView
    {
        private readonly Texture _texture;
        private readonly Dictionary<TextureViewDescription, Veldrid.TextureView> _textureViews;

        private uint _mipLevel;
        private BackgroundColor _backgroundColor;
        private bool _scaleToFit = true;

        public TextureView(DiagnosticViewContext context, Texture texture)
            : base(context)
        {
            _texture = texture;
            _textureViews = new Dictionary<TextureViewDescription, Veldrid.TextureView>();
        }

        public override void Draw()
        {
            ImGui.BeginChild("sidebar", new Vector2(150, 0), false);
            {
                ImGui.BeginChild("mip level", new Vector2(150, 200), true, 0);
                {
                    for (var i = 0u; i < _texture.MipLevels; i++)
                    {
                        if (ImGui.Selectable($"MipMap {i}", i == _mipLevel))
                        {
                            _mipLevel = i;
                        }
                    }
                }
                ImGui.EndChild();

                ImGui.BeginChild("texture viewer settings", new Vector2(150, 0), true, 0);
                {
                    ImGui.Text("Background color");
                    {
                        if (ImGui.RadioButton("None", _backgroundColor == BackgroundColor.None))
                        {
                            _backgroundColor = BackgroundColor.None;
                        }

                        if (ImGui.RadioButton("Black", _backgroundColor == BackgroundColor.Black))
                        {
                            _backgroundColor = BackgroundColor.Black;
                        }

                        if (ImGui.RadioButton("White", _backgroundColor == BackgroundColor.White))
                        {
                            _backgroundColor = BackgroundColor.White;
                        }
                    }

                    ImGui.Checkbox("Scale to fit", ref _scaleToFit);
                }
                ImGui.EndChild();
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

            var size = _scaleToFit
                ? SizeF.CalculateSizeFittingAspectRatio(
                    new SizeF(_texture.Width, _texture.Height),
                    new Size((int) availableSize.X, (int) availableSize.Y)
                )
                : new Size((int)_texture.Width, (int)_texture.Height);

            var uiSize = _scaleToFit ? size.ToVector2() : new Vector2(_texture.Width, _texture.Height);

            ImGui.PushStyleColor(ImGuiCol.ChildBg, _backgroundColor switch {
                BackgroundColor.None => Vector4.Zero,
                BackgroundColor.Black => new Vector4(0, 0, 0, 1),
                BackgroundColor.White => new Vector4(1, 1, 1, 1),
                _ => throw new System.InvalidOperationException()
            });

            ImGui.BeginChild("texture", _scaleToFit ? Vector2.Zero : size.ToVector2(), false, 0);
            {
                ImGui.Image(
                    imagePointer,
                    new Vector2(size.Width, size.Height),
                    Vector2.Zero,
                    Vector2.One,
                    Vector4.One,
                    Vector4.Zero);
            }
            ImGui.EndChild();

            ImGui.PopStyleColor();
        }

        private enum BackgroundColor
        {
            None,
            Black,
            White
        }
    }
}
