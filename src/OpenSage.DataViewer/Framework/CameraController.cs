namespace OpenSage.DataViewer.Framework
{
    public abstract class CameraController
    {
        public abstract void OnLeftMouseButtonDragged(float deltaX, float deltaY);
        public abstract void OnMiddleMouseButtonDragged(float deltaY);
        public abstract void OnRightMouseButtonDragged(float deltaX, float deltaY);
    }
}
