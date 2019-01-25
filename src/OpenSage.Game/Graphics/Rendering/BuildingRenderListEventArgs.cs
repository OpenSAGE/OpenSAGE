using System;
using OpenSage.Graphics.Cameras;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class BuildingRenderListEventArgs : EventArgs
    {
        public readonly RenderList RenderList;
        public readonly Camera Camera;
        public readonly TimeInterval GameTime;

        internal BuildingRenderListEventArgs(RenderList renderList, Camera camera, in TimeInterval gameTime)
        {
            RenderList = renderList;
            Camera = camera;
            GameTime = gameTime;
        }
    }
}
