namespace OpenSage.Graphics.Cameras.Controllers
{
    public abstract class CameraController : EntityComponent
    {
        public abstract void OnLeftMouseButtonDragged(float deltaX, float deltaY);
        public abstract void OnMiddleMouseButtonDragged(float deltaY);
        public abstract void OnRightMouseButtonDragged(float deltaX, float deltaY);
    }
}
