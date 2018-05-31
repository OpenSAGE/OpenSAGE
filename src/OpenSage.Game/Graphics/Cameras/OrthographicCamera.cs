using System.Numerics;

namespace OpenSage.Graphics.Cameras
{
    public class OrthographicCamera : Camera
    {
        private float _minX;
        private float _minY;
        private float _maxX;
        private float _maxY;

        public float MinX
        {
            get { return _minX; }
            set
            {
                _minX = value;
                SetProjectionDirty();
            }
        }

        public float MinY
        {
            get { return _minY; }
            set
            {
                _minY = value;
                SetProjectionDirty();
            }
        }

        public float MaxX
        {
            get { return _maxX; }
            set
            {
                _maxX = value;
                SetProjectionDirty();
            }
        }

        public float MaxY
        {
            get { return _maxY; }
            set
            {
                _maxY = value;
                SetProjectionDirty();
            }
        }

        public OrthographicCamera(
            float minX, float minY, float maxX, float maxY,
            float nearZ, float farZ)
        {
            _minX = minX;
            _minY = minY;
            _maxX = maxX;
            _maxY = maxY;

            NearPlaneDistance = nearZ;
            FarPlaneDistance = farZ;

            SetProjectionDirty();
        }

        protected override void CreateProjection(out Matrix4x4 projection)
        {
            projection = Matrix4x4.CreateOrthographicOffCenter(_minX, _maxX, _minY, _maxY, NearPlaneDistance, FarPlaneDistance);
        }
    }
}
