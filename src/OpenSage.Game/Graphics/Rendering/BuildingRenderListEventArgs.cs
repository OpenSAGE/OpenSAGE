using System;
using OpenSage.Graphics.Cameras;

namespace OpenSage.Graphics.Rendering
{
    public sealed class BuildingRenderListEventArgs : EventArgs
    {
        public RenderList RenderList { get; }
        public Camera Camera { get; }

        public BuildingRenderListEventArgs(RenderList renderList, Camera camera)
        {
            RenderList = renderList;
            Camera = camera;
        }
    }

    public sealed class GameUpdatingEventArgs : EventArgs
    {
        public GameTime GameTime { get; }

        public GameUpdatingEventArgs(GameTime gameTime)
        {
            GameTime = gameTime;
        }
    }
}
