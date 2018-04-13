using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class DdsView : ImageView
    {
        private uint _mipLevel;

        public DdsView(AssetViewContext context)
            : base(context)
        {

        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("mip level", new Vector2(150, 0), true, 0);

            for (var i = 0u; i < Texture.MipLevels; i++)
            {
                if (ImGui.Selectable($"MipMap {i}", i == _mipLevel))
                {
                    _mipLevel = i;
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();

            base.Draw(ref isGameViewFocused);
        }

        protected override Texture GetTexture(AssetViewContext context)
        {
            return context.Game.ContentManager.Load<Texture>(context.Entry.FilePath);
        }

        protected override TextureViewDescription GetTextureViewDescription(Texture texture)
        {
            return new TextureViewDescription(texture, _mipLevel, 1, 0, 1);
        }
    }
}
